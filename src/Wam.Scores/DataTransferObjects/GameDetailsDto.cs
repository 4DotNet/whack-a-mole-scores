using Wam.Core.Enums;

namespace Wam.Scores.DataTransferObjects;

public record GameDetailsDto(
    Guid Id, 
    string Code, 
    List<GamePlayerDto> Players,
    DateTimeOffset CreatedOn,
    DateTimeOffset? StartedOn,
    DateTimeOffset? FinishedOn);