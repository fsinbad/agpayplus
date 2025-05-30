﻿using AGooday.AgPay.Application.DataTransfer;
using AGooday.AgPay.Common.Constants;
using AGooday.AgPay.Components.Third.Models;
using AGooday.AgPay.Components.Third.RQRS.Msg;
using AGooday.AgPay.Components.Third.Services;

namespace AGooday.AgPay.Components.Third.Channel.PpPay
{
    public class PpPayPayOrderQueryService : IPayOrderQueryService
    {
        private readonly ConfigContextQueryService _configContextQueryService;

        public PpPayPayOrderQueryService(ConfigContextQueryService configContextQueryService)
        {
            _configContextQueryService = configContextQueryService;
        }

        public PpPayPayOrderQueryService()
        {
        }

        public string GetIfCode()
        {
            return CS.IF_CODE.PPPAY;
        }

        public async Task<ChannelRetMsg> QueryAsync(PayOrderDto payOrder, MchAppConfigContext mchAppConfigContext)
        {
            return await (await _configContextQueryService.GetPaypalWrapperAsync(mchAppConfigContext)).ProcessOrderAsync(null, payOrder);
        }
    }
}
