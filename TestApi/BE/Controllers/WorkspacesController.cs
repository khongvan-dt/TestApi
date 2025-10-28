using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AutoApiTester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspaceController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserWorkspaces(int userId)
    {
        var result = await _workspaceService.GetUserWorkspacesAsync(userId);
        return Ok(ApiResponse<IEnumerable<WorkspaceResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _workspaceService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Workspace not found"));

        return Ok(ApiResponse<WorkspaceResponseDto>.SuccessResponse(result));
    }

    [HttpPost("{userId}")]
    public async Task<IActionResult> Create(int userId, [FromBody] CreateWorkspaceDto dto)
    {
        var result = await _workspaceService.CreateAsync(dto, userId);
        return Ok(ApiResponse<WorkspaceResponseDto>.SuccessResponse(result, "Workspace created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkspaceDto dto)
    {
        var result = await _workspaceService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Workspace not found"));

        return Ok(ApiResponse<WorkspaceResponseDto>.SuccessResponse(result, "Workspace updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _workspaceService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse("Workspace not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Workspace deleted successfully"));
    }
}
