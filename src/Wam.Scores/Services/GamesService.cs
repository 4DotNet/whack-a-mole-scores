using Dapr.Client;
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
        private readonly ILogger<GamesService> _logger;
        private readonly string _remoteServiceBaseUrl;
        private readonly DaprClient _daprClient;
        private const string StateStoreName = "statestore";

        public Task<GameDetailsDto?> GetGameDetails(Guid gameId, CancellationToken cancellationToken)
        {
            return GetFromStateStoreOrRemoveService(gameId, cancellationToken);
        }


        private string RemoteServiceBaseUrl()
        {
            return $"http://{_servicesConfiguration.Value.GamesService}/api";
        }

        private async Task<GameDetailsDto?> GetFromStateStoreOrRemoveService(Guid gameId, CancellationToken cancellationToken)
        {
            var stateStoreValue = await _daprClient.GetStateAsync<GameDetailsDto>(StateStoreName, CacheName.GameDetails(gameId), cancellationToken: cancellationToken);
            if (stateStoreValue is not null)
            {
                return stateStoreValue;
            }
            var scoreBoard = await GetGameDetailsFromServer(gameId, cancellationToken);
            await _daprClient.SaveStateAsync(StateStoreName, CacheName.GameDetails(gameId), scoreBoard, cancellationToken: cancellationToken);
            return scoreBoard;
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
            DaprClient daprClient,
            ILogger<GamesService> logger)
        {
            _httpClient = httpClient;
            _servicesConfiguration = servicesConfiguration;
            _daprClient = daprClient;
            _logger = logger;

            _remoteServiceBaseUrl = RemoteServiceBaseUrl();
        }


    }
}
