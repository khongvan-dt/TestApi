using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Repositories
{
   
        public interface IRequestBodyRepository : IRepository<RequestBodyEntity>
        {
            
            Task<RequestBodyEntity?> GetByIdAsync(int id);
            
        }
    
}
