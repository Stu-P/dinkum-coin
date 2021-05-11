using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace DinkumCoin.Blockchain.Api.Middleware
{
    public class CorrelationMiddleware
    {
        private readonly string[] _correlationHeaders = { "X-Trace-Id", "CorrelationId" };
        private readonly RequestDelegate _next;

        public CorrelationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {

            foreach (var altHeader in _correlationHeaders)
            {
                if (context.Request.Headers.ContainsKey(altHeader))
                {
                    var correlationId = context.Request.Headers[altHeader].FirstOrDefault();

                    context
                        .Response
                        .Headers
                        .Add(altHeader, correlationId);

                    using (LogContext.PushProperty("Correlation-Id", correlationId))
                    {
                        return _next(context);
                    }
                }
            }
            return _next(context);
        }
    }
}
