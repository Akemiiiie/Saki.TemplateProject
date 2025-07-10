using Saki.ModelTemplate.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saki.ModelTemplate.Dtos
{
    /// <summary>
    /// 模拟UserDto
    /// </summary>
    public class UsersDto: BaseDto
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public required string UserName { get; set; }

        /// <summary>
        /// Email地址
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// hash后的密码
        /// </summary>
        public required string PasswordHash { get; set; } // 存储哈希后的密码
    }
}
