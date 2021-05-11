using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DinkumCoin.Mining.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.MiningService
{
    public class MiningWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<MiningWorker> _logger;


        public MiningWorker(
            IServiceProvider serviceProvider,
            ILogger<MiningWorker> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (!stoppingToken.IsCancellationRequested)
            {

                _logger.LogInformation($"Mining service starting at: {DateTimeOffset.Now}");
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var miningService = scope.ServiceProvider.GetRequiredService<IMiningService>();
                        await miningService.StartAsync(stoppingToken);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Mining service failure(s)");
                }
                _logger.LogInformation($"Task completed at: {DateTimeOffset.Now}, taking {timer.ElapsedMilliseconds} ms to complete ");

                timer.Reset();
            }
        }
    }
}
