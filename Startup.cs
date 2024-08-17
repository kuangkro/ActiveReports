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
                #region �÷�ʽ����datatable����ѯ���漰��ʹ��dataset�᲻֧��
                //settings.LocateDataSource = args =>
                //{
                //    DataTable dt = new DataTable();
                //    if (args.DataSet.Query.DataSourceName == "DataSource1" && args.Report.Name.Contains("����"))
                //    {
                //        if (args.DataSet.Name == "DataSet1")
                //        {
                //            dt.Columns.Add("��Ʒ���");
                //            dt.Columns.Add("��Ʒ����");
                //            dt.Columns.Add("����");
                //            dt.Columns.Add("�����");
                //            dt.Columns.Add("����");
                //            dt.Rows.Add("A001", "ƻ��", 10, 300, "�й�");
                //            dt.Rows.Add("A002", "����", 20, 200, "�й�");
                //            dt.Rows.Add("A003", "�㽶", 30, 400, "�й�");

                //            dt.Rows.Add("A004", "����", 10, 300, "�й�");

                //            dt.Rows.Add("A005", "��֦", 20, 200, "�й�");

                //            dt.Rows.Add("A006", "â��", 30, 400, "�й�");

                //            dt.Rows.Add("A007", "⨺���", 110, 300, "�й�");

                //            dt.Rows.Add("A008", "����", 210, 200, "�й�");

                //            dt.Rows.Add("A009", "����", 320, 400, "�й�");

                //            dt.Rows.Add("A010", "������", 100, 300, "�й�");

                //            dt.Rows.Add("A011", "��â", 250, 200, "�й�");

                //            dt.Rows.Add("A012", "�͵�ľ", 320, 200, "�й�");

                //            dt.Rows.Add("A013", "����", 380, 400, "�й�");

                //            dt.Rows.Add("A014", "ƻ��", 110, 300, "�й�");

                //            dt.Rows.Add("A015", "����", 420, 200, "�й�");

                //            dt.Rows.Add("A016", "�㽶", 530, 400, "�й�");

                //            dt.Rows.Add("A017", "����", 380, 400, "�й�");

                //            dt.Rows.Add("A018", "ƻ��", 110, 300, "�й�");

                //            dt.Rows.Add("A019", "����", 420, 200, "�й�");

                //            dt.Rows.Add("A020", "�㽶", 530, 400, "�й�");

                //            dt.Rows.Add("A021", "ƻ��", 10, 300, "�й�");

                //            dt.Rows.Add("A022", "����", 20, 200, "�й�");

                //            dt.Rows.Add("A023", "�㽶", 30, 400, "�й�");

                //            dt.Rows.Add("A024", "����", 10, 300, "�й�");

                //            dt.Rows.Add("A025", "��֦", 20, 200, "�й�");

                //            dt.Rows.Add("A026", "â��", 30, 400, "�й�");

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
            if (args.DataSet.Query.DataSourceName == "DataSource1" && args.Report.Name.Contains("����"))
            {
                if (args.DataSet.Name == "DataSet1")
                {
                    dt.Columns.Add("��Ʒ���");
                    dt.Columns.Add("��Ʒ����");
                    dt.Columns.Add("����");
                    dt.Columns.Add("�����");
                    dt.Columns.Add("����");
                    dt.Rows.Add("A001", "ƻ��", 10, 300, "�й�");
                    dt.Rows.Add("A002", "����", 20, 200, "�й�");
                    dt.Rows.Add("A003", "�㽶", 30, 400, "�й�");

                    dt.Rows.Add("A004", "����", 10, 300, "�й�");

                    dt.Rows.Add("A005", "��֦", 20, 200, "�й�");

                    dt.Rows.Add("A006", "â��", 30, 400, "�й�");

                    dt.Rows.Add("A007", "⨺���", 110, 300, "�й�");

                    dt.Rows.Add("A008", "����", 210, 200, "�й�");

                    dt.Rows.Add("A009", "����", 320, 400, "�й�");

                    dt.Rows.Add("A010", "������", 100, 300, "�й�");

                    dt.Rows.Add("A011", "��â", 250, 200, "�й�");

                    dt.Rows.Add("A012", "�͵�ľ", 320, 200, "�й�");

                    dt.Rows.Add("A013", "����", 380, 400, "�й�");

                    dt.Rows.Add("A014", "ƻ��", 110, 300, "�й�");

                    dt.Rows.Add("A015", "����", 420, 200, "�й�");

                    dt.Rows.Add("A016", "�㽶", 530, 400, "�й�");

                    dt.Rows.Add("A017", "����", 380, 400, "�й�");

                    dt.Rows.Add("A018", "ƻ��", 110, 300, "�й�");

                    dt.Rows.Add("A019", "����", 420, 200, "�й�");

                    dt.Rows.Add("A020", "�㽶", 530, 400, "�й�");

                    dt.Rows.Add("A021", "ƻ��", 10, 300, "�й�");

                    dt.Rows.Add("A022", "����", 20, 200, "�й�");

                    dt.Rows.Add("A023", "�㽶", 30, 400, "�й�");

                    dt.Rows.Add("A024", "����", 10, 300, "�й�");

                    dt.Rows.Add("A025", "��֦", 20, 200, "�й�");

                    dt.Rows.Add("A026", "â��", 30, 400, "�й�");

                }
            }
            args.Data = dt;
        }
    }
}
