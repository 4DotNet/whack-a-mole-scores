using Wam.Scores.DataTransferObjects;

namespace Wam.Scores.Repositories;

public interface IScoresRepository
{
    Task<ScoreBoardOverviewDto> GetScoreBoardOverviewAsync(Guid gameId, GameDetailsDto? gameDetails, CancellationToken cancellationToken);
    Task<ScorePersistenseResultDto> StoreScores(ScoreCreateDto dto, CancellationToken cancellationToken);
}