﻿using AGooday.AgPay.Application.DataTransfer;
using AGooday.AgPay.Application.Interfaces;
using AGooday.AgPay.Common.Exceptions;
using AGooday.AgPay.Components.Third.Models;
using AGooday.AgPay.Components.Third.RQRS;
using AGooday.AgPay.Components.Third.RQRS.PayOrder;
using AGooday.AgPay.Components.Third.RQRS.PayOrder.PayWay;
using AGooday.AgPay.Components.Third.Services;
using AGooday.AgPay.Components.Third.Utils;

namespace AGooday.AgPay.Components.Third.Channel.YsePay.PayWay
{
    /// <summary>
    /// 银盛 支付宝 条码支付
    /// </summary>
    public class AliBar : YsePayPaymentService
    {
        public AliBar(ILogger<AliBar> logger,
            IServiceProvider serviceProvider,
            ISysConfigService sysConfigService,
            ConfigContextQueryService configContextQueryService)
            : base(logger, serviceProvider, sysConfigService, configContextQueryService)
        {
        }

        public override async Task<AbstractRS> PayAsync(UnifiedOrderRQ rq, PayOrderDto payOrder, MchAppConfigContext mchAppConfigContext)
        {
            string logPrefix = "【银盛条码(alipay)支付】";
            AliBarOrderRQ bizRQ = (AliBarOrderRQ)rq;
            // 构造函数响应数据
            AliBarOrderRS res = ApiResBuilder.BuildSuccess<AliBarOrderRS>();

            SortedDictionary<string, string> reqParams = new SortedDictionary<string, string>();
            reqParams.Add("auth_code", bizRQ.AuthCode.Trim()); //授权码 通过扫码枪/声波获取设备获取的支付宝/微信/银联付款码
            reqParams.Add("scene", "bar_code");
            // 银盛 bar 统一参数赋值
            BarParamsSet(reqParams, payOrder, GetNotifyUrl());

            var channelRetMsg = await BarAsync(reqParams, GetNotifyUrl(), logPrefix, mchAppConfigContext);
            res.ChannelRetMsg = channelRetMsg;
            return res;
        }

        public override Task<string> PreCheckAsync(UnifiedOrderRQ rq, PayOrderDto payOrder, MchAppConfigContext mchAppConfigContext)
        {
            AliBarOrderRQ bizRQ = (AliBarOrderRQ)rq;
            if (string.IsNullOrWhiteSpace(bizRQ.AuthCode))
            {
                throw new BizException("用户支付条码[authCode]不可为空");
            }

            return Task.FromResult<string>(null);
        }
    }
}
