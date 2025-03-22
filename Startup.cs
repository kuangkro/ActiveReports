using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using GrapeCity.ActiveReports.Aspnetcore.Viewer;
using GrapeCity.ActiveReports.Web.Viewer;
using GrapeCity.ActiveReports;
using System.Data;
using System.Linq;
using GrapeCity.ActiveReports.Document;
using System.Threading;

namespace JSViewer_MVCCore
{
    public class Startup
    {
        private static readonly string CurrentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? String.Empty;
        public static readonly DirectoryInfo ReportsDirectory = new DirectoryInfo(Path.Combine(CurrentDir, "Reports"));

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services
                .AddLogging(config =>
                {
                    // Disable the default logging configuration
                    config.ClearProviders();

                    // Enable logging for debug mode only
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
                    {
                        config.AddConsole();
                    }
                })
                .AddReportViewer()
                .AddMvc(options => options.EnableEndpointRouting = false);
        }

        private static ThreadLocal<string> values = new ThreadLocal<string>();

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseFileServer();

            app.UseReporting(settings =>
            {
                settings.UseFileStore(ReportsDirectory);
                settings.UseCustomStore(fileName =>
                {
                    var bts = File.ReadAllBytes(Path.Combine(CurrentDir, "Reports/" + fileName));
                    MemoryStream ms = new MemoryStream(bts);
                    TextReader textred = new StreamReader(ms);
                    var pageReport = new PageReport(textred);
                    var ds = pageReport.Report.DataSources.First();
                    pageReport.Report.DataSources.ToList().ForEach(item =>
                    {
                        item.ConnectionProperties.DataProvider = "DATASET";
                        string sql = item.ConnectionProperties.ConnectString;
                        item.ConnectionProperties.ConnectString = null;
                    });

                    pageReport.Report.ReportParameters.Add(new GrapeCity.ActiveReports.PageReportModel.ReportParameter()
                    {
                        Name = "out_parameter",
                        Prompt = "内置参数",
                        DefaultValue = new GrapeCity.ActiveReports.PageReportModel.DefaultValue() { Values = { "1" } }
                    });
                    pageReport.Report.Name = fileName + " -- test";

                    PageDocument document = new PageDocument(pageReport);
                    document.LocateDataSource += Document_LocateDataSource;
                    return pageReport;
                });

                #region 该方式加载datatable，查询中涉及到使用dataset会不支持
                //获取前端传参并绑定报表
                settings.SetLocateDataSource(args =>
                {
                    Console.WriteLine("SetLocateDataSource:" + args.Report.Name);

                    var parameters = args.ReportParameters;
                    DataTable dt = new DataTable();
                    if (args.DataSet.Query.DataSourceName == "DataSource1" && args.Report.Name.Contains("报表"))
                    {
                        if (args.DataSet.Name == "DataSet1")
                        {
                            dt = buildData();
                        }
                    }
                    //todo 获取前端回传的参数信息
                    return dt;
                });
                #endregion
            });

            app.UseMvc();
        }

        /// <summary>
        /// 参数中涉及到数据库加载会优先执行该方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Document_LocateDataSource(object sender, LocateDataSourceEventArgs args)
        {
            Console.WriteLine("Document_LocateDataSource:" + args.Report.Name);
            DataTable dt = new DataTable();

            dt.Columns.Add("BH");
            dt.Columns.Add("MC");
            var dr = dt.NewRow();
            dr["BH"] = "01";
            dr["MC"] = "filed - MC";
            dt.Rows.Add(dr);

            args.Data = dt;
        }


        private DataTable buildData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("产品编号");
            dt.Columns.Add("产品名称");
            dt.Columns.Add("单价");
            dt.Columns.Add("库存量");
            dt.Columns.Add("产地");
            dt.Rows.Add("A001", "苹果", 10, 300, "中国");
            dt.Rows.Add("A002", "葡萄", 20, 200, "中国");
            dt.Rows.Add("A003", "香蕉", 30, 400, "中国");

            dt.Rows.Add("A004", "甘蔗", 10, 300, "中国");

            dt.Rows.Add("A005", "荔枝", 20, 200, "中国");

            dt.Rows.Add("A006", "芒果", 30, 400, "中国");

            dt.Rows.Add("A007", "猕猴桃", 110, 300, "中国");

            dt.Rows.Add("A008", "柠檬", 210, 200, "中国");

            dt.Rows.Add("A009", "栗子", 320, 400, "中国");

            dt.Rows.Add("A010", "火龙果", 100, 300, "中国");

            dt.Rows.Add("A011", "青芒", 250, 200, "中国");

            dt.Rows.Add("A012", "巴旦木", 320, 200, "中国");

            dt.Rows.Add("A013", "土豆", 380, 400, "中国");

            dt.Rows.Add("A014", "苹果", 110, 300, "中国");

            dt.Rows.Add("A015", "葡萄", 420, 200, "中国");

            dt.Rows.Add("A016", "香蕉", 530, 400, "中国");

            dt.Rows.Add("A017", "土豆", 380, 400, "中国");

            dt.Rows.Add("A018", "苹果", 110, 300, "中国");

            dt.Rows.Add("A019", "葡萄", 420, 200, "中国");

            dt.Rows.Add("A020", "香蕉", 530, 400, "中国");

            dt.Rows.Add("A021", "苹果", 10, 300, "中国");

            dt.Rows.Add("A022", "葡萄", 20, 200, "中国");

            dt.Rows.Add("A023", "香蕉", 30, 400, "中国");

            dt.Rows.Add("A024", "甘蔗", 10, 300, "中国");

            dt.Rows.Add("A025", "荔枝", 20, 200, "中国");

            dt.Rows.Add("A026", "芒果", 30, 400, "中国");

            return dt;

        }
    }
}
