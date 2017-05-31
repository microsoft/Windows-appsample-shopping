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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.ApplicationModel.Payments;

namespace YetAnotherShoppingApp
{
    class MicrosoftPayProtocolResponse
    {
        public string Token { get; set; } = null;

        public string PayerId { get; set; } = null;

        public string PaymentRequestId { get; set; } = null;

        public string MerchantId { get; set; } = null;

        public DateTime Expiry { get; set; } = DateTime.MinValue;

        public DateTime TimeStamp { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Known valid values include: "Invalid", "Error", "Standard" and "Stripe".
        /// </summary>
        public string Format { get; set; } = null;

        /// <summary>
        /// Only valid when 'Format' is equal to "Error".
        /// </summary>
        public MicrosoftPayErrorInfo ErrorInfo { get; set; } = null;

        /// <summary>
        /// Only valid when 'Format' is equal to "Standard".
        /// </summary>
        public MicrosoftPayStandardInfo StandardInfo { get; set; } = null;
    }

    class MicrosoftPayErrorInfo
    {
        public string ErrorCode { get; set; } = null;
        public string ErrorText { get; set; } = null;
        public string ErrorSource { get; set; } = null;
    }

    class MicrosoftPayStandardInfo
    {
        
        public string EPK { get; set; } = null;
        public string KeyId { get; set; } = null;
        public string Nonce { get; set; } = null;
        public string Tag { get; set; } = null;
    }

    class MicrosoftPayParseResponseException :
        Exception
    {
        public MicrosoftPayParseResponseException() { }
        public MicrosoftPayParseResponseException(string message) : base(message) { }
        public MicrosoftPayParseResponseException(string message, Exception innerException) : base(message, innerException) { }
    }

    static class MicrosoftPayProtocol
    {
        /// <summary>
        /// The Payment Method ID of the Microsoft Pay protocol.
        /// </summary>
        public const string PaymentMethodId = "https://pay.microsoft.com/microsoftpay";

        public static PaymentMethodData GeneratePaymentMethodData(string merchantId, bool testMode = false)
        {
            return GeneratePaymentMethodData(merchantId, null, null, testMode);
        }

        public static PaymentMethodData GeneratePaymentMethodData(string merchantId, IReadOnlyCollection<string> supportedNetworks, IReadOnlyCollection<BasicCardType> supportedTypes, bool testMode = false)
        {
            string jsonData = GenerateJsonData(merchantId, supportedNetworks, supportedTypes, testMode);
            string[] supportedMethodIds = new string[] { PaymentMethodId };

            return new PaymentMethodData(supportedMethodIds, jsonData);
        }

        public static MicrosoftPayProtocolResponse ParseResponse(string jsonDetails)
        {
            string token = GetTokenFromResponse(jsonDetails);

            string[] tokenParts = token.Split('.');
            if (tokenParts.Length < 3)
            {
                throw new MicrosoftPayParseResponseException("Incorrect token format.");
            }

            string tokenHeadersJson = DecodeBase64UrlString(tokenParts[0]);
            string tokenBody = DecodeBase64UrlString(tokenParts[1]);
            byte[] tokenSignature = DecodeBase64Url(tokenParts[2]);

            MicrosoftPayProtocolResponse result = ParseHeaders(tokenHeadersJson);
            result.Token = tokenBody;

            return result;
        }

        private static string GenerateJsonData(string merchantId, IReadOnlyCollection<string> supportedNetworks, IReadOnlyCollection<BasicCardType> supportedTypes, bool testMode)
        {
            StringWriter stringWriter = new StringWriter();

            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();

                jsonWriter.WritePropertyName("merchantId");
                jsonWriter.WriteValue(merchantId);

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
                        jsonWriter.WriteValue(BasicCardPaymentProtocol.BasicCardTypeStrings[cardType]);
                    }

                    jsonWriter.WriteEndArray();
                }

                if (testMode)
                {
                    jsonWriter.WritePropertyName("mode");
                    jsonWriter.WriteValue("TEST");
                }

                jsonWriter.WriteEndObject();

                return stringWriter.ToString();
            }
        }

        private static string GetTokenFromResponse(string jsonDetails)
        {
            StringReader stringReader = new StringReader(jsonDetails);

            using (JsonReader jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read())
                {
                    switch (jsonReader.TokenType)
                    {
                    case JsonToken.StartObject:
                        return ParseMicrosoftPayResponse(jsonReader);

                    case JsonToken.Comment:
                        // Do nothing
                        break;

                    default:
                        throw new MicrosoftPayParseResponseException("Incorrect JSON format. Expected root object.");
                    }
                }

                throw new MicrosoftPayParseResponseException("Unexpected end of file.");
            }
        }

        private static string ParseMicrosoftPayResponse(JsonReader jsonReader)
        {
            string result = null;

            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                case JsonToken.PropertyName:
                    string propertyName = (string)jsonReader.Value;
                    switch (propertyName)
                    {
                    case "paymentToken":
                        result = jsonReader.ReadAsString();
                        break;

                    default:
                        // Ignore extra data.
                        break;
                    }
                    break;

                case JsonToken.EndObject:
                    return result;

                case JsonToken.Comment:
                    // Do nothing
                    break;

                default:
                    throw new MicrosoftPayParseResponseException("Incorrect JSON format.");
                }
            }

            throw new MicrosoftPayParseResponseException("Unexpected end of file.");
        }

        private static MicrosoftPayProtocolResponse ParseHeaders(string headers)
        {
            StringReader stringReader = new StringReader(headers);

            using (JsonReader jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read())
                {
                    switch (jsonReader.TokenType)
                    {
                    case JsonToken.StartObject:
                        return ParsePaymentTokenHeaders(jsonReader);

                    case JsonToken.Comment:
                        // Do nothing
                        break;

                    default:
                        throw new MicrosoftPayParseResponseException("Incorrect JSON format. Expected root object.");
                    }
                }

                throw new MicrosoftPayParseResponseException("Unexpected end of file.");
            }
        }

        private static MicrosoftPayProtocolResponse ParsePaymentTokenHeaders(JsonReader jsonReader)
        {
            MicrosoftPayProtocolResponse result = new MicrosoftPayProtocolResponse();
            MicrosoftPayErrorInfo errorInfo = new MicrosoftPayErrorInfo();
            MicrosoftPayStandardInfo standardInfo = new MicrosoftPayStandardInfo();

            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                case JsonToken.PropertyName:
                    string propertyName = (string)jsonReader.Value;
                    switch (propertyName)
                    {
                    case "PayerId":
                        result.PayerId = jsonReader.ReadAsString();
                        break;

                    case "PaymentRequestId":
                        result.PaymentRequestId = jsonReader.ReadAsString();
                        break;

                    case "MerchantId":
                        result.MerchantId = jsonReader.ReadAsString();
                        break;

                    case "Expiry":
                        result.Expiry = (DateTime)jsonReader.ReadAsDateTime();
                        break;

                    case "TimeStamp":
                        result.TimeStamp = (DateTime)jsonReader.ReadAsDateTime();
                        break;

                    case "Format":
                        result.Format = jsonReader.ReadAsString();
                        break;

                    case "ErrorCode":
                        errorInfo.ErrorCode = jsonReader.ReadAsString();
                        break;

                    case "ErrorText":
                        errorInfo.ErrorText = jsonReader.ReadAsString();
                        break;

                    case "ErrorSource":
                        errorInfo.ErrorSource = jsonReader.ReadAsString();
                        break;

                    case "EPK":
                        standardInfo.EPK = jsonReader.ReadAsString();
                        break;

                    case "KeyId":
                        standardInfo.KeyId = jsonReader.ReadAsString();
                        break;

                    case "Nonce":
                        standardInfo.Nonce = jsonReader.ReadAsString();
                        break;

                    case "Tag":
                        standardInfo.Tag = jsonReader.ReadAsString();
                        break;

                    default:
                        // Ignore extra data.
                        jsonReader.Read(); // goto value
                        jsonReader.Skip(); // skip value
                        break;
                    }
                    break;

                case JsonToken.EndObject:
                    switch (result.Format)
                    {
                    case "Error":
                        result.ErrorInfo = errorInfo;
                        break;

                    case "Standard":
                        result.StandardInfo = standardInfo;
                        break;
                    }

                    return result;

                case JsonToken.Comment:
                    // Do nothing
                    break;

                default:
                    throw new MicrosoftPayParseResponseException("Incorrect JSON format.");
                }
            }

            throw new MicrosoftPayParseResponseException("Unexpected end of file.");
        }

        private static string DecodeBase64UrlString(string base64Url)
        {
            byte[] decodedBytes = DecodeBase64Url(base64Url);
            return Encoding.UTF8.GetString(decodedBytes);
        }

        private static byte[] DecodeBase64Url(string base64Url)
        {
            //
            // The Microsoft Pay protocol uses base64url (RFC7515) whereas 'Convert.FromBase64String' uses RFC4648.
            // So we need to convert the string.
            //

            // Convert chars for values 62 and 63.
            StringBuilder sb = new StringBuilder(base64Url);
            sb.Replace('-', '+');
            sb.Replace('_', '/');

            // Add padding chars.
            int paddingCharsRequired = (4 - base64Url.Length % 4) % 4;
            for (int i = 0; i != paddingCharsRequired; ++i)
            {
                sb.Append('=');
            }

            return Convert.FromBase64String(sb.ToString());
        }
    }
}
