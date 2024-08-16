using Microsoft.Extensions.Options;
using Serilog;
using System.Net;
using System.Text;
using testbook.ConfigurationClasses;

namespace testbook.MiddleWare
{
    public class BasicAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BasicAuthenticationOptions _options;

        public BasicAuthenticationMiddleware(RequestDelegate next, IOptions<BasicAuthenticationOptions> options)
        {
            _next = next;
            _options = options.Value;
        }
        private bool hasLoggedAccess = false;
        public async Task Invoke(HttpContext context)
        {
            if (!hasLoggedAccess)
            {
                Log.Information("Basic Aunthentication is requested");
            }
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                string? authHeader = context.Request.Headers.Authorization;

                if (authHeader != null && authHeader.StartsWith("Basic"))
                {
                    string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                    Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                    string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

                    int separatorIndex = usernamePassword.IndexOf(':');
                    var username = usernamePassword.Substring(0, separatorIndex);
                    var password = usernamePassword.Substring(separatorIndex + 1);
                    if (username == _options.Username && password == _options.Password)
                    {
                        if (!hasLoggedAccess)
                        {
                            Log.Information("Basic Authentication Successful, Access the Swagger testbook");
                            hasLoggedAccess = true;
                        }
                        await _next.Invoke(context);
                        return;
                    }
                }
                context.Response.Headers.WWWAuthenticate = "Basic";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Authentication is required.");
            }
            else { await _next.Invoke(context); }
        }

    }
}
