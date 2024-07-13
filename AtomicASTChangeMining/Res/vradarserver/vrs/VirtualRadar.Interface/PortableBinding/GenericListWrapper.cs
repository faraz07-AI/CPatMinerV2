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

namespace VirtualRadar.Interface.PortableBinding
{
    /// <summary>
    /// A wrapper that accepts a generic IList and exposes the non-generic IList interface for it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Using generics with WinForms controls is a PITA but using the non-generic version of IList outside of
    /// controls is not good. This is a simple-minded wrapper that takes the non-generic version of a list
    /// that the program uses and translates it into an IList that can be used with WinForms controls.
    /// </remarks>
    public class GenericListWrapper<T> : IList
    {
        private IList<T> _List;

        /// <summary>
        /// Gets the list that is being wrapped.
        /// </summary>
        public IList<T> GenericList
        {
            get { return _List; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object this[int index]
        {
            get { return _List[index]; }
            set { _List[index] = (T)value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Count
        {
            get { return _List.Count; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public object SyncRoot
        {
            get { return null; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsReadOnly
        {
            get { return _List.IsReadOnly; }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="list"></param>
        public GenericListWrapper(IList<T> list)
        {
            _List = list;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(object value)
        {
            var result = _List.Count;
            _List.Add((T)value);
            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Clear()
        {
            _List.Clear();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(object value)
        {
            return _List.Contains((T)value);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(object value)
        {
            return _List.IndexOf((T)value);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, object value)
        {
            _List.Insert(index, (T)value);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="value"></param>
        public void Remove(object value)
        {
            _List.Remove((T)value);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            _List.RemoveAt(index);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(Array array, int index)
        {
            _List.CopyTo((T[])array, index);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return _List.GetEnumerator();
        }
    }
}
