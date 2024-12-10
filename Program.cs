global using ShipSel3.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShipSel3;

using ShipSel3.Services.LocalStorageService;
using ShipSel3.Services.UnitsandListsServiceClient;
using ShipSel3.Services.UploadDownloadService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IUnitsandListsServiceClient, UnitsandListsServiceClient>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IUploadDownloadServiceClient, UploadDownloadServiceClient>();


await builder.Build().RunAsync();
