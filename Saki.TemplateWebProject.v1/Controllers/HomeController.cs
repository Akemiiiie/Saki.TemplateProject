using Microsoft.AspNetCore.Mvc;
using Saki.BaseTemplate.AutofacRegister;
using Saki.BaseTemplate.BaseControllers;
using Saki.InterfaceTemplate.Users;

namespace Saki.TemplateWebProject.v1.Controllers;

/// <summary>
///     Home控制器
/// </summary>
// [DynamicWebApi]
public class HomeController : BaseController
{
    [AutowiredProperty] private IUsersServiceInterface _usersService { get; set; }

    /// <summary>
    ///     用户信息获取接口
    /// </summary>
    /// <param name="Id">用户主键Id</param>
    /// <returns>基础用户信息dto</returns>
    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery] string Id)
    {
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
}