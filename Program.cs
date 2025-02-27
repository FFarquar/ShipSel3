global using ShipSel3.Models;
global using SH = SharedLibrary.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SharedLibrary.LibraryServices.libraryLocalStorageService;
using SharedLibrary.Models;
using SharedLibrary.Services.TestService;
using ShipSel3;
using ShipSel3.Services;


//using ShipSel3.Services.LocalStorageService;

using ShipSel3.Services.UnitsandListsServiceClient;
//using SharedLibrary.Services.UnitsandListsServiceClient;
using ShipSel3.Services.UploadDownloadService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IUnitsandListsServiceClient, UnitsandListsServiceClient>();
builder.Services.AddScoped<ILocalStorageFromLibrary, LocalStorageFromLibrary>();
//builder.Services.AddScoped <BroadsideSDS>();
builder.Services.AddScoped<IUploadDownloadServiceClient, UploadDownloadServiceClient>();
builder.Services.AddScoped<ITestInterface, TestInterface>();    //A test to see if a service can be used from the shared library

await builder.Build().RunAsync();
