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
using Windows.UI.Xaml.Controls;

namespace YetAnotherShoppingApp
{
    sealed partial class FrontPage : Page
    {
        internal FrontPageViewModel ViewModel { private set; get; }

        public FrontPage()
        {
            this.ViewModel = new FrontPageViewModel();

            this.InitializeComponent();
        }

        private void ProductListItemClicked(object sender, ItemClickEventArgs e)
        {
            var productViewModel = (ProductViewModel)e.ClickedItem;
            this.ViewModel.OnProductListItemClicked(productViewModel);
        }

        private void OnItemBuyClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var button = (Button)sender;
            var productViewModel = (ProductViewModel)button.Tag;
            this.ViewModel.OnBuyClick(productViewModel);
        }
    }
}
