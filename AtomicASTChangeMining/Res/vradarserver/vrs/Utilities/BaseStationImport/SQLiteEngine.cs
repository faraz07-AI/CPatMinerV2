﻿// Copyright © 2017 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface.Database;

namespace BaseStationImport
{
    /// <summary>
    /// The SQLite specialisation of <see cref="Engine"/>.
    /// </summary>
    class SQLiteEngine : Engine
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool UsesFileName => true;

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override string[] ValidateOptions(DatabaseEngineOptions options)
        {
            var result = new List<string>();
            var direction = options.IsSource ? "source" : "target";

            var fileName = options.ConnectionString;
            if(String.IsNullOrWhiteSpace(fileName)) {
                result.Add($"You must specify the name of the {direction} SQLite file");
            } else if(options.IsSource) {
                if(!File.Exists(fileName)) {
                    result.Add($"The {direction} SQLite file {fileName} does not exist");
                }
            } else if(options.IsTarget) {
                var path = Path.GetDirectoryName(fileName);
                if(!Directory.Exists(path)) {
                    result.Add($"The {direction} SQLite file {fileName} specifies a folder that does not exist");
                }
            }

            if(options.CommandTimeoutSeconds != null) {
                result.Add("VRS does not support command timeouts for SQLite databases");
            }

            return result.ToArray();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override IBaseStationDatabase CreateRepository(DatabaseEngineOptions options)
        {
            var result = Factory.Resolve<IBaseStationDatabaseSQLite>();
            result.FileName = options.ConnectionString;
            result.WriteSupportEnabled = options.IsTarget;

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override string[] UpdateSchema(DatabaseEngineOptions options)
        {
            using(var repository = CreateRepository(options)) {
                repository.CreateDatabaseIfMissing(repository.FileName);
                return new string[] { $"{repository.FileName} created / updated" };
            }
        }
    }
}
