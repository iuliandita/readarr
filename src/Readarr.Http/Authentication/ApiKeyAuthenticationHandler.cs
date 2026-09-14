using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NzbDrone.Core.Configuration;

namespace Readarr.Http.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Key";

        public string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;

        public string HeaderName { get; set; }
        public string QueryName { get; set; }
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly byte[] _apiKeyBytes;

        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfigFileProvider config)
            : base(options, logger, encoder)
        {
            _apiKeyBytes = Encoding.UTF8.GetBytes(config.ApiKey ?? string.Empty);
        }

        private string ParseApiKey()
        {
            // Prefer header-based secrets over the query string, which leaks into
            // logs, browser history, and referrers.
            if (Request.Headers.TryGetValue(Options.HeaderName, out var headerValue))
            {
                var apiKey = headerValue.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    return apiKey;
                }
            }

            if (Request.Headers.TryGetValue("Authorization", out var authorizationValue))
            {
                var authorization = authorizationValue.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(authorization))
                {
                    const string bearerPrefix = "Bearer ";
                    if (authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authorization.Substring(bearerPrefix.Length).Trim();
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            return token;
                        }
                    }
                }
            }

            // Backwards compatible: query-string token support.
            if (Request.Query.TryGetValue(Options.QueryName, out var queryValue))
            {
                return queryValue.FirstOrDefault();
            }

            return null;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var providedApiKey = ParseApiKey();

            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var providedBytes = Encoding.UTF8.GetBytes(providedApiKey);
            if (providedBytes.Length == _apiKeyBytes.Length &&
                CryptographicOperations.FixedTimeEquals(providedBytes, _apiKeyBytes))
            {
                var claims = new List<Claim>
                {
                    new Claim("ApiKey", "true")
                };

                var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
                var identities = new List<ClaimsIdentity> { identity };
                var principal = new ClaimsPrincipal(identities);
                var ticket = new AuthenticationTicket(principal, Options.Scheme);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            return Task.CompletedTask;
        }
    }
}
