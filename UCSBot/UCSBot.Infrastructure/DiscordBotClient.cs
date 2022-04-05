﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UCSBot.Infrastructure.Configuration;
using UCSBot.Infrastructure.Contracts;

namespace UCSBot.Infrastructure;

public sealed class DiscordBotClient : IBotClient
{
    private readonly ILogger _logger;
    private DiscordSocketClient? _client;
    private readonly DiscordConfiguration _configuration;

    public DiscordBotClient(ILogger<DiscordBotClient> logger, IOptions<DiscordConfiguration> options)
    {
        _logger = logger;
        _configuration = options.Value;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        if (_client != null) throw new Exception("Discord client was already started");

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info
        });

        _client.Log += msg =>
        {
            _logger.LogInformation(msg.Message);
            return Task.CompletedTask;
        };
        await _client.LoginAsync(TokenType.Bot, _configuration.BotToken);
        await _client.StartAsync();
    }

    public async Task Stop(CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new Exception("Client was never started");
        await _client.StopAsync();
        await _client.DisposeAsync();
    }

    public async Task SendMessages(IEnumerable<string> messages, IEnumerable<ulong> channelIds,
        CancellationToken cancellationToken = default)
    {
        if (_client == null) throw new Exception("Client was never started");
        foreach (var channelId in channelIds)
        {
            if (await _client.GetChannelAsync(channelId) is not IMessageChannel channel)
            {
                _logger.LogWarning($"Channel {channelId} was not found");
                continue;
            }

            foreach (var message in messages)
                try
                {
                    await channel.SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Could not send message to channel {channelId}. Message :{message}", ex);
                }
        }
    }
}