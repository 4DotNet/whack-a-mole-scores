using HexMaster.RedisCache.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wam.Core.Cache;
using Wam.Core.Configuration;
using Wam.Scores.DataTransferObjects;

namespace Wam.Scores.Services
{
    public class GamesService : IGamesService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<ServicesConfiguration> _servicesConfiguration;
        private readonly ICacheClientFactory _cacheClientFactory;
        private readonly ILogger<GamesService> _logger;
        private readonly string _remoteServiceBaseUrl;


        public Task<GameDetailsDto?> GetGameDetails(Guid gameId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting game details from games service {gameId}", gameId);
            var cacheClient = _cacheClientFactory.CreateClient();
            var cacheKey = CacheName.GameDetails(gameId);
            return cacheClient.GetOrInitializeAsync(() => GetGameDetailsFromServer(gameId, cancellationToken), cacheKey);
        }


        private string RemoteServiceBaseUrl()
        {
            return $"http://{_servicesConfiguration.Value.GamesService}/api";
        }

        private async Task<GameDetailsDto?> GetGameDetailsFromServer(Guid gameId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Downloading game information from games service for user {gameId}", gameId);
            var uri = $"{_remoteServiceBaseUrl}/games/{gameId}";
            _logger.LogInformation("Downloading from {gameDetailsUrl}", uri);
            var responseString = await _httpClient.GetStringAsync(uri, cancellationToken);
            return JsonConvert.DeserializeObject<GameDetailsDto>(responseString);
        }


        public GamesService(
            HttpClient httpClient,
            IOptions<ServicesConfiguration> servicesConfiguration,
            ICacheClientFactory cacheClientFactory,
            ILogger<GamesService> logger)
        {
            _httpClient = httpClient;
            _servicesConfiguration = servicesConfiguration;
            _cacheClientFactory = cacheClientFactory;
            _logger = logger;

            _remoteServiceBaseUrl = RemoteServiceBaseUrl();
        }
        

    }
}
