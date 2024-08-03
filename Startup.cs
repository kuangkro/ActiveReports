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

            app.UseReportViewer(settings =>
            {
                //https://gcdn.grapecity.com.cn/forum.php?mod=viewthread&tid=54413&extra=page%3D1
                settings.UseFileStore(ReportsDirectory);
                settings.UseReportProvider(reportid =>
                {
                    Console.WriteLine($"UseReportProvider Action01:{reportid}");
                    return null;
                }, reportid =>
                {
                    Console.WriteLine($"UseReportProvider Action02:{reportid}");
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
    }
}
