﻿using AGooday.AgPay.Application.DataTransfer;

namespace AGooday.AgPay.Application.Interfaces
{
    public interface IAuthService
    {
        Task<SysUserDto> GetUserByIdAsync(long userId);
        IEnumerable<SysUserDto> GetUserByIds(List<long> userIds);
        IEnumerable<SysUserRoleRelaDto> GetUserRolesByUserId(long userId);
        IEnumerable<SysEntitlementDto> GetEntsBySysType(string sysType, string entId);
        IEnumerable<SysEntitlementDto> GetEntsBySysType(string sysType, List<string> entIds, List<string> entTypes);
        IEnumerable<SysEntitlementDto> GetEntsByUserId(long userId, byte userType, string sysType);
        Task<SysUserAuthInfoDto> GetUserAuthInfoByIdAsync(long userId);
        Task<bool> UserHasLeftMenuAsync(long userId, string sysType);
        Task<SysUserAuthInfoDto> LoginAuthAsync(string identifier, byte identityType, string sysType);
    }
}
