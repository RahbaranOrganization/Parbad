// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Parbad.Storage.Abstractions.Models;
using System;
using System.Collections.Generic;

namespace Parbad.Internal
{
    public static class AdditionalDataConverter
    {
        public static IDictionary<string, string> ToDictionary(this Transaction transaction)
        {
            return JsonConvert.DeserializeObject<IDictionary<string, string>>(transaction.AdditionalData);
        }

        internal static string ToJson(PaymentResult paymentResult)
        {
            if (paymentResult == null) throw new ArgumentNullException(nameof(paymentResult));

            var collection = paymentResult.DatabaseAdditionalData;

            foreach (var keyValueItem in paymentResult.AdditionalData)
            {
                if (!collection.ContainsKey(keyValueItem.Key))
                {
                    try
                    {
                        var serializeValue = JsonConvert.SerializeObject(keyValueItem.Value); 
                        collection.Add(keyValueItem.Key, serializeValue);
                    }
                    catch (Exception e)
                    {
                        collection.Add(keyValueItem.Key, keyValueItem.Value.ToString());
                    }
                }
            }
                
            return JsonConvert.SerializeObject(paymentResult.DatabaseAdditionalData);
        }
    }
}
