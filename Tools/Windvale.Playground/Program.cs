using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Windvale.Playground.Web;

var Builder = WebAssemblyHostBuilder.CreateDefault(args);
Builder.RootComponents.Add<App>("#app");
Builder.RootComponents.Add<HeadOutlet>("head::after");

await Builder.Build().RunAsync();
