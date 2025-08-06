using Com.Ctrip.Framework.Apollo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using Panda.DynamicWebApi.Attributes;
using Saki.AutoFac.AutofacRegister;
using Saki.BaseTemplate.BaseControllers;
using Saki.InterfaceTemplate.Users;
using StackExchange.Profiling;

namespace Saki.TemplateWebProject.v2.Controllers;

/// <summary>
/// Home控制器
/// <remarks>当控制器与动态 api 混用时，需要在控制器中使用属性路由</remarks>
/// </summary>
[Route("api/[controller]/[action]")]
public class HomeController : BaseController
{
    [AutowiredProperty] private IUsersServiceInterface _usersService { get; set; }
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public HomeController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    /// <summary>
    /// 用户信息获取接口
    /// </summary>
    /// <param name="Id">用户主键Id</param>
    /// <returns>基础用户信息dto</returns>
    [HttpGet]
    // [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetUser([FromQuery] string Id)
    {
        // Test
        var res = await _usersService.GetUser(Id);
        return Ok(res);
    }

    /// <summary>
    /// 用户信息获取接口
    /// </summary>
    /// <param name="Id">用户主键Id</param>
    /// <returns>基础用户信息dto</returns>
    [HttpGet]
    public async Task<IActionResult> GetJoinUser()
    {
        var res = await _usersService.GetJoinList();
        return Ok(res);
    }

    /// <summary>
    /// 用户信息获取接口
    /// </summary>
    /// <param name="Id">用户主键Id</param>
    /// <returns>基础用户信息dto</returns>
    [HttpGet]
    public IActionResult GetSetting(string key)
    {
        var config = _configuration.GetValue<string>(key);
        return Ok(config);
    }

    [NonDynamicWebApi]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetCounts()
    {
        var html = MiniProfiler.Current.RenderIncludes(_httpContextAccessor.HttpContext);
        return Ok(html.Value);
    }
}

/// <summary>
/// 心跳检查
/// </summary>
[Route("api/[controller]/[action]")]
public class HealthController : BaseController
{
    private ILogger<HealthController> _logger;
    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 健康检查
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult CheckHealth()
    {
        _logger.LogInformation($"Check Health : {DateTime.Now}");
        return Ok("Web2_Demo:" + DateTime.Now);
    }
}