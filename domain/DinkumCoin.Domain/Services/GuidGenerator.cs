using System;
namespace DinkumCoin.Domain.Services
{
    public class GuidGenerator : IGuidGenerator
    {
        public Guid Generate() => Guid.NewGuid();
    }
}
