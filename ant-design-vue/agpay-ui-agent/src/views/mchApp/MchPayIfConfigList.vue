<template>
  <a-drawer
    :visible="visible"
    @close="onClose"
    :closable="true"
    :drawer-style="{ overflow: 'hidden', backgroundColor: '#f0f2f5' }"
    :body-style="{ paddingBottom: '80px', overflow: 'auto' }"
    width="80%"
  >
    <template slot="title">
      <a-steps :current="currentStep" type="navigation" style="width:80%">
        <a-step title="支付参数配置" @click="stepChange(0)" />
        <a-step title="支付通道配置" @click="stepChange(1)" />
      </a-steps>
    </template>
    <div v-if="currentStep === 0">
      <AgCard
        ref="infoCard"
        :reqCardListFunc="reqCardListFunc"
        :span="agpayCard.span"
        :height="agpayCard.height"
      >
        <div slot="cardContentSlot" slot-scope="{record}">
          <div :style="{'height': agpayCard.height + 'px'}" class="ag-card-content">
            <!-- 卡片自定义样式 -->
            <div class="ag-card-content-header" :style="{backgroundColor: record.bgColor, height: agpayCard.height/2 + 'px'}">
              <img v-if="record.icon" :src="record.icon" :style="{height: agpayCard.height/5 + 'px'}">
            </div>
            <div class="ag-card-content-body" :style="{height: (agpayCard.height/2 - 50) + 'px'}">
              <div class="title">
                {{ record.ifName }}
              </div>
              <a-badge :status="record.ifConfigState ===1 ? 'processing' : 'error'" :text="record.ifConfigState ===1 ? '启用' : '未开通'" ></a-badge>
            </div>
            <!-- 卡片底部操作栏 -->
            <div class="ag-card-ops">
              <a v-if="record.mchType == 2 && record.ifCode == 'alipay' && $access('ENT_MCH_PAY_CONFIG_ADD')" @click="toAlipayAuthPageFunc(record)">扫码授权 <a-icon key="right" type="right" style="fontSize: 13px"></a-icon></a>

              <a v-if="$access('ENT_MCH_PAY_CONFIG_ADD')" @click="editPayIfConfigFunc(record)">填写参数 <a-icon key="right" type="right" style="fontSize: 13px"></a-icon></a>
              <a v-else>暂无操作</a>

            </div>
          </div>
        </div>
      </AgCard>
    </div>
    <div v-else-if="currentStep === 1">
      <a-card>
        <div class="table-page-search-wrapper">
          <a-form layout="inline">
            <a-row :gutter="10">
              <a-col :md="4">
                <a-form-item label="">
                  <a-input placeholder="支付方式代码" v-model="searchData2.wayCode"/>
                </a-form-item>
              </a-col>
              <a-col :md="4">
                <a-form-item label="">
                  <a-input placeholder="支付方式名称" v-model="searchData2.wayName"/>
                </a-form-item>
              </a-col>
              <a-col :sm="6">
                <span class="table-page-search-submitButtons">
                  <a-button type="primary" icon="search" @click="searchFunc(true)">查询</a-button>
                  <a-button style="margin-left: 8px" icon="reload" @click="() => this.searchData2 = {}">重置</a-button>
                </span>
              </a-col>
            </a-row>
          </a-form>
        </div>
        <div class="split-line"/>
        <!-- 列表渲染 -->
        <AgTable
          ref="infoTable"
          :initData="true"
          :reqTableDataFunc="reqTableDataFunc"
          :tableColumns="tableColumns"
          :searchData="searchData2"
          rowKey="wayCode"
        >
          <template slot="stateSlot" slot-scope="{record}">
            <a-badge :status="record.passageState === 0?'error':'processing'" :text="record.passageState === 0?'禁用':'启用'" />
          </template>
          <template slot="opSlot" slot-scope="{record}">  <!-- 操作列插槽 -->
            <AgTableColumns>
              <a-button type="link" v-if="$access('ENT_MCH_PAY_PASSAGE_CONFIG')" @click="editPayPassageFunc(record)">配置</a-button>
            </AgTableColumns>
          </template>
        </AgTable>
      </a-card>
    </div>
    <div class="drawer-btn-center ">
      <a-button :style="{ marginRight: '8px' }" @click="onClose" icon="close">关闭</a-button>
      <a-button type="primary" icon="arrow-left" v-if="$access('ENT_MCH_PAY_CONFIG_LIST') && currentStep ===1" @click="stepChange(0)">上一步</a-button>
      <a-button type="primary" icon="arrow-right" v-if="$access('ENT_MCH_PAY_PASSAGE_LIST') && currentStep ===0" @click="stepChange(1)">下一步</a-button>
    </div>

    <!-- 支付参数配置JSON渲染页面组件  -->
    <MchPayConfigAddOrEdit ref="mchPayConfigAddOrEdit" :callbackFunc="refCardList" />
    <!-- 支付参数配置自定义页面组件 wxpay  -->
    <WxpayPayConfig ref="wxpayPayConfig" :callbackFunc="refCardList" />
    <!-- 支付参数配置自定义页面组件 alipay  -->
    <AlipayPayConfig ref="alipayPayConfig" :callbackFunc="refCardList" />
    <!-- 支付通道配置页面组件  -->
    <MchPayPassageAddOrEdit ref="mchPayPassageAddOrEdit" :callbackFunc="searchFunc"/>
    <!-- 支付宝授权弹层  -->
    <AlipayAuth ref="alipayAuthPage" :callbackFunc="refCardList"/>

  </a-drawer>
</template>

<script>
import AgCard from '@/components/AgCard/AgCard'
import AgTable from '@/components/AgTable/AgTable'
import AgTableColumns from '@/components/AgTable/AgTableColumns'
import { API_URL_MCH_PAYCONFIGS_LIST, API_URL_MCH_PAYPASSAGE_LIST, req, getAvailablePayInterfaceList } from '@/api/manage'
import MchPayConfigAddOrEdit from './MchPayConfigAddOrEdit'
import MchPayPassageAddOrEdit from './MchPayPassageAddOrEdit'
import WxpayPayConfig from './custom/WxpayPayConfig'
import AlipayPayConfig from './custom/AlipayPayConfig'
import AlipayAuth from './AlipayAuth'

// eslint-disable-next-line no-unused-vars
const tableColumns = [
  { key: 'wayCode', dataIndex: 'wayCode', title: '支付方式代码' },
  { key: 'wayName', dataIndex: 'wayName', title: '支付方式名称' },
  { key: 'passageState', title: '状态', scopedSlots: { customRender: 'stateSlot' } },
  { key: 'op', title: '操作', width: 160, fixed: 'right', align: 'center', scopedSlots: { customRender: 'opSlot' } }
]

export default {
  components: {
    AgCard,
    AgTable,
    AgTableColumns,
    MchPayConfigAddOrEdit,
    MchPayPassageAddOrEdit,
    WxpayPayConfig,
    AlipayPayConfig,
    AlipayAuth
  },
  data () {
    return {
      currentStep: 0, // 当前步骤条index
      btnLoading: false,
      appId: null, // 应用appId
      visible: false, // 抽屉开关
      agpayCard: { // 卡片配置
        height: 300,
        span: { xxl: 6, xl: 4, lg: 4, md: 3, sm: 2, xs: 1 }
      },
      tableColumns: tableColumns,
      searchData2: {}
    }
  },
  methods: {
    // 弹层打开事件
    show (appId) {
      this.appId = appId
      this.ifCode = null
      this.currentStep = 0
      this.visible = true
      this.refCardList()
    },
    // 步骤条切换
    stepChange (current) {
      this.currentStep = current
    },
    // 请求支付接口定义数据
    reqCardListFunc () {
      return req.list(API_URL_MCH_PAYCONFIGS_LIST, { 'appId': this.appId })
    },
    // 刷新支付接口card列表
    refCardList () {
      if (this.$refs.infoCard) {
        this.$refs.infoCard.refCardList()
      }
    },
    // 请求支付通道数据
    reqTableDataFunc (params) {
      const that = this
      return req.list(API_URL_MCH_PAYPASSAGE_LIST, Object.assign(params, { appId: that.appId }))
    },
    searchFunc (isToFirst = false) { // 点击【查询】按钮点击事件
      this.$refs.infoTable.refTable(isToFirst)
    },
    // 支付参数配置
    editPayIfConfigFunc (record) {
      if (!record) {
        return
      }
      if (record.subMchIsvConfig === 0) {
        this.$error({
          title: '提示',
          content: '当前应用所属商户为特约商户，请先配置服务商支付参数！'
        })
      } else if (record.configPageType === 1) {
        this.$refs.mchPayConfigAddOrEdit.show(this.appId, record)
      } else if (record.configPageType === 2) {
        this.$refs[record.ifCode + 'PayConfig'].show(this.appId, record)
      }
    },
    // 支付通道配置
    editPayPassageFunc (record) {
      const that = this
      getAvailablePayInterfaceList(that.appId, record.wayCode).then(resData => {
        if (!resData.records || resData.records.length === 0) {
          that.$error({
            title: '提示',
            content: '暂无可用支付接口配置'
          })
        } else {
          that.$refs.mchPayPassageAddOrEdit.show(that.appId, record.wayCode)
        }
      })
    },
    // 抽屉关闭
    onClose () {
      this.visible = false
    },

    // 支付宝子商户 扫码授权
    toAlipayAuthPageFunc (record) {
      if (!record) {
        return
      }
      if (record.subMchIsvConfig === 0) {
        this.$error({
          title: '提示',
          content: '当前应用所属商户为特约商户，请先配置服务商支付参数！'
        })
        return
      }
      this.$refs.alipayAuthPage.show(this.appId)
    }
  }
}
</script>

<style lang="less" scoped>
  .ag-card-content {
    width: 100%;
    position: relative;
    background-color: @ag-card-back;
    border-radius: 6px;
    overflow:hidden;
  }
  .ag-card-ops {
    width: 100%;
    height: 50px;
    background-color: @ag-card-back;
    display: flex;
    flex-direction: row;
    justify-content: space-around;
    align-items: center;
    border-top: 1px solid @ag-back;
    position: absolute;
    bottom: 0;
  }
  .ag-card-content-header {
    width: 100%;
    display: flex;
    flex-direction: row;
    justify-content: center;
    align-items: center;
  }
  .ag-card-content-body {
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items: center;
  }
  .title {
    font-size: 16px;
    font-family: PingFang SC, PingFang SC-Bold;
    font-weight: 700;
    color: #1a1a1a;
    letter-spacing: 1px;
  }
</style>
