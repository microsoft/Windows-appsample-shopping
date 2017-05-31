//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using System;

namespace YetAnotherShoppingApp
{
    /// <summary>
    /// The type of change made to the shopping cart entries list.
    /// </summary>
    enum ShoppingCartEntriesChangedType
    {
        /// <summary>
        /// The entries list has had significant modification made to it.
        /// </summary>
        EntriesReset,

        /// <summary>
        /// An entry has been added.
        /// </summary>
        EntryAdded,

        /// <summary>
        /// An entry has been removed.
        /// </summary>
        EntryRemoved,

        /// <summary>
        /// An entry has been modified.
        /// </summary>
        EntryUpdated,
    }

    /// <summary>
    /// The event arguments for the 'ShoppingCart.EntriesChanged' event.
    /// </summary>
    class ShoppingCartEntriesChangedEventArgs
    {
        public ShoppingCartEntriesChangedEventArgs(ShoppingCartEntriesChangedType type, int index)
        {
            this.Type = type;
            this.Index = index;
        }

        /// <summary>
        /// The type of change that was made.
        /// </summary>
        public ShoppingCartEntriesChangedType Type
        {
            private set;
            get;
        }

        /// <summary>
        /// The index of the changed entry (if applicable).
        /// </summary>
        public int Index
        {
            private set;
            get;
        }
    }
}
