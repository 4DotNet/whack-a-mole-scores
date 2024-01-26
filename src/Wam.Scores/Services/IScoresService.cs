using Wam.Scores.DataTransferObjects;

namespace Wam.Scores.Services;

public interface IScoresService
{
    Task<ScoreBoardOverviewDto> Scoreboard(Guid gameId, CancellationToken cancellationToken);
    Task<ScorePersistenseResultDto> StoreScores(ScoreCreateDto dto, CancellationToken cancellationToken);

}