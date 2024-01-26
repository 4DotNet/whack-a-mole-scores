namespace Wam.Scores.DataTransferObjects;

public record ScoreDto(string UniqueId, Guid PlayerId, int Score, DateTimeOffset CreatedOn);