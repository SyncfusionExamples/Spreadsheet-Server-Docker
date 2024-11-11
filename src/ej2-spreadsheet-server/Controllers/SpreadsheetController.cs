using System;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Spreadsheet;

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
                    IFormFile formFile = openRequest.Files[0];
                    open.File = formFile;
                }

                // Enable the below line of setting open.Guid, if you want use MaximumDataLimit or MaximumFileSize property
                //Microsoft.Extensions.Primitives.StringValues guid;
                //if (openRequest.TryGetValue("Guid", out guid))
                //{
                //    open.Guid = guid;
                //}
            }

            // Used to skip loading Excel files with maximum data or maximum file size
            //open.ThresholdLimit.MaximumDataLimit = 50000;
            //open.ThresholdLimit.MaximumFileSize = 2097152;

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
