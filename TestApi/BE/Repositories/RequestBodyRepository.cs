using AutoApiTester.App.Repositories;
using AutoApiTester.Data;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoApiTester.Repositories
{
    public class RequestBodyRepository : Repository<RequestBodyEntity>, IRequestBodyRepository
    {
        private readonly ApplicationDbContext _db; 

        public RequestBodyRepository(ApplicationDbContext context) : base(context)
        {
            _db = context;
        }


        public async Task<RequestBodyEntity?> GetByIdAsync(int id)
        {
            try
            {
               
                var requestBody = await _db.RequestBodies
                    .Where(rb => rb.RequestId == id)  
                    .FirstOrDefaultAsync();

                if (requestBody == null)
                {
                    Console.WriteLine($"[GetByIdAsync] Body not found for id: {id}");
                    return null;
                }

 
                return new RequestBodyEntity
                {
                    Id = requestBody.Id,
                    Value = requestBody.Value,
                    BodyType = requestBody.BodyType
                };
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }


    }
}
