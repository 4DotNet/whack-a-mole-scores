using Azure.Messaging.WebPubSub;
using Microsoft.Extensions.Logging;
using Azure.Core;
using Wam.Core.Cache;
using Wam.Core.Events;
using Wam.Scores.DataTransferObjects;
using Wam.Scores.Repositories;
using Dapr.Client;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.IdentityModel.Abstractions;

namespace Wam.Scores.Services;

public class ScoresService(
    IScoresRepository scoresRepository,
    IGamesService gamesService,
    WebPubSubServiceClient pubsubClient,
    DaprClient daprClient,
    TelemetryClient telemetry,
    ILogger<ScoresService> logger) : IScoresService
{
    private const string StateStoreName = "statestore";

    public Task<ScoreBoardOverviewDto> Scoreboard(Guid gameId, CancellationToken cancellationToken)
    {
        return GetFromCacheOrRepository(gameId, cancellationToken);
    }

    private async Task<ScoreBoardOverviewDto> GetFromCacheOrRepository(Guid gameId, CancellationToken cancellationToken)
    {
        var stateStoreValue = await daprClient.GetStateAsync<ScoreBoardOverviewDto>(StateStoreName, CacheName.GameScoreBoard(gameId), cancellationToken: cancellationToken);
        if (stateStoreValue is not null)
        {
            return stateStoreValue;
        }
        var scoreBoard = await GetScoreboardFromRepository(gameId, cancellationToken);
        await daprClient.SaveStateAsync(StateStoreName, CacheName.GameScoreBoard(gameId), scoreBoard, cancellationToken: cancellationToken);
        return scoreBoard;
    }

    private async Task<ScoreBoardOverviewDto> GetScoreboardFromRepository(Guid gameId,
        CancellationToken cancellationToken)
    {
        var gameDetails = await gamesService.GetGameDetails(gameId, cancellationToken);
        var scoreBoard = await scoresRepository.GetScoreBoardOverviewAsync(gameId, gameDetails, cancellationToken);
        await ScoreBoardProcessedEvent(gameDetails.Code, scoreBoard);
        return scoreBoard;
    }

    public async Task<ScorePersistenseResultDto> StoreScores(
        ScoreCreateDto dto,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing {scoreCount} scores for game {gameId} ", dto.Scores.Count, dto.GameId);
        var processedScores = await scoresRepository.StoreScores(dto, cancellationToken);
        if (processedScores.UniqueIds.Any())
        {
            TrackMetric(processedScores);
            await ScoreProcessedEvent(dto);
        }
        return processedScores;
    }

    private Task ScoreProcessedEvent(ScoreCreateDto dto)
    {
        if (dto.Scores.Any())
        {
            var scores = dto.Scores.Select(s => s.Score).ToList();
            var playerId = dto.Scores.First().PlayerId;

            var message = new RealtimeEvent<PlayerIntermediateScoreDto>
            {
                Message = "game-score-added",
                Data = new PlayerIntermediateScoreDto(dto.GameId, dto.Code, playerId, scores)
            };
            return RaiseEvent(message, dto.Code);
        }
        return Task.CompletedTask;
    }

    private Task ScoreBoardProcessedEvent(string code, ScoreBoardOverviewDto dto)
    {
        if (dto.Players.Any())
        {
            var message = new RealtimeEvent<ScoreBoardOverviewDto>
            {
                Message = "score-board-processed",
                Data = dto
            };
            return RaiseEvent(message, code);
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

    private void TrackMetric(ScorePersistenseResultDto persistedScores)
    {
        var whackedMoles = new MetricTelemetry
        {
            Name = "whackedMoles",
            Sum = persistedScores.UniqueIds.Count
        };
        telemetry.TrackMetric(whackedMoles);
    }

}
