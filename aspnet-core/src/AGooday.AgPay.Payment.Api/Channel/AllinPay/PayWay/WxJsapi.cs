﻿using AGooday.AgPay.Application.DataTransfer;
using AGooday.AgPay.Application.Interfaces;
using AGooday.AgPay.Application.Params.AllinPay;
using AGooday.AgPay.Common.Exceptions;
using AGooday.AgPay.Common.Utils;
using AGooday.AgPay.Payment.Api.Models;
using AGooday.AgPay.Payment.Api.RQRS;
using AGooday.AgPay.Payment.Api.RQRS.Msg;
using AGooday.AgPay.Payment.Api.RQRS.PayOrder;
using AGooday.AgPay.Payment.Api.RQRS.PayOrder.PayWay;
using AGooday.AgPay.Payment.Api.Services;
using AGooday.AgPay.Payment.Api.Utils;
using Newtonsoft.Json.Linq;

namespace AGooday.AgPay.Payment.Api.Channel.AllinPay.PayWay
{
    /// <summary>
    /// 通联 微信jsapi
    /// </summary>
    public class WxJsapi : AllinPayPaymentService
    {
        public WxJsapi(IServiceProvider serviceProvider,
            ISysConfigService sysConfigService,
            ConfigContextQueryService configContextQueryService)
            : base(serviceProvider, sysConfigService, configContextQueryService)
        {
        }

        public override AbstractRS Pay(UnifiedOrderRQ rq, PayOrderDto payOrder, MchAppConfigContext mchAppConfigContext)
        {
            string logPrefix = "【通联(wechatJs)jsapi支付】";
            JObject reqParams = new JObject();
            WxJsapiOrderRS res = ApiResBuilder.BuildSuccess<WxJsapiOrderRS>();
            ChannelRetMsg channelRetMsg = new ChannelRetMsg();
            res.ChannelRetMsg = channelRetMsg;

            // 请求参数赋值
            UnifiedParamsSet(reqParams, payOrder, GetNotifyUrl(), GetReturnUrl());

            WxJsapiOrderRQ bizRQ = (WxJsapiOrderRQ)rq;
            //通联扫一扫支付， 需要传入buyerUserId参数
            /*用户号（微信openid / 支付宝userid / 银联userid）
            payType == "WECHAT"或"ALIPAY"时必传*/
            reqParams.Add("acct", bizRQ.Openid);

            // 获取微信官方配置的 appId
            //AllinPayIsvSubMchParams allinpayIsvParams = (AllinPayIsvSubMchParams)_configContextQueryService.QueryIsvSubMchParams(mchAppConfigContext.MchNo, mchAppConfigContext.AppId, GetIfCode());
            reqParams.Add("sub_appid", "");

            // 发送请求
            JObject resJSON = PackageParamAndReq("/apiweb/unitorder/pay", reqParams, logPrefix, mchAppConfigContext);
            //请求 & 响应成功， 判断业务逻辑
            string code = resJSON.GetValue("retcode").ToString(); //请求响应码
            string msg = resJSON.GetValue("retmsg").ToString(); //响应信息
            reqParams.TryGetString("cusid", out string cusid); // 商户号
            channelRetMsg.ChannelMchNo = cusid;
            //channelRetMsg.ChannelIsvNo = orgid;
            try
            {
                if ("SUCCESS".Equals(code))
                {
                    string trxid = resJSON.GetValue("trxid").ToString();
                    string payinfo = resJSON.GetValue("payinfo").ToString();
                    res.PayInfo = payinfo;
                    channelRetMsg.ChannelOrderId = trxid;
                    channelRetMsg.ChannelState = ChannelState.WAITING;
                }
                else
                {
                    channelRetMsg.ChannelState = ChannelState.WAITING;
                    channelRetMsg.IsNeedQuery = true; // 开启轮询查单
                }
            }
            catch (Exception)
            {
                channelRetMsg.ChannelErrCode = code;
                channelRetMsg.ChannelErrMsg = msg;
            }
            return res;
        }

        public override string PreCheck(UnifiedOrderRQ rq, PayOrderDto payOrder)
        {
            WxJsapiOrderRQ bizRQ = (WxJsapiOrderRQ)rq;
            if (string.IsNullOrWhiteSpace(bizRQ.Openid))
            {
                throw new BizException("[openId]不可为空");
            }

            return null;
        }
    }
}
