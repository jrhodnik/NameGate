using DNS.Client.RequestResolver;
using DNS.Server;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using NameGate;
using NameGate.Database;
using NameGate.Services;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddWindowsService();

services.AddLogging(b => b.AddConsole(o => o.IncludeScopes = true));
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddMudServices();

services.AddSingleton<GlobalConfig>(sp => sp.GetRequiredService<IConfiguration>().Get<GlobalConfig>() ?? throw new InvalidOperationException("Invalid global config."));

services.AddScoped<IRequestResolver, CustomRequestResolver>();
services.AddSingleton<DnsServer>(sp =>
{
    var scope = sp.CreateScope();
    var logger = sp.GetRequiredService<ILogger<DnsServer>>();

    var ret = new DnsServer(scope.ServiceProvider.GetRequiredService<IRequestResolver>(), logger);

    return ret;
});
services.AddHostedService<DnsServerBootstrapper>();

services.AddScoped<DirectoryManager>();

services.AddSingleton<BlockListManager>();
services.AddHostedService(sp => sp.GetRequiredService<BlockListManager>());
services.AddHttpClient();
services.AddSingleton<QueryLogger>();
services.AddDatabase();
services.AddFusionCache("BlackListedNames");
services.AddSingleton<StatisticsService>();
services.AddHostedService(sp => sp.GetRequiredService<StatisticsService>());

services.AddFusionCache("HostNameResolver").WithDefaultEntryOptions(new FusionCacheEntryOptions()
{
    Duration = TimeSpan.FromHours(1),
    IsFailSafeEnabled = true
});
services.AddSingleton<HostNameResolver>();
services.AddHostedService(sp => sp.GetRequiredService<HostNameResolver>());

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

_ = app.Services.GetRequiredService<QueryLogger>(); //Hotload the service...

await app.RunAsync();
