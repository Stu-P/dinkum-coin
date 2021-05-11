using System.Collections.Generic;

namespace DinkumCoin.Blockchain.Api.Dto
{
    public class ErrorResponse
    {
        public ErrorResponse(string errorMsg)
        {
            ErrorMessage = errorMsg;
        }

        public ErrorResponse()
        { }

        public string ErrorMessage { get; set; }
        public IEnumerable<ValidationErrorItem> ValidationErrors { get; set; }
    }
}
