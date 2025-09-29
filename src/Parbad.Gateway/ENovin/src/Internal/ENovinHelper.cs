// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Parbad.Abstraction;
using Parbad.Gateway.ENovin.Internal.Models;
using Parbad.Gateway.ENovin.Internal.ResultTranslators;
using Parbad.Http;
using Parbad.Internal;
using Parbad.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Storage.Abstractions.Models;

namespace Parbad.Gateway.ENovin.Internal;

internal static class ENovinHelper
{
	private const string RefNumKey = nameof(RefNumKey);
	private const string CallbackSuccessCode = "0";
	private const int VerificationSuccessCode = 0;
	public const string AdditionalVerificationDataKey = "ENovinAdditionalVerificationData";
	public const string CellNumberPropertyKey = "ENovinCellNumber";

	public static ENovinTokenRequest CreateTokenRequestModel(Invoice invoice, ENovinGatewayAccount account)
	{
		var model = new ENovinTokenRequest
		{
			Amount = invoice.Amount,
			CorporationPin = account.CorporationPin,
			OrderId = invoice.TrackingNumber,
			CallBackUrl = invoice.CallbackUrl,
			AdditionalData = invoice.GetENovinAdditionalData(),
			Originator = invoice.GetENovinCellNumber()
		};

		return model;
	}

	public static IPaymentRequestResult CreatePaymentRequestResult(ENovinTokenResponse tokenResponse,
		ENovinGatewayAccount account,
		HttpContext httpContext,
		ENovinGatewayOptions gatewayOptions,
		MessagesOptions messagesOptions)
	{
		if (tokenResponse.Status == 0)
		{
			return PaymentRequestResult.SucceedWithRedirect(account.Name, httpContext, string.Format(gatewayOptions.PaymentPageUrl, tokenResponse.Token));
		}
		
		var message = string.IsNullOrWhiteSpace(tokenResponse.ErrorDesc)
			? ENovinResultTranslator.Translate(tokenResponse.ErrorCode, messagesOptions)
			: tokenResponse.ErrorDesc;

		return PaymentRequestResult.Failed(message, account.Name, tokenResponse.ErrorCode);
	}

	public static async Task<ENovinCallbackResponse> BindCallbackResponse(HttpRequest httpRequest,
		CancellationToken cancellationToken)
	{
		var status = await httpRequest.TryGetParamAsync("Status", cancellationToken).ConfigureAwaitFalse();
		var rrn = await httpRequest.TryGetParamAsync("RRN", cancellationToken).ConfigureAwaitFalse();
		var token = await httpRequest.TryGetParamAsync("Token", cancellationToken).ConfigureAwaitFalse();

		return new ENovinCallbackResponse
		{
			Status = status.Value,
			Rrn = rrn.Value,
			Token = token.Value,
		};
	}

	public static IPaymentFetchResult CreateFetchResult(ENovinCallbackResponse callbackResponse,
		MessagesOptions messagesOptions)
	{
		var isCallbackResponseValid = ValidateCallbackResponse(callbackResponse, out var message);

		var isReceivedSuccessFromGateway = callbackResponse.Status == CallbackSuccessCode;

		var isSucceed = isCallbackResponseValid && isReceivedSuccessFromGateway;

		if (!isReceivedSuccessFromGateway)
		{
			message = ENovinResultTranslator.Translate(callbackResponse.Status, messagesOptions);
		}
		else if (isSucceed)
		{
			message = messagesOptions.PaymentSucceed;
		}

		var result = new PaymentFetchResult
		{
			Status = isSucceed ? PaymentFetchResultStatus.ReadyForVerifying : PaymentFetchResultStatus.Failed,
			GatewayResponseCode = callbackResponse.Status,
			Message = message
		};

		result.AddENovinAdditionalData(MapToAdditionalData(callbackResponse));

		return result;
	}

	public static ENovinVerificationRequest CreateVerificationRequest(ENovinCallbackResponse callbackResponse,
		ENovinGatewayAccount gatewayAccount)
	{
		return new ENovinVerificationRequest
		{
			CorporationPin = gatewayAccount.CorporationPin,
			Token = callbackResponse.Token!
		};
	}

	public static PaymentVerifyResult CreateVerifyResult(ENovinGatewayAccount gatewayAccount,
		ENovinCallbackResponse callbackResponse,
		ENovinVerificationAndRefundResponse verificationResponse,
		InvoiceContext invoiceContext,
		MessagesOptions messagesOptions)
	{
		var message = verificationResponse.IsSuccess
			? messagesOptions.PaymentSucceed
			: ENovinResultTranslator.Translate(verificationResponse.Status, messagesOptions);

		var result = new PaymentVerifyResult
		{
			Status = verificationResponse.IsSuccess ? PaymentVerifyResultStatus.Succeed : PaymentVerifyResultStatus.Failed,
			TransactionCode = verificationResponse.RRN,
			GatewayResponseCode = verificationResponse.Status,
			Message = message
		};

		result.AddENovinAdditionalData(MapToAdditionalData(callbackResponse));

		return result;
	}

	public static ENovinReverseRequest CreateReverseRequest(InvoiceContext context,
		ENovinGatewayAccount account,
		ENovinCallbackResponse callbackResponse)
	{
		var verificationTransaction =
			context.Transactions.SingleOrDefault(transaction => transaction.Type == TransactionType.Verify);

		if (string.IsNullOrEmpty(verificationTransaction?.AdditionalData) ||
			!verificationTransaction.ToDictionary().ContainsKey(RefNumKey))
		{
			throw new
				InvalidOperationException(
					$"No Transaction of type Verification or additional data found for reversing the invoice {context.Payment.TrackingNumber}.");
		}

		return new ENovinReverseRequest
		{
			Token = callbackResponse.Token,
			CorporationPin = account.CorporationPin
		};
	}

	public static PaymentRefundResult CreateRefundResult(ENovinVerificationAndRefundResponse response)
	{
		return new PaymentRefundResult
		{
			Status = response.IsSuccess ? PaymentRefundResultStatus.Succeed : PaymentRefundResultStatus.Failed,
			GatewayResponseCode = response.Status
		};
	}

	private static bool ValidateCallbackResponse(ENovinCallbackResponse callbackResponse, out string failures)
	{
		const string nullOrEmptyString = "IsReceivedFromTheGatewayAsNullOrEmpty";

		var validationFailures = new StringBuilder();
		var isValid = true;

		if (string.IsNullOrWhiteSpace(callbackResponse.Status))
		{
			isValid = false;

			validationFailures.AppendLine($"{nameof(ENovinCallbackResponse.Status)}{nullOrEmptyString}");
		}

		if (string.IsNullOrWhiteSpace(callbackResponse.Token))
		{
			isValid = false;

			validationFailures.AppendLine($"{nameof(ENovinCallbackResponse.Token)}{nullOrEmptyString}");
		}

		if (string.IsNullOrWhiteSpace(callbackResponse.Rrn))
		{
			isValid = false;

			validationFailures.AppendLine($"{nameof(ENovinCallbackResponse.Rrn)}{nullOrEmptyString}");
		}

		failures = validationFailures.ToString();

		return isValid;
	}

	private static ENovinVerificationAdditionalData MapToAdditionalData(ENovinCallbackResponse callbackResponse)
	{
		return new ENovinVerificationAdditionalData
		{
			Rrn = callbackResponse.Rrn,
			Token = callbackResponse.Token,
			Status = callbackResponse.Status
		};
	}
}
