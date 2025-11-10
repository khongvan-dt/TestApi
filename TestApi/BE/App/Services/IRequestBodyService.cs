using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Services
{
    public interface IRequestBodyService
    {
         Task<RequestResponseDto?> GetByIdAsync(int id);
        

    }
}
