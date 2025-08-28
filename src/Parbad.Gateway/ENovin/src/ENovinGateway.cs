using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Parbad.Gateway.ENovin.Internal;
using Parbad.Gateway.ENovin.Internal.Models;

namespace Parbad.Gateway.ENovin;

[Gateway(Name)]
public class ENovinGateway : GatewayBase<ENovinGatewayAccount>
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly HttpClient _httpClient;
	private readonly ENovinGatewayOptions _gatewayOptions;
	private readonly MessagesOptions _messageOptions;

	public const string Name = "ENovin";

	public ENovinGateway(
		IGatewayAccountProvider<ENovinGatewayAccount> accountProvider,
		IHttpContextAccessor httpContextAccessor,
		IHttpClientFactory httpClientFactory,
		IOptions<ENovinGatewayOptions> gatewayOptions,
		IOptions<MessagesOptions> messageOptions) : base(accountProvider)
	{
		_httpContextAccessor = httpContextAccessor;
		_httpClient = httpClientFactory.CreateClient(nameof(ENovinGateway));
		_gatewayOptions = gatewayOptions.Value;
		_messageOptions = messageOptions.Value;
	}

	/// <inheritdoc />
	public override async Task<IPaymentRequestResult> RequestAsync(Invoice invoice,
		CancellationToken cancellationToken = default)
	{
		if (invoice == null) throw new ArgumentNullException(nameof(invoice));

		var account = await GetAccountAsync(invoice);

		var tokenRequestModel = ENovinHelper.CreateTokenRequestModel(invoice, account);

		var jsonSettings = new JsonSerializerSettings
		{
			Converters = { new StringEnumConverter() }
		};

		var responseMessage = await _httpClient.PostJsonAsync<ENovinTokenResponse>(_gatewayOptions.ApiTokenUrl, tokenRequestModel, jsonSettings, cancellationToken);
		
        return ENovinHelper.CreatePaymentRequestResult(responseMessage, account, _httpContextAccessor.HttpContext, _gatewayOptions, _messageOptions);
	}

	/// <inheritdoc />
	public override async Task<IPaymentFetchResult> FetchAsync(InvoiceContext context,
		CancellationToken cancellationToken = default)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));

		var callbackResultResponse = await ENovinHelper.BindCallbackResponse(_httpContextAccessor.HttpContext.Request, cancellationToken);

		return ENovinHelper.CreateFetchResult(callbackResultResponse, _messageOptions);
	}

	/// <inheritdoc />
	public override async Task<IPaymentVerifyResult> VerifyAsync(InvoiceContext context, CancellationToken cancellationToken = default)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));

		var callbackResponse = await ENovinHelper.BindCallbackResponse(_httpContextAccessor.HttpContext.Request, cancellationToken);

		var account = await GetAccountAsync(context.Payment);

		var fetchResult = ENovinHelper.CreateFetchResult(callbackResponse, _messageOptions);

		if (!fetchResult.IsSucceed)
		{
			return PaymentVerifyResult.Failed(fetchResult.Message);
		}

		var verificationRequest = ENovinHelper.CreateVerificationRequest(callbackResponse, account);

		var verificationResponse = await _httpClient.PostJsonAsync<ENovinVerificationAndRefundResponse>(_gatewayOptions.ApiVerificationUrl,
			verificationRequest,
			cancellationToken: cancellationToken);

		return ENovinHelper.CreateVerifyResult(account,
			callbackResponse,
			verificationResponse,
			context,
			_messageOptions);
	}

	/// <inheritdoc />
	public override async Task<IPaymentRefundResult> RefundAsync(InvoiceContext context,
		Money amount,
		CancellationToken cancellationToken = default)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));

		var account = await GetAccountAsync(context.Payment).ConfigureAwaitFalse();

		var callbackResponse = await ENovinHelper.BindCallbackResponse(_httpContextAccessor.HttpContext.Request, cancellationToken);
		
		var request = ENovinHelper.CreateReverseRequest(context, account, callbackResponse);

		var response = await _httpClient.PostJsonAsync<ENovinVerificationAndRefundResponse>(
			_gatewayOptions.ApiReverseUrl,
			request,
			cancellationToken: cancellationToken);

		return ENovinHelper.CreateRefundResult(response);
	}
}