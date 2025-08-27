// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.ENovin.Internal.Models;

internal class ENovinTokenRequest
{
    public string CorporationPin { get; set; }
        
    public long Amount { get; set; }
        
    public long OrderId { get; set; }
        
    public string CallBackUrl { get; set; }
        
    public string AdditionalData { get; set; }
        
    public string Originator { get; set; }
}
