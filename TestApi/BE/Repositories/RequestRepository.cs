using AutoApiTester.App.Repositories;
using AutoApiTester.Data;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoApiTester.Repositories
{
    public class RequestRepository : Repository<RequestEntity>, IRequestRepository
    {
        private readonly ApplicationDbContext _db; 

        public RequestRepository(ApplicationDbContext context) : base(context)
        {
            _db = context;
        }

        public async Task<IEnumerable<RequestResponseDto>> GetByCollectionIdAsync(int collectionId)
        {
            var requests = await _db.Requests
                .Include(r => r.RequestHeaders)
                .Include(r => r.RequestParams)
                .Include(r => r.RequestBodies)
                .Where(r => r.CollectionId == collectionId)
                .ToListAsync();

            return requests.Select(r => new RequestResponseDto
            {
                Id = r.Id,
                CollectionId = r.CollectionId,
                Name = r.Name,
                Method = r.Method,
                Url = r.Url,
                AuthType = r.AuthType,
                AuthValue = r.AuthValue,
                CreatedAt = r.CreatedAt,
                Headers = r.RequestHeaders?.Select(h => new KeyValuePairDto { Key = h.Key, Value = h.Value }).ToList(),
                QueryParams = r.RequestParams?.Select(p => new KeyValuePairDto { Key = p.Key, Value = p.Value }).ToList(),
                Bodies = r.RequestBodies?.Select(b => new RequestBodyDto { BodyType = b.BodyType, Value = b.Value }).ToList(),
                TestDataName = r.TestDataName,
                TestDataContent = r.TestDataContent
            });
        }

        public async Task<RequestResponseDto?> GetByIdAsync(int id)
        {
            var r = await _db.Requests
                .Include(r => r.RequestHeaders)
                .Include(r => r.RequestParams)
                .Include(r => r.RequestBodies)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (r == null) return null;

            return new RequestResponseDto
            {
                Id = r.Id,
                CollectionId = r.CollectionId,
                Name = r.Name,
                 Method = r.Method,
                Url = r.Url,
                AuthType = r.AuthType,
                AuthValue = r.AuthValue,
                CreatedAt = r.CreatedAt,
                Headers = r.RequestHeaders?.Select(h => new KeyValuePairDto { Key = h.Key, Value = h.Value }).ToList(),
                QueryParams = r.RequestParams?.Select(p => new KeyValuePairDto { Key = p.Key, Value = p.Value }).ToList(),
                Bodies = r.RequestBodies?.Select(b => new RequestBodyDto { BodyType = b.BodyType, Value = b.Value }).ToList()
            };
        }

        public async Task<RequestResponseDto> CreateAsync(CreateRequestDto dto)
        {
            var request = new RequestEntity
            {
                CollectionId = dto.CollectionId,
                Name = dto.Name,
                 Method = dto.Method,
                Url = dto.Url,
                AuthType = dto.AuthType,
                AuthValue = dto.AuthValue,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Requests.AddAsync(request);
            await _db.SaveChangesAsync(); // ensures request.Id is set

            // add headers
            if (dto.Headers != null && dto.Headers.Any())
            {
                var headers = dto.Headers.Select(h => new RequestHeaderEntity
                {
                    RequestId = request.Id,
                    Key = h.Key,
                    Value = h.Value
                });
                await _db.RequestHeaders.AddRangeAsync(headers);
            }

            // add params
            if (dto.QueryParams != null && dto.QueryParams.Any())
            {
                var queryParams = dto.QueryParams.Select(p => new RequestParamEntity
                {
                    RequestId = request.Id,
                    Key = p.Key,
                    Value = p.Value
                });
                await _db.RequestParams.AddRangeAsync(queryParams);
            }

            // add bodies
            if (dto.Bodies != null && dto.Bodies.Any())
            {
                var bodies = dto.Bodies.Select(b => new RequestBodyEntity
                {
                    RequestId = request.Id,
                    BodyType = b.BodyType,
                    Value = b.Value
                });
                await _db.RequestBodies.AddRangeAsync(bodies);
            }

            await _db.SaveChangesAsync();

            // prepare response DTO
            return new RequestResponseDto
            {
                Id = request.Id,
                CollectionId = request.CollectionId,
                Name = request.Name,
                
                Method = request.Method,
                Url = request.Url,
                AuthType = request.AuthType,
                AuthValue = request.AuthValue,
                CreatedAt = request.CreatedAt,
                Headers = dto.Headers,
                QueryParams = dto.QueryParams,
                Bodies = dto.Bodies
            };
        }

        public async Task<RequestResponseDto?> UpdateAsync(int id, UpdateRequestDto dto)
        {
            var request = await _db.Requests
                .Include(r => r.RequestHeaders)
                .Include(r => r.RequestParams)
                .Include(r => r.RequestBodies)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return null;

            request.Name = dto.Name;

            request.Method = dto.Method;
            request.Url = dto.Url;
            request.AuthType = dto.AuthType;
            request.AuthValue = dto.AuthValue;

            // remove existing children
            if (request.RequestHeaders.Any()) _db.RequestHeaders.RemoveRange(request.RequestHeaders);
            if (request.RequestParams.Any()) _db.RequestParams.RemoveRange(request.RequestParams);
            if (request.RequestBodies.Any()) _db.RequestBodies.RemoveRange(request.RequestBodies);

            await _db.SaveChangesAsync();

            // add new children
            if (dto.Headers != null && dto.Headers.Any())
            {
                var headers = dto.Headers.Select(h => new RequestHeaderEntity
                {
                    RequestId = request.Id,
                    Key = h.Key,
                    Value = h.Value
                });
                await _db.RequestHeaders.AddRangeAsync(headers);
            }

            if (dto.QueryParams != null && dto.QueryParams.Any())
            {
                var queryParams = dto.QueryParams.Select(p => new RequestParamEntity
                {
                    RequestId = request.Id,
                    Key = p.Key,
                    Value = p.Value
                });
                await _db.RequestParams.AddRangeAsync(queryParams);
            }

            if (dto.Bodies != null && dto.Bodies.Any())
            {
                var bodies = dto.Bodies.Select(b => new RequestBodyEntity
                {
                    RequestId = request.Id,
                    BodyType = b.BodyType,
                    Value = b.Value
                });
                await _db.RequestBodies.AddRangeAsync(bodies);
            }

            await _db.SaveChangesAsync();

            return new RequestResponseDto
            {
                Id = request.Id,
                CollectionId = request.CollectionId,
                Name = request.Name,
                
                Method = request.Method,
                Url = request.Url,
                AuthType = request.AuthType,
                AuthValue = request.AuthValue,
                CreatedAt = request.CreatedAt,
                Headers = dto.Headers,
                QueryParams = dto.QueryParams,
                Bodies = dto.Bodies
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var request = await _db.Requests.FindAsync(id);
            if (request == null) return false;

            _db.Requests.Remove(request);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task UpdateTestDataContentAsync(UpdateTestDataRequestDto dto)
        {
            var request = await _db.Requests.FindAsync(dto.RequestId);
            if (request == null)
                throw new Exception($"Request with Id {dto.RequestId} not found.");

            request.TestDataContent = dto.NewTestDataContent;

            await _db.SaveChangesAsync();
        }

    }
}
