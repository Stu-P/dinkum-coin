using System;
using Microsoft.AspNetCore.Http;

namespace DinkumCoin.Blockchain.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static bool IsHealthCheckEndpoint(this HttpContext ctx)
        {
            var endpoint = ctx.GetEndpoint();
            if (endpoint != null)
            {
                return string.Equals(
                    endpoint.DisplayName,
                    "Health checks",
                    StringComparison.Ordinal);
            }
            return false;
        }
    }
}
