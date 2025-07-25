﻿using AGooday.AgPay.Application.DataTransfer;
using AGooday.AgPay.Application.Interfaces;
using AGooday.AgPay.Application.Permissions;
using AGooday.AgPay.Common.Constants;
using AGooday.AgPay.Common.Models;
using AGooday.AgPay.Common.Utils;
using AGooday.AgPay.Components.Cache.Services;
using AGooday.AgPay.Merchant.Api.Attributes;
using AGooday.AgPay.Merchant.Api.Authorization;
using AGooday.AgPay.Merchant.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGooday.AgPay.Merchant.Api.Controllers.SysUser
{

    /// <summary>
    /// 角色 权限管理
    /// </summary>
    [Route("api/sysRoleEntRelas")]
    [ApiController, Authorize]
    public class SysRoleEntRelaController : CommonController
    {
        private readonly ISysRoleService _sysRoleService;
        private readonly ISysRoleEntRelaService _sysRoleEntRelaService;
        private readonly ISysUserRoleRelaService _sysUserRoleRelaService;

        public SysRoleEntRelaController(ILogger<SysRoleEntRelaController> logger,
            ICacheService cacheService,
            IAuthService authService,
            ISysRoleService sysRoleService,
            ISysRoleEntRelaService sysRoleEntRelaService,
            ISysUserRoleRelaService sysUserRoleRelaService)
            : base(logger, cacheService, authService)
        {
            _sysRoleEntRelaService = sysRoleEntRelaService;
            _sysUserRoleRelaService = sysUserRoleRelaService;
            _sysRoleService = sysRoleService;
        }

        /// <summary>
        /// 角色权限列表
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet, Route("")]
        [PermissionAuth(PermCode.MCH.ENT_UR_ROLE_DIST), NoLog]
        public async Task<ApiPageRes<SysRoleEntRelaDto>> ListAsync([FromQuery] SysRoleEntRelaQueryDto dto)
        {
            var data = await _sysRoleEntRelaService.GetPaginatedDataAsync(dto);
            return ApiPageRes<SysRoleEntRelaDto>.Pages(data);
        }

        /// <summary>
        /// 重置角色权限关联信息
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="entIds"></param>
        /// <returns></returns>
        [HttpPost, Route("relas/{roleId}"), MethodLog("重置角色权限关联信息")]
        [PermissionAuth(PermCode.MCH.ENT_UR_ROLE_DIST)]
        public async Task<ApiRes> RelasAsync(string roleId, RelasEntModel model)
        {
            var role = await _sysRoleService.GetByIdAsync(roleId);
            if (role == null || !role.SysType.Equals(CS.SYS_TYPE.MCH) || !role.BelongInfoId.Equals(await GetCurrentMchNoAsync()))
            {
                ApiRes.Fail(ApiCode.SYS_OPERATION_FAIL_SELETE);
            }
            if (model.EntIds.Count > 0)
            {
                await _sysRoleEntRelaService.ResetRelaAsync(roleId, model.EntIds);

                //查询到该角色的人员， 将redis更新
                var sysUserIdList = _sysUserRoleRelaService.SelectUserIdsByRoleId(roleId).ToList();
                await RefAuthenticationAsync(sysUserIdList);
            }
            return ApiRes.Ok();
        }
    }
}
