﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGooday.AgPay.Domain.Models
{
    /// <summary>
    /// 商户应用表
    /// </summary>
    [Table("t_mch_app")]
    [Comment("商户应用表")]
    public class MchApp
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        [Comment("应用ID")]
        [Key, Required, Column("app_id", TypeName = "varchar(64)")]
        public string AppId { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        [Comment("应用名称")]
        [Required, Column("app_name", TypeName = "varchar(64)")]
        public string AppName { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [Comment("商户号")]
        [Required, Column("mch_no", TypeName = "varchar(64)")]
        public string MchNo { get; set; }

        /// <summary>
        /// 应用状态: 0-停用, 1-正常
        /// </summary>
        [Comment("应用状态: 0-停用, 1-正常")]
        [Required, Column("state", TypeName = "tinyint(6)")]
        public byte State { get; set; }

        /// <summary>
        /// 应用私钥
        /// </summary>
        [Comment("应用私钥")]
        [Required, Column("app_secret", TypeName = "varchar(128)")]
        public string AppSecret { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Comment("备注")]
        [Required, Column("remark", TypeName = "varchar(128)")]
        public string Remark { get; set; }

        /// <summary>
        /// 创建者用户ID
        /// </summary>
        [Comment("创建者用户ID")]
        [Column("created_uid", TypeName = "bigint")]
        public long? CreatedUid { get; set; }

        /// <summary>
        /// 创建者姓名
        /// </summary>
        [Comment("创建者姓名")]
        [Column("created_by", TypeName = "varchar(64)")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Comment("创建时间")]
        [Required, Column("created_at", TypeName = "timestamp(6)")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Comment("更新时间")]
        [Required, Column("updated_at", TypeName = "timestamp(6)")]
        public DateTime UpdatedAt { get; set; }
    }
}