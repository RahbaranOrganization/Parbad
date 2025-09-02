// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.Pasargad.Api.Models;

public class PasargadVerifyPaymentResponseModel
{
    public string ResultMsg { get; set; }

    public int ResultCode { get; set; }

    public PasargadVerifyPaymentResponseDataModel Data { get; set; }
}

public record PasargadVerifyPaymentResponseDataModel(
    string Invoice,
    string ReferenceNumber,
    string TrackId,
    string MaskedCardNumber,
    string HashedCardNumber,
    DateTime RequestDate,
    decimal Amount);


