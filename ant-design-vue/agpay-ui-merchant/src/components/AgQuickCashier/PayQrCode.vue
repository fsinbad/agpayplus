<template>
  <a-modal
    v-model="visible"
    title=""
    :footer="null"
    :maskClosable="false"
    :centered="true"
    :width="330"
    @cancel="handleClose">
    <div class="modal-body" v-if="apiRes.payDataType == 'codeImgUrl'">
      <div>
        <img width="252" height="252" v-if="apiRes.payDataType == 'codeImgUrl'" :src="apiRes.payData" alt="">
        <div class="zfb-wx" style="margin-bottom: 10px;text-align: center;">
          <img src="~@/assets/svg/alipay.svg" alt="">
          <img src="~@/assets/svg/wechatpay.svg" alt="" style="margin: 0px 5px;">
          <img src="~@/assets/svg/unionpay.svg" alt="">
          <p style="color: grey;">支持支付宝、微信与云闪付支付</p>
        </div>
      </div>
      <p style="font-size: 20px; font-weight: 500; color: grey;">请扫描付款码收款</p>
      <a-button type="primary" size="large" style="width: 100%" @click="handleClose">取消收款</a-button>
    </div>
    <div class="modal-body" v-else><p style="font-size: 20px; font-weight: 500; color: grey;">等待用户支付,请稍后...</p></div>
  </a-modal>
</template>

<script>
import ReconnectingWebSocket from 'reconnectingwebsocket'
import { getWebSocketPrefix } from '@/api/manage'

export default {
  name: 'PayQrCode',
  data () {
    return {
      visible: false,
      apiRes: {}, // 接口返回数据包
      payOrderWebSocket: null // 支付订单webSocket对象
    }
  },
  methods: {
    show (wayCode, apiRes) {
      console.log(apiRes)
      const that = this
      this.visible = true // 打开弹窗
      // 关闭上一个webSocket监听
      if (this.payOrderWebSocket) {
        this.payOrderWebSocket.close()
      }

      this.apiRes = apiRes

      // 此处判断接口中返回的orderState，值为0，1 代表支付中，直接放行无需处理，2 成功 3 失败
      if (apiRes.orderState === 2 || apiRes.orderState === 3) {
        if (apiRes.orderState === 2) {
          that.handleClose()
          const succModal = that.$infoBox.modalSuccess('支付成功', <div>2s后自动关闭...</div>)
          setTimeout(() => { succModal.destroy() }, 2000)
          that.$emit('closeBarCode') // 关闭条码框
        } else if (apiRes.orderState === 3) {
          that.handleClose()
          that.$emit('closeBarCode') // 关闭条码框
          that.$infoBox.modalError('支付失败', <div><div>错误码：{ apiRes.errCode}</div>
            <div>错误信息：{ apiRes.errMsg}</div></div>)
        }
        return
      }

      // 如果上面未关闭条码框，则代表进入webScoket，那么先在此处关闭条码框
      that.$emit('closeBarCode') // 关闭条码框
      // 监听响应结果
      this.payOrderWebSocket = new ReconnectingWebSocket(getWebSocketPrefix() + '/api/anon/ws/payOrder/' + apiRes.payOrderId + '/' + new Date().getTime())
      this.payOrderWebSocket.onopen = () => {}
      this.payOrderWebSocket.onmessage = (msgObject) => {
        const resMsgObject = JSON.parse(msgObject.data)
        if (resMsgObject.state === 2) {
          that.handleClose()
          const succModal = that.$infoBox.modalSuccess('支付成功', <div>2s后自动关闭...</div>)
          setTimeout(() => { succModal.destroy() }, 2000)
        } else {
          that.handleClose()
          that.$infoBox.modalError('支付失败', <div><div>错误码：{ resMsgObject.errCode}</div>
            <div>错误信息：{ resMsgObject.errMsg}</div></div>)
        }
      }
    },
    handleClose (e) {
      if (this.payOrderWebSocket) {
        this.payOrderWebSocket.close()
      }
      this.visible = false
    }
  }
}
</script>

<style scoped>
  .modal-body {
    width: 282px;
    display: flex;
    position: relative;
    padding: 24px;
    left: 0;
    top: 0;
    flex-direction: column;
    align-items: center;
    justify-content: center
  }
</style>
