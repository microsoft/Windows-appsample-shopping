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
using System.Threading;

namespace YetAnotherShoppingApp
{
    /// <summary>
    /// Contains the details for a product that is available for sale.
    /// </summary>
    class Product
    {
        public Product(string imageEmojiText, string title, string description, decimal cost, ShippingCost shippingCost)
        {
            this.ImageEmojiText = imageEmojiText;
            this.Title = title;
            this.Description = description;
            this.Cost = cost;
            this.ShippingCost = shippingCost;
        }

        /// <summary>
        /// The image of the product.
        /// We are using emojis so that we don't have to manage a set of image assets.
        /// </summary>
        public string ImageEmojiText { get; private set; }

        /// <summary>
        /// The name of the product.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The detailed description of the product.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The amount the product cost.
        /// Note: Using the 'decimal' type significantly reduces the likelihood of rouding errors compared to other
        /// types such as 'double'.
        /// </summary>
        public decimal Cost { get; private set; }

        /// <summary>
        /// All the costs associated with shipping this product for different speeds and destinations.
        /// </summary>
        public ShippingCost ShippingCost { get; private set; }
    }
}
