using Azure.Data.Tables;
using HexMaster.RedisCache.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using Wam.Core.Cache;
using Wam.Core.Configuration;
using Wam.Core.Identity;
using Wam.Scores.DataTransferObjects;
using Wam.Scores.Entities;

namespace Wam.Scores.Repositories;

public class ScoresRepository : IScoresRepository
{
    private readonly ILogger<ScoresRepository> _logger;
    private const string TableName = "scores";
    private readonly TableClient _tableClient;


    public async Task<ScoreBoardOverviewDto> GetScoreBoardOverviewAsync(Guid gameId, CancellationToken cancellationToken)
    {
        var query = _tableClient.QueryAsync<ScoreEntity>($"{nameof(ScoreEntity.PartitionKey)} eq {gameId}");
        var allScores = new List<ScoreEntity>();
        await foreach (var queryPage in query.AsPages().WithCancellation(cancellationToken))
        {
            allScores.AddRange(queryPage.Values);
        }

        var players = new List<ScoreBoardPlayerDto>();

        var groupedScores = allScores.GroupBy(se => se.PlayerId, (key, group) => new { PlayerId = key, Scores = group.ToList() });
        foreach (var groupedScore in groupedScores)
        {
            players.Add(new ScoreBoardPlayerDto(groupedScore.PlayerId, groupedScore.Scores.Select(s => new ScoreBoardPlayerScoreDto(Guid.Parse(s.RowKey), s.Score)).ToList()));
        }

        return new ScoreBoardOverviewDto(gameId, players);
    }

    public ScoresRepository(
        IOptions<AzureServices> configuration,
        ILogger<ScoresRepository> logger)
    {
        _logger = logger;
        var tableStorageUrl = $"https://{configuration.Value.UsersStorageAccountName}.table.core.windows.net";
        _tableClient = new TableClient(new Uri(tableStorageUrl), TableName, CloudIdentity.GetCloudIdentity());
    }

}