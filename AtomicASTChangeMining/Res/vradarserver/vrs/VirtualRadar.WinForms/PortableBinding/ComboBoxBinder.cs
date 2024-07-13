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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// A binder that populates a combo box with a list and lets the user select a single
    /// value from it.
    /// </summary>
    public class ComboBoxBinder<TModel, TListModel, TValue> : ValueFromListBinder<TModel, ComboBox, TValue, TListModel>
        where TModel: class, INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="list"></param>
        /// <param name="getModelValue"></param>
        /// <param name="setModelValue"></param>
        public ComboBoxBinder(TModel model, ComboBox control, IList<TListModel> list, Expression<Func<TModel, TValue>> getModelValue, Action<TModel, TValue> setModelValue)
            : base(model, control, list,
                   getModelValue, setModelValue)
        {
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="itemDescriptions"></param>
        protected override void DoCopyListToControl(IEnumerable<ItemDescription<TListModel>> itemDescriptions)
        {
            Control.Items.Clear();
            foreach(var itemDescription in itemDescriptions) {
                Control.Items.Add(itemDescription);
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        protected override TValue DoGetSelectedListValue()
        {
            var selectedItem = Control.SelectedItem as ItemDescription<TListModel>;
            var result = selectedItem == null ? default(TValue) : GetListItemValue(selectedItem.Item);
            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="value"></param>
        protected override void DoSetSelectedListValue(TValue value)
        {
            var itemDescription = Control.Items.OfType<ItemDescription<TListModel>>()
                                         .FirstOrDefault(r => {
                                            var controlValue = GetListItemValue(r.Item);
                                            var areEqual = Object.Equals(controlValue, value);
                                            return areEqual;
                                         });
            Control.SelectedItem = itemDescription;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected override void DoHookControlPropertyChanged(EventHandler eventHandler)
        {
            Control.SelectedIndexChanged += eventHandler;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected override void DoUnhookControlPropertyChanged(EventHandler eventHandler)
        {
            Control.SelectedIndexChanged -= eventHandler;
        }
    }
}
