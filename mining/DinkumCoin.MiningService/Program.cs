using System;
using System.Net;
using Amazon.Kinesis;
using DinkumCoin.Domain.Events;
using DinkumCoin.Infrastructure.Messaging;
using DinkumCoin.Mining.Core.Clients;
using DinkumCoin.Mining.Core.EventHandlers;
using DinkumCoin.Mining.Core.Services;
using DinkumCoin.Mining.Data.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace DinkumCoin.MiningService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Hostname", Dns.GetHostName())
            .WriteTo.Console(new CompactJsonFormatter())
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ConsumerWorker>();
                    services.AddHostedService<MiningWorker>();

                    services.AddScoped<IMiningService, Mining.Core.Services.MiningService>()
                    .Configure<MiningServiceSettings>(hostContext.Configuration);

                    services.AddRepository(hostContext.Configuration);

                    services
                        .AddScoped<IEventConsumer, KinesisConsumer>()
                        .AddSingleton<KinesisConsumerManager>()
                        .Configure<KinesisConsumerManagerSettings>(hostContext.Configuration.GetSection("streamSettings"))
                        .AddAWSService<IAmazonKinesis>(hostContext.Configuration.GetAWSOptions("aws-kinesis"));

                    services.AddHttpClient<IBlockchainApiClient, BlockchainApiClient>(config =>
                    {
                        var settings = hostContext.Configuration.GetSection("BlockchainApiClient").Get<HttpClientSettings>();
                        config.BaseAddress = new Uri(settings.BaseAddress);
                        config.Timeout = TimeSpan.FromSeconds(settings.Timeout);
                    });

                    services
                     .AddScoped<IProofOfWorkCalculator, ProofOfWorkCalculator>()
                     .AddScoped<BlockCreatedEventHandler>()
                     .AddScoped<TransactionCreatedEventHandler>()
                     .AddSingleton<ICancelTokenProvider, MiningCancelTokenProvider>()
                     .AddSingleton<MessageDelegator>()
                     .AddSingleton(provider =>
                     {
                         var manager = new EventSubscriptionManager();
                         manager.AddSubscription<BlockCreatedEvent, BlockCreatedEventHandler>(Constants.BlockCreatedEventName);
                         manager.AddSubscription<TransactionCreatedEvent, TransactionCreatedEventHandler>(Constants.TransactionCreatedEventName);
                         return manager;
                     });

                });
    }
}
