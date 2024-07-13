﻿// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interop;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A self-contained implementation of IComparer that can automatically sort
    /// columns when the user clicks on their headers. 
    /// </summary>
    public class AutoListViewSorter : IComparer, IDisposable
    {
        /// <summary>
        /// The list view that we're attached to.
        /// </summary>
        private ListView _ListView;

        /// <summary>
        /// True if <see cref="_ListView"/> has been hooked.
        /// </summary>
        private bool _HookedListView;

        /// <summary>
        /// True if the native sort indicators have been set up.
        /// </summary>
        private bool _UpdatedSortIndicators;

        /// <summary>
        /// Gets a value indicating that the list view is currently using this comparer to handle
        /// its sorting.
        /// </summary>
        public bool Attached { get { return _ListView.ListViewItemSorter == this; } }

        private ColumnHeader _SortColumn;
        /// <summary>
        /// Gets or sets the column header that the list view is being sorted by.
        /// </summary>
        public ColumnHeader SortColumn
        {
            get { return _SortColumn; }
            set {
                var current = _SortColumn;
                if(current == null) current = _ListView.Columns.OfType<ColumnHeader>().FirstOrDefault(r => r.Index == 0);
                if(current == value) SortAscending = !SortAscending;
                else {
                    _SortColumn = value;
                    SortAscending = true;
                }

                _UpdatedSortIndicators = false;
            }
        }

        private bool _SortAscending;
        /// <summary>
        /// Gets or sets a value indicating that the sort direction is in ascending order.
        /// </summary>
        public bool SortAscending
        {
            get { return _SortAscending; }
            set {
                if(_SortAscending != value) {
                    _SortAscending = value;
                    _UpdatedSortIndicators = false;
                }
            }
        }

        /// <summary>
        /// Gets the sub-item index represented by the <see cref="SortColumn"/>. If <see cref="SortColumn"/>
        /// is null then 0 is returned (i.e. the first column).
        /// </summary>
        public int SortColumnSubItemIndex
        {
            get { return SortColumn == null ? 0 : SortColumn.Index; }
        }

        /// <summary>
        /// Gets or sets a value indicating that case should be ignored when comparing strings.
        /// </summary>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the sort column and direction should be shown on
        /// column headers using the native API.
        /// </summary>
        /// <remarks>
        /// At the time of writing this only has an effect when running under Windows.
        /// </remarks>
        public bool ShowNativeSortIndicators { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="showNativeSortIndicators"></param>
        public AutoListViewSorter(ListView listView, bool showNativeSortIndicators = true)
        {
            _ListView = listView;
            _ListView.ColumnClick += ListView_ColumnClick;
            _HookedListView = true;
            SortAscending = true;
            ShowNativeSortIndicators = showNativeSortIndicators;
            IgnoreCase = true;

            // This is unlikely to do anything if the listview isn't already on display, but we can give it a shot
            RefreshSortIndicators();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~AutoListViewSorter()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(_HookedListView) {
                    _HookedListView = false;
                    _ListView.ColumnClick -= ListView_ColumnClick;
                }
            }
        }

        /// <summary>
        /// Returns the current value of <see cref="SortColumn"/> for the row
        /// passed across.
        /// </summary>
        /// <param name="listViewItem"></param>
        /// <returns></returns>
        public virtual IComparable GetRowValue(ListViewItem listViewItem)
        {
            var result = listViewItem.Text;
            var subItem = listViewItem.SubItems[SortColumnSubItemIndex];

            return subItem.Text;
        }

        /// <summary>
        /// Returns the comparable sort order between the left- and right-hand
        /// side values passed across. By default it performs a string comparison on
        /// the list view's sub-item text.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public int CompareRowValues(IComparable lhs, IComparable rhs)
        {
            var result = 0;

            if(lhs != null || rhs != null) {
                if(lhs == null) result = -1;
                else if(rhs == null) result = 1;
                else {
                    if(IgnoreCase && lhs is string) result = String.Compare((string)lhs, (string)rhs, StringComparison.CurrentCultureIgnoreCase);
                    else result = lhs.CompareTo(rhs);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            int result = 0;
            var lhsItem = x as ListViewItem;
            var rhsItem = y as ListViewItem;
            if(lhsItem != null && rhsItem != null) {
                RefreshSortIndicators();

                var lhsValue = GetRowValue(lhsItem);
                var rhsValue = GetRowValue(rhsItem);
                result = CompareRowValues(lhsValue, rhsValue);
                if(!SortAscending) result = -result;
            }

            return result;
        }

        /// <summary>
        /// Displays the sort indicators on the columns.
        /// </summary>
        public void RefreshSortIndicators()
        {
            if(_ListView.IsHandleCreated && !_UpdatedSortIndicators && ShowNativeSortIndicators) {
                _UpdatedSortIndicators = true;

                const uint LVM_GETHEADER = 0x1000 + 31;

                var header = Window.CallSendMessage(_ListView.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
                if(header != IntPtr.Zero) {
                    for(var i = 0;i < _ListView.Columns.Count;++i) {
                        var isSortColumn = SortColumn == null ? i == 0 : SortColumn.Index == i;
                        
                        var colIndex = new IntPtr(i);
                        var hditem = new HDITEM() {
                            mask = HDITEM.HDI_FORMAT,
                        };
                        if(Window.CallSendMessage(header, HDITEM.HDM_GETITEM, colIndex, ref hditem) != IntPtr.Zero) {
                            if(!isSortColumn) {
                                hditem.fmt &= ~(HDITEM.HDF_SORTUP | HDITEM.HDF_SORTDOWN);
                            } else {
                                if(SortAscending) {
                                    hditem.fmt &= ~HDITEM.HDF_SORTDOWN;
                                    hditem.fmt |= HDITEM.HDF_SORTUP;
                                } else {
                                    hditem.fmt &= ~HDITEM.HDF_SORTUP;
                                    hditem.fmt |= HDITEM.HDF_SORTDOWN;
                                }
                            }

                            Window.CallSendMessage(header, HDITEM.HDM_SETITEM, colIndex, ref hditem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the column header for the click index passed across.
        /// </summary>
        /// <param name="clickIndex"></param>
        /// <returns></returns>
        public ColumnHeader FindColumnHeaderForClickIndex(int clickIndex)
        {
            ColumnHeader result = _ListView.Columns.OfType<ColumnHeader>().FirstOrDefault(r => r.Index == clickIndex);
            return result;
        }

        /// <summary>
        /// Called when the user clicks a column header on the list view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ListView_ColumnClick(object sender, ColumnClickEventArgs args)
        {
            var sortColumn = FindColumnHeaderForClickIndex(args.Column);
            SortColumn = sortColumn;
            if(Attached) {
                RefreshSortIndicators();
                _ListView.Sort();
            }
        }
    }
}
