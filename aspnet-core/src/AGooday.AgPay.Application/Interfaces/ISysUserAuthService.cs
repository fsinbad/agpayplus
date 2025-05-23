﻿using AGooday.AgPay.Application.DataTransfer;

namespace AGooday.AgPay.Application.Interfaces
{
    public interface ISysUserAuthService : IAgPayService<SysUserAuthDto, long>
    {
        Task<SysUserAuthDto> GetByIdentifierAsync(byte identityType, string identifier, string sysType);
        Task ResetAuthInfoAsync(long resetUserId, string authLoginUserName, string telphone, string newPwd, string sysType);
    }
}
