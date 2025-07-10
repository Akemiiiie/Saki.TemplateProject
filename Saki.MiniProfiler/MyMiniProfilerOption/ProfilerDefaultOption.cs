
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Profiling;

namespace Saki.MiniProfilerOption
{
    /// <summary>
    /// 分析器的默认选项配置
    /// </summary>
    public class ProfilerDefaultOption
    {
        /// <summary>
        /// 分析器默认选项配置
        /// </summary>
        /// <returns></returns>
        public static void GetProfilerDefaultOption(MiniProfilerOptions _options)
        {
            // 设置默认选项
            // 所有这些都是可选的。您只需调用.AddMiniProfiler()即可获得所有默认值
            // 分析器默认url路径
            _options.RouteBasePath = "/profiler";

            // (Optional)控制存储
            // (default is 30 minutes in MemoryCacheStorage)
            // 如果MemoryCache 设置了SizeLimit，MiniProfiler将无法工作！
            // (_options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

            // 控制使用哪种SQL格式化方式，默认为InlineFormatter
            _options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

            // (Optional) 分析器是否需要鉴权, you can use the Func<HttpRequest, bool> _options:
            // 默认情况下任何人都可以访问分析器
            // _options.ResultsAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
            // _options.ResultsListAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
            // 分析器鉴权的异步版本:
            // _options.ResultsAuthorizeAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfiler;
            // _options.ResultsListAuthorizeAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfilerLists;

            // (Optional) 控制对哪些请求进行分析
            // (默认所有请求都会进行分析)
            // _options.ShouldProfile = request => MyShouldThisBeProfiledFunction(request);

            // (Optional) Profiles are stored under a user ID, function to get it:
            // (default is null, since above methods don't use it by default)
            // _options.UserIdProvider = request => MyGetUserIdFunction(request);

            // (Optional) 可以及实现分析器功能重载
            // _options.ProfilerProvider = new MyProfilerProvider();

            // (Optional) 禁用打开连接/关闭连接(以及对应的异步操作)的监视
            // You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
            // (defaults to true, and connection opening/closing is tracked)
            _options.TrackConnectionOpenClose = true;

            // 主体风格/默认浅色
            _options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

            // 毫秒计时器的显示样式（默认为2）
            _options.PopupDecimalPlaces = 1;

            // 以下选项仅在.netCore3或更高版本中生效

            // 禁用对mvc的过滤器分析
            _options.EnableMvcFilterProfiling = true;

            // 或者仅对时间超过一定时长的MVC过滤器进行分析
            // _options.MvcFilterMinimumSaveMs = 1.0m;

            // 禁用对MVC视图请求的分析
            _options.EnableMvcViewProfiling = true;
            // 或者仅对超过一定时长的MVC视图请求进行分析
            // _options.MvcViewMinimumSaveMs = 1.0m;

            // 监听MiniProfiler出现的异常
            // _options.OnInternalError = e => MyExceptionLogger(e);

            // 使用内存存储时，可以启用带堆栈和工具提示的重度调试模式
            // 与普通剖析相比，它的开销很大，因此在使用时应考虑到这一点
            //_options.EnableDebugMode = true;
        }
    }
}
