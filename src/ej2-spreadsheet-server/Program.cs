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
using Syncfusion.Licensing;
using Microsoft.Extensions.Hosting;
using ej2_spreadsheet_server.Services;

var builder = WebApplication.CreateBuilder(args);

// Initialize SkiaSharp font resolver for Windows nanoserver compatibility
SkiaSharpFontPatcher.Initialize();

// Configuration Setup
var env = builder.Environment;
builder.Configuration.SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Syncfusion License Registration
string licenseKey = builder.Configuration["SYNCFUSION_LICENSE_KEY"];

SyncfusionLicenseProvider.RegisterLicense(licenseKey);

// Configure services
var MyAllowSpecificOrigins = "AllowAllOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                        builder => {
                            builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                        });
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.ValueLengthLimit = int.MaxValue;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue;
});

builder.Services.AddMemoryCache();
builder.Services.AddMvc(endPoint => endPoint.EnableEndpointRouting = false);
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});
builder.Services.AddResponseCompression();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

// Build app
var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();
app.UseResponseCompression();
app.MapControllers();
app.Run();
