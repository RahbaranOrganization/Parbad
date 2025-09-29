// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.ENovin.Internal.Models;

internal class ENovinVerificationAndRefundResponse
{
    public string Status { get; set; }
    public string CardNumberMasked { get; set; }
    public string RRN { get; set; }
    public string Token { get; set; }

    public bool IsSuccess => Status.Equals("0", StringComparison.OrdinalIgnoreCase);
}