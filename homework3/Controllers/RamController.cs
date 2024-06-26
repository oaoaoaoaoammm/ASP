﻿using Microsoft.AspNetCore.Mvc;
using WorkWithBD;

namespace Metrics.Controllers
{
    [Route("api/metric/ram")]
    [ApiController]
    public class RamController : ControllerBase
    {
        private IRamMetricsRepository repository;
        private readonly ILogger<RamController> _logger;


        public RamController(ILogger<RamController> logger, IRamMetricsRepository repository)
        {
            _logger = logger;
            _logger.LogDebug(1, "NLog entered in RamController");
            this.repository = repository;
        }



        [HttpPost("create")]
        public IActionResult Create([FromBody] RamMetricCreateRequest request)
        {
            _logger.LogInformation(1, $"Entered request: Value: {request.Value},\n Time: {request.Time}");
            repository.Create(new RamMetrics
            {
                Time = request.Time,
                Value = request.Value
            });
            return Ok("Created");
        }


        [HttpGet("all")]
        public IActionResult GetAll()
        {
            _logger.LogInformation(1, $"Entered request: Display rammetrics database");
            var metrics = repository.GetAll();
            var response = new AllRamMetricsResponse()
            {
                Metrics = new List<RamMetricDto>()
            };
            foreach (var metric in metrics)
            {
                response.Metrics.Add(new RamMetricDto { Id = metric.Id, Value = metric.Value, Time = metric.Time });
                _logger.LogInformation(2, $"Displayed: {metric.Id}, {metric.Value}, {metric.Time}");
            }
            return Ok(response);
        }


        [HttpPost("delete")]
        public IActionResult Delete([FromQuery] int id)
        {
            _logger.LogInformation(1, $"Entered request: Delete {id} item in rammetrics database");
            repository.Delete(id);
            return Ok("Deleted");
        }


        [HttpPost("update")]
        public IActionResult Update([FromBody] RamMetrics metric)
        {
            _logger.LogInformation(1, $"Entered request: Update rammetrics database");
            var metrics = repository.GetAll();
            foreach (var metricq in metrics)
            {
                if (metricq.Id == metric.Id)
                {
                    _logger.LogInformation(2, $"From: {metricq.Id}  {metricq.Value}  {metricq.Time}");
                }
            }
            _logger.LogInformation(2, $"To: {metric.Id}  {metric.Value}  {metric.Time}");
            repository.Update(metric);
            return Ok("Updated");
        }


        [HttpPost("getbyid")]
        public IActionResult GetById([FromQuery] int id)
        {
            _logger.LogInformation(1, $"Entered request: Display {id} item from rammetrics database");
            var metrics = repository.GetById(id);
            return Ok(metrics);
        }


        [HttpGet("api/metrics/ram/available/from/{fromTime}/to/{toTime}")]
        public IActionResult GetMetricsFromAgent([FromRoute] int agentId, [FromRoute] TimeSpan fromTime, [FromRoute] TimeSpan toTime)
        {
            _logger.LogInformation($"Id: {agentId}\n from time: {fromTime}\n to time: {toTime}");
            return Ok();
        }
    }
}

