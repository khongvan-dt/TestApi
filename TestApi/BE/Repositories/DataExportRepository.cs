using AutoApiTester.App.Repositories;
using AutoApiTester.Data;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using Microsoft.EntityFrameworkCore;

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
        // 1️⃣ User info
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

        // 2️⃣ Collections + nested requests
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

                  Body = r.RequestBodies
                      .Select(b => new RequestBodyDto { BodyType = b.BodyType, Content = b.Content })
                      .FirstOrDefault()
              })
              .ToList()
      })
      .ToListAsync();

        // 3️⃣ Summary (không còn TestData)
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


    // ✅ Lấy tất cả collections với nested data
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

                        Body = r.RequestBodies
                            .Select(b => new RequestBodyDto
                            {
                                BodyType = b.BodyType,
                                Content = b.Content
                            })
                            .FirstOrDefault()
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<ImportResultDto> ImportUserDataAsync(int userId, UserDataExportDto importData)
    {
        var result = new ImportResultDto();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1️⃣ Validate user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                result.Success = false;
                result.ErrorMessage = "User not found";
                return result;
            }

            // 2️⃣ Import Collections
            foreach (var collectionDto in importData.Collections)
            {
                // Check if collection already exists (optional - skip duplicates)
                var existingCollection = await _context.Collections
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == collectionDto.Name);

                Collection collection;

                if (existingCollection != null)
                {
                    // Update existing collection
                    existingCollection.Description = collectionDto.Description;
                    collection = existingCollection;
                    result.UpdatedCollections++;
                }
                else
                {
                    // Create new collection
                    collection = new Collection
                    {
                        UserId = userId,
                        Name = collectionDto.Name,
                        Description = collectionDto.Description,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Collections.Add(collection);
                    await _context.SaveChangesAsync(); // Save to get Collection ID
                    result.ImportedCollections++;
                }

                // 3️⃣ Import Requests for this collection
                foreach (var requestDto in collectionDto.Requests)
                {
                    // Check if request already exists (optional)
                    var existingRequest = await _context.Requests
                        .FirstOrDefaultAsync(r => r.CollectionId == collection.Id &&
                                                r.Name == requestDto.Name &&
                                                r.Url == requestDto.Url);

                    Request request;

                    if (existingRequest != null)
                    {
                        // Update existing request
                        existingRequest.Method = requestDto.Method;
                        existingRequest.Url = requestDto.Url;
                        existingRequest.AuthType = requestDto.AuthType;
                        existingRequest.AuthValue = requestDto.AuthValue;

                        // Remove old related data
                        _context.RequestParams.RemoveRange(existingRequest.RequestParams);
                        _context.RequestHeaders.RemoveRange(existingRequest.RequestHeaders);
                        _context.RequestBodies.RemoveRange(existingRequest.RequestBodies);

                        request = existingRequest;
                        result.UpdatedRequests++;
                    }
                    else
                    {
                        // Create new request
                        request = new Request
                        {
                            CollectionId = collection.Id,
                            Name = requestDto.Name,
                            Method = requestDto.Method,
                            Url = requestDto.Url,
                            AuthType = requestDto.AuthType,
                            AuthValue = requestDto.AuthValue,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Requests.Add(request);
                        await _context.SaveChangesAsync(); // Save to get Request ID
                        result.ImportedRequests++;
                    }

                    // 4️⃣ Import Query Params
                    if (requestDto.QueryParams?.Any() == true)
                    {
                        foreach (var paramDto in requestDto.QueryParams)
                        {
                            var param = new RequestParam
                            {
                                RequestId = request.Id,
                                Key = paramDto.Key,
                                Value = paramDto.Value
                            };
                            _context.RequestParams.Add(param);
                        }
                    }

                    // 5️⃣ Import Headers
                    if (requestDto.Headers?.Any() == true)
                    {
                        foreach (var headerDto in requestDto.Headers)
                        {
                            var header = new RequestHeader
                            {
                                RequestId = request.Id,
                                Key = headerDto.Key,
                                Value = headerDto.Value
                            };
                            _context.RequestHeaders.Add(header);
                        }
                    }

                    // 6️⃣ Import Body
                    if (requestDto.Body != null)
                    {
                        var body = new RequestBody
                        {
                            RequestId = request.Id,
                            BodyType = requestDto.Body.BodyType,
                            Content = requestDto.Body.Content
                        };
                        _context.RequestBodies.Add(body);
                    }
                }
            }

            // Final save
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

    public async Task<SaveRequestResultDto> SaveRequestAsync(int userId, SaveRequestDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1️⃣ Verify collection belongs to user
            var collection = await _context.Collections
                .FirstOrDefaultAsync(c => c.Id == dto.CollectionId && c.UserId == userId);

            if (collection == null)
            {
                return new SaveRequestResultDto
                {
                    Success = false,
                    Message = "Collection not found or access denied"
                };
            }

            Request request;
            bool isNew = false;

            // 2️⃣ Check if updating existing request or creating new
            if (dto.RequestId.HasValue && dto.RequestId.Value > 0)
            {
                // UPDATE existing request
                request = await _context.Requests
                    .Include(r => r.RequestParams)
                    .Include(r => r.RequestHeaders)
                    .Include(r => r.RequestBodies)
                    .FirstOrDefaultAsync(r => r.Id == dto.RequestId.Value && r.CollectionId == dto.CollectionId);

                if (request == null)
                {
                    return new SaveRequestResultDto
                    {
                        Success = false,
                        Message = "Request not found"
                    };
                }

                // Update properties
                request.Name = dto.Name;
                request.Method = dto.Method;
                request.Url = dto.Url;
                request.AuthType = dto.AuthType;
                request.AuthValue = dto.AuthValue;

                // Remove old related data
                _context.RequestParams.RemoveRange(request.RequestParams);
                _context.RequestHeaders.RemoveRange(request.RequestHeaders);
                _context.RequestBodies.RemoveRange(request.RequestBodies);
            }
            else
            {
                // CREATE new request
                isNew = true;
                request = new Request
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
                await _context.SaveChangesAsync(); // Get RequestId
            }

            // 3️⃣ Add query params
            if (dto.QueryParams?.Any() == true)
            {
                foreach (var param in dto.QueryParams.Where(p => !string.IsNullOrEmpty(p.Key)))
                {
                    _context.RequestParams.Add(new RequestParam
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
                    _context.RequestHeaders.Add(new RequestHeader
                    {
                        RequestId = request.Id,
                        Key = header.Key,
                        Value = header.Value
                    });
                }
            }

            // 5️⃣ Add body
            if (dto.Body != null && !string.IsNullOrWhiteSpace(dto.Body.Content))
            {
                _context.RequestBodies.Add(new RequestBody
                {
                    RequestId = request.Id,
                    BodyType = dto.Body.BodyType,
                    Content = dto.Body.Content
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new SaveRequestResultDto
            {
                Success = true,
                RequestId = request.Id,
                IsNew = isNew,
                Message = isNew ? "Request created successfully" : "Request updated successfully"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
 
            return new SaveRequestResultDto
            {
                Success = false,
                Message = $"Failed to save request: {ex.Message}"
            };
        }
    }
}
