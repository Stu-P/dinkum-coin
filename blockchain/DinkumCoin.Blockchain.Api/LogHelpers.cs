using System;
using DinkumCoin.Blockchain.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;

namespace DinkumCoin.Blockchain.Api
{
    public static class LogHelpers
    {
        public static void EnrichFromRequest(
            IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;
            diagnosticContext.Set("Host", request.Host);
            diagnosticContext.Set("Protocol", request.Protocol);
            diagnosticContext.Set("Scheme", request.Scheme);
            if (request.QueryString.HasValue)
            {
                diagnosticContext.Set("QueryString", request.QueryString.Value);
            }

            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

            var endpoint = httpContext.GetEndpoint();
            if (endpoint != null)
            {
                diagnosticContext.Set("EndpointName", endpoint.DisplayName);
            }
        }

        public static LogEventLevel ExcludeHealthChecks(HttpContext ctx, double _, Exception ex) =>
            ex != null
                ? LogEventLevel.Error
                : ctx.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : ctx.IsHealthCheckEndpoint()
                        ? LogEventLevel.Verbose
                        : LogEventLevel.Information;
    }
}
