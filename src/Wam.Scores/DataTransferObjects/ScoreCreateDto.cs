namespace Wam.Scores.DataTransferObjects;

public class ScoreCreateDto
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public int Milliseconds { get; set; }
}