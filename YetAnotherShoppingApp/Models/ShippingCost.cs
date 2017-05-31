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
    class ShippingCost
    {
        private Dictionary<ShippingType, decimal> _costs;

        public ShippingCost(IDictionary<ShippingType, decimal> costs)
        {
            _costs = new Dictionary<ShippingType, decimal>(costs);
        }

        /// <summary>
        /// Creates a 'ShippingCost' from an arbitrary baseline value.
        /// </summary>
        /// <param name="baseline"></param>
        /// <returns></returns>
        public static ShippingCost CreateFromBaseline(decimal baseline)
        {
            return new ShippingCost(new Dictionary<ShippingType, decimal>
            {
                { ShippingType.NationalStandard, baseline },
                { ShippingType.NationalTwoDay, baseline * 1.99m },
                { ShippingType.NationalOvernight, baseline * 3.05m },
                { ShippingType.InternationalStandard, baseline * 2.12m },
                { ShippingType.InternationalThreeDay, baseline * 4.10m },
            });
        }

        /// <summary>
        /// The set of shipping types available and their associated cost.
        /// </summary>
        public IReadOnlyDictionary<ShippingType, decimal> Costs
        {
            get
            {
                return _costs;
            }
        }
    }
}
