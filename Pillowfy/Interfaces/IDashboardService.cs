using Pillowfy.DTOs;
namespace Pillowfy.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetClientDashboardAsync(string userId);
        Task<List<OccupancyDto>> GetOccupancyAsync();

    }
}