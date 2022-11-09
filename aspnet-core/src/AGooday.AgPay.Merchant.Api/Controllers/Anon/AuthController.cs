using AGooday.AgPay.Application.Interfaces;
using AGooday.AgPay.Application.Services;
using AGooday.AgPay.Application.DataTransfer;
using AGooday.AgPay.Common.Constants;
using AGooday.AgPay.Common.Exceptions;
using AGooday.AgPay.Common.Models;
using AGooday.AgPay.Common.Utils;
using AGooday.AgPay.Domain.Core.Notifications;
using AGooday.AgPay.Domain.Models;
using AGooday.AgPay.Merchant.Api.Models;
using CaptchaGen.NetCore;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System;
using System.Buffers.Text;
using System.Runtime.InteropServices;
using AGooday.AgPay.Merchant.Api.Extensions;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace AGooday.AgPay.Merchant.Api.Controllers.Anon
{
    /// <summary>
    /// ��֤�ӿ�
    /// </summary>
    [ApiController, AllowAnonymous]
    [Route("api/anon/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly ISysUserAuthService _sysUserAuthService;
        private readonly ISysUserRoleRelaService _sysUserRoleRelaService;
        private readonly ISysRoleEntRelaService _sysRoleEntRelaService;
        private readonly IMemoryCache _cache;
        private readonly IDatabase _redis;
        // ������֪ͨ��������ע��Controller
        private readonly DomainNotificationHandler _notifications;

        public AuthController(ILogger<AuthController> logger, IOptions<JwtSettings> jwtSettings, IMemoryCache cache, RedisUtil client,
            INotificationHandler<DomainNotification> notifications,
            ISysUserAuthService sysUserAuthService,
            ISysRoleEntRelaService sysRoleEntRelaService,
            ISysUserRoleRelaService sysUserRoleRelaService)
        {
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
            _sysUserAuthService = sysUserAuthService;
            _cache = cache;
            _redis = client.GetDatabase();
            _notifications = (DomainNotificationHandler)notifications;
            _sysRoleEntRelaService = sysRoleEntRelaService;
            _sysUserRoleRelaService = sysUserRoleRelaService;
        }

        /// <summary>
        /// �û���Ϣ��֤ ��ȡiToken
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        [HttpPost, Route("validate")]
        public ApiRes Validate(Validate model)
        {
            string account = Base64Util.DecodeBase64(model.ia); //�û��� i account, ����base64����
            string ipassport = Base64Util.DecodeBase64(model.ip); //���� i passport, ����base64����
            string vercode = Base64Util.DecodeBase64(model.vc); //��֤�� vercode, ����base64����
            string vercodeToken = Base64Util.DecodeBase64(model.vt); //��֤��token, vercode token , ����base64����

#if !DEBUG
            string cacheCode = _redis.StringGet(CS.GetCacheKeyImgCode(vercodeToken));
            if (string.IsNullOrWhiteSpace(cacheCode) || !cacheCode.Equals(vercode))
            {
                throw new BizException("��֤������");
            } 
#endif

            //��¼��ʽ�� Ĭ��Ϊ�˺������¼
            byte identityType = CS.AUTH_TYPE.LOGIN_USER_NAME;
            if (RegUtil.IsMobile(account))
            {
                identityType = CS.AUTH_TYPE.TELPHONE; //�ֻ��ŵ�¼
            }

            var auth = _sysUserAuthService.SelectByLogin(account, identityType, CS.SYS_TYPE.MCH);

            if (auth == null)
            {
                //û�и��û���Ϣ
                throw new BizException("�û���/�������");
            }

            //https://jasonwatmore.com/post/2022/01/16/net-6-hash-and-verify-passwords-with-bcrypt
            //https://bcrypt.online/
            bool verified = BCrypt.Net.BCrypt.Verify(ipassport, auth.Credential);
            if (!verified)
            {
                //û�и��û���Ϣ
                throw new BizException("�û���/�������");
            }

            //�ǳ�������Ա && ���������˵� ���д�����ʾ
            if (auth.IsAdmin != CS.YES && !_sysRoleEntRelaService.UserHasLeftMenu(auth.SysUserId, auth.SysType))
            {
                throw new BizException("��ǰ�û�δ�����κβ˵�Ȩ�ޣ�����ϵ����Ա���з�����ٵ�¼��");
            }

            //����token
            string cacheKey = CS.GetCacheKeyToken(auth.SysUserId, Guid.NewGuid().ToString("N").ToUpper());
            var authorities = _sysUserRoleRelaService.SelectRoleIdsByUserId(auth.SysUserId).ToList();
            authorities.AddRange(_sysRoleEntRelaService.SelectEntIdsByUserId(auth.SysUserId, auth.IsAdmin, auth.SysType));

            // ����ǰ�� accessToken
            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, auth.SysUserId.ToString()),
                new Claim(ClaimTypes.Name, auth.LoginUsername),
                new Claim("sysUserId",auth.SysUserId.ToString()),
                new Claim("avatarUrl",auth.AvatarUrl),
                new Claim("realname",auth.Realname),
                new Claim("loginUsername",auth.LoginUsername),
                new Claim("telphone",auth.Telphone),
                new Claim("userNo",auth.UserNo.ToString()),
                new Claim("sex",auth.Sex.ToString()),
                new Claim("state",auth.State.ToString()),
                new Claim("isAdmin",auth.IsAdmin.ToString()),
                new Claim("sysType",auth.SysType),
                new Claim("belongInfoId",auth.BelongInfoId),
                new Claim("createdAt",auth.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                new Claim("updatedAt",auth.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                new Claim("cacheKey",cacheKey)
            });
            var accessToken = JwtBearerAuthenticationExtension.GetJwtAccessToken(_jwtSettings, claimsIdentity);

            var currentUser = JsonConvert.SerializeObject(new CurrentUser
            {
                CacheKey = cacheKey,
                SysUser = auth,
                Authorities = authorities
            });
            _redis.StringSet(cacheKey, currentUser, new TimeSpan(0, 0, CS.TOKEN_TIME));

            // ɾ��ͼ����֤�뻺������
            _redis.KeyDelete(CS.GetCacheKeyImgCode(vercodeToken));

            return ApiRes.Ok4newJson(CS.ACCESS_TOKEN_NAME, accessToken);
        }

        /// <summary>
        /// ͼƬ��֤��
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("vercode")]
        public ApiRes Vercode()
        {
            //����ͼ����֤��ĳ��Ϳ� // 4λ��֤��
            //string code = ImageFactory.CreateCode(6);
            //string imageBase64Data;
            //using (var picStream = ImageFactory.BuildImage(code, 40, 137, 20, 10))
            //{
            //    var imageBytes = picStream.ToArray();
            //    imageBase64Data = $"data:image/jpg;base64,{Convert.ToBase64String(imageBytes)}";
            //}
            var code = VerificationCodeUtil.RandomVerificationCode(6);
            var bitmap = VerificationCodeUtil.DrawImage(code, 137, 40, 20);
            var imageBase64Data = $"data:image/jpg;base64,{VerificationCodeUtil.BitmapToBase64Str(bitmap)}";

            //redis
            string vercodeToken = Guid.NewGuid().ToString("N");
            _redis.StringSet(CS.GetCacheKeyImgCode(vercodeToken), code, new TimeSpan(0, 0, CS.VERCODE_CACHE_TIME)); //ͼƬ��֤�뻺��ʱ��: 1����

            return ApiRes.Ok(new { imageBase64Data, vercodeToken, expireTime = CS.VERCODE_CACHE_TIME });
        }
    }
}