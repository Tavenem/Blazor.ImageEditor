using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tavenem.Blazor.ImageEditor.Sample.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddImageEditor();

await builder.Build().RunAsync().ConfigureAwait(false);
