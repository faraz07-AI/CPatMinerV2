﻿// Copyright © 2018 onwards, Andrew Whewell
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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Plugin.SqlServer.WinForms
{
    /// <summary>
    /// Performs actions on behalf of an options view.
    /// </summary>
    class OptionsPresenter
    {
        /// <summary>
        /// Returns null if the connection string is good, otherwise the error reported when trying to use the connection string.
        /// </summary>
        public string TestConnection(string connectionString)
        {
            string result = null;

            try {
                using(var connection = new SqlConnection(connectionString)) {
                    connection.Open();
                }
            } catch(Exception ex) {
                result = String.IsNullOrEmpty(ex?.Message) ? SqlServerStrings.UnspecifiedError : ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Updates the schema using the connection details passed across and returns an array of output lines from the update script.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public string[] UpdateSchema(string connectionString, int timeoutSeconds)
        {
            var database = new BaseStationDatabase() {
                ConnectionString =      connectionString,
                CommandTimeoutSeconds = timeoutSeconds,
                CanUpdateSchema =       true,
            };
            var scriptOutput = database.UpdateSchema();

            if(   scriptOutput == null
               || scriptOutput.Length == 0
               || (scriptOutput.Length == 1 && String.IsNullOrEmpty(scriptOutput[0]))
            ) {
                scriptOutput = new string[] { SqlServerStrings.SchemaUpdateFailed };
            }

            return scriptOutput;
        }
    }
}
