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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays and allows for the editing of the list of users.
    /// </summary>
    public partial class PageUsers : Page
    {
        #region PageSummary
        /// <summary>
        /// The page summary object.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = ResourceImages.User16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override string PageTitle { get { return Strings.Users; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return _PageIcon; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageUsers();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            protected override void AssociateChildPages()
            {
                base.AssociateChildPages();
                AssociateListWithChildPages(SettingsView.Users, () => new PageUser.Summary());
            }
        }
        #endregion

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool PageUseFullHeight { get { return true; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageUsers()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();

            AddControlBinder(new LabelStringBinder<SettingsView>(SettingsView, labelUserManager, r => r.UserManager, (r,v) => r.UserManager = v));
            AddControlBinder(new MasterListToListBinder<SettingsView, IUser>(SettingsView, listUsers, r => r.Users) {
                FetchColumns = (user, e) => {
                    e.Checked = user.Enabled;
                    e.ColumnTexts.Add(user.LoginName ?? "");
                    e.ColumnTexts.Add(user.Name ?? "");
                },
                AddHandler = () => SettingsView.CreateUser(),
                DeleteHandler = (r) => SettingsView.RemoveUsers(r),
                EditHandler = (user) => SettingsView.DisplayPageForPageObject(user),
                CheckedChangedHandler = (user, isChecked) => user.Enabled = isChecked,
            });
        }
    }
}
