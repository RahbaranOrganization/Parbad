// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.ENovin.Internal.Models;

internal class ENovinCallbackResponse
{
    public string? Status { get; set; }
    
    public string? Rrn { get; set; }

    public string? Token { get; set; }
}
