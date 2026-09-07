#nullable enable
using NewJira.Domain.Entities;
namespace NewJira.Application.Interfaces.Repositories;

public interface IPriorityRepository
{
    Task<IEnumerable<Priority>> GetAllPrioritiesAsync();

    Task<Priority?> GetPriorityByIdAsync(int id);

    Task AddPriorityAsync(Priority priority);

    Task UpdatePriorityAsync(Priority priority);

    Task DeletePriorityAsync(int id);
}
