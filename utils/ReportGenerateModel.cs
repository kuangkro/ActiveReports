using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JSViewer_MVC_Core
{
    /// <summary>
    /// 报表文件数据集及数据集参数
    /// </summary>
    public class DataSet_Model
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// sql语句
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public Dictionary<string, object> Parameters_Model { get; set; }
    }

    public class Key_Value
    {
        public string key { get; set; }
        public decimal vale { get; set; }
    }
    /// <summary>
    /// 诊间支付入参
    /// </summary>
    public class Param_model
    {
        /// <summary>
        /// 挂号单号
        /// </summary>
        public string ghdh { get; set; }
        /// <summary>
        /// 医嘱序号
        /// </summary>
        public List<string> yzxh { get; set; }
        /// <summary>
        /// 身份证号码
        /// </summary>
        public string sfzh { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string name { get; set; }
    }
}