<template>
  <div>
    <a-card>
      <AgSearchForm
        :searchData="searchData"
        :openIsShowMore="false"
        :isShowMore="isShowMore"
        :btnLoading="btnLoading"
        @update-search-data="handleSearchFormData"
        @set-is-show-more="setIsShowMore"
        @query-func="queryFunc">
        <template slot="formItem">
          <a-form-item label="" class="table-head-layout">
            <a-select v-model="searchData.queryDateType" @change="queryDateTypeChange" placeholder="" default-value="">
              <a-select-option value="day">日报</a-select-option>
              <a-select-option value="month">月报</a-select-option>
              <a-select-option value="year">年报</a-select-option>
            </a-select>
          </a-form-item>
          <a-form-item label="" class="table-head-layout">
            <a-range-picker
              v-model="dateRangeValue"
              @change="onChange"
              @panelChange="onPanelChange"
              style="width: 100%"
              :format="dateFormat"
              :placeholder="[`开始${dateRangeMode==='date'? '日期' : (dateRangeMode==='month' ? '月份' : '年份')}`, `结束${dateRangeMode==='date'? '日期' : (dateRangeMode==='month' ? '月份' : '年份')}`]"
              :mode="[dateRangeMode, dateRangeMode]"
              :disabled-date="disabledDate"
              :open="dateRangeOpen"
              @openChange="dateRangeOpen = !dateRangeOpen"
            >
              <a-icon slot="suffixIcon" type="sync" />
            </a-range-picker>
          </a-form-item>
          <!-- <ag-text-up :placeholder="'商户号'" :msg="searchData.mchNo" v-model="searchData.mchNo" /> -->
          <a-form-item label="" class="table-head-layout">
            <ag-select
              v-model="searchData.mchNo"
              :api="searchMch"
              valueField="mchNo"
              labelField="mchName"
              placeholder="商户号（搜索商户名称）"
            />
          </a-form-item>
          <ag-text-up :placeholder="'代理商号'" :msg="searchData.agentNo" v-model="searchData.agentNo" />
          <ag-text-up :placeholder="'服务商号'" :msg="searchData.isvNo" v-model="searchData.isvNo" />
        </template>
      </AgSearchForm>
      <!-- 列表渲染 -->
      <AgTable
        @btnLoadClose="btnLoading=false"
        ref="infoTable"
        :initData="true"
        :autoRefresh="true"
        :isShowAutoRefresh="false"
        :isShowDownload="true"
        :isEnableDataStatistics="true"
        :reqTableDataFunc="reqTableDataFunc"
        :reqTableCountFunc="reqTableCountFunc"
        :reqDownloadDataFunc="reqDownloadDataFunc"
        :tableColumns="tableColumns"
        :searchData="searchData"
        :countInitData="countInitData"
        rowKey="groupDate"
        :tableRowCrossColor="true"
      >
        <template slot="dataStatisticsSlot" slot-scope="{countData}">
          <div class="data-statistics" style="background: rgb(250, 250, 250);">
            <div class="statistics-list">
              <div class="item">
                <div class="title">总成交金额</div>
                <div class="amount" style="color: rgb(26, 102, 255);">
                  <span class="amount-num">{{ (countData.payAmount).toFixed(2) }}</span>
                </div>
              </div>
              <div class="item">
                <div class="line"></div>
                <div class="title"></div>
              </div>
              <div class="item">
                <div class="title">总成交笔数</div>
                <div class="amount">
                  <span class="amount-num">{{ countData.payCount }}</span>
                </div>
              </div>
              <div class="item">
                <div class="line"></div>
                <div class="title"></div>
              </div>
              <div class="item">
                <div class="title">总退款金额</div>
                <div class="amount">
                  <span class="amount-num">{{ countData.refundAmount.toFixed(2) }}</span>
                </div>
              </div>
              <div class="item">
                <div class="line"></div>
                <div class="title"></div>
              </div>
              <div class="item">
                <div class="title">总退款笔数</div>
                <div class="amount">
                  <span class="amount-num">{{ countData.refundCount }}</span>
                </div>
              </div>
              <div class="item">
                <div class="line"></div>
                <div class="title"></div>
              </div>
              <div class="item">
                <div class="title">支付成功率</div>
                <div class="amount" style="color: rgb(250, 173, 20);">
                  <span class="amount-num">{{ (countData.round*100).toFixed(2) }}%</span>
                </div>
              </div>
            </div>
          </div>
        </template>

        <template slot="payAmountTitle" slot-scope="{record}">
          <div style="display: flex;">
            <span>{{ record }}</span>
            <a-tooltip title="支付成功的订单金额，包含部分退款及全额退款的订单">
              <a-icon class="bi" type="info-circle" style="margin-left: 5px;" />
            </a-tooltip>
          </div>
        </template>
        <template slot="amountTitle" slot-scope="{record}">
          <div style="display: flex;">
            <span>{{ record }}</span>
            <a-tooltip title="扣除手续费后的实际到账金额">
              <a-icon class="bi" type="info-circle" style="margin-left: 5px;" />
            </a-tooltip>
          </div>
        </template>
        <template slot="feeTitle" slot-scope="{record}">
          <div style="display: flex;">
            <span>{{ record }}</span>
            <a-tooltip title="成交订单产生的手续费金额">
              <a-icon class="bi" type="info-circle" style="margin-left: 5px;" />
            </a-tooltip>
          </div>
        </template>
        <template slot="refundFeeTitle" slot-scope="{record}">
          <div style="display: flex;">
            <span>{{ record }}</span>
            <a-tooltip title="退款订单产生的手续费退费金额">
              <a-icon class="bi" type="info-circle" style="margin-left: 5px;" />
            </a-tooltip>
          </div>
        </template>
        <template slot="refundCountTitle" slot-scope="{record}">
          <div style="display: flex;">
            <span>{{ record }}</span>
            <a-tooltip title="实际退款订单笔数，若一笔已成交订单退款多次，则计多次">
              <a-icon class="bi" type="info-circle" style="margin-left: 5px;" />
            </a-tooltip>
          </div>
        </template>
        <template slot="roundTitle" slot-scope="{record}">
          <div style="display: flex;">
            <span>{{ record }}</span>
            <a-tooltip title="成交笔数与总订单笔数除得的百分比">
              <a-icon class="bi" type="info-circle" style="margin-left: 5px;"/>
            </a-tooltip>
          </div>
        </template>

        <template slot="payAmountSlot" slot-scope="{record}"><b style="color: rgb(21, 184, 108)">￥{{ (record.payAmount/100).toFixed(2) }}</b></template> <!-- 自定义插槽 -->
        <template slot="amountSlot" slot-scope="{record}"><b style="color: rgb(21, 184, 108)">￥{{ ((record.payAmount-record.fee)/100).toFixed(2) }}</b></template> <!-- 自定义插槽 -->
        <template slot="feeSlot" slot-scope="{record}"><b style="color: rgb(255, 104, 72)">￥{{ (record.fee/100).toFixed(2) }}</b></template> <!-- 自定义插槽 -->
        <template slot="refundAmountSlot" slot-scope="{record}"><b style="color: rgb(255, 104, 72)">￥{{ (record.refundAmount/100).toFixed(2) }}</b></template> <!-- 自定义插槽 -->
        <template slot="refundFeeSlot" slot-scope="{record}"><b style="color: rgb(21, 184, 108)">￥{{ (record.refundFee/100).toFixed(2) }}</b></template> <!-- 自定义插槽 -->
        <template slot="refundCountSlot" slot-scope="{record}"><b style="color: rgb(255, 104, 72)">{{ record.refundCount }}</b></template> <!-- 自定义插槽 -->
        <template slot="countSlot" slot-scope="{record}"><b style="color: rgb(21, 184, 108)">{{ record.payCount }}/{{ record.allCount }}</b></template> <!-- 自定义插槽 -->
        <template slot="roundSlot" slot-scope="{record}"><b style="color: rgb(255, 136, 0)">{{ (record.round*100).toFixed(2) }}%</b></template> <!-- 自定义插槽 -->
        <template slot="opSlot" slot-scope="{record}">  <!-- 操作列插槽 -->
          <AgTableColumns>
            <a-button type="link" v-if="$access('ENT_STATISTIC_MCH')" @click="detailFunc(record.groupDate)">详情</a-button>
          </AgTableColumns>
        </template>
      </AgTable>
    </a-card>
  </div>
</template>
<script>
import AgTextUp from '@/components/AgTextUp/AgTextUp' // 文字上移组件
import AgSearchForm from '@/components/AgSearch/AgSearchForm'
import AgTable from '@/components/AgTable/AgTable'
import AgSelect from '@/components/AgSelect/AgSelect'
import AgTableColumns from '@/components/AgTable/AgTableColumns'
import { API_URL_ORDER_STATISTIC, API_URL_MCH_LIST, req } from '@/api/manage'
import moment from 'moment'

// eslint-disable-next-line no-unused-vars
const tableColumns = [
  { key: 'groupDate', dataIndex: 'groupDate', title: '日期', width: 120, fixed: 'left' },
  { key: 'payAmount', width: 110, ellipsis: true, scopedSlots: { title: 'payAmountTitle', titleValue: '成交金额', customRender: 'payAmountSlot' } },
  { key: 'amount', width: 110, scopedSlots: { title: 'amountTitle', titleValue: '实收金额', customRender: 'amountSlot' } },
  { key: 'fee', width: 110, scopedSlots: { title: 'feeTitle', titleValue: '手续费', customRender: 'feeSlot' } },
  { key: 'refundAmount', title: '退款金额', width: 110, scopedSlots: { customRender: 'refundAmountSlot' } },
  { key: 'refundFee', width: 125, scopedSlots: { title: 'refundFeeTitle', titleValue: '手续费回退', customRender: 'refundFeeSlot' } },
  { key: 'refundCount', width: 110, scopedSlots: { title: 'refundCountTitle', titleValue: '退款笔数', customRender: 'refundCountSlot' } },
  { key: 'count', title: '成交/总笔数', width: 120, scopedSlots: { customRender: 'countSlot' } },
  { key: 'round', width: 110, scopedSlots: { title: 'roundTitle', titleValue: '成功率', customRender: 'roundSlot' } },
  { key: 'op', title: '操作', width: 120, fixed: 'right', align: 'center', scopedSlots: { customRender: 'opSlot' } }
]

export default {
  name: 'TransactionPage',
  components: { AgSearchForm, AgTable, AgTableColumns, AgSelect, AgTextUp },
  data () {
    // 计算开始时间和结束时间
    const startDate = moment().subtract(1, 'month').startOf('day')
    const endDate = moment().startOf('day').subtract(1, 'days')
    const queryDateRange = `customDateTime_${startDate.format('YYYY-MM-DD')} 00:00:00_${endDate.format('YYYY-MM-DD')} 23:59:59`
    return {
      isShowMore: false,
      btnLoading: false,
      tableColumns: tableColumns,
      searchData: {
        method: 'transaction',
        queryDateType: 'day',
        queryDateRange: queryDateRange
      },
      countInitData: {
        allAmount: 0.00,
        allCount: 0,
        payAmount: 0.00,
        payCount: 0,
        fee: 0.00,
        refundAmount: 0.00,
        refundCount: 0,
        refundFeeAmount: 0.00,
        round: 0.00
      },
      dateFormat: 'YYYY-MM-DD',
      dateRangeMode: 'date',
      dateRangeOpen: false,
      dateRangeValue: [startDate, endDate]
    }
  },
  computed: {
  },
  mounted () {
  },
  methods: {
    searchMch (params) {
      return req.list(API_URL_MCH_LIST, params)
    },
    handleSearchFormData (searchData) {
      this.searchData = searchData
      // if (!Object.keys(searchData).length) {
      //   this.searchData.queryDateRange = 'today'
      // }
      // this.$forceUpdate()
    },
    setIsShowMore (isShowMore) {
      this.isShowMore = isShowMore
    },
    queryFunc () {
      this.btnLoading = true
      this.$refs.infoTable.refTable(true)
    },
    // 请求table接口数据
    reqTableDataFunc: (params) => {
      return req.list(API_URL_ORDER_STATISTIC, params)
    },
    reqTableCountFunc: (params) => {
      return req.total(API_URL_ORDER_STATISTIC, params)
    },
    reqDownloadDataFunc: (params) => {
      req.export(API_URL_ORDER_STATISTIC, 'excel', params).then(res => {
        // 将响应体中的二进制数据转换为Blob对象
        const blob = new Blob([res])
        const fileName = '交易报表.xlsx' // 要保存的文件名称
        if ('download' in document.createElement('a')) {
          // 非IE下载
          // 创建一个a标签，设置download属性和href属性，并触发click事件下载文件
          const elink = document.createElement('a')
          elink.download = fileName
          elink.style.display = 'none'
          elink.href = URL.createObjectURL(blob) // 创建URL.createObjectURL(blob) URL，并将其赋值给a标签的href属性
          document.body.appendChild(elink)
          elink.click()
          URL.revokeObjectURL(elink.href) // 释放URL 对象
          document.body.removeChild(elink)
        } else {
          // IE10+下载
          navigator.msSaveBlob(blob, fileName)
        }
      }).catch((error) => {
        console.error(error)
      })
    },
    searchFunc: function () { // 点击【查询】按钮点击事件
      this.$refs.infoTable.refTable(true)
    },
    detailFunc: function (groupDate) {
      const startDate = moment(groupDate, 'YYYY-MM-DD').startOf(this.searchData.queryDateType)
      const endDate = moment(groupDate, 'YYYY-MM-DD').endOf(this.searchData.queryDateType)
      // 获取开始时间和结束时间的时间戳
      const startTimestamp = startDate.valueOf() // 或者使用 startDate.unix() // 获取秒级时间戳
      const endTimestamp = endDate.valueOf() // 或者使用 endDate.unix() // 获取秒级时间戳
      this.$router.push({
        path: '/statistic/mch',
        query: { queryDate: `${startTimestamp}_${endTimestamp}` }
      })
    },
    moment,
    queryDateTypeChange (value) {
      this.queryDateType = value
      let startDate = moment().subtract(1, 'year').startOf('month')
      let endDate = moment().startOf('day').subtract(1, 'days')
      switch (value) {
        case 'day':
          this.dateFormat = 'YYYY-MM-DD'
          this.dateRangeMode = 'date'
          startDate = moment().subtract(1, 'month')
          break
        case 'month':
          this.dateFormat = 'YYYY-MM'
          this.dateRangeMode = 'month'
          break
        case 'year':
          this.dateFormat = 'YYYY'
          this.dateRangeMode = 'year'
          break
      }
      startDate = startDate.startOf(this.queryDateType)
      endDate = endDate.endOf(this.queryDateType)
      this.searchData.queryDateRange = `customDateTime_${startDate.format('YYYY-MM-DD')} 00:00:00_${endDate.format('YYYY-MM-DD')} 23:59:59`
      this.dateRangeValue = [startDate, endDate]
    },
    onPanelChange (value, mode) {
      this.dateRangeValue = value
      const startDate = value[0].startOf(this.queryDateType)
      const endDate = value[1].endOf(this.queryDateType)
      this.searchData.queryDateRange = `customDateTime_${startDate.format('YYYY-MM-DD')} 00:00:00_${endDate.format('YYYY-MM-DD')} 23:59:59`
      if (mode[1] === 'date' || !mode[1]) {
        this.dateRangeOpen = false
      }
    },
    onChange (date, dateString) {
      const startDate = dateString[0] // 开始时间
      const endDate = dateString[1] // 结束时间
      const start = moment(startDate)
      const end = moment(endDate)
      this.dateRangeValue = !startDate || !endDate ? dateString : [start, end]
      this.searchData.queryDateRange = !startDate || !endDate ? '' : `customDateTime_${start.format('YYYY-MM-DD')} 00:00:00_${end.format('YYYY-MM-DD')} 23:59:59`
    },
    disabledDate (current) { // 今日之后日期不可选
      return current && current > moment().endOf('day')
    }
  }
}
</script>
<style lang="less" scoped>
.order-list {
  -webkit-text-size-adjust:none;
  font-size: 12px;
  display: flex;
  flex-direction: column;

  p {
    white-space:nowrap;
    span {
      display: inline-block;
      font-weight: 800;
      height: 16px;
      line-height: 16px;
      width: 35px;
      border-radius: 5px;
      text-align: center;
      margin-right: 2px;
    }
  }
}

.modal-title,.modal-describe {
  text-align: center;
  margin-bottom: 15px
}

.modal-title {
  margin-bottom: 20px;
  text-align: center;
  font-size: 18px;
  font-weight: 600
}

.close {
  position: absolute;
  left: 0;
  bottom: 0;
  width: 100%;
  border-top: 1px solid #EFEFEF;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 10px 0
}

.icon-style {
  border-radius: 5px;
  padding-left: 2px;
  padding-right: 2px
}

.icon {
  width: 15px;
  height: 14px;
  margin-bottom: 3px
}

.data-statistics {
  margin: 0 30px 10px;
  padding: 28px 0 32px;
  border-radius: 3px;
  border: 1px solid #ebebeb;
  transform: translateY(-10px)
}

.statistics-list {
  display: flex;
  flex-direction: row;
  justify-content: space-around
}

.statistics-list .item .title {
  color: gray;
  margin-bottom: 10px
}

.statistics-list .item .amount {
  margin-bottom: 10px;
  max-width: 150px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.statistics-list .item .amount .amount-num {
  padding-right: 3px;
  font-weight: 600;
  font-size: 20px
}

.statistics-list .item .symbol {
  padding-right: 3px
}

.statistics-list .item .detail-text {
  color: rgb(26, 102, 255);
  padding-left: 5px;
  cursor: pointer
}

.statistics-list .line {
  width: 1px;
  height: 100%;
  border-right: 1px solid #efefef
}
</style>
