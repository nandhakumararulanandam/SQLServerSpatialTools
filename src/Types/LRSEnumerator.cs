// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SQLSpatialTools.Types
{
    /// <summary>
    /// Enumerator for LRS Types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.IEnumerator" />
    internal class LRSEnumerator<T> : IEnumerator
    {
        internal List<T> ListOfItems;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int _position = -1;

        public LRSEnumerator(List<T> list)
        {
            ListOfItems = list;
        }

        public bool MoveNext()
        {
            _position++;
            return (_position < ListOfItems.Count);
        }

        public void Reset()
        {
            _position = -1;
        }

        object IEnumerator.Current => Current;

        public T Current
        {
            get
            {
                try
                {
                    return ListOfItems[_position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}