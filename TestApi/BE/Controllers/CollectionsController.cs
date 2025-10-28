using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

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

    //[HttpGet("workspace/{workspaceId}")]
    //public async Task<IActionResult> GetByWorkspaceId(int workspaceId)
    //{
    //    var result = await _collectionService.GetByWorkspaceIdAsync(workspaceId);
    //    return Ok(ApiResponse<IEnumerable<CollectionResponseDto>>.SuccessResponse(result));
    //}

    [HttpGet("{id}")]
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
}
