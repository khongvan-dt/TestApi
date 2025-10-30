using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoApiTester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpGet("collection/{collectionId}")]
    public async Task<IActionResult> GetByCollectionId(int collectionId)
    {
        var result = await _requestService.GetByCollectionIdAsync(collectionId);
        return Ok(ApiResponse<IEnumerable<RequestResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _requestService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Request not found"));

        return Ok(ApiResponse<RequestResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
    {
        var result = await _requestService.CreateAsync(dto);
        return Ok(ApiResponse<RequestResponseDto>.SuccessResponse(result, "Request created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRequestDto dto)
    {
        var result = await _requestService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Request not found"));

        return Ok(ApiResponse<RequestResponseDto>.SuccessResponse(result, "Request updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _requestService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse("Request not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Request deleted successfully"));
    }

    [HttpPost("updateTestdata")]
    [Authorize]
    public async Task<IActionResult> UpdateTestdataRequest([FromBody] UpdateTestDataRequestDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.NewTestDataContent))
            return BadRequest(ApiResponse<object>.ErrorResponse("Request data cannot be null or empty"));

        // Lấy userId từ claim
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        if (!int.TryParse(userIdClaim, out var userId))
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid user ID"));

        try
        {
            await _requestService.UpdateTestDataContentAsync(dto);
            return Ok(ApiResponse<object>.SuccessResponse("Request updated successfully"));
        }
        catch (Exception ex)
        {
             return StatusCode(500, ApiResponse<object>.ErrorResponse("Internal server error"));
        }
    }

}
