﻿// Copyright © 2015 onwards, Andrew Whewell
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
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.WinForms;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.Plugin.FeedFilter.WinForms
{
    /// <summary>
    /// The view that presents the plugin's options to the user.
    /// </summary>
    public partial class OptionsView : BaseForm, IOptionsView
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public Options Options { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FilterSettingsUrl
        {
            get { return linkLabelFilterSettingsLink.Text; }
            set { linkLabelFilterSettingsLink.Text = value; }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OptionsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                PluginLocalise.Form(this);
                ApplyBindings();
                InitialiseControlBinders();
            }
        }

        /// <summary>
        /// Binds controls to the properties.
        /// </summary>
        private void ApplyBindings()
        {
            AddControlBinder(new CheckBoxBoolBinder<Options>(Options, checkBoxEnabled,                      r => r.Enabled,                     (r,v) => r.Enabled = v));
            AddControlBinder(new CheckBoxBoolBinder<Options>(Options, checkBoxProhibitUnfilterableFeeds,    r => r.ProhibitUnfilterableFeeds,   (r,v) => r.ProhibitUnfilterableFeeds = v));
            AddControlBinder(new AccessControlBinder<Options>(Options, accessControl, r => r.Access));
        }

        private void ShowFilterSettingsPage()
        {
            Process.Start(FilterSettingsUrl);
        }

        private void linkLabelFilterSettingsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowFilterSettingsPage();
        }
    }
}
