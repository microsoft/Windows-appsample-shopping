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
    /// The "database" that stores our list of products.
    /// </summary>
    static class ProductDatabase
    {
        /// <summary>
        /// A list of all the products that are avaliable for sale.
        /// </summary>
        public static readonly Product[] ProductList = new Product[]
        {
            new Product("😃", "Happy face", "A beautiful smile to brighten up your day.", 10.00m, ShippingCostCategories.MediumItem),
            new Product("😟", "Unhappy face", "A depressing frown that brings you back down to reality.", -0.50m, ShippingCostCategories.SmallItem),
            new Product("🐱‍👤", "Ninja cat", "Will fix everything.", 1000.00m, ShippingCostCategories.LargeItem),
        };
    }
}
