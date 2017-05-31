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

//
// This file helps an app use the Basic Card Payment protocol as defined by
// http://www.w3.org/TR/payment-method-basic-card/. As of writing, the latest
// verion was published on 2017-05-04.
//

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.Payments;

namespace YetAnotherShoppingApp
{
    enum BasicCardType
    {
        Credit,
        Debit,
        Prepaid,
    }

    class BasicCardNetwork
    {
        public const string AmericanExpress = "amex";
        public const string Mastercard = "mastercard";
        public const string Visa = "visa";
    }

    class BasicCardResponse
    {
        public string CardholderName { get; set; } = null;
        public string CardNumber { get; set; } = null;
        public string ExpiryMonth { get; set; } = null;
        public string ExpiryYear { get; set; } = null;
        public string CardSecurityCode { get; set; } = null;
        public PaymentAddress BillingAddress { get; set; } = null;
    }

    class BasicCardParseResponseException :
        Exception
    {
        public BasicCardParseResponseException() { }
        public BasicCardParseResponseException(string message) : base(message) { }
        public BasicCardParseResponseException(string message, Exception innerException) : base(message, innerException) { }
    }

    static class BasicCardPaymentProtocol
    {
        /// <summary>
        /// The Payment Method ID of the Basic Card Payment protocol.
        /// </summary>
        public const string PaymentMethodId = "basic-card";

        public static readonly IReadOnlyDictionary<BasicCardType, string> BasicCardTypeStrings = new Dictionary<BasicCardType, string>()
        {
            { BasicCardType.Credit, "credit" },
            { BasicCardType.Debit, "debit" },
            { BasicCardType.Prepaid, "prepaid" },
        };

        /// <summary>
        /// Generates a 'Windows.ApplicationModel.Payments.PaymentMethodData' instance for the Basic Card Payment protocol that supports
        /// any network and any card type.
        /// </summary>
        public static PaymentMethodData GeneratePaymentMethodData()
        {
            return GeneratePaymentMethodData(null, null);
        }

        /// <summary>
        /// Generates a 'Windows.ApplicationModel.Payments.PaymentMethodData' instance for the Basic Card Payment protocol that only supports
        /// the specified networks and card types.
        /// </summary>
        /// <param name="supportedNetworks">
        /// If 'null' or empty, then all networks are supported.
        /// </param>
        /// <param name="supportedTypes">
        /// If 'null' or empty, then all card types are supported.
        /// </param>
        public static PaymentMethodData GeneratePaymentMethodData(IReadOnlyCollection<string> supportedNetworks, IReadOnlyCollection<BasicCardType> supportedTypes)
        {
            string jsonData = GenerateJsonData(supportedNetworks, supportedTypes);
            string[] supportedMethodIds = new string[] { PaymentMethodId };

            return new PaymentMethodData(supportedMethodIds, jsonData);
        }

        private static string GenerateJsonData(IReadOnlyCollection<string> supportedNetworks, IReadOnlyCollection<BasicCardType> supportedTypes)
        {
            StringWriter stringWriter = new StringWriter();

            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();

                if (supportedNetworks != null && supportedNetworks.Count > 0)
                {
                    jsonWriter.WritePropertyName("supportedNetworks");

                    jsonWriter.WriteStartArray();

                    foreach (string network in supportedNetworks)
                    {
                        jsonWriter.WriteValue(network);
                    }

                    jsonWriter.WriteEndArray();
                }

                if (supportedTypes != null && supportedTypes.Count > 0)
                {
                    jsonWriter.WritePropertyName("supportedTypes");

                    jsonWriter.WriteStartArray();

                    foreach (BasicCardType cardType in supportedTypes)
                    {
                        jsonWriter.WriteValue(BasicCardTypeStrings[cardType]);
                    }

                    jsonWriter.WriteEndArray();
                }

                jsonWriter.WriteEndObject();

                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Parses the JSON response for the Basic Card Payment protocol.
        /// </summary>
        public static BasicCardResponse ParseResponse(string jsonDetails)
        {
            StringReader stringReader = new StringReader(jsonDetails);

            using (JsonReader jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read())
                {
                    switch (jsonReader.TokenType)
                    {
                    case JsonToken.StartObject:
                        return ParseBasicCardResponse(jsonReader);

                    case JsonToken.Comment:
                        // Do nothing
                        break;

                    default:
                        throw new BasicCardParseResponseException("Incorrect JSON format. Expected root object.");
                    }
                }

                throw new BasicCardParseResponseException("Unexpected end of file.");
            }
        }

        private static BasicCardResponse ParseBasicCardResponse(JsonReader jsonReader)
        {
            BasicCardResponse result = new BasicCardResponse();

            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                case JsonToken.PropertyName:
                    string propertyName = (string)jsonReader.Value;
                    switch(propertyName)
                    {
                    case "cardholderName":
                        result.CardholderName = jsonReader.ReadAsString();
                        break;

                    case "cardNumber":
                        result.CardNumber = jsonReader.ReadAsString();
                        break;

                    case "expiryMonth":
                        result.ExpiryMonth = jsonReader.ReadAsString();
                        break;

                    case "expiryYear":
                        result.ExpiryYear = jsonReader.ReadAsString();
                        break;

                    case "cardSecurityCode":
                        result.CardSecurityCode = jsonReader.ReadAsString();
                        break;

                    case "billingAddress":
                        jsonReader.Read();

                        if (jsonReader.TokenType != JsonToken.StartObject)
                        {
                            throw new BasicCardParseResponseException("Property 'billingAddress' must have value that is an object type.");
                        }

                        result.BillingAddress = ParsePaymentAddress(jsonReader);
                        break;

                    default:
                        // Ignore extra data.
                        jsonReader.Read(); // goto value
                        jsonReader.Skip(); // skip value
                        break;
                    }
                    break;

                case JsonToken.EndObject:
                    return result;

                case JsonToken.Comment:
                    // Do nothing
                    break;

                default:
                    throw new BasicCardParseResponseException("Incorrect JSON format.");
                }
            }

            throw new BasicCardParseResponseException("Unexpected end of file.");
        }

        private static PaymentAddress ParsePaymentAddress(JsonReader jsonReader)
        {
            PaymentAddress result = new PaymentAddress();

            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                case JsonToken.PropertyName:
                    string propertyName = (string)jsonReader.Value;
                    switch (propertyName)
                    {
                    case "country":
                        result.Country = jsonReader.ReadAsString();
                        break;

                    case "addressLine":
                        jsonReader.Read();

                        if (jsonReader.TokenType != JsonToken.StartArray)
                        {
                            throw new BasicCardParseResponseException("Property 'addressLine' must have value that is a string array type.");
                        }

                        result.AddressLines = ParseStringArray(jsonReader);
                        break;

                    case "region":
                        result.Region = jsonReader.ReadAsString();
                        break;

                    case "city":
                        result.City = jsonReader.ReadAsString();
                        break;

                    case "dependentLocality":
                        result.DependentLocality = jsonReader.ReadAsString();
                        break;

                    case "postalCode":
                        result.PostalCode = jsonReader.ReadAsString();
                        break;

                    case "sortingCode":
                        result.SortingCode = jsonReader.ReadAsString();
                        break;

                    case "languageCode":
                        result.LanguageCode = jsonReader.ReadAsString();
                        break;

                    case "organization":
                        result.Organization = jsonReader.ReadAsString();
                        break;

                    case "recipient":
                        result.Recipient = jsonReader.ReadAsString();
                        break;

                    case "phone":
                        result.PhoneNumber = jsonReader.ReadAsString();
                        break;

                    default:
                        // Ignore extra data.
                        jsonReader.Read(); // goto value
                        jsonReader.Skip(); // skip value
                        break;
                    }
                    break;

                case JsonToken.EndObject:
                    return result;

                case JsonToken.Comment:
                    // Do nothing
                    break;

                default:
                    throw new BasicCardParseResponseException("Incorrect JSON format.");
                }
            }

            throw new BasicCardParseResponseException("Unexpected end of file.");
        }

        private static IReadOnlyList<string> ParseStringArray(JsonReader jsonReader)
        {
            List<string> result = new List<string>();

            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                case JsonToken.String:
                    result.Add((string)jsonReader.Value);
                    break;

                case JsonToken.EndArray:
                    return result.AsReadOnly();

                case JsonToken.Comment:
                    // Do nothing
                    break;

                default:
                    throw new BasicCardParseResponseException("Incorrect JSON format. Expecting type of string.");
                }
            }

            throw new BasicCardParseResponseException("Unexpected end of file.");
        }

    }
}
