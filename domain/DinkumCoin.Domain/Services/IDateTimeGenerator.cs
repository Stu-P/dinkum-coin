using System;
namespace DinkumCoin.Domain.Services
{
    public interface IDateTimeGenerator
    {
        public DateTime GenerateUTC();
    }
}
