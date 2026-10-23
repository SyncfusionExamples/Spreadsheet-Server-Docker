using System;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Spreadsheet;
using System.Text.Json;
using Syncfusion.XlsIO;
using System.IO;

namespace EJ2SpreadsheetServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpreadsheetController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        string path;
        public SpreadsheetController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            path = _hostingEnvironment.ContentRootPath;
        }

        // To open excel file
        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("Open")]
        public IActionResult Open(IFormCollection openRequest)
        {
            OpenRequest open = new OpenRequest();
            if (openRequest != null)
            {
                if (openRequest.Files != null && openRequest.Files.Count > 0)
                {
                    // Assigning the file.
                    open.File = openRequest.Files[0];
                    if (open.File != null && open.File.GetType().FullName.Contains("FormFile"))
                    {
                        IFormFile formFile = openRequest.Files[0];
                        if (formFile.FileName.EndsWith(".html"))
                        {
                            ExcelEngine excelEngine = new ExcelEngine();
                            IApplication application = excelEngine.Excel;
                            IWorkbook workbook = application.Workbooks.Create(1);
                            IWorksheet worksheet = workbook.Worksheets[0];
                            Stream fileStream = formFile.OpenReadStream();
                            worksheet.ImportHtmlTable(fileStream, 1, 1);
                            worksheet.UsedRange.AutofitColumns();
                            worksheet.UsedRange.AutofitRows();
                            MemoryStream ms = new MemoryStream();
                            workbook.SaveAs(ms);
                            ms.Position = 0;
                            workbook.Close();
                            excelEngine.Dispose();
                            open.File = new FormFile(ms, 0, ms.Length, "Sample", "Sample.xlsx");
                        }
                    }
                }
                // Setting the calculation mode.
                if (openRequest.ContainsKey("IsManualCalculationEnabled") && bool.TryParse(openRequest["IsManualCalculationEnabled"].ToString(), out bool flag))
                {
                    open.IsManualCalculationEnabled = flag;
                }
                // Assigning the parse options to control which properties to skip while loading Excel files.
                if (openRequest.ContainsKey("ParseOptions"))
                {
                    var parseOptions = openRequest["ParseOptions"];
                    if (!string.IsNullOrEmpty(parseOptions))
                    {
                        open.ParseOptions = JsonSerializer.Deserialize<WorkbookParseOptions>(parseOptions.ToString(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    }
                }
                // Assigning the Workbook password.
                if (openRequest.ContainsKey("Password"))
                {
                    open.Password = openRequest["Password"];
                }
                // Enable the below line of setting open.Guid, if you want use MaximumDataLimit or MaximumFileSize property
                //Microsoft.Extensions.Primitives.StringValues guid;
                //if (openRequest.TryGetValue("Guid", out guid))
                //{
                //    open.Guid = guid;
                //}
                // Used to skip loading Excel files with maximum data or maximum file size
                //open.ThresholdLimit.MaximumDataLimit = 50000;
                //open.ThresholdLimit.MaximumFileSize = 2097152;
            }
            // Process the Excel file and return the workbook JSON result
            var result = Workbook.Open(open);
            return Content(result);
        }

        // To save as excel file
        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("Save")]
        public IActionResult Save([FromForm] SaveSettings saveSettings)
        {
            // Process the workbook JSON and return as file stream result.
            return Workbook.Save(saveSettings);
        }
    }
}
