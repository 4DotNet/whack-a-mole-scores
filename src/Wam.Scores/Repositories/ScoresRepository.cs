using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure;
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

    public async Task<ScorePersistenseResultDto> StoreScores(ScoreCreateDto dto, CancellationToken cancellationToken)
    {
        if (!dto.Scores.Any())
        {
            return new ScorePersistenseResultDto([]);
        }

        var transactions = new List<TableTransactionAction>();
        foreach (var scoreDto in dto.Scores)
        {
            var scoreEntity = new ScoreEntity
            {
                PartitionKey = dto.GameId.ToString(),
                RowKey = scoreDto.UniqueId,
                PlayerId = scoreDto.PlayerId,
                Score = scoreDto.Score,
                Timestamp = scoreDto.CreatedOn,
                ETag = ETag.All
            };
            transactions.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, scoreEntity));
        }

        if (transactions.Any())
        {
            var response = await _tableClient.SubmitTransactionAsync(transactions, cancellationToken);
            if (response.Value.Any(r => r.IsError))
            {
                throw new Exception($"Either one of the received scores for game {dto.GameId} was not succesfully saved.");
            }
        }

        return new ScorePersistenseResultDto(dto.Scores.Select(d => d.UniqueId).ToList());
    }

    public ScoresRepository(
        IOptions<AzureServices> configuration,
        ILogger<ScoresRepository> logger)
    {
        _logger = logger;
        var tableStorageUrl = $"https://{configuration.Value.ScoresStorageAccountName}.table.core.windows.net";
        _tableClient = new TableClient(new Uri(tableStorageUrl), TableName, CloudIdentity.GetCloudIdentity());
    }

}