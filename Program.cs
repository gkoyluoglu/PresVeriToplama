using PresVeriToplama;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<DatalayerReadService>();

var host = builder.Build();
host.Run();