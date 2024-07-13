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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays a single user's details and accepts edits to them.
    /// </summary>
    public partial class PageUser : Page
    {
        #region PageSummary
        /// <summary>
        /// Describes the content of the user page.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = ResourceImages.User16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return _PageIcon; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public IUser User { get { return PageObject as IUser; } }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            public Summary() : base()
            {
                SetPageTitleProperty<IUser>(r => r.LoginName, () => User.LoginName);
                SetPageEnabledProperty<IUser>(r => r.Enabled, () => User.Enabled);
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageUser();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="record"></param>
            /// <returns></returns>
            internal override bool IsForSameRecord(object record)
            {
                var user = record as IUser;
                return user != null && User != null && user.UniqueId == User.UniqueId;
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageUser;
                SetValidationFields(new Dictionary<ValidationField,Control>() {
                    { ValidationField.LoginName,    page == null ? null : page.textBoxLoginName },
                    { ValidationField.Password,     page == null ? null : page.textBoxPassword },
                    { ValidationField.Name,         page == null ? null : page.textBoxUserName },
                });
            }
        }
        #endregion

        /// <summary>
        /// See base docs.
        /// </summary>
        public IUser User { get { return ((Summary)PageSummary).User; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageUser()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void InitialiseControls()
        {
            base.InitialiseControls();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();

            AddControlBinder(new CheckBoxBoolBinder<IUser>(User, checkBoxEnabled, r => r.Enabled, (r,v) => r.Enabled = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new TextBoxStringBinder<IUser>(User, textBoxLoginName, r => r.LoginName,   (r,v) => r.LoginName = v)   { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new TextBoxStringBinder<IUser>(User, textBoxPassword,  r => r.UIPassword,  (r,v) => r.UIPassword = v));
            AddControlBinder(new TextBoxStringBinder<IUser>(User, textBoxUserName,  r => r.Name,        (r,v) => r.Name = v));
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxEnabled,  Strings.Enabled,    Strings.OptionsDescribeUserEnabled);
            SetInlineHelp(textBoxLoginName, Strings.LoginName,  Strings.OptionsDescribeLoginName);
            SetInlineHelp(textBoxPassword,  Strings.Password,   Strings.OptionsDescribeUserPassword);
            SetInlineHelp(textBoxUserName,  Strings.Name,       Strings.OptionsDescribeUserName);
        }
    }
}
