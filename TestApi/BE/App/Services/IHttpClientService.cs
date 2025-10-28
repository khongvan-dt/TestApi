using AutoApiTester.Models.DTOs;
using System.Threading.Tasks;

namespace AutoApiTester.App.Services
{
    public interface IHttpClientService
    {
        Task<RunRequestResponseDto> ExecuteRequestAsync(RunRequestDto requestDto);
    }
}
