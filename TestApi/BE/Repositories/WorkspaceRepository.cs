//using AutoApiTester.App.Repositories;
//using AutoApiTester.Data;
//using AutoApiTester.Models;
//using AutoApiTester.Models.DTOs;
//using Microsoft.EntityFrameworkCore;

//namespace AutoApiTester.Repositories
//{
//    public class WorkspaceRepository : Repository<Workspace>, IWorkspaceRepository
//    {
//        private readonly ApplicationDbContext _context;

//        public WorkspaceRepository(ApplicationDbContext context) : base(context)
//        {
//            _context = context;
//        }

//        // ✅ Lấy danh sách Workspace của 1 User
//        public async Task<IEnumerable<WorkspaceResponseDto>> GetUserWorkspacesAsync(int userId)
//        {
//            var workspaces = await _context.Workspaces
//                .Include(w => w.Collections)
//                .Include(w => w.Environments)
//                .Where(w => w.UserId == userId)
//                .ToListAsync();

//            return workspaces.Select(w => new WorkspaceResponseDto
//            {
//                Id = w.Id,
//                Name = w.Name,
//                Description = w.Description,
//                UserId = w.UserId,
//                CreatedAt = w.CreatedAt,
//                CollectionsCount = w.Collections?.Count ?? 0,
//                EnvironmentsCount = w.Environments?.Count ?? 0
//            });
//        }

//        // ✅ Lấy 1 Workspace theo Id
//        public async Task<WorkspaceResponseDto?> GetByIdAsync(int id)
//        {
//            var workspace = await _context.Workspaces
//                .Include(w => w.Collections)
//                .Include(w => w.Environments)
//                .FirstOrDefaultAsync(w => w.Id == id);

//            if (workspace == null) return null;

//            return new WorkspaceResponseDto
//            {
//                Id = workspace.Id,
//                Name = workspace.Name,
//                Description = workspace.Description,
//                UserId = workspace.UserId,
//                CreatedAt = workspace.CreatedAt,
//                CollectionsCount = workspace.Collections?.Count ?? 0,
//                EnvironmentsCount = workspace.Environments?.Count ?? 0
//            };
//        }

//        // ✅ Tạo mới Workspace
//        public async Task<WorkspaceResponseDto> CreateAsync(CreateWorkspaceDto dto, int userId)
//        {
//            var workspace = new Workspace
//            {
               
//                Name = dto.Name,
//                Description = dto.Description,
//                UserId = userId,
//                CreatedAt = DateTime.UtcNow
//            };

//            await _context.Workspaces.AddAsync(workspace);
//            await _context.SaveChangesAsync();

//            return new WorkspaceResponseDto
//            {
//                Id = workspace.Id,
//                Name = workspace.Name,
//                Description = workspace.Description,
//                UserId = workspace.UserId,
//                CreatedAt = workspace.CreatedAt,
//                CollectionsCount = 0,
//                EnvironmentsCount = 0
//            };
//        }

//        // ✅ Cập nhật Workspace
//        public async Task<WorkspaceResponseDto?> UpdateAsync(int id, UpdateWorkspaceDto dto)
//        {
//            var workspace = await _context.Workspaces.FindAsync(id);
//            if (workspace == null) return null;

//            workspace.Name = dto.Name;
//            workspace.Description = dto.Description;

//            _context.Workspaces.Update(workspace);
//            await _context.SaveChangesAsync();

//            return new WorkspaceResponseDto
//            {
//                Id = workspace.Id,
//                Name = workspace.Name,
//                Description = workspace.Description,
//                UserId = workspace.UserId,
//                CreatedAt = workspace.CreatedAt,
//                CollectionsCount = await _context.Collections.CountAsync(c => c.WorkspaceId == id),
//                EnvironmentsCount = await _context.Environments.CountAsync(e => e.WorkspaceId == id)
//            };
//        }

//        // ✅ Xóa Workspace
//        public async Task<bool> DeleteAsync(int id)
//        {
//            var workspace = await _context.Workspaces.FindAsync(id);
//            if (workspace == null) return false;

//            _context.Workspaces.Remove(workspace);
//            await _context.SaveChangesAsync();

//            return true;
//        }
//    }
//}
