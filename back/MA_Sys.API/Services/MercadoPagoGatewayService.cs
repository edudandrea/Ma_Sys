using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MA_Sys.API.Dto.Pagamentos;

namespace MA_Sys.API.Services
{
    public class MercadoPagoGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public MercadoPagoGatewayService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<MercadoPagoPaymentResult> ProcessarPagamentoCartaoAsync(
            PagamentoCartaoPublicoDto dto,
            string descricao,
            string? accessTokenOverride = null,
            CancellationToken cancellationToken = default)
        {
            var accessToken = string.IsNullOrWhiteSpace(accessTokenOverride)
                ? _configuration["MercadoPago:AccessToken"]
                : accessTokenOverride;
            if (string.IsNullOrWhiteSpace(accessToken) || accessToken.Contains("__CONFIGURE_VIA_", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("MercadoPago:AccessToken nao configurado.");
            }

            var baseUrl = _configuration["MercadoPago:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "https://api.mercadopago.com";
            }

            if (string.IsNullOrWhiteSpace(dto.CardToken))
            {
                throw new InvalidOperationException("Token do cartao nao informado.");
            }

            var installments = dto.Parcelas <= 0 ? 1 : dto.Parcelas;
            var paymentRequest = new
            {
                transaction_amount = dto.Valor,
                token = dto.CardToken,
                description = descricao,
                installments,
                payment_method_id = string.IsNullOrWhiteSpace(dto.PaymentMethodId) ? "visa" : dto.PaymentMethodId,
                payer = new
                {
                    email = string.IsNullOrWhiteSpace(dto.PayerEmail) ? "test_user_123456@testuser.com" : dto.PayerEmail
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/v1/payments");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(paymentRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Falha ao processar pagamento no gateway: {ExtrairMensagemErro(payload)}");
            }

            var result = JsonSerializer.Deserialize<MercadoPagoPaymentResponse>(payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null || result.Id <= 0 || string.IsNullOrWhiteSpace(result.Status))
            {
                throw new InvalidOperationException("Resposta invalida do gateway de pagamento.");
            }

            return new MercadoPagoPaymentResult
            {
                ExternalId = result.Id.ToString(),
                Status = result.Status,
                StatusDetail = result.StatusDetail ?? string.Empty
            };
        }

        public async Task<MercadoPagoPixResult> CriarPagamentoPixAsync(
            decimal valor,
            string descricao,
            string payerEmail,
            string? accessTokenOverride = null,
            CancellationToken cancellationToken = default)
        {
            var accessToken = ObterAccessToken(accessTokenOverride);
            var baseUrl = ObterBaseUrl();

            var paymentRequest = new
            {
                transaction_amount = valor,
                description = descricao,
                payment_method_id = "pix",
                payer = new
                {
                    email = payerEmail
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/v1/payments");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(paymentRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Falha ao gerar PIX no gateway: {ExtrairMensagemErro(payload)}");
            }

            var result = JsonSerializer.Deserialize<MercadoPagoPixResponse>(payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null || result.Id <= 0)
            {
                throw new InvalidOperationException("Resposta invalida ao gerar PIX.");
            }

            return new MercadoPagoPixResult
            {
                ExternalId = result.Id.ToString(),
                Status = result.Status ?? "pending",
                StatusDetail = result.StatusDetail ?? string.Empty,
                Payload = result.PointOfInteraction?.TransactionData?.QrCode,
                QrCodeBase64 = result.PointOfInteraction?.TransactionData?.QrCodeBase64
            };
        }

        public async Task<MercadoPagoPaymentResult> ConsultarPagamentoAsync(
            string externalId,
            string? accessTokenOverride = null,
            CancellationToken cancellationToken = default)
        {
            var accessToken = ObterAccessToken(accessTokenOverride);
            var baseUrl = ObterBaseUrl();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl.TrimEnd('/')}/v1/payments/{externalId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Falha ao consultar pagamento no gateway: {ExtrairMensagemErro(payload)}");
            }

            var result = JsonSerializer.Deserialize<MercadoPagoPaymentResponse>(payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null || result.Id <= 0 || string.IsNullOrWhiteSpace(result.Status))
            {
                throw new InvalidOperationException("Resposta invalida ao consultar pagamento.");
            }

            return new MercadoPagoPaymentResult
            {
                ExternalId = result.Id.ToString(),
                Status = result.Status,
                StatusDetail = result.StatusDetail ?? string.Empty
            };
        }

        private string ObterAccessToken(string? accessTokenOverride)
        {
            var accessToken = string.IsNullOrWhiteSpace(accessTokenOverride)
                ? _configuration["MercadoPago:AccessToken"]
                : accessTokenOverride;
            if (string.IsNullOrWhiteSpace(accessToken) || accessToken.Contains("__CONFIGURE_VIA_", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("MercadoPago:AccessToken nao configurado.");
            }

            return accessToken;
        }

        private string ObterBaseUrl()
        {
            var baseUrl = _configuration["MercadoPago:BaseUrl"];
            return string.IsNullOrWhiteSpace(baseUrl) ? "https://api.mercadopago.com" : baseUrl;
        }

        private static string ExtrairMensagemErro(string payload)
        {
            try
            {
                var error = JsonSerializer.Deserialize<MercadoPagoErrorResponse>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (!string.IsNullOrWhiteSpace(error?.Message))
                {
                    return error.Message;
                }

                if (!string.IsNullOrWhiteSpace(error?.Cause?.FirstOrDefault()?.Description))
                {
                    return error.Cause.First().Description!;
                }
            }
            catch
            {
            }

            return "Erro desconhecido.";
        }

        private class MercadoPagoPaymentResponse
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("status_detail")]
            public string? StatusDetail { get; set; }
        }

        private class MercadoPagoPixResponse : MercadoPagoPaymentResponse
        {
            [JsonPropertyName("point_of_interaction")]
            public MercadoPagoPointOfInteraction? PointOfInteraction { get; set; }
        }

        private class MercadoPagoPointOfInteraction
        {
            [JsonPropertyName("transaction_data")]
            public MercadoPagoTransactionData? TransactionData { get; set; }
        }

        private class MercadoPagoTransactionData
        {
            [JsonPropertyName("qr_code")]
            public string? QrCode { get; set; }

            [JsonPropertyName("qr_code_base64")]
            public string? QrCodeBase64 { get; set; }
        }

        private class MercadoPagoErrorResponse
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("cause")]
            public List<MercadoPagoErrorCause>? Cause { get; set; }
        }

        private class MercadoPagoErrorCause
        {
            [JsonPropertyName("description")]
            public string? Description { get; set; }
        }
    }

    public class MercadoPagoPaymentResult
    {
        public string ExternalId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusDetail { get; set; } = string.Empty;
    }

    public class MercadoPagoPixResult : MercadoPagoPaymentResult
    {
        public string? Payload { get; set; }
        public string? QrCodeBase64 { get; set; }
    }
}
