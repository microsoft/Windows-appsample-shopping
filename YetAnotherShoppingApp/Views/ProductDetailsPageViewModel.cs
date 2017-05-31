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
using System.Collections.Generic;

namespace YetAnotherShoppingApp
{
    class ProductDetailsPageViewModel
    {
        /// <summary>
        /// The standard list of options available in the quantity combo box.
        /// </summary>
        private static readonly int[] s_quantityListOptions = new int[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9,
        };

        public ProductDetailsPageViewModel()
        {
            this.SelectedQuantity = QuantityList[0];
            this.ShoppingCart = AppState.ShoppingCart;
        }

        /// <summary>
        /// The view model of the product being displayed on this page.
        /// </summary>
        public ProductViewModel ProductViewModel
        {
            private set;
            get;
        }

        /// <summary>
        /// The product being displayed on this page.
        /// </summary>
        public Product Product
        {
            get
            {
                return this.ProductViewModel.Product;
            }
        }

        /// <summary>
        /// The shopping cart currently in use.
        /// </summary>
        public ShoppingCart ShoppingCart
        {
            private set;
            get;
        }

        /// <summary>
        /// The list of options available in the quantity combo box.
        /// </summary>
        public IReadOnlyList<int> QuantityList
        {
            get
            {
                return s_quantityListOptions;
            }
        }

        /// <summary>
        /// The currently selected item in the quantity combo box.
        /// </summary>
        public object SelectedQuantity
        {
            set;
            get;
        }

        /// <summary>
        /// Sets the product to be displayed in this page.
        /// </summary>
        public void SetProduct(ProductViewModel productViewModel)
        {
            this.ProductViewModel = productViewModel;
        }

        /// <summary>
        /// Invoked when the user clicks on the 'Add to cart' button.
        /// </summary>
        public void AddToCartClicked()
        {
            this.ShoppingCart.Add(this.ProductViewModel.Product, (int)this.SelectedQuantity);
        }
    }
}
