using BuildSystem.Business;
using BuildSystem.Data;
using BuildSystem.Endpoints;
using FastEndpoints;
using Microsoft.AspNetCore.SpaServices.AngularCli;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBuildSystemBusiness();
builder.Services.AddBuildSystemData();
builder.Services.AddBuildSystemEndpoints();
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseSpaStaticFiles();

app.UseAuthentication();
app.UseFastEndpoints();
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "../BuildSystem.Client";
    if (app.Environment.IsDevelopment())
        spa.UseAngularCliServer(npmScript: "start");
});

app.Run();
