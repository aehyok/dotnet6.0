﻿using aehyok.Basic.Dtos;
using aehyok.Basic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aehyok.Basic.Api.Controllers
{

    /// <summary>
    /// Token 管理
    /// </summary>
    public class TokenController : BasicControllerBase
    {
        public IUserTokenService userTokenService;

        public TokenController(IUserTokenService userTokenService)
        {
            this.userTokenService = userTokenService;
        }
        /// <summary>
        /// 获取图片验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet("captcha")]
        [AllowAnonymous]
        public Task<CaptchaDto> GetCaptchaAsync()
        {
            return this.userTokenService.GenerateCaptchaAsync();
        }

        /// <summary>
        /// 账号密码登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("password")]
        [AllowAnonymous]
        public async Task<UserTokenDto> PostAsync(PasswordLoginDto model)
        {
            if (!await this.userTokenService.ValidateCaptchaAsync(model.Captcha, model.CaptchaKey))
            {
                throw new Exception("验证码错误");
            }

            return null;
            //return await this.userTokenService.LoginWithPasswordAsync(model.UserName, model.Password, model.Platform);
        }
    }
}
