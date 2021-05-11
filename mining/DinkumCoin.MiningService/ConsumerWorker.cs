using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DinkumCoin.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.MiningService
{
    public class ConsumerWorker : BackgroundService
    {
        private readonly ILogger<ConsumerWorker> _logger;
        private readonly KinesisConsumerManager _consumerManager;


        public ConsumerWorker(ILogger<ConsumerWorker> logger, KinesisConsumerManager consumerManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consumerManager = consumerManager ?? throw new ArgumentNullException(nameof(consumerManager));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (!stoppingToken.IsCancellationRequested)
            {

                _logger.LogInformation($"Stream Consumers starting at: {DateTimeOffset.Now}");
                try
                {
                    await _consumerManager.StartConsumers(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Stream processing failure(s)");
                }
                _logger.LogInformation($"Task completed at: {DateTimeOffset.Now}, taking {timer.ElapsedMilliseconds} ms to complete ");

                timer.Reset();
            }
        }
    }
}
