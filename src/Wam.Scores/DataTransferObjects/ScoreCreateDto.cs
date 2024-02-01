namespace Wam.Scores.DataTransferObjects;

public record ScoreCreateDto(Guid GameId, string Code,List<ScoreDto> Scores);
