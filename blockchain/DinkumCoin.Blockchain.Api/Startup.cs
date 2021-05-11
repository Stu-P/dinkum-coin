using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Kinesis;
using DinkumCoin.Blockchain.Api.Extensions;
using DinkumCoin.Blockchain.Api.Filters;
using DinkumCoin.Blockchain.Api.Middleware;
using DinkumCoin.Blockchain.Core.Repositories;
using DinkumCoin.Blockchain.Core.Services;
using DinkumCoin.Blockchain.Data.Clients;
using DinkumCoin.Blockchain.Data.Mappers;
using DinkumCoin.Blockchain.Data.Repositories;
using DinkumCoin.Domain.Services;
using DinkumCoin.Infrastructure.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace DinkumCoin.Blockchain.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnv)
        {
            Configuration = configuration;
            HostingEnv = hostingEnv;

        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnv { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers(opts => opts.Filters.Add<ExceptionFilter>())
                .AddJsonOptions(opts => opts.JsonSerializerOptions.IgnoreNullValues = true)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddHealthChecks();
            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blockchain Api", Version = "v1" }));

            services
                .AddSingleton<IDynamoDbClient, ExtendedDynamoDbClient>()
                .AddScoped<IBlockchainRepository, DynamoDbBlockchainRepository>()
                .AddScoped<IBlockchainService, BlockchainService>()
                .Configure<BlockchainServiceSettings>(Configuration)
                .AddScoped<IBlockMapper, BlockMapper>()
                .AddScoped<IHashService, HashService>()
                .AddScoped<ISolutionValidator, SolutionValidator>()
                .Configure<SolutionValidatorSettings>(Configuration)
                .AddScoped<IGuidGenerator, GuidGenerator>()
                .AddScoped<IDateTimeGenerator, DateTimeGenerator>();


            services
                .Configure<DynamoDbClientSettings>(Configuration.GetSection("tableSettings"))
                .AddAWSService<IAmazonDynamoDB>(Configuration.GetAWSOptions("aws-dynamodb"))
                .AddTransient<IDynamoDBContext, DynamoDBContext>(
                    provider =>
                        new DynamoDBContext(
                            provider.GetRequiredService<IAmazonDynamoDB>(),
                            new DynamoDBContextConfig
                            {
                                TableNamePrefix = $"{HostingEnv.EnvironmentName}-",
                            })
                );

            services
                .AddScoped<IEventPublisher, KinesisStreamPublisher>()
                .AddAWSService<IAmazonKinesis>(Configuration.GetAWSOptions("aws-kinesis"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<CorrelationMiddleware>();
            app.UseSerilogRequestLogging(opts =>
            {
                opts.EnrichDiagnosticContext = LogHelpers.EnrichFromRequest;
                opts.GetLevel = LogHelpers.ExcludeHealthChecks; // Use the custom level
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blockchain Api");
            });

            app.InitialiseBlockChain();
        }


    }
}
