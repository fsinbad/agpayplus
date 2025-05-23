/*
 * 登录用户
 *
 */
import { defineStore } from 'pinia';
import { localClear, localRead, localSave } from '/@/utils/local-util';
import LocalStorageKeyConst from '/@/constants/local-storage-key-const';

export const useUserStore = defineStore('userStore', {
  state: () => ({
    token: '',
    userName: '', // 真实姓名
    safeWord: '', // 预留信息
    userId: '', // 用户ID
    avatarImgPath: '', // 头像
    allMenuRouteTree: [], // 全部动态 router
    accessList: [], // 用户权限集合
    isAdmin: '', // 是否是超级管理员
    loginUsername: '', // 登录用户名
    state: '', // 用户状态
    sysType: '', // 所属系统
    telphone: '', // 手机号
    sex: '' // 性别
  }),
  getters: {
    getToken(state) {
      console.log('getToken - Current state:', state);
      console.log('getToken - Current token:', state.token);
      if (state.token) {
        return state.token;
      }
      const localToken = localRead(LocalStorageKeyConst.USER_TOKEN);
      console.log('getToken - Local token:', localToken);
      return localToken || ''; // 确保返回一个默认值
    },
  },

  actions: {
    logout() {
      return new Promise((resolve) => {
        this.token = '';
        this.allMenuRouteTree = [];
        localClear();
        resolve(); // 确保返回一个 Promise
      });
    },
    //设置登录信息
    setUserLoginInfo(data) {
      this.userId = data.sysUserId // 用户ID
      this.userName = data.realname // 真实姓名
      this.safeWord = data.safeWord // 预留信息
      this.avatarImgPath = data.avatarUrl // 头像
      this.accessList = data.entIdList // 权限集合
      this.allMenuRouteTree = data.allMenuRouteTree // 全部路由集合
      this.isAdmin = data.isAdmin // 是否是超级管理员
      this.loginUsername = data.loginUsername // 登录用户名
      this.state = data.state // 用户状态
      this.sysType = data.sysType // 所属系统
      this.telphone = data.telphone // 手机号
      this.sex = data.sex // 性别
    },
    setToken(token, isSaveLocal) {
      console.log('setToken - New token:', token);
      this.token = token;
      console.log('setToken - Updated state.token:', this.token);
      if (isSaveLocal) {
        localSave(LocalStorageKeyConst.USER_TOKEN, token || '');
        console.log('setToken - Token saved to local storage');
      }
    },
  },
});
