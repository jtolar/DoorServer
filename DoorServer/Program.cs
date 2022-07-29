using DoorServer.Configurations;
using DoorServer.TcpServers.HostedServices;
using DoorServer.TcpServers.Management;
using DoorServer.TcpServers.Rlogin.Server;
using DoorServer.TcpServers.SshTunnel;

var builder = WebApplication.CreateBuilder(args);
ConfigureHostedServices(builder);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ServerManagementHub>("/servermanagement");

app.Run();

static void ConfigureHostedServices(WebApplicationBuilder builder)
{
    builder.Configuration.AddJsonFile("serverConfiguration.json", optional: false, reloadOnChange: true);
    //Keep your login creds safe by creating your own serverConfiguration.developer.json file. This file is ignored by git
    //and used to override the serverConfiguration.json file.
    builder.Configuration.AddJsonFile("serverConfiguration.developer.json", optional: true, reloadOnChange: true);

    var rloginServerConfig = builder.Configuration.GetSection("Servers:RloginServer").Get<RloginServerConfiguration>();
    var hostServerConfig = builder.Configuration.GetSection("Servers:HostServer").Get<HostServerConfiguration>();
    var doorPartyClientConfig = builder.Configuration.GetSection("Clients:DoorParty").Get<DoorPartyConfiguration>();

    if (!Debugger.IsAttached)
        builder.WebHost.UseKestrel((hostContext, conf) =>
        {
            conf.Listen(IPAddress.Parse(hostServerConfig.IpAddress).MapToIPv4(), hostServerConfig.HostPort);
        });

    builder.Services.AddSingleton(rloginServerConfig);
    builder.Services.AddSingleton<ISshTunnelConfiguration>(doorPartyClientConfig);
    builder.Services.AddSingleton<RloginHostedService>();

    //builder.Services.AddHostedService<RloginHostedService>();
    builder.Services.AddHostedService<RloginHostedService>(provider => provider.GetRequiredService<RloginHostedService>());
    builder.Services.AddSignalR();

}