using Microsoft.AspNetCore.Mvc;

namespace Saki.TemplateWebProject.v3.Controllers
{
    /// <summary>
    /// 心跳检查
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HealthController(ILogger<HealthController> logger) : Controller
    {
        private ILogger<HealthController> _logger = logger;

        /// <summary>
        /// 健康检查
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CheckHealth()
        {
            _logger.LogInformation($"Check Health : {DateTime.Now}");
            return Ok("Web3_Demo:" + DateTime.Now);
        }
    }
}
