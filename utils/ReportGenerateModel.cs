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

        /// <summary>
        /// 对应数据集
        /// </summary>
        public object ReportData { get; set; }
    }
}