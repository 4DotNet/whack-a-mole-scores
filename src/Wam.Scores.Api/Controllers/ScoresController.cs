using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wam.Scores.DataTransferObjects;

namespace Wam.Scores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> CreateScore(ScoreCreateDto scoreCreateDto)
        {
            //var score = await _scoreService.CreateScore(scoreCreateDto);
            //return CreatedAtAction(nameof(GetScore), new { id = score.Id }, score);
            return await Task.FromResult( Created());
        }
    }
}
