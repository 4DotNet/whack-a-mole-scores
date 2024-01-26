using HexMaster.RedisCache.Abstractions;
using Wam.Core.Cache;
using Wam.Scores.DataTransferObjects;
using Wam.Scores.Repositories;

namespace Wam.Scores.Services;
    public class ScoresService (IScoresRepository scoresRepository, ICacheClientFactory cacheClientFactory)
    {

        public Task<ScoreBoardOverviewDto> Scoreboard(Guid gameId, CancellationToken cancellationToken)
        {
            var cacheKey = CacheName.GameScoreBoard(gameId);
            var cacheClient = cacheClientFactory.CreateClient();
            return cacheClient.GetOrInitializeAsync(
                () => scoresRepository.GetScoreBoardOverviewAsync(gameId, cancellationToken), cacheKey);
        }

}
