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
using System.Linq;
using Windows.UI.Xaml;

namespace YetAnotherShoppingApp
{
    class ShoppingCartIconViewModel :
        NotificationBase
    {
        public ShoppingCartIconViewModel()
        {
            this.ShoppingCart = AppState.ShoppingCart;
        }

        /// <summary>
        /// The shopping cart model.
        /// </summary>
        public ShoppingCart ShoppingCart
        {
            private set;
            get;
        }

        /// <summary>
        /// The number of items in the shopping cart.
        /// </summary>
        public int ItemCount
        {
            private set;
            get;
        }

        /// <summary>
        /// Whether or not to show the item count over the icon.
        /// </summary>
        public Visibility ItemCountVisibility
        {
            private set;
            get;
        }

        /// <summary>
        /// Called when the 'ShoppingCartIcon' control is loaded into the XAML tree.
        /// </summary>
        public void OnLoaded()
        {
            // Register the shopping cart change notifications.
            this.ShoppingCart.EntriesChanged += ShoppingCartEntriesChanged;

            // Ensure our generated values are up to date.
            UpdateGeneratedFields();
        }

        /// <summary>
        /// Called when the 'ShoppingCartIcon' control is unloaded from the XAML tree.
        /// </summary>
        public void OnUnloaded()
        {
            // Unregister the shopping cart change notifications.
            // Since the shopping cart's lifetime will outlive this page, we don't want the shopping cart
            // holding a reference to this class as this will prevent the memory from being freed.
            this.ShoppingCart.EntriesChanged -= ShoppingCartEntriesChanged;
        }

        /// <summary>
        /// Called when the user taps on the icon.
        /// </summary>
        public void IconTapped()
        {
            // Nagivate to the shopping cart page (if we aren't on it already).
            if (App.AppFrame.CurrentSourcePageType == typeof(ShoppingCartPage))
            {
                return;
            }

            App.AppFrame.Navigate(typeof(ShoppingCartPage));
        }

        /// <summary>
        /// Called when the entries list in the shopping cart has been changed.
        /// </summary>
        private void ShoppingCartEntriesChanged(ShoppingCart sender, ShoppingCartEntriesChangedEventArgs args)
        {
            UpdateGeneratedFields();
        }

        /// <summary>
        /// Updates the properties that are generated from data within the shopping cart.
        /// </summary>
        private void UpdateGeneratedFields()
        {
            this.ItemCount = ShoppingCart.Entries.Select(entry => entry.Quantity).Sum();
            this.ItemCountVisibility = this.ItemCount != 0 ? Visibility.Visible : Visibility.Collapsed;

            RaisePropertyChanged(nameof(this.ItemCount));
            RaisePropertyChanged(nameof(this.ItemCountVisibility));
        }
    }
}
