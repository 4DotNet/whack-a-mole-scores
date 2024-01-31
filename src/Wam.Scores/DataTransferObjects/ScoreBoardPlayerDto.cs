namespace Wam.Scores.DataTransferObjects;

public record ScoreBoardPlayerDto(Guid PlayerId, string DisplayName, List<ScoreBoardPlayerScoreDto> Scores)
{
    public int ScoreCount => Scores.Count;
    public double AverageScore => Scores.Average(s => s.Score);
}