using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Diacritical;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using Readarr.Http.Extensions;

namespace Readarr.Http.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        private static readonly Regex CookieNameRegex = new Regex(@"[^a-z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder authenticationBuilder, string name, Action<ApiKeyAuthenticationOptions> options)
        {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(name, options);
        }

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder authenticationBuilder, string name)
        {
            return authenticationBuilder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(name, options => { });
        }

        public static AuthenticationBuilder AddNone(this AuthenticationBuilder authenticationBuilder, string name)
        {
            return authenticationBuilder.AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>(name, options => { });
        }

        public static AuthenticationBuilder AddExternal(this AuthenticationBuilder authenticationBuilder, string name)
        {
            return authenticationBuilder.AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>(name, options => { });
        }

        public static AuthenticationBuilder AddAppAuthentication(this IServiceCollection services)
        {
            services.AddOptions<CookieAuthenticationOptions>(AuthenticationType.Forms.ToString())
                .Configure<IConfigFileProvider>((options, configFileProvider) =>
                {
                    // Replace diacritics and replace non-word characters to ensure cookie name doesn't contain any valid URL characters not allowed in cookie names
                    var instanceName = configFileProvider.InstanceName;
                    instanceName = instanceName.RemoveDiacritics();
                    instanceName = CookieNameRegex.Replace(instanceName, string.Empty);

                    options.Cookie.Name = $"{instanceName}Auth";
                    options.AccessDeniedPath = "/login?loginFailed=true";
                    options.LoginPath = "/login";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.ReturnUrlParameter = "returnUrl";

                    // ASP.NET Core's default cookie challenge returns 401 (with a Location header) for
                    // every unauthenticated request on modern runtimes, where .NET 6 issued a 302 redirect
                    // to the login page for browser navigations. Browsers ignore Location on a 401 and render
                    // the body, so the login form never loads. Restore the redirect for UI requests while
                    // keeping API requests answered with a status code (401/403) instead of an HTML redirect.
                    options.Events.OnRedirectToLogin = context =>
                    {
                        if (context.Request.IsApiRequest())
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }

                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.IsApiRequest())
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }

                        return Task.CompletedTask;
                    };
                });

            return services.AddAuthentication()
                .AddNone(AuthenticationType.None.ToString())
                .AddExternal(AuthenticationType.External.ToString())
                .AddBasic(AuthenticationType.Basic.ToString())
                .AddCookie(AuthenticationType.Forms.ToString())
                .AddApiKey("API", options =>
                {
                    options.HeaderName = "X-Api-Key";
                    options.QueryName = "apikey";
                })
                .AddApiKey("SignalR", options =>
                {
                    options.HeaderName = "X-Api-Key";
                    options.QueryName = "access_token";
                });
        }
    }
}
