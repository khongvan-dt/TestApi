using AutoApiTester.App.Repositories;
using AutoApiTester.Data;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AutoApiTester.Repositories;

public class DataExportRepository : IDataExportRepository
{
    private readonly ApplicationDbContext _context;

    public DataExportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> GetAllDataAsync()
    {
        var users = await _context.Users
            .Select(u => new UserDataDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                IsActive = true,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        var collections = await GetAllCollectionsWithNestedDataAsync();

        return new
        {
            Users = users,
            Collections = collections
        };
    }

    public async Task<UserDataExportDto> GetUserDataAsync(int userId)
    {
        // 1️ User info
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserDataDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                IsActive = true,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync();

        // 2️ Collections + nested requests
        var collections = await _context.Collections
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CollectionDataDto
            {
                Id = c.Id,
                UserId = c.UserId,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                Requests = c.Requests
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new RequestDataDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Method = r.Method,
                        Url = r.Url,
                        AuthType = r.AuthType,
                        AuthValue = r.AuthValue,
                        CreatedAt = r.CreatedAt,
                        Headers = r.RequestHeaders
                            .Select(h => new RequestHeaderDto { Key = h.Key, Value = h.Value })
                            .ToList(),
                        QueryParams = r.RequestParams
                            .Select(p => new RequestParamDto { Key = p.Key, Value = p.Value })
                            .ToList(),

                        Bodies = r.RequestBodies
                            .Select(b => new RequestBodyDto
                            {
                                Id = b.Id,
                                BodyType = b.BodyType,
                                Value = b.Value,
                                Type=b.Type
                            })
                            .ToList(),

                        DataBaseTest = r.TestDataContent
                    })
                    .ToList()
            })
            .ToListAsync();

        // 3️⃣ Summary
        var totalRequests = collections.Sum(c => c.Requests.Count);

        return new UserDataExportDto
        {
            User = user,
            Collections = collections,
            Summary = new DataSummaryDto
            {
                TotalCollections = collections.Count,
                TotalRequests = totalRequests
            }
        };
    }

    private async Task<List<CollectionDataDto>> GetAllCollectionsWithNestedDataAsync()
    {
        return await _context.Collections
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CollectionDataDto
            {
                Id = c.Id,
                UserId = c.UserId,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,

                Requests = c.Requests
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new RequestDataDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Method = r.Method,
                        Url = r.Url,
                        AuthType = r.AuthType,
                        AuthValue = r.AuthValue,
                        CreatedAt = r.CreatedAt,

                        Headers = r.RequestHeaders
                            .Select(h => new RequestHeaderDto
                            {
                                Key = h.Key,
                                Value = h.Value
                            })
                            .ToList(),

                        QueryParams = r.RequestParams
                            .Select(p => new RequestParamDto
                            {
                                Key = p.Key,
                                Value = p.Value
                            })
                            .ToList(),

                      
                        Bodies = r.RequestBodies
                            .Select(b => new RequestBodyDto
                            {
                                Id = b.Id,
                                BodyType = b.BodyType,
                                Value = b.Value
                            })
                            .ToList(),

                        DataBaseTest = r.TestDataContent
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    // Import - Xử lý Bodies thay vì Body
    public async Task<ImportResultDto> ImportUserDataAsync(int userId, UserDataExportDto importData)
    {
        var result = new ImportResultDto();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1️ Validate user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                result.Success = false;
                result.ErrorMessage = "User not found";
                return result;
            }

            // 2️ Import Collections
            foreach (var collectionDto in importData.Collections)
            {
                var existingCollection = await _context.Collections
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == collectionDto.Name);

                CollectionEntity collection;

                if (existingCollection != null)
                {
                    existingCollection.Description = collectionDto.Description;
                    collection = existingCollection;
                    result.UpdatedCollections++;
                }
                else
                {
                    collection = new CollectionEntity
                    {
                        UserId = userId,
                        Name = collectionDto.Name,
                        Description = collectionDto.Description,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Collections.Add(collection);
                    await _context.SaveChangesAsync();
                    result.ImportedCollections++;
                }

                // 3️ Import Requests
                foreach (var requestDto in collectionDto.Requests)
                {
                    var existingRequest = await _context.Requests
                        .Include(r => r.RequestParams)
                        .Include(r => r.RequestHeaders)
                        .Include(r => r.RequestBodies)
                        .FirstOrDefaultAsync(r => r.CollectionId == collection.Id &&
                                                r.Name == requestDto.Name &&
                                                r.Url == requestDto.Url);

                    RequestEntity request;

                    if (existingRequest != null)
                    {
                        existingRequest.Method = requestDto.Method;
                        existingRequest.Url = requestDto.Url;
                        existingRequest.AuthType = requestDto.AuthType;
                        existingRequest.AuthValue = requestDto.AuthValue;
                        existingRequest.TestDataContent = requestDto.DataBaseTest;

                        // Xóa dữ liệu cũ
                        _context.RequestParams.RemoveRange(existingRequest.RequestParams);
                        _context.RequestHeaders.RemoveRange(existingRequest.RequestHeaders);
                        _context.RequestBodies.RemoveRange(existingRequest.RequestBodies);

                        request = existingRequest;
                        result.UpdatedRequests++;
                    }
                    else
                    {
                        request = new RequestEntity
                        {
                            CollectionId = collection.Id,
                            Name = requestDto.Name,
                            Method = requestDto.Method,
                            Url = requestDto.Url,
                            AuthType = requestDto.AuthType,
                            AuthValue = requestDto.AuthValue,
                            TestDataContent = requestDto.DataBaseTest,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Requests.Add(request);
                        await _context.SaveChangesAsync();
                        result.ImportedRequests++;
                    }

                    // 4️ Import Query Params
                    if (requestDto.QueryParams?.Any() == true)
                    {
                        foreach (var paramDto in requestDto.QueryParams)
                        {
                            _context.RequestParams.Add(new RequestParamEntity
                            {
                                RequestId = request.Id,
                                Key = paramDto.Key,
                                Value = paramDto.Value
                            });
                        }
                    }

                    // 5️ Import Headers
                    if (requestDto.Headers?.Any() == true)
                    {
                        foreach (var headerDto in requestDto.Headers)
                        {
                            _context.RequestHeaders.Add(new RequestHeaderEntity
                            {
                                RequestId = request.Id,
                                Key = headerDto.Key,
                                Value = headerDto.Value
                            });
                        }
                    }

                    // 6 Import NHIỀU Bodies thay vì chỉ 1
                    if (requestDto.Bodies?.Any() == true)
                    {
                        foreach (var bodyDto in requestDto.Bodies)
                        {
                            if (string.IsNullOrWhiteSpace(bodyDto.Value))
                                continue;

                            _context.RequestBodies.Add(new RequestBodyEntity
                            {
                                RequestId = request.Id,
                                BodyType = bodyDto.BodyType,
                                Value = bodyDto.Value
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Success = true;
            result.TotalProcessed = result.ImportedCollections + result.UpdatedCollections;

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Success = false;
            result.ErrorMessage = $"Import failed: {ex.Message}";
            return result;
        }
    }

    public async Task<List<SaveRequestResultDto>> SaveRequestsAsync(int userId, List<SaveRequestDto> dtos)
    {
        var results = new List<SaveRequestResultDto>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var dto in dtos)
            {
                // 1️⃣ Verify collection belongs to user
                var collection = await _context.Collections
                    .FirstOrDefaultAsync(c => c.Id == dto.CollectionId && c.UserId == userId);

                if (collection == null)
                {
                    results.Add(new SaveRequestResultDto
                    {
                        Success = false,
                        Message = $"Collection {dto.CollectionId} not found or access denied"
                    });
                    continue;
                }

                RequestEntity request;
                bool isNew = false;

                // 2️⃣ Check if updating existing request or creating new
                if (dto.RequestId.HasValue && dto.RequestId.Value > 0)
                {
                    request = await _context.Requests
                        .Include(r => r.RequestParams)
                        .Include(r => r.RequestHeaders)
                        .Include(r => r.RequestBodies)
                        .FirstOrDefaultAsync(r => r.Id == dto.RequestId.Value && r.CollectionId == dto.CollectionId);

                    if (request == null)
                    {
                        results.Add(new SaveRequestResultDto
                        {
                            Success = false,
                            Message = $"Request {dto.RequestId} not found"
                        });
                        continue;
                    }

                    // Update properties
                    request.Name = dto.Name;
                    request.Method = dto.Method;
                    request.Url = dto.Url;
                    request.AuthType = dto.AuthType;
                    request.AuthValue = dto.AuthValue;

                    // Chỉ xóa params và headers
                    _context.RequestParams.RemoveRange(request.RequestParams);
                    _context.RequestHeaders.RemoveRange(request.RequestHeaders);
                }
                else
                {
                    // CREATE new request
                    isNew = true;
                    request = new RequestEntity
                    {
                        CollectionId = dto.CollectionId,
                        Name = dto.Name,
                        Method = dto.Method,
                        Url = dto.Url,
                        AuthType = dto.AuthType,
                        AuthValue = dto.AuthValue,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Requests.Add(request);
                    await _context.SaveChangesAsync();
                }

                // 3️⃣ Add query params
                if (dto.QueryParams?.Any() == true)
                {
                    foreach (var param in dto.QueryParams.Where(p => !string.IsNullOrEmpty(p.Key)))
                    {
                        _context.RequestParams.Add(new RequestParamEntity
                        {
                            RequestId = request.Id,
                            Key = param.Key,
                            Value = param.Value
                        });
                    }
                }

                // 4️⃣ Add headers
                if (dto.Headers?.Any() == true)
                {
                    foreach (var header in dto.Headers.Where(h => !string.IsNullOrEmpty(h.Key)))
                    {
                        _context.RequestHeaders.Add(new RequestHeaderEntity
                        {
                            RequestId = request.Id,
                            Key = header.Key,
                            Value = header.Value
                        });
                    }
                }

                if (dto.Body != null && !string.IsNullOrWhiteSpace(dto.Body.Value))
                {
                    if (dto.Body.Id > 0)
                    {
                        // UPDATE existing body
                        var existingBody = await _context.RequestBodies
                            .FirstOrDefaultAsync(b => b.Id == dto.Body.Id && b.RequestId == request.Id);

                        if (existingBody != null)
                        {
                            existingBody.BodyType = dto.Body.BodyType;
                            existingBody.Value = dto.Body.Value;
                            _context.RequestBodies.Update(existingBody);
                        }
                        else
                        {

                            _context.RequestBodies.Add(new RequestBodyEntity
                            {
                                RequestId = request.Id,
                                BodyType = dto.Body.BodyType,
                                Value = dto.Body.Value
                            });
                        }
                    }
                    else
                    {
                        // INSERT new body
                        _context.RequestBodies.Add(new RequestBodyEntity
                        {
                            RequestId = request.Id,
                            BodyType = dto.Body.BodyType,
                            Value = dto.Body.Value
                        });
                    }
                }
            }

            var requestIds = dtos
                .Where(d => d.RequestId.HasValue && d.RequestId.Value > 0)
                .Select(d => d.RequestId.Value)
                .Distinct()
                .ToList();

            foreach (var requestId in requestIds)
            {
                var bodyIdsInDto = dtos
                    .Where(d => d.RequestId == requestId
                        && d.Body != null
                        && d.Body.Id > 0
                        && !string.IsNullOrWhiteSpace(d.Body.Value))  
                    .Select(d => d.Body.Id)
                    .ToList();

            
                var bodiesToDelete = await _context.RequestBodies
                    .Where(b => b.RequestId == requestId && !bodyIdsInDto.Contains(b.Id))
                    .ToListAsync();

                if (bodiesToDelete.Any())
                {
                    _context.RequestBodies.RemoveRange(bodiesToDelete);
                }
            }

            await _context.SaveChangesAsync();

            foreach (var dto in dtos)
            {
                if (dto.RequestId.HasValue && dto.RequestId.Value > 0)
                {
                    results.Add(new SaveRequestResultDto
                    {
                        Success = true,
                        RequestId = dto.RequestId.Value,
                        IsNew = false,
                        Message = "Request updated successfully"
                    });
                }
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            results.Add(new SaveRequestResultDto
            {
                Success = false,
                Message = $"Failed to save requests: {ex.Message}"
            });
        }

        return results;
    }



    public async Task<SaveRequestResultDto> DeleteRequestAsync(int userId, int requestId)
    {
        try
        {
            var request = await _context.Requests
                .Include(r => r.RequestParams)
                .Include(r => r.RequestHeaders)
                .Include(r => r.RequestBodies)
                .Include(r => r.Collection)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return new SaveRequestResultDto { Success = false, Message = "Request not found" };

            if (request.Collection.UserId != userId)
                return new SaveRequestResultDto { Success = false, Message = "Access denied" };

            // Xóa các bảng con
            _context.RequestParams.RemoveRange(request.RequestParams);
            _context.RequestHeaders.RemoveRange(request.RequestHeaders);
            _context.RequestBodies.RemoveRange(request.RequestBodies);

            // Xóa request chính
            _context.Requests.Remove(request);

            await _context.SaveChangesAsync();

            return new SaveRequestResultDto { Success = true, Message = "Request deleted successfully" };
        }
        catch (Exception ex)
        {
            return new SaveRequestResultDto { Success = false, Message = $"Failed to delete request: {ex.Message}" };
        }
    }
}