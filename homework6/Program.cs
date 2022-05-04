﻿using NLog.Web;
using AutoMapper;
using FluentMigrator.Runner;
using Quartz;
using Quartz.Spi;
using Quartz.Impl;
using Polly;
using MManager.Client;
using MetricsAgent.Mapper;
using MManager.Job.Quartz;
using MManager.Repo.IMetricsRepo;
using MManager.Repo.MetricsRepo;
using MManager.Job.Factory;
using MManager.Job;
using MManager.Job.Schedule;




var builder = WebApplication.CreateBuilder(args);
var _logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
var mapperConfiguration = new MapperConfiguration(mp => mp.AddProfile(new MapperProfile()));
var _mapper = mapperConfiguration.CreateMapper();

_logger.Debug("Run");

try
{
    builder.Services.AddHttpClient<IMetricsAgentClient, MetricsAgentClient>().
        AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)));

    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    builder.Services.AddControllers();

    builder.Services.AddHostedService<QuartzHostedService>();

    builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString("Data Source=test.db")
                    .ScanIn(typeof(Program).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());

    builder.Services.AddSingleton<ICpuMetricsRepository, CpuMetricsRepository>();
    builder.Services.AddSingleton<INetWorkMetricsRepository, NetWorkMetricsRepository>();
    builder.Services.AddSingleton<IRamMetricsRepository, RamMetricsRepository>();
    builder.Services.AddSingleton<IDotNetMetricsRepository, DotNetMetricsRepository>();
    builder.Services.AddSingleton<IHddMetricsRepository, HddMetricsRepository>();

    builder.Services.AddSingleton<IJobFactory, SingletonJobFactory>();
    builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

    builder.Services.AddSingleton<CpuMetricJob>();
    builder.Services.AddSingleton(new JobSchedule(jobType: typeof(CpuMetricJob), cronExpression: "0/5 * * * * ?"));

    builder.Services.AddSingleton<RamMetricJob>();
    builder.Services.AddSingleton(new JobSchedule(jobType: typeof(RamMetricJob), cronExpression: "0/5 * * * * ?"));

    builder.Services.AddSingleton<NetWorkMetricJob>();
    builder.Services.AddSingleton(new JobSchedule(jobType: typeof(NetWorkMetricJob), cronExpression: "0/5 * * * * ?"));

    builder.Services.AddSingleton<HddMetricJob>();
    builder.Services.AddSingleton(new JobSchedule(jobType: typeof(HddMetricJob), cronExpression: "0/5 * * * * ?"));

    builder.Services.AddSingleton<DotNetMetricJob>();
    builder.Services.AddSingleton(new JobSchedule(jobType: typeof(DotNetMetricJob), cronExpression: "0/5 * * * * ?"));

    builder.Services.AddSingleton(_mapper);

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    _logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
