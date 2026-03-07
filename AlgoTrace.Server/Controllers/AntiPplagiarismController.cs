using Microsoft.AspNetCore.Mvc;
using AlgoTrace.Server.Services;
using AlgoTrace.Server.Interfaces;
using AlgoTrace.Server.Models.DTO; 

namespace AlgoTrace.Server.Controllers
{
    [ApiController]
    [Route("api/analysis")] // Общий базовый путь для всех видов анализа
    public class AnalysisController : ControllerBase
    {
        private readonly ITextAnalysisService _textService;
        private readonly IGraphAnalysisService _graphService;
        private readonly IMetricAnalysisService _metricService; // Добавлен новый сервис

        // Внедряем все три сервиса через зависимости
        public AnalysisController(
            ITextAnalysisService textService,
            IGraphAnalysisService graphService,
            IMetricAnalysisService metricService)
        {
            _textService = textService;
            _graphService = graphService;
            _metricService = metricService;
        }

        // Итоговый маршрут: POST api/analysis/text/compare
        [HttpPost("text/compare")]
        public IActionResult CompareText([FromBody] TextCompareRequest request)
        {
            var result = _textService.Analyze(
                request.SourceCode,
                request.ReferenceCode,
                request.SourceName,
                request.RefName
            );
            return Ok(result);
        }

        // Итоговый маршрут: POST api/analysis/graph/compare
        [HttpPost("graph/compare")]
        public IActionResult CompareGraph([FromBody] GraphAnalysisRequest request)
        {
            var result = _graphService.Analyze(request);
            return Ok(result);
        }

        // Итоговый маршрут: POST api/analysis/metric/compare
        [HttpPost("metric/compare")]
        public IActionResult CompareMetric([FromBody] MetricAnalysisRequest request)
        {
            var result = _metricService.Analyze(request);
            return Ok(result);
        }
    }

    // DTO класс (напоминаю, что в идеале его стоит вынести в отдельный файл)
    public class TextCompareRequest
    {
        public string SourceCode { get; set; } = "";
        public string ReferenceCode { get; set; } = "";
        public string SourceName { get; set; } = "input_file.py";
        public string RefName { get; set; } = "reference_file.py";
    }
}