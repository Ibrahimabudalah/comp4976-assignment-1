using Blazored.SessionStorage;
using MEMS.Public;
using MEMS.Public.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// shortcut: API url comes from wwwroot/appsettings.json because browser code cannot read Aspire env vars; upgrade to the Aspire4Wasm package if service discovery for the WASM app is required.
var apiBase = builder.Configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl missing from wwwroot/appsettings.json");
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<MemsApiClient>();
builder.Services.AddScoped<ImageUrls>();

await builder.Build().RunAsync();
