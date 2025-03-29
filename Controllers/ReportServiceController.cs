using GrapeCity.ActiveReports.Document;
using GrapeCity.ActiveReports;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Presentation;
using JSViewer_MVC_Core.model;
using JSViewer_MVC_Core.utils;
using System.Threading.Tasks;

namespace JSViewer_MVC_Core.Controllers
{
    [Route("/reportserver")]
    public class ReportServiceController : Controller
    {
        [HttpGet("reports/{reportName}/info")]
        public Dictionary<string, object> report(string reportName)
        {
            var bts = System.IO.File.ReadAllBytes(Path.Combine("D:\\gitrepos\\ActiveReports\\bin\\Debug\\net8.0", "Reports/" + reportName));
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

            PageDocument document = new PageDocument(pageReport);
            document.LocateDataSource += Document_LocateDataSource;

            var response = "{\r\n    \"name\": \"RDL报表.rdlx -- test\",\r\n    \"galleyModeAllowed\": true,\r\n    \"parameters\": [\r\n        {\r\n            \"name\": \"out_parameter\",\r\n            \"prompt\": \"内置参数\",\r\n            \"dataType\": \"String\",\r\n            \"allowBlank\": false,\r\n            \"nullable\": false,\r\n            \"multiValue\": false,\r\n            \"multiline\": false,\r\n            \"hidden\": false,\r\n            \"usedInQuery\": \"Auto\",\r\n            \"dependsOn\": [],\r\n            \"selectAllValue\": {\r\n                \"selectAll\": \"$selectAll\"\r\n            },\r\n            \"validValues\": {},\r\n            \"defaultValue\": {\r\n                \"value\": null\r\n            },\r\n            \"dateOnly\": true,\r\n            \"displayFormat\": null\r\n        }\r\n    ],\r\n    \"parametersView\": null,\r\n    \"exports\": [\r\n        \"Pdf\",\r\n        \"Xlsx\",\r\n        \"Xls\",\r\n        \"XlsxData\",\r\n        \"CsvData\",\r\n        \"Docx\",\r\n        \"Tiff\",\r\n        \"Mht\",\r\n        \"Csv\",\r\n        \"Json\",\r\n        \"Xml\",\r\n        \"TextPrint\"\r\n    ],\r\n    \"displayType\": null,\r\n    \"sizeType\": null,\r\n    \"type\": 2,\r\n    \"viewerType\": \"Report\",\r\n    \"sections\": []\r\n}";


            return JsonSerializer.Deserialize<Dictionary<string, object>>(response);
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

        /// <summary>
        /// 数据缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost("data/cache/{key}")]
        public async Task<ProccessResult> reportDataCache(string key)
        {
            try
            {
                string rawBody = null;
                using (StreamReader reader = new StreamReader(Request.Body))
                {
                    rawBody = await reader.ReadToEndAsync();
                }

                ReportDataCache.setCache(key, rawBody);

                return ProccessResult.OnOk();
            }
            catch (Exception ex)
            {
                return ProccessResult.OnFailed("缓存失败：" + ex.Message);
            }
        }
    }
}
