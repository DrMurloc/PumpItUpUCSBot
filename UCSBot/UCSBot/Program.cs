using MediatR;
using Microsoft.EntityFrameworkCore;
using UCSBot.Application.Handlers;
using UCSBot.Domain.Contracts;
using UCSBot.HostedServices;
using UCSBot.Infrastructure;
using UCSBot.Infrastructure.Configuration;
using UCSBot.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>()
    .AddEnvironmentVariables();
var discordConfig = builder.Configuration.GetSection("Discord").Get<DiscordConfiguration>();
var cosmosConfig = builder.Configuration.GetSection("Cosmos").Get<CosmosConfiguration>();

builder.Services.AddSingleton<IBotClient, DiscordBotClient>()
    .AddHostedService<BotHostedService>()
    .AddMediatR(typeof(RegisterChannelToFeedHandler))
    .AddTransient<IChannelRepository, EfChannelRepository>()
    .Configure<DiscordConfiguration>(o => { o.BotToken = discordConfig.BotToken; })
    .AddDbContext<UcsBotDbContext>(o =>
        o.UseCosmos(cosmosConfig.AccountEndpoint, cosmosConfig.AccountKey, cosmosConfig.DatabaseName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseHttpsRedirection();

app.UseRouting();


app.Run();