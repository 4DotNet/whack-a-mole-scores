namespace Wam.Scores.DataTransferObjects;

public record ScoreCreateDto(Guid GameId, List<ScoreDto> Scores);
