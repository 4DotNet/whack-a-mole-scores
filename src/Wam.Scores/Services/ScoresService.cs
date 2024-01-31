using HexMaster.RedisCache.Abstractions;
using Wam.Core.Cache;
using Wam.Scores.DataTransferObjects;
using Wam.Scores.Repositories;

namespace Wam.Scores.Services;

public class ScoresService(
    IScoresRepository scoresRepository, 
    IGamesService    gamesService,
    ICacheClientFactory cacheClientFactory) : IScoresService
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

    public Task<ScorePersistenseResultDto> StoreScores(
        ScoreCreateDto dto,
        CancellationToken cancellationToken)
    {
        return scoresRepository.StoreScores(dto, cancellationToken);
    }
}
