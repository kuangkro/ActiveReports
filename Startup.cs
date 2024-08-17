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
                //setting.UseCustomStore(args =>
                //{
                //    Console.WriteLine($"UseReporting UseCustomStore:{args}");
                //    return args;
                //});
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
                    PageDocument document = new PageDocument(pageReport);
                    document.LocateDataSource += Document_LocateDataSource;
                    return pageReport;
                });
                #region 该方式加载datatable，查询中涉及到使用dataset会不支持
                //settings.LocateDataSource = args =>
                //{
                //    DataTable dt = new DataTable();
                //    if (args.DataSet.Query.DataSourceName == "DataSource1" && args.Report.Name.Contains("报表"))
                //    {
                //        if (args.DataSet.Name == "DataSet1")
                //        {
                //            dt.Columns.Add("产品编号");
                //            dt.Columns.Add("产品名称");
                //            dt.Columns.Add("单价");
                //            dt.Columns.Add("库存量");
                //            dt.Columns.Add("产地");
                //            dt.Rows.Add("A001", "苹果", 10, 300, "中国");
                //            dt.Rows.Add("A002", "葡萄", 20, 200, "中国");
                //            dt.Rows.Add("A003", "香蕉", 30, 400, "中国");

                //            dt.Rows.Add("A004", "甘蔗", 10, 300, "中国");

                //            dt.Rows.Add("A005", "荔枝", 20, 200, "中国");

                //            dt.Rows.Add("A006", "芒果", 30, 400, "中国");

                //            dt.Rows.Add("A007", "猕猴桃", 110, 300, "中国");

                //            dt.Rows.Add("A008", "柠檬", 210, 200, "中国");

                //            dt.Rows.Add("A009", "栗子", 320, 400, "中国");

                //            dt.Rows.Add("A010", "火龙果", 100, 300, "中国");

                //            dt.Rows.Add("A011", "青芒", 250, 200, "中国");

                //            dt.Rows.Add("A012", "巴旦木", 320, 200, "中国");

                //            dt.Rows.Add("A013", "土豆", 380, 400, "中国");

                //            dt.Rows.Add("A014", "苹果", 110, 300, "中国");

                //            dt.Rows.Add("A015", "葡萄", 420, 200, "中国");

                //            dt.Rows.Add("A016", "香蕉", 530, 400, "中国");

                //            dt.Rows.Add("A017", "土豆", 380, 400, "中国");

                //            dt.Rows.Add("A018", "苹果", 110, 300, "中国");

                //            dt.Rows.Add("A019", "葡萄", 420, 200, "中国");

                //            dt.Rows.Add("A020", "香蕉", 530, 400, "中国");

                //            dt.Rows.Add("A021", "苹果", 10, 300, "中国");

                //            dt.Rows.Add("A022", "葡萄", 20, 200, "中国");

                //            dt.Rows.Add("A023", "香蕉", 30, 400, "中国");

                //            dt.Rows.Add("A024", "甘蔗", 10, 300, "中国");

                //            dt.Rows.Add("A025", "荔枝", 20, 200, "中国");

                //            dt.Rows.Add("A026", "芒果", 30, 400, "中国");

                //        }

                //    }

                //    return dt;
                //};
                #endregion
            });

            app.UseReportViewer(settings =>
            {
                //https://gcdn.grapecity.com.cn/forum.php?mod=viewthread&tid=54413&extra=page%3D1
                settings.UseFileStore(ReportsDirectory);
                settings.UseReportProvider(reportid =>
                {
                    Console.WriteLine($"UseReportViewer UseReportProvider Action01:{reportid}");
                    return null;
                }, reportid =>
                {
                    Console.WriteLine($"UseReportViewer UseReportProvider Action02:{reportid}");
                    return null;
                });
                settings.LocateDataSource = args =>
                {
                    var dt = args.DataSet;
                    Console.WriteLine($"LocateDataSource Action:{args}");
                    return null;
                };
            });

            app.UseMvc();
        }

        private void Document_LocateDataSource(object sender, LocateDataSourceEventArgs args)
        {
            DataTable dt = new DataTable();
            if (args.DataSet.Query.DataSourceName == "DataSource1" && args.Report.Name.Contains("报表"))
            {
                if (args.DataSet.Name == "DataSet1")
                {
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

                }
            }
            args.Data = dt;
        }
    }
}
