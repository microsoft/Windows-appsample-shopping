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
using System.Globalization;
using Windows.ApplicationModel.Payments;

namespace YetAnotherShoppingApp
{
    public static class PriceStringUtilities
    {
        private static readonly CultureInfo USDCulture = new CultureInfo("en-US");

        public static readonly string Currency = "USD";

        /// <summary>
        /// Correctly formats a price as a string.
        /// </summary>
        public static string CreatePriceString(decimal cost)
        {
            return String.Format(USDCulture, "{0:C}", cost);
        }

        public static string CreatePriceString(PaymentCurrencyAmount cost)
        {
            decimal value = FromInvariantString(cost.Value);
            return String.Format(USDCulture, "{0:C}", value);
        }

        public static string ToInvariantString(decimal cost)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F2}", cost);
        }

        public static decimal FromInvariantString(string value)
        {
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
