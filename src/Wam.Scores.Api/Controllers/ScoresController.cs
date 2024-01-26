using Microsoft.AspNetCore.Mvc;
using Wam.Scores.DataTransferObjects;
using Wam.Scores.Services;

namespace Wam.Scores.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScoresController(IScoresService scoresService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetGameScoreboard(Guid id, CancellationToken cancellationToken)
    {
        var scoreBoard = await scoresService.Scoreboard(id, cancellationToken);
        return Ok(scoreBoard);
    }

    [HttpPost]
    public async Task<IActionResult> CreateScore(ScoreCreateDto scoreCreateDto, CancellationToken cancellationToken)
    {
        var scoresStorageResult = await scoresService.StoreScores(scoreCreateDto, cancellationToken);
        return Ok(scoresStorageResult);
    }
}