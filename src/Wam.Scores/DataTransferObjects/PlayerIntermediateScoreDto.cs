﻿namespace Wam.Scores.DataTransferObjects;

public record PlayerIntermediateScoreDto(
    Guid GameId,
    string GameCode,
    Guid PlayerId,
    List<SimpleScoreDto> Scores);