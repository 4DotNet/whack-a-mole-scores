using Wam.Scores.DataTransferObjects;

namespace Wam.Scores.Services;

public interface IGamesService
{
    Task<GameDetailsDto?> GetGameDetails(Guid gameId, CancellationToken cancellationToken);
}