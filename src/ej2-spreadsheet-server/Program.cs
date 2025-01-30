using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.ValueLengthLimit = int.MaxValue;
});
var MyAllowSpecificOrigins = "AllowAllOrigins";
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                        builder => {
                            builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                        });
});

builder.Services.AddMemoryCache();
builder.Services.AddMvc(endPoint => endPoint.EnableEndpointRouting = false);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
builder.Services.AddResponseCompression();

var app = builder.Build();

// Access LICENSE_KEY from environment variables
string license_key = builder.Configuration["SYNCFUSION_LICENSE_KEY"];

if (license_key!=null && license_key!=string.Empty)
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(license_key);

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();
app.UseResponseCompression();
app.MapControllers();

app.Run();
