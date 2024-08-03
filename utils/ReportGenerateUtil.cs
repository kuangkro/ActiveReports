using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.PageReportModel;
using JSViewer_MVC_Core;
using JSViewer_MVC_Core.model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace xp.viewer.web.utils
{
    public class ReportGenerateUtil
    {
        /// <summary>
        /// 
        /// </summary>
        List<object> _DataList = new List<object>();
        // <summary>
        /// 数据集
        /// </summary>
        static List<DataSet_Model> setlist = new List<DataSet_Model>();

        /// <summary>
        /// 数据库连接方式
        /// </summary>
        string datasourceType = "OLEDB";//LEDB，SQL

        public ReportGenerateUtil()
        {
               
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="ParametersList"></param>
        /// <param name="fplb"></param>
        /// <param name="IsPreview">是否预览</param>
        /// <returns></returns>
        public PageReport GetPageReport(List<object> DataList, List<string> ParametersList, Fplb fplb)
        {
            _DataList = DataList;
            setlist.Clear();
            PageReport pageReport = null;

            #region 获取报表文件
            var report_file_result = GetReportFile(fplb);
            if (!report_file_result.Flag)
            {
                throw new Exception($"未查到报表信息：{report_file_result.Msg}");
            }
            else pageReport = report_file_result.Data;

            #endregion

            #region 默认参数赋值
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "images" + @"\";
            for (int i = 0; i < pageReport.Report.ReportParameters.Count; i++)
            {
                #region 默认参数值
                if (pageReport.Report.ReportParameters[i].Name.ToString() == "医院名称" || pageReport.Report.ReportParameters[i].Name.ToString() == "单位名称")
                {
                    if (pageReport.Report.ReportParameters[i].DefaultValue?.Values.Count > 0)
                        pageReport.Report.ReportParameters[i].DefaultValue.Values[0] = LoginInfo.Qjbl.gs_dwmc;
                    else
                        pageReport.Report.ReportParameters[i].DefaultValue.Values.Add(LoginInfo.Qjbl.gs_dwmc);
                }
                else if (pageReport.Report.ReportParameters[i].Name.ToString() == "账套号" || pageReport.Report.ReportParameters[i].Name.ToString() == "帐套号")
                {
                    if (pageReport.Report.ReportParameters[i].DefaultValue?.Values.Count > 0)
                        pageReport.Report.ReportParameters[i].DefaultValue.Values[0] = LoginInfo.Instance.XPSM_CUD_ZTH;
                    else
                        pageReport.Report.ReportParameters[i].DefaultValue.Values.Add(LoginInfo.Instance.XPSM_CUD_ZTH);
                }
                else if (pageReport.Report.ReportParameters[i].Name.ToString() == "人员名称")//默认人员
                {
                    if (pageReport.Report.ReportParameters[i].DefaultValue?.Values.Count > 0)
                        pageReport.Report.ReportParameters[i].DefaultValue.Values[0] = LoginInfo.Instance.Rybm;
                    else
                        pageReport.Report.ReportParameters[i].DefaultValue.Values.Add(LoginInfo.Instance.Rybm);
                }
                else if (pageReport.Report.ReportParameters[i].Name.ToString() == "部门名称")//默认科室
                {
                    if (pageReport.Report.ReportParameters[i].DefaultValue?.Values.Count > 0)
                        pageReport.Report.ReportParameters[i].DefaultValue.Values[0] = LoginInfo.Instance.bmbm;
                    else
                        pageReport.Report.ReportParameters[i].DefaultValue.Values.Add(LoginInfo.Instance.bmbm);
                }
                else if (pageReport.Report.ReportParameters[i].Name.ToString() == "图片路径")
                {
                    if (pageReport.Report.ReportParameters[i].DefaultValue?.Values.Count > 0)
                        pageReport.Report.ReportParameters[i].DefaultValue.Values[0] = path;
                    else
                        pageReport.Report.ReportParameters[i].DefaultValue.Values.Add(path);
                }
                #endregion
                #region 传入参数
                try
                {
                    for (int k = 0; k < ParametersList.Count; k++)
                    {
                        if (k + 1 <= pageReport.Report.ReportParameters.Count)
                            pageReport.Report.ReportParameters[k].DefaultValue.Values.Add(ParametersList[k]);
                    }

                }
                catch (Exception ee)
                {
                    throw new Exception($"内置报表参数个数{pageReport.Report.ReportParameters.Count}和软件传传入参数个数{ParametersList.Count}不匹配！请核对报表文件！" + ee.Message);
                }
                #endregion
            }
            #endregion

            #region 提取报表文件的数据集收起来语句及数据集参数
            setlist.Clear();
            try
            {
                foreach (var item in pageReport.Report.DataSets)
                {
                    DataSet_Model model = new DataSet_Model();
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic.Clear();
                    model.name = item.Name;
                    model.CommandText = item.Query.CommandText;
                    for (int i = 0; i < item.Query.QueryParameters.Count; i++)
                    {
                        if (item.Query.QueryParameters[i].Name.Contains("@"))
                            dic.Add(item.Query.QueryParameters[i].Name, item.Query.QueryParameters[i].Value.ToString().Replace("=", "").Replace("Parameters!", "").Replace(".Value", "").Replace("\"%\"", "'%'").Replace("“%”", "'%'"));
                        else
                            dic.Add("@" + item.Query.QueryParameters[i].Name, item.Query.QueryParameters[i].Value.ToString().Replace("=", "").Replace("Parameters!", "").Replace(".Value", "").Replace("\"%\"", "'%'").Replace("“%”", "'%'"));
                    }
                    model.Parameters_Model = dic;
                    setlist.Add(model);

                    item.Query.QueryParameters.Clear();
                }
            }
            catch (Exception ee)
            {
                throw ee;
            }
            #endregion

            try
            {
                #region 设计时转运行时
                ///更换数据类型及删除sql语句
                datasourceType = pageReport.Report.DataSources[0].ConnectionProperties.DataProvider;
                pageReport.Report.DataSources[0].ConnectionProperties.DataProvider = "DATASET";//更换数据类型为dataset
                pageReport.Report.DataSources[0].ConnectionProperties.ConnectString = null;
                pageReport.Report.DataSources[0].Transaction = false;//是否使用同一个数据库事务
                #endregion
                return pageReport;
            }
            catch (Exception ee)
            {
                throw ee;
            }
        }

        /// <summary>
        /// 获取报表文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static ProccessResult<PageReport> GetReportFile(Fplb fplb)
        {
            PageReport pageReport = null;
            var rep = WCFClient.ClientBb.Excute(a => a.GetReportsFile_model(fplb));
            if (rep == null || rep.Code < 0)
            {
                return ProccessResult.OnFailed(pageReport, "获取报表文件失败！请核对是否已导入相应报表文件！");
            }

            #region 获取报表文件
            if (rep.ReContent == null)
            {
                return ProccessResult.OnFailed(pageReport, "没有查询到相应的报表文件；" + rep.CodeMsg);
            }
            byte[] filebyte = rep.ReContent.word_wj;
            if (filebyte == null)
            {
                return ProccessResult.OnFailed(pageReport, "获取报表文件失败");
            }
            if (filebyte != null || filebyte.LongLength > 0)
            {
                try
                {
                    MemoryStream ms = new MemoryStream(filebyte);
                    TextReader textred = new StreamReader(ms);
                    pageReport = new PageReport(textred);
                }
                catch (Exception e)
                {
                    return ProccessResult.OnFailed(pageReport, "报表文件格式化错误！" + e);
                }
            }
            else
            {
                return ProccessResult.OnFailed(pageReport, "获取报表文件失败！");
            }
            #endregion

            #region logo图片展示及水印
            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            #region 页眉
            if (pageReport.Report.PageHeader != null)
            {
                for (int i = 0; i < pageReport.Report.PageHeader?.ReportItems.Count; i++)
                {
                    if (pageReport.Report.PageHeader.ReportItems[i].Name.Contains("xp_logo"))
                    {
                        var image = pageReport.Report.PageHeader.ReportItems[i] as Image;
                        image.MIMEType = "image/png";
                        image.Source = GrapeCity.ActiveReports.Extensibility.Rendering.Components.ImageSource.External;
                        image.Value = path + $@"\images\xp_logo.png";
                        pageReport.Report.PageHeader.ReportItems[i] = image;
                    }

                }
                #region 水印
                if (pageReport.Report.PageHeader?.Style.BackgroundImage.Value.Expression != "Inherit")
                    pageReport.Report.PageHeader.Style.BackgroundImage.Value = path + $@"\images\xp_sy.png";
                #endregion
            }


            #endregion
            #region 内容
            if (pageReport.Report.Body != null)
            {
                for (int i = 0; i < pageReport.Report.Body?.ReportItems.Count; i++)
                {
                    if (pageReport.Report.Body.ReportItems[i].Name.Contains("xp_logo"))
                    {
                        var image = pageReport.Report.Body.ReportItems[i] as Image;
                        image.MIMEType = "image/png";
                        image.Source = GrapeCity.ActiveReports.Extensibility.Rendering.Components.ImageSource.External;
                        image.Value = path + $@"\images\xp_logo.png";
                        pageReport.Report.Body.ReportItems[i] = image;
                    }

                }
                #region 水印
                if (pageReport.Report.Body.Style?.BackgroundImage.Value.Expression != "Inherit")
                    pageReport.Report.Body.Style.BackgroundImage.Value = path + $@"\images\xp_sy.png";
                #endregion
            }

            #endregion
            #region 页脚
            if (pageReport.Report.PageFooter != null)
            {
                for (int i = 0; i < pageReport.Report.PageFooter?.ReportItems.Count; i++)
                {
                    if (pageReport.Report.PageFooter.ReportItems[i].Name.Contains("xp_logo"))
                    {
                        var image = pageReport.Report.PageFooter.ReportItems[i] as Image;
                        image.MIMEType = "image/png";
                        image.Source = GrapeCity.ActiveReports.Extensibility.Rendering.Components.ImageSource.External;
                        image.Value = path + $@"\images\xp_logo.png";
                        pageReport.Report.PageFooter.ReportItems[i] = image;
                    }
                }
                #region 水印
                if (pageReport.Report.PageFooter?.Style.BackgroundImage.Value.Expression != "Inherit")
                    pageReport.Report.PageFooter.Style.BackgroundImage.Value = path + $@"\images\xp_sy.png";
                #endregion
            }
            #endregion
            #endregion
            return ProccessResult.OnOk(pageReport);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public DataTable locatedata_comm(LocateDataSourceEventArgs args)
        {
            /// <summary>
            /// 报表参数
            /// </summary>
            Dictionary<string, object> dddlist = new Dictionary<string, object>();
            ///单个数据集参数
            List<Tuple<string, object>> list = new List<Tuple<string, object>>();
            for (int i = 0; i < args.Parameters.Count; i++)
            {
                dddlist.Add(args.Parameters[i].Name, args.Parameters[i].Value);
            }

            for (int i = 0; i < setlist.Count; i++)
            {
                ///第几个（0开始）
                int index = setlist.Where(o => o.name.Contains("DataSet")).ToList().FindIndex(o => o.name == args.DataSet.Name);

                if (setlist[i].name.Contains("DataSet") && index < _DataList.Count())
                {
                    if (args.DataSet.Name == setlist[i].name)
                        args.Data = _DataList[index];
                }
                else
                {
                    if (args.DataSet.Name == setlist[i].name)
                    {
                        string sql = setlist[i].CommandText;//sql语句
                        if (datasourceType == "SQL" || datasourceType == "DATASET")
                        {
                            #region SQL连接方式，参数转换
                            foreach (var item2 in setlist[i].Parameters_Model)
                            {
                                if (item2.Value.ToString().Trim() == "账套号" || item2.Value.ToString().Trim() == "帐套号" || item2.Value.ToString().Trim() == "xpsm_zth")
                                    sql = sql.Replace(item2.Key.ToString(), "1");
                                else
                                {
                                    var value = dddlist.Where(o => item2.Value.ToString() == o.Key);
                                    list.Add(new Tuple<string, object>(item2.Key, value.FirstOrDefault().Value?.ToString()));
                                    if (value.FirstOrDefault().Value is IEnumerable<string>)
                                    {
                                        var result = "'" + String.Join("','", (value.FirstOrDefault().Value as List<string>).ToArray()) + "'";
                                        sql = sql.Replace(item2.Key.ToString(), result);
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (datasourceType == "OLEDB")
                        {
                            #region OLEDB连接方式，参数转换
                            var Parameters_Model_list = setlist[i].Parameters_Model.ToList();
                            int zthsl = 0;
                            for (int k = 0; k < Parameters_Model_list.Count; k++)
                            {
                                if (Parameters_Model_list[k].Value.ToString() == "账套号" || Parameters_Model_list[k].Value.ToString() == "帐套号" || Parameters_Model_list[k].Value.ToString() == "xpsm_zth")
                                {
                                    sql = replace(sql, "?", "zth", k - zthsl);
                                    zthsl++;
                                }
                                else
                                {
                                    var value = dddlist.Where(o => Parameters_Model_list[k].Value.ToString() == o.Key);
                                    if (value.FirstOrDefault().Value is IEnumerable<string>)
                                    {
                                        var result = "'" + String.Join("','", (value.FirstOrDefault().Value as List<string>).ToArray()) + "'";
                                        sql = replace(sql, "?", result, k - zthsl);
                                        zthsl++;
                                    }
                                    else
                                        list.Add(new Tuple<string, object>(Parameters_Model_list[k].Key, value.FirstOrDefault().Value == null ? "" : value.FirstOrDefault().Value.ToString()));
                                }
                            }
                            #endregion
                        }

                        return returndata(sql, list);
                    }
                }
            }
        }


        /// <summary>
        /// 获取datatable
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static object returndata(string sql, List<Tuple<string, object>> list)
        {
            DataTable dt = new DataTable();

            var rep = WCFClient.ClientBb.Excute(a => a.GetDataTable(sql, list));
            if (rep.Code > 0)
                dt = rep.ReContent;
            return dt;
        }

        /// <summary>
        /// 替换字符串中指定位置的字符
        /// </summary>
        /// <param name="sql">原字符串</param>
        /// <param name="B">需要被替换的字符串</param>
        /// <param name="T">新的字符</param>
        /// <param name="D">替换第几个出现的</param>
        /// <returns></returns>
        private string replace(string sql, string B, string T, int D)
        {
            string[] array;
            array = sql.Split('?');
            string str = null;
            for (int j = 0; j < array.Length; j++)
            {
                if (j == D + 1)
                {
                    str += T + array[j];
                    if (j != array.Length - 1)
                        str += B;
                }
                else
                {
                    if (j == D || j == array.Length - 1)
                        str += array[j];
                    else
                        str += array[j] + B;
                }
            }
            sql = str;
            return sql;
        }
    }
}