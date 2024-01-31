namespace Wam.Scores.DataTransferObjects;

public record GamePlayerDto(Guid Id, string DisplayName, string EmailAddress, bool IsBanned);