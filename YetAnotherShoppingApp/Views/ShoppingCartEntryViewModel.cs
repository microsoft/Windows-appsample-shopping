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
    class ShoppingCartEntryViewModel :
        NotificationBase
    {
        ShoppingCart _shoppingCart;

        public ShoppingCartEntryViewModel(ShoppingCart shoppingCart, ShoppingCartEntry entry)
        {
            _shoppingCart = shoppingCart;
            this.ProductViewModel = new ProductViewModel(entry.Product);

            Update(entry);
        }

        public ProductViewModel ProductViewModel
        {
            private set;
            get;
        }

        public Product Product
        {
            get
            {
                return this.ProductViewModel.Product;
            }
        }

        /// <summary>
        /// The quantity of the product to be bought, as a string.
        /// </summary>
        private string _quantityString = string.Empty;
        public string QuantityString
        {
            get
            {
                return _quantityString;
            }

            set
            {
                // Try to parse the string.
                int newQuantitiy = 0;
                bool parseSucceeded = int.TryParse(value, out newQuantitiy);

                // Check if the parse succeeded.
                if (parseSucceeded && newQuantitiy >= 0)
                {
                    // Update the quantity of the product in the shopping cart.
                    // This should result in our 'Update' function getting called.
                    _shoppingCart.SetProductQuantity(this.Product, newQuantitiy);
                }
                else
                {
                    // Parse failed.
                    // Inform XAML that we have refused their change.
                    RaisePropertyChanged(nameof(this.QuantityString));
                }
            }
        }

        /// <summary>
        /// Updates this view model with the latest information from the shopping cart entry.
        /// </summary>
        /// <param name="entry"></param>
        public void Update(ShoppingCartEntry entry)
        {
            if (this.Product != entry.Product)
            {
                throw new Exception("Updating with wrong 'Product' entry.");
            }

            _quantityString = entry.Quantity.ToString();
            RaisePropertyChanged(nameof(QuantityString));
        }
    }
}
