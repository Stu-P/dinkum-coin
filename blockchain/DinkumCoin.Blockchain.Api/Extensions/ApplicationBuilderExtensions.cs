using System;
using DinkumCoin.Blockchain.Core.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace DinkumCoin.Blockchain.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {

        public static void InitialiseBlockChain(this IApplicationBuilder app)
        {

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var logger = serviceScope.ServiceProvider.GetService<ILogger<Startup>>();
                var repository = serviceScope.ServiceProvider.GetService<IBlockchainRepository>();
                try
                {
                    logger.LogInformation("Run Blockchain DB initialisation");

                    Policy
                      .Handle<Exception>()
                      .WaitAndRetry(new[]
                      {
                        TimeSpan.FromSeconds(15),
                        TimeSpan.FromSeconds(15),
                        TimeSpan.FromSeconds(30)
                      }, (exception, timeSpan) =>
                      {
                          logger.LogWarning("Blockchain DB initialisation, retrying");
                      }).Execute(() => repository.InitialiseChain());

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Blockchain DB initialisation failed");
                }
            }
        }
    }
}
