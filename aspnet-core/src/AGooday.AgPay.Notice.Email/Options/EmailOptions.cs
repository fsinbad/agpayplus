﻿namespace AGooday.AgPay.Notice.Email
{
    public class EmailOptions
    {
        public const string SectionName = "Mail";

        /// <summary>
        /// 同一消息发送间隔
        /// </summary>
        public int? IntervalSeconds { get; set; }

        /// <summary>
        /// SMTP地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// SMTP端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 发件人
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 发件地址
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// 发件密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 收件箱
        /// </summary>
        public List<string> ToAddress { get; set; }
    }
}
