﻿using Wam.Scores.DataTransferObjects;

namespace Wam.Scores.Repositories;

public interface IScoresRepository
{
    Task<ScoreBoardOverviewDto> GetScoreBoardOverviewAsync(Guid gameId, CancellationToken cancellationToken);
}