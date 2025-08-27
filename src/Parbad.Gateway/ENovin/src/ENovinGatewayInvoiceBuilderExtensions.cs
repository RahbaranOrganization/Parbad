// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Abstraction;
using Parbad.Gateway.ENovin.Internal;
using Parbad.InvoiceBuilder;
using System;

namespace Parbad.Gateway.ENovin;

public static class ENovinGatewayInvoiceBuilderExtensions
{
    /// <summary>
    /// The invoice will be sent to ENovin gateway.
    /// </summary>
    /// <param name="builder"></param>
    public static IInvoiceBuilder UseENovin(this IInvoiceBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder.SetGateway(ENovinGateway.Name);
    }

    /// <summary>
    /// Sets additional data which will be sent to ENovin Gateway.
    /// </summary>
    public static IInvoiceBuilder SetENovinData(this IInvoiceBuilder builder, string cellNumber)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        if (!string.IsNullOrWhiteSpace(cellNumber))
        {
            builder.AddOrUpdateProperty(ENovinHelper.CellNumberPropertyKey, cellNumber);
        }

        return builder;
    }

    internal static string GetENovinCellNumber(this Invoice invoice)
    {
        if (invoice.Properties.TryGetValue(ENovinHelper.CellNumberPropertyKey, out var cellNumber))
        {
            return cellNumber.ToString();
        }

        return null;
    }
    
    internal static string GetENovinAdditionalData(this Invoice invoice)
    {
        if (invoice.Properties.TryGetValue(ENovinHelper.AdditionalVerificationDataKey, out var data))
        {
            return data.ToString();
        }

        return null;
    }
}
