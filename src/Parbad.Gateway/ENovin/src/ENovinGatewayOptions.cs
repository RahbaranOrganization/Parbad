// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.ENovin;

public class ENovinGatewayOptions
{
    public string ApiTokenUrl { get; set; } = "https://pna.shaparak.ir/mhipg/api/Payment/NormalSale";

    public string PaymentPageUrl { get; set; } = "https://pna.shaparak.ir/mhui/home/index/{0}";

    public string ApiVerificationUrl { get; set; } = "https://pna.shaparak.ir/mhipg/api/Payment/confirm";

    public string ApiReverseUrl { get; set; } = "https://pna.shaparak.ir/mhipg/api/Payment/Reverse";
}
