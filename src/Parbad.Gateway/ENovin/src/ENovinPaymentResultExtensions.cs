using System;
using Parbad.Gateway.ENovin.Internal;

namespace Parbad.Gateway.ENovin;

public static class ENovinPaymentResultExtensions
{
    /// <summary>
    /// Gets the additional data which is received from ENovin Gateway. 
    /// </summary>
    public static ENovinVerificationAdditionalData GetENovinAdditionalData(this IPaymentFetchResult paymentFetchResult) => GetAdditionalData(paymentFetchResult);

    /// <summary>
    /// Gets the additional data which is received from ENovin Gateway. 
    /// </summary>
    public static ENovinVerificationAdditionalData GetENovinAdditionalData(this IPaymentVerifyResult paymentVerifyResult) => GetAdditionalData(paymentVerifyResult);

    internal static void AddENovinAdditionalData(this IPaymentFetchResult paymentFetchResult, ENovinVerificationAdditionalData additionalData)
    {
        if (paymentFetchResult == null) throw new ArgumentNullException(nameof(paymentFetchResult));

        paymentFetchResult.AdditionalData.Add(ENovinHelper.AdditionalVerificationDataKey, additionalData);
    }

    internal static void AddENovinAdditionalData(this IPaymentVerifyResult paymentVerifyResult, ENovinVerificationAdditionalData additionalData)
    {
        if (paymentVerifyResult == null) throw new ArgumentNullException(nameof(paymentVerifyResult));

        paymentVerifyResult.AdditionalData.Add(ENovinHelper.AdditionalVerificationDataKey, additionalData);
    }

    private static ENovinVerificationAdditionalData GetAdditionalData(this IPaymentResult paymentResult)
    {
        if (paymentResult == null) throw new ArgumentNullException(nameof(paymentResult));

        if (!paymentResult.AdditionalData.ContainsKey(ENovinHelper.AdditionalVerificationDataKey))
        {
            return null;
        }

        return (ENovinVerificationAdditionalData)paymentResult.AdditionalData[ENovinHelper.AdditionalVerificationDataKey];
    }
}
