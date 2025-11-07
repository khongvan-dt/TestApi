

using AutoApiTester.App.Repositories;
using AutoApiTester.Data;
using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoApiTester.Repositories
{
    public class JobApiTestSuiteRepository : IJobApiTestSuiteRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApiTestSuiteRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<CollectionResponseDto>> GetByUserIdAsync(int userId)
        {
            var collections = await _context.Collections
                .Include(c => c.Requests)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var result = collections.Select(c => new CollectionResponseDto
            {
                Id = c.Id,
                UserId = c.UserId,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                RequestsCount = c.Requests?.Count ?? 0
            }).ToList();

            return result;
        }

        public async Task<List<JobApiTestSuiteEntity>> UpsertAsync(List<JobApiTestSuiteDto> dtoList, string userName)
        {
            var result = new List<JobApiTestSuiteEntity>();

            foreach (var dto in dtoList)
            {
                JobApiTestSuiteEntity entity;

                if (dto.Id == null || dto.Id == 0)
                {
                    // INSERT mới
                    entity = new JobApiTestSuiteEntity
                    {
                        Name = dto.Name ?? "Unnamed Suite",
                        Endpoint = dto.Endpoint,
                        Method = dto.Method,
                        Headers = JsonSerializer.Serialize(dto.Headers),
                        DataBase = JsonSerializer.Serialize(dto.DataBase),
                        Description = dto.Description,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userName,
                        IsActive = true
                    };

                    entity.TestCases = dto.TestCases?.Select(tc => new JobApiTestCaseEntity
                    {
                        CaseName = tc.CaseName ?? "Untitled Case",
                        TestData = JsonSerializer.Serialize(tc.TestData),
                        ExpectedStatus = tc.ExpectedStatus
                    }).ToList() ?? new List<JobApiTestCaseEntity>();

                    _context.JobApiTestSuites.Add(entity);
                }
                else
                {
                    // UPDATE
                    entity = await _context.JobApiTestSuites
                        .Include(s => s.TestCases)
                        .FirstOrDefaultAsync(s => s.Id == dto.Id);

                    if (entity == null)
                        throw new Exception($"JobApiTestSuite with Id={dto.Id} not found.");

                    entity.Name = dto.Name ?? entity.Name;
                    entity.Endpoint = dto.Endpoint;
                    entity.Method = dto.Method;
                    entity.Headers = JsonSerializer.Serialize(dto.Headers);
                    entity.DataBase = JsonSerializer.Serialize(dto.DataBase);
                    entity.Description = dto.Description;
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = userName;

                    foreach (var tcDto in dto.TestCases ?? new List<JobApiTestCaseDto>())
                    {
                        if (tcDto.Id == null || tcDto.Id == 0)
                        {
                            entity.TestCases.Add(new JobApiTestCaseEntity
                            {
                                CaseName = tcDto.CaseName ?? "Untitled Case",
                                TestData = JsonSerializer.Serialize(tcDto.TestData),
                                ExpectedStatus = tcDto.ExpectedStatus
                            });
                        }
                        else
                        {
                            var existing = entity.TestCases.FirstOrDefault(t => t.Id == tcDto.Id);
                            if (existing != null)
                            {
                                existing.CaseName = tcDto.CaseName ?? existing.CaseName;
                                existing.TestData = JsonSerializer.Serialize(tcDto.TestData);
                                existing.ExpectedStatus = tcDto.ExpectedStatus;
                            }
                        }
                    }
                }

                result.Add(entity);
            }

            await _context.SaveChangesAsync();
            return result;
        }

   
    
    }
}
