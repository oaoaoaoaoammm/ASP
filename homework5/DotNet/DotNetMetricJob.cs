﻿using Quartz;
using System.Diagnostics;
using WorkWithBD;

namespace MetricsAgent
{
	public class DotNetMetricJob : IJob
	{
		private IDotNetMetricsRepository _repository;
		private PerformanceCounter _performanceCounter;

		public DotNetMetricJob(IDotNetMetricsRepository repository)
		{
			_repository = repository;
			_performanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
		}

		public Task Execute(IJobExecutionContext context)
		{
			_repository.Create(new Microsoft.OpenApi.Models.DotNetMetric
			{
				DateTime = DateTime.Now,
				Value = Convert.ToInt32(_performanceCounter.NextValue())
			});
			return Task.CompletedTask;
		}
	}
}

