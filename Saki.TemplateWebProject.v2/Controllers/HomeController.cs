using Microsoft.AspNetCore.Mvc;
using Saki.AutoFac.AutofacRegister;
using Saki.BaseTemplate.BaseControllers;
using Saki.InterfaceTemplate.Users;
using StackExchange.Profiling;

namespace Saki.TemplateWebProject.v1.Controllers;

/// <summary>
///     Home控制器
/// </summary>
public class HomeController : BaseController
{
    [AutowiredProperty] private IUsersServiceInterface _usersService { get; set; }
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HomeController(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    ///     用户信息获取接口
    /// </summary>
    /// <param name="Id">用户主键Id</param>
    /// <returns>基础用户信息dto</returns>
    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery] string Id)
    {
        // Test
        var res = await _usersService.GetUser(Id);
        return Ok(res);
    }


    /// <summary>
    ///     用户信息获取接口
    /// </summary>
    /// <param name="Id">用户主键Id</param>
    /// <returns>基础用户信息dto</returns>
    [HttpGet]
    public async Task<IActionResult> GetJoinUser()
    {
        var res = await _usersService.GetJoinList();
        return Ok(res);
    }

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