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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A subclass of ListView that doesn't mess about with the check state when
    /// you double-click a row.
    /// </summary>
    public class ListViewPlus : ListView
    {
        /// <summary>
        /// Gets or sets a value indicating that the Checked state is toggled when the row is double-clicked. By
        /// default WinForms list views toggle the checked state.
        /// </summary>
        [DefaultValue(true)]
        public bool ChangeCheckedOnDoubleClick { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ListViewPlus()
        {
            ChangeCheckedOnDoubleClick = true;

            // Switch on double-buffering
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // Optionally prevent the control from toggling Checked on a double-click
            if(ChangeCheckedOnDoubleClick || m.Msg != 0x203) {
                base.WndProc(ref m);
            } else {
                OnDoubleClick(EventArgs.Empty);
            }
        }
    }
}
