using Microsoft.AspNetCore.Mvc;
using AutoApiTester.Data;
using AutoApiTester.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AutoApiTester.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _ctx;
    public UsersController(ApplicationDbContext ctx) => _ctx = ctx;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _ctx.Users.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var u = await _ctx.Users.FindAsync(id);
        if (u == null) return NotFound();
        return Ok(u);
    }
}
