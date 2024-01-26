namespace Wam.Scores.DataTransferObjects;

public record ScoreBoardOverviewDto(Guid GameId, List<ScoreBoardPlayerDto> Players);