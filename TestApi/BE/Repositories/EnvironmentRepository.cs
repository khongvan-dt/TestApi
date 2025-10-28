//using AutoApiTester.App.Repositories;
//using AutoApiTester.Data;
//using AutoApiTester.Models;
//using AutoApiTester.Models.DTOs;
//using Microsoft.EntityFrameworkCore;

//namespace AutoApiTester.Repositories;

//public class EnvironmentRepository : Repository<Environmentss>, IEnvironmentRepository
//{
//    private readonly ApplicationDbContext _context;

//    public EnvironmentRepository(ApplicationDbContext context) : base(context)
//    {
//        _context = context;
//    }

//    // ✅ Lấy danh sách environment theo workspaceId
//    public async Task<IEnumerable<EnvironmentResponseDto>> GetByWorkspaceIdAsync(int workspaceId)
//    {
//        var environments = await _context.Environments
//            .Where(e => e.WorkspaceId == workspaceId)
//            .ToListAsync();

//        return environments.Select(e => new EnvironmentResponseDto
//        {
//            Id = e.Id,
//            WorkspaceId = e.WorkspaceId,
//            Name = e.Name,
//            Variables = e.Variables,
//            IsActive = e.IsActive,
//            CreatedAt = e.CreatedAt
//        });
//    }

//    // ✅ Lấy 1 environment theo Id
//    public async Task<EnvironmentResponseDto?> GetByIdAsync(int id)
//    {
//        var environment = await _context.Environments.FirstOrDefaultAsync(e => e.Id == id);
//        if (environment == null) return null;

//        return new EnvironmentResponseDto
//        {
//            Id = environment.Id,
//            WorkspaceId = environment.WorkspaceId,
//            Name = environment.Name,
//            Variables = environment.Variables,
//            IsActive = environment.IsActive,
//            CreatedAt = environment.CreatedAt
//        };
//    }

//    // ✅ Lấy environment đang active trong workspace
//    public async Task<EnvironmentResponseDto?> GetActiveByWorkspaceIdAsync(int workspaceId)
//    {
//        var environment = await _context.Environments
//            .FirstOrDefaultAsync(e => e.WorkspaceId == workspaceId && e.IsActive);
//        if (environment == null) return null;

//        return new EnvironmentResponseDto
//        {
//            Id = environment.Id,
//            WorkspaceId = environment.WorkspaceId,
//            Name = environment.Name,
//            Variables = environment.Variables,
//            IsActive = environment.IsActive,
//            CreatedAt = environment.CreatedAt
//        };
//    }

//    // ✅ Tạo mới environment
//    public async Task<EnvironmentResponseDto> CreateAsync(CreateEnvironmentDto dto)
//    {
//        // Nếu tạo mới và IsActive = true, tắt các môi trường khác trong workspace
//        if (dto.IsActive)
//        {
//            var existingEnvs = await _context.Environments
//                .Where(e => e.WorkspaceId == dto.WorkspaceId && e.IsActive)
//                .ToListAsync();

//            foreach (var env in existingEnvs)
//                env.IsActive = false;

//            _context.Environments.UpdateRange(existingEnvs);
//        }

//        var environment = new Environmentss
//        {
           
//            WorkspaceId = dto.WorkspaceId,
//            Name = dto.Name,
//            Variables = dto.Variables,
//            IsActive = dto.IsActive,
//            CreatedAt = DateTime.UtcNow
//        };

//        await _context.Environments.AddAsync(environment);
//        await _context.SaveChangesAsync();

//        return new EnvironmentResponseDto
//        {
//            Id = environment.Id,
//            WorkspaceId = environment.WorkspaceId,
//            Name = environment.Name,
//            Variables = environment.Variables,
//            IsActive = environment.IsActive,
//            CreatedAt = environment.CreatedAt
//        };
//    }

//    // ✅ Cập nhật environment
//    public async Task<EnvironmentResponseDto?> UpdateAsync(int id, UpdateEnvironmentDto dto)
//    {
//        var environment = await _context.Environments.FindAsync(id);
//        if (environment == null) return null;

//        // Nếu bật IsActive, thì tắt các environment khác trong workspace
//        if (dto.IsActive && !environment.IsActive)
//        {
//            var existingEnvs = await _context.Environments
//                .Where(e => e.WorkspaceId == environment.WorkspaceId && e.Id != id)
//                .ToListAsync();

//            foreach (var env in existingEnvs)
//                env.IsActive = false;

//            _context.Environments.UpdateRange(existingEnvs);
//        }

//        environment.Name = dto.Name;
//        environment.Variables = dto.Variables;
//        environment.IsActive = dto.IsActive;

//        _context.Environments.Update(environment);
//        await _context.SaveChangesAsync();

//        return new EnvironmentResponseDto
//        {
//            Id = environment.Id,
//            WorkspaceId = environment.WorkspaceId,
//            Name = environment.Name,
//            Variables = environment.Variables,
//            IsActive = environment.IsActive,
//            CreatedAt = environment.CreatedAt
//        };
//    }

//    // ✅ Xóa environment
//    public async Task<bool> DeleteAsync(int id)
//    {
//        var environment = await _context.Environments.FindAsync(id);
//        if (environment == null) return false;

//        _context.Environments.Remove(environment);
//        await _context.SaveChangesAsync();

//        return true;
//    }

//    // ✅ Đặt 1 environment làm active
//    public async Task<bool> SetActiveAsync(int id, int workspaceId)
//    {
//        var environments = await _context.Environments
//            .Where(e => e.WorkspaceId == workspaceId)
//            .ToListAsync();

//        if (!environments.Any()) return false;

//        foreach (var env in environments)
//            env.IsActive = env.Id == id;

//        _context.Environments.UpdateRange(environments);
//        await _context.SaveChangesAsync();

//        return true;
//    }
//}
