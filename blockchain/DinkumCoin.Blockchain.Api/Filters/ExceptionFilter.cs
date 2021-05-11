using System.Net;
using DinkumCoin.Blockchain.Api.Dto;
using DinkumCoin.Blockchain.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.Blockchain.Api.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case NotFoundException _:
                    context.Result = new NotFoundObjectResult(new ErrorResponse { ErrorMessage = context.Exception.Message });
                    break;
                case AlreadyExistsException _:
                    context.Result = new ConflictObjectResult(new ErrorResponse { ErrorMessage = context.Exception.Message });
                    break;
                case RateLimitException _:
                    context.Result = new ObjectResult(new ErrorResponse { ErrorMessage = context.Exception.Message })
                    {
                        StatusCode = (int)HttpStatusCode.TooManyRequests
                    };
                    break;
                default:
                    _logger.LogError(context.Exception, "Unexpected exception");
                    context.Result = new ObjectResult(new ErrorResponse { ErrorMessage = "An unexpected error was encountered" })
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                    break;
            }
        }
    }
}