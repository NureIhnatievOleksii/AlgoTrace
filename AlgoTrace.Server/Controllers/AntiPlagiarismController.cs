using AlgoTrace.Server.Interfaces;
using AlgoTrace.Server.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTrace.Server.Controllers
{
    [ApiController]
    [Route("api/analysis")]
    public class AnalysisController : ControllerBase
    {
        private readonly ITextAnalysisService _textService;
        private readonly ITokenAnalysisService _tokenService;
        private readonly ITreeAnalysisService _treeService;
        private readonly IGraphAnalysisService _graphService;
        private readonly IMetricAnalysisService _metricService;
        private readonly IUnifiedAnalysisService _unifiedService;

        public AnalysisController(
            ITextAnalysisService textService,
            ITokenAnalysisService tokenService,
            ITreeAnalysisService treeService,
            IGraphAnalysisService graphService,
            IMetricAnalysisService metricService,
            IUnifiedAnalysisService unifiedService
        )
        {
            _textService = textService;
            _tokenService = tokenService;
            _treeService = treeService;
            _graphService = graphService;
            _metricService = metricService;
            _unifiedService = unifiedService;
        }

        [HttpPost("unified")]
        public IActionResult CompareUnified([FromBody] UnifiedAnalysisRequest request)
        {
            var result = _unifiedService.Analyze(request);
            return Ok(result);
        }
    }
}
