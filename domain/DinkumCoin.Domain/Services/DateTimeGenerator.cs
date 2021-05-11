using System;
namespace DinkumCoin.Domain.Services
{
    public class DateTimeGenerator : IDateTimeGenerator
    {
        public DateTime GenerateUTC() => DateTime.UtcNow;
    }
}
