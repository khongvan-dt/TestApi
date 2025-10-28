using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoApiTester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionService _collectionService;

    public CollectionController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

  

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _collectionService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Collection not found"));

        return Ok(ApiResponse<CollectionResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCollectionDto dto)
    {
        var result = await _collectionService.CreateAsync(dto);
        return Ok(ApiResponse<CollectionResponseDto>.SuccessResponse(result, "Collection created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCollectionDto dto)
    {
        var result = await _collectionService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Collection not found"));

        return Ok(ApiResponse<CollectionResponseDto>.SuccessResponse(result, "Collection updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _collectionService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse("Collection not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Collection deleted successfully"));
    }
    [HttpPost("myCollections")]
    [Authorize]
    public async Task<IActionResult> GetMyCollections() 
    {
         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid user ID"));
        }

        try
        {
            var result = await _collectionService.GetByUserIdAsync(userId);

            return Ok(ApiResponse<List<CollectionResponseDto>>.SuccessResponse(
                result,
                $"Retrieved {result.Count} collection(s)"
            ));
        }
        catch (Exception ex)
        {
             return StatusCode(500, ApiResponse<object>.ErrorResponse("Failed to retrieve collections"));
        }
    }
}
