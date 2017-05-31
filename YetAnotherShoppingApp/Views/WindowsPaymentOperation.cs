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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.ApplicationModel.Payments;
using Windows.UI.Popups;

namespace YetAnotherShoppingApp
{
    static class WindowsPaymentOperation
    {
        private static readonly string[] SupportedPaymentMethodIds = new string[]
        {
            BasicCardPaymentProtocol.PaymentMethodId,
        };

        private static readonly string[] BasicCardSupportedNetworks = new string[]
        {
            BasicCardNetwork.Visa,
            BasicCardNetwork.Mastercard,
            BasicCardNetwork.AmericanExpress,
        };

        private static readonly BasicCardType[] BasicCardSupportedTypes = new BasicCardType[]
        {
            BasicCardType.Credit,
            BasicCardType.Debit,
            BasicCardType.Prepaid,
        };

        private static readonly PaymentMethodData BasicCardPaymentMethodData = BasicCardPaymentProtocol.GeneratePaymentMethodData(BasicCardSupportedNetworks, BasicCardSupportedTypes);

        private const string MicrosoftPayMerchantId = "<FILL-ME-IN>";

        private static readonly PaymentMethodData MicrosoftPayMethodData = MicrosoftPayProtocol.GeneratePaymentMethodData(MicrosoftPayMerchantId, BasicCardSupportedNetworks, BasicCardSupportedTypes, testMode: true);

        private static readonly PaymentMethodData[] SupportedPaymentMethods = new PaymentMethodData[]
        {
            BasicCardPaymentMethodData,
            //MicrosoftPayMethodData,
        };

        /// <summary>
        /// Checks to see if the OS supports at least one of our supported payment methods.
        /// </summary>
        public static async Task<bool> HasSupportedPaymentMethod()
        {
            PaymentMediator mediator = new PaymentMediator();
            IReadOnlyList<string> paymentAppSupportedMethodIds = await mediator.GetSupportedMethodIdsAsync();

            IEnumerable<string> methodIdIntersection = Enumerable.Intersect(SupportedPaymentMethodIds, paymentAppSupportedMethodIds);

            return methodIdIntersection.Count() > 0;
        }

        /// <summary>
        /// Starts a checkout operation and runs it to completion.
        /// </summary>
        public static async Task CheckoutAsync(ShoppingCart shoppingCart)
        {
            Uri merchantUri = new Uri("http://www.contoso.com");
            PaymentMerchantInfo merchantInfo = new PaymentMerchantInfo(merchantUri);

            PaymentOptions options = new PaymentOptions();
            options.RequestShipping = true;
            options.ShippingType = PaymentShippingType.Delivery;
            options.RequestPayerEmail = PaymentOptionPresence.Optional;
            options.RequestPayerName = PaymentOptionPresence.Required;
            options.RequestPayerPhoneNumber = PaymentOptionPresence.Optional;

            PaymentRequest request = new PaymentRequest(
                CreatePaymentDetails(shoppingCart),
                SupportedPaymentMethods,
                merchantInfo,
                options);

            IAsyncOperation<PaymentRequestSubmitResult> submitOperation = null;

            Action abortSubmitFunc = () =>
            {
                submitOperation.Cancel();
            };

            PaymentRequestChangedHandler changedHandler = (sender, args) =>
            {
                PaymentRequestChangedEventHandler(abortSubmitFunc, args, shoppingCart);
            };

            PaymentMediator mediator = new PaymentMediator();
            submitOperation = mediator.SubmitPaymentRequestAsync(request, changedHandler);

            PaymentRequestSubmitResult result = await submitOperation;

            if (result.Status != PaymentRequestStatus.Succeeded)
            {
                string message = result.Status == PaymentRequestStatus.Canceled ?
                    "The payment was canceled." :
                    "The payment failed.";

                MessageDialog failedMessageDialog = new MessageDialog(message);
                await failedMessageDialog.ShowAsync();

                return;
            }

            PaymentResponse response = result.Response;
            PaymentToken token = response.PaymentToken;

            bool paymentSucceeded = false;
            switch (token.PaymentMethodId)
            {
            case BasicCardPaymentProtocol.PaymentMethodId:
                BasicCardResponse basicCardResponse = BasicCardPaymentProtocol.ParseResponse(token.JsonDetails);

                //
                // TODO: Use data inside 'basicCardResponse' to collect payment.
                //

                paymentSucceeded = true;
                break;

            case MicrosoftPayProtocol.PaymentMethodId:
                MicrosoftPayProtocolResponse microsoftPayResponse = MicrosoftPayProtocol.ParseResponse(token.JsonDetails);

                switch (microsoftPayResponse.Format)
                {
                case "Stripe":
                    //
                    // TODO: Use data inside 'microsoftPayResponse.Token' to collect payment.
                    //

                    paymentSucceeded = true;
                    break;

                case "Error":
                    paymentSucceeded = false;
                    break;

                case "Invalid":
                    throw new Exception("The payment request was invalid.");

                default:
                    throw new Exception("Unsupported payment gateway.");
                }
                break;

            default:
                await result.Response.CompleteAsync(PaymentRequestCompletionStatus.Unknown);
                throw new Exception("Unsupported payment method ID.");
            }

            await result.Response.CompleteAsync(paymentSucceeded ? PaymentRequestCompletionStatus.Succeeded : PaymentRequestCompletionStatus.Failed);

            string completionMessage = paymentSucceeded ?
                "The payment succeeded." :
                "Unable to process the payment.";

            MessageDialog messageDialog = new MessageDialog(completionMessage);
            await messageDialog.ShowAsync();
        }

        private static void PaymentRequestChangedEventHandler(
            Action abortSubmitFunc,
            PaymentRequestChangedArgs args,
            ShoppingCart shoppingCart)
        {
            switch (args.ChangeKind)
            {
            case PaymentRequestChangeKind.ShippingAddress:
                shoppingCart.SetShippingAddress(args.ShippingAddress);
                break;

            case PaymentRequestChangeKind.ShippingOption:
                HandleShippingOptionUpdated(args, shoppingCart);
                break;
            }

            var result =  new PaymentRequestChangedResult(
                changeAcceptedByMerchant: true,
                updatedPaymentDetails: CreatePaymentDetails(shoppingCart));

            args.Acknowledge(result);
        }

        private static void HandleShippingOptionUpdated(PaymentRequestChangedArgs args, ShoppingCart shoppingCart)
        {
            string selectedShippingOptionId = args.SelectedShippingOption.Tag;

            // If the payment provider app decides to send us nothing, just ignore the result.
            if (!string.IsNullOrEmpty(selectedShippingOptionId))
            {
                ShippingType selectedShippingType;

                if (!Enum.TryParse<ShippingType>(selectedShippingOptionId, out selectedShippingType))
                {
                    // A proper merchant app implementation might want to just ignore the bad result and
                    // send a correction to the payment provider app.
                    throw new Exception("Unrecognized shipping method id.");
                }

                shoppingCart.ShippingType = selectedShippingType;
            }
        }

        private static PaymentDetails CreatePaymentDetails(ShoppingCart shoppingCart)
        {
            return new PaymentDetails()
            {
                Total = CreateTotalCostItem(shoppingCart),
                DisplayItems = CreateCostBreakdown(shoppingCart),
                ShippingOptions = CreateShippingOptions(shoppingCart),
            };
        }

        private static PaymentItem CreateTotalCostItem(ShoppingCart shoppingCart)
        {
            return new PaymentItem(
                "Total",
                CreateCurrencyAmount(shoppingCart.CostsSummary.Total));
        }

        private static IReadOnlyList<PaymentShippingOption> CreateShippingOptions(ShoppingCart shoppingCart)
        {
            List<PaymentShippingOption> paymentShippingOptions = new List<PaymentShippingOption>();

            ShippingType selectedShippingType = shoppingCart.ShippingType;
            IReadOnlyDictionary<ShippingType, ShoppingCartCostsSummary> cartShippingOptions = shoppingCart.CalculateShippingOptions();

            ShoppingCartCostsSummary costsOfSelectedShipping = cartShippingOptions[selectedShippingType];

            foreach (var kvp in cartShippingOptions)
            {
                ShippingType shippingType = kvp.Key;
                ShoppingCartCostsSummary costs = kvp.Value;

                PaymentShippingOption shippingOption = new PaymentShippingOption(
                    label: ShippingTypeStringUtilities.CreateShippingOptionTitle(shippingType, costs, costsOfSelectedShipping),
                    selected: (shippingType == selectedShippingType),
                    tag: shippingType.ToString(),
                    amount: CreateCurrencyAmount(costs.Shipping));

                paymentShippingOptions.Add(shippingOption);
            }

            return paymentShippingOptions;
        }

        private static PaymentCurrencyAmount CreateCurrencyAmount(decimal value)
        {
            return new PaymentCurrencyAmount(
                value: PriceStringUtilities.ToInvariantString(value),
                currency: PriceStringUtilities.Currency);
        }

        private static IReadOnlyList<PaymentItem> CreateCostBreakdown(ShoppingCart shoppingCart)
        {
            var paymentItems = new List<PaymentItem>();

            foreach (ShoppingCartEntry entry in shoppingCart.Entries)
            {
                string label = string.Empty;

                if (entry.Quantity == 1)
                {
                    label = entry.Product.Title;
                }
                else
                {
                    label = $"{entry.Product.Title} x{entry.Quantity}";
                }

                decimal cost = entry.Product.Cost * entry.Quantity;

                paymentItems.Add(new PaymentItem(label, CreateCurrencyAmount(cost)));
            }

            if (shoppingCart.CostsSummary.TotalTax != 0)
            {
                paymentItems.Add(new PaymentItem("Tax", CreateCurrencyAmount(shoppingCart.CostsSummary.TotalTax)));
            }

            return paymentItems;
        }
    }
}
