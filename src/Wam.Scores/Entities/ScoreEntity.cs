﻿using Azure;
using Azure.Data.Tables;

namespace Wam.Scores.Entities;

public class ScoreEntity : ITableEntity
{
    public string PartitionKey { get; set; } = null!; // GameId
    public string RowKey { get; set; } = null!; // Unique ID
    public Guid PlayerId { get; set; }
    public int Score { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}