//using AutoApiTester.App.Services;
//using AutoApiTester.Models.DTOs;
//using Microsoft.AspNetCore.Mvc;

//namespace AutoApiTester.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class EnvironmentController : ControllerBase
//{
//    private readonly IEnvironmentService _environmentService;

//    public EnvironmentController(IEnvironmentService environmentService)
//    {
//        _environmentService = environmentService;
//    }

//    [HttpGet("workspace/{workspaceId}")]
//    public async Task<IActionResult> GetByWorkspaceId(int workspaceId)
//    {
//        var result = await _environmentService.GetByWorkspaceIdAsync(workspaceId);
//        return Ok(ApiResponse<IEnumerable<EnvironmentResponseDto>>.SuccessResponse(result));
//    }

//    [HttpGet("{id}")]
//    public async Task<IActionResult> GetById(int id)
//    {
//        var result = await _environmentService.GetByIdAsync(id);
//        if (result == null)
//            return NotFound(ApiResponse<object>.ErrorResponse("Environment not found"));

//        return Ok(ApiResponse<EnvironmentResponseDto>.SuccessResponse(result));
//    }

//    [HttpPost]
//    public async Task<IActionResult> Create([FromBody] CreateEnvironmentDto dto)
//    {
//        var result = await _environmentService.CreateAsync(dto);
//        return Ok(ApiResponse<EnvironmentResponseDto>.SuccessResponse(result, "Environment created successfully"));
//    }

//    [HttpPut("{id}")]
//    public async Task<IActionResult> Update(int id, [FromBody] UpdateEnvironmentDto dto)
//    {
//        var result = await _environmentService.UpdateAsync(id, dto);
//        if (result == null)
//            return NotFound(ApiResponse<object>.ErrorResponse("Environment not found"));

//        return Ok(ApiResponse<EnvironmentResponseDto>.SuccessResponse(result, "Environment updated successfully"));
//    }

//    [HttpDelete("{id}")]
//    public async Task<IActionResult> Delete(int id)
//    {
//        var success = await _environmentService.DeleteAsync(id);
//        if (!success)
//            return NotFound(ApiResponse<object>.ErrorResponse("Environment not found"));

//        return Ok(ApiResponse<object>.SuccessResponse(null, "Environment deleted successfully"));
//    }

//    [HttpPut("{id}/set-active/{workspaceId}")]
//    public async Task<IActionResult> SetActive(int id, int workspaceId)
//    {
//        var success = await _environmentService.SetActiveAsync(id, workspaceId);
//        if (!success)
//            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to set environment active"));

//        return Ok(ApiResponse<object>.SuccessResponse(null, "Environment set active successfully"));
//    }
//}
