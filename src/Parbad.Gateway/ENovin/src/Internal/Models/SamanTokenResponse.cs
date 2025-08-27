// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.ENovin.Internal.Models;

internal class ENovinTokenResponse
{
    public int Status { get; set; }

    public string Token { get; set; }

    public string ErrorCode { get; set; }

    public string ErrorDesc { get; set; }
}
