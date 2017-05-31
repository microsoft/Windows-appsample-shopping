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
using System.Linq;
using Windows.ApplicationModel.Payments;

namespace YetAnotherShoppingApp
{
    delegate void ShoppingCartEntriesChangedEventHandler(ShoppingCart sender, ShoppingCartEntriesChangedEventArgs args);
    delegate void ShoppingCartCostsSummaryChangedEventHandler(ShoppingCart sender);

    /// <summary>
    /// An entry in a shopping cart.
    /// </summary>
    struct ShoppingCartEntry
    {
        public Product Product;
        public int Quantity;
    }

    /// <summary>
    /// The costs summary (e.g. sub-total, tax, etc.) of a shopping cart.
    /// </summary>
    struct ShoppingCartCostsSummary
    {
        public decimal ItemsSubtotal;
        public decimal Shipping;
        public decimal ItemsTax;
        public decimal ShippingTax;
        public decimal TotalTax;
        public decimal Total;
    }

    /// <summary>
    /// Manages all the business logic of a shopping cart.
    /// </summary>
    class ShoppingCart
    {
        private List<ShoppingCartEntry> _shoppingCartEntries = null;
        private ShippingType _shippingType = ShippingType.NationalStandard;

        public ShoppingCart()
        {
            _shippingType = ShippingType.NationalStandard;
            _shoppingCartEntries = new List<ShoppingCartEntry>();

            UpdateCostsSummary();
        }

        /// <summary>
        /// The list of entries in the shopping cart.
        /// </summary>
        public IReadOnlyList<ShoppingCartEntry> Entries
        {
            get
            {
                return _shoppingCartEntries;
            }
        }

        /// <summary>
        /// Raised when the 'Entries' list is changed or when one of its items is updated.
        /// </summary>
        public event ShoppingCartEntriesChangedEventHandler EntriesChanged;

        /// <summary>
        /// Raised when the 'CostsSummary' property is changed.
        /// </summary>
        public event ShoppingCartCostsSummaryChangedEventHandler CostsSummaryChanged;

        /// <summary>
        /// The type of shipping to be used.
        /// </summary>
        public ShippingType ShippingType
        {
            get
            {
                return _shippingType;
            }

            set
            {
                _shippingType = value;
                UpdateCostsSummary();
            }
        }

        /// <summary>
        /// The costs summary (e.g. sub-total, tax, etc.) of the shopping cart
        /// </summary>
        public ShoppingCartCostsSummary CostsSummary
        {
            private set;
            get;
        }

        /// <summary>
        /// The address to be shipped to.
        /// </summary>
        public PaymentAddress ShippingAddress
        {
            private set;
            get;
        }

        /// <summary>
        /// Sets the address to be shipped to.
        /// </summary>
        /// <param name="address"></param>
        public void SetShippingAddress(PaymentAddress address)
        {
            this.ShippingAddress = address;

            if (!DoesAddressRequireInternationalShipping(this.ShippingAddress))
            {
                if (!ShippingTypeInfo.NationalShippingTypes.Contains(_shippingType))
                {
                    _shippingType = ShippingType.NationalStandard;
                }
            }
            else
            {
                if (!ShippingTypeInfo.InternationalShippingTypes.Contains(_shippingType))
                {
                    _shippingType = ShippingType.InternationalStandard;
                }
            }

            UpdateCostsSummary();
        }

        private void AddOrUpdateEntry(Product product, Func<ShoppingCartEntry, ShoppingCartEntry> updateFunc)
        {
            int productIndex = _shoppingCartEntries.IndexOf((entry) => (entry.Product == product));

            if (productIndex == -1)
            {
                // Create new entry.
                ShoppingCartEntry entry = new ShoppingCartEntry()
                {
                    Product = product,
                    Quantity = 0,
                };

                // Allow update.
                entry = updateFunc(entry);

                // Add to list.
                _shoppingCartEntries.Add(entry);

                // Raise changed event.
                RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType.EntryAdded, _shoppingCartEntries.Count - 1);
            }
            else
            {
                ShoppingCartEntry entry = _shoppingCartEntries[productIndex];

                // Apply update
                entry = updateFunc(entry);
                _shoppingCartEntries[productIndex] = entry;

                // Raise changed event.
                RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType.EntryUpdated, productIndex);
            }
        }

        /// <summary>
        /// Adds the specified quantity of the product in the cart. If the product doesn't currently exist
        /// it will be added.
        /// </summary>
        public void Add(Product product, int quantity)
        {
            AddOrUpdateEntry(
                product,
                (entry) =>
            {
                int newQuantity = entry.Quantity + quantity;

                if (newQuantity < 0)
                {
                    throw new Exception("Quantity can't go below 0.");
                }

                entry.Quantity = newQuantity;
                return entry;
            });
        }

        /// <summary>
        /// Sets the quantity of the specified product within the shopping cart. This will add
        /// the product to the cart if it doesn't already exist. Otherwise the existing quantity will
        /// be updated.
        /// </summary>
        public void SetProductQuantity(Product product, int quantity)
        {
            if (quantity < 0)
            {
                throw new Exception("'quantity' can't be less than 0.");
            }

            AddOrUpdateEntry(
                product,
                (entry) =>
            {
                entry.Quantity = quantity;
                return entry;
            });
        }

        private void RemoveIf(Func<ShoppingCartEntry, bool> pred)
        {
            int lastRemovedIndex = -1;
            int removedCount = 0;

            for (int i = _shoppingCartEntries.Count - 1; i >= 0; --i)
            {
                if (pred(_shoppingCartEntries[i]))
                {
                    _shoppingCartEntries.RemoveAt(i);
                    lastRemovedIndex = i;
                    removedCount++;
                }
            }

            // Raise changed event
            if (removedCount == 1)
            {
                RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType.EntryRemoved, lastRemovedIndex);
            }
            else if (removedCount > 1)
            {
                // Too many items were changed.
                // So just report a reset.
                RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType.EntriesReset);
            }
        }

        /// <summary>
        /// Removes the specified product from the shopping cart.
        /// </summary>
        /// <param name="product">The product to be removed.</param>
        public void Remove(Product product)
        {
            RemoveIf((entry) => entry.Product == product);
        }

        /// <summary>
        /// Removes all the items from the Shopping Cart.
        /// </summary>
        public void Clear()
        {
            RemoveIf((entry) => true);
        }

        /// <summary>
        /// Goes through the items in the shopping cart and removes any items that have a 'Quantity' of 0.
        /// </summary>
        public void RemoveZeroQuantityItems()
        {
            RemoveIf((entry) => (entry.Quantity <= 0));
        }

        /// <summary>
        /// Checks whether or not the shopping cart contains the specified 'Product'.
        /// </summary>
        /// <param name="product">The product to check for.</param>
        /// <returns>Whether or not the product is in the shopping cart.</returns>
        public bool Contains(Product product)
        {
            return _shoppingCartEntries.Any((entry) => (entry.Product == product));
        }

        public IReadOnlyDictionary<ShippingType, ShoppingCartCostsSummary> CalculateShippingOptions()
        {
            Dictionary<ShippingType, ShoppingCartCostsSummary> result = new Dictionary<ShippingType, ShoppingCartCostsSummary>();

            IReadOnlyList<ShippingType> avaliableShippingTypes = DoesAddressRequireInternationalShipping(this.ShippingAddress) ?
                ShippingTypeInfo.InternationalShippingTypes :
                ShippingTypeInfo.NationalShippingTypes;

            foreach (ShippingType shippingType in avaliableShippingTypes)
            {
                result.Add(shippingType, CalculateCostsSummary(this.Entries, shippingType, this.ShippingAddress));
            }

            return result;
        }

        /// <summary>
        /// Called when the 'Entries' list has changed, so that listeners can be notified.
        /// </summary>
        private void RaiseEntriesChangedEvent(ShoppingCartEntriesChangedType type, int itemIndex = -1)
        {
            // Update 'CostsSummary' property.
            UpdateCostsSummary();

            // Invoke public event.
            var args = new ShoppingCartEntriesChangedEventArgs(type, itemIndex);
            this.EntriesChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Updates the 'CostsSummary' property.
        /// </summary>
        private void UpdateCostsSummary()
        {
            this.CostsSummary = CalculateCostsSummary(this.Entries, this.ShippingType, this.ShippingAddress);
            this.CostsSummaryChanged?.Invoke(this);
        }

        /// <summary>
        /// Calculate the summary of costs (e.g. sub-total, tax, etc.) for the given list of shopping cart entries.
        /// </summary>
        private static ShoppingCartCostsSummary CalculateCostsSummary(IReadOnlyList<ShoppingCartEntry> entries, ShippingType shippingType, PaymentAddress shippingAddress)
        {
            ShoppingCartCostsSummary costsSummary = new ShoppingCartCostsSummary();

            foreach (ShoppingCartEntry entry in entries)
            {
                costsSummary.ItemsSubtotal += entry.Quantity * entry.Product.Cost;
                costsSummary.Shipping += entry.Product.ShippingCost.Costs[shippingType];
            }

            costsSummary.Shipping += ShippingFixedCosts.Costs[shippingType];

            CalculateTax(
                shippingAddress,
                costsSummary.ItemsSubtotal,
                costsSummary.Shipping,
                out costsSummary.ItemsTax,
                out costsSummary.ShippingTax);

            costsSummary.TotalTax = costsSummary.ItemsTax + costsSummary.ShippingTax;
            costsSummary.Total = costsSummary.ItemsSubtotal + costsSummary.Shipping + costsSummary.TotalTax;

            return costsSummary;
        }

        /// <summary>
        /// Calculates the amount of sales tax required for the current shopping cart.
        /// We are deliberately applying tax to the shipping cost, so the shipping cost can change based on the
        /// address the user provides.
        /// </summary>
        private static void CalculateTax(PaymentAddress shippingAddress, decimal subtotal, decimal shippingCost, out decimal subtotalTax, out decimal shippingTax)
        {
            subtotalTax = 0;
            shippingTax = 0;

            if (shippingAddress == null)
            {
                return;
            }

            //
            // WARNING!
            //
            // THIS CODE IS FOR EXAMPLE PURPOSES ONLY. IT HAS NO BASIS IN REALITY. ANY SIMILARITIES TO ANY
            // REAL TAX CODES IS COINCIDENTAL AND UNINTENTIONAL.
            // 
            switch (shippingAddress.Country.ToUpperInvariant())
            {
            case "US":
            case "USA":
            case "UNITED STATES":
            case "UNITED STATES OF AMERICA":
                switch(shippingAddress.Region.ToUpperInvariant())
                {
                case "WA":
                case "WASHINGTON":
                case "WASHINGTON STATE":
                    subtotalTax = subtotal * 0.2m;
                    shippingTax = shippingCost * 0.2m;
                    return;
                }
                break;
            }
        }

        /// <summary>
        /// Checks if the address assigned to 'this.ShippingAddress' requires international shipping.
        /// </summary>
        /// <returns></returns>
        private static bool DoesAddressRequireInternationalShipping(PaymentAddress address)
        {
            string county = address?.Country?.ToUpperInvariant();

            switch (county)
            {
            case "US":
            case "USA":
            case "UNITED STATES":
            case "UNITED STATES OF AMERICA":
                return false;
            }

            return !string.IsNullOrWhiteSpace(county);
        }
    }
}
