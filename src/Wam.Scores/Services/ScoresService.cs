﻿using Azure.Messaging.WebPubSub;
using HexMaster.RedisCache.Abstractions;
using Microsoft.Extensions.Logging;
using System.Numerics;
using Azure.Core;
using Wam.Core.Cache;
using Wam.Core.Events;
using Wam.Scores.DataTransferObjects;
using Wam.Scores.Repositories;

namespace Wam.Scores.Services;

public class ScoresService(
    IScoresRepository scoresRepository, 
    IGamesService    gamesService,
    WebPubSubServiceClient pubsubClient,
    ICacheClientFactory cacheClientFactory,
    ILogger<ScoresService> logger) : IScoresService
{

    public async Task<ScoreBoardOverviewDto> Scoreboard(Guid gameId, CancellationToken cancellationToken)
    {
        var cacheKey = CacheName.GameScoreBoard(gameId);
        var cacheClient = cacheClientFactory.CreateClient();
        return await cacheClient.GetOrInitializeAsync(
            () => GetScoreboardFromRepository(gameId, cancellationToken), cacheKey);
    }

    private async Task<ScoreBoardOverviewDto> GetScoreboardFromRepository(Guid gameId,
        CancellationToken cancellationToken)
    {
        var gameDetails = await gamesService.GetGameDetails(gameId, cancellationToken);
        var scoreBoard = await scoresRepository.GetScoreBoardOverviewAsync(gameId, gameDetails, cancellationToken);

      return scoreBoard;
    }

    public async Task<ScorePersistenseResultDto> StoreScores(
        ScoreCreateDto dto,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing {scoreCount} scores for game {gameId} ", dto.Scores.Count,  dto.GameId);
        var processedScores = await scoresRepository.StoreScores(dto, cancellationToken);
        if (processedScores.UniqueIds.Any())
        {
            await ScoreProcessedEvent(dto);
        }
        return processedScores;
    }

    private Task ScoreProcessedEvent(ScoreCreateDto dto)
    {
        if (dto.Scores.Any())
        {
            var latestScore = dto.Scores.OrderByDescending(s => s.CreatedOn).First().Score;
            var averageScore = dto.Scores.Average(s => s.Score);
            var playerId = dto.Scores.First().PlayerId;

            var message = new RealtimeEvent<PlayerIntermediateScoreDto>
            {
                Message = "game-score-added",
                Data = new PlayerIntermediateScoreDto(dto.GameId, dto.Code, playerId, latestScore, averageScore)
            };
            return RaiseEvent(message, dto.Code);
        }
        return Task.CompletedTask;
    }

    private async Task RaiseEvent<T>(RealtimeEvent<T> realtimeEvent, string group)
    {
        try
        {
            await pubsubClient.SendToGroupAsync(group, realtimeEvent.ToJson(), ContentType.ApplicationJson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to raise event {event} to group {group}", realtimeEvent.Message, group);
        }
    }

}
