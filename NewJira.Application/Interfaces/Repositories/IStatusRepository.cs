using System.Collections.Generic;
using System.Threading.Tasks;
using NewJira.Domain.Entities;

#nullable enable
namespace NewJira.Application.Interfaces.Repositories;

public interface IStatusRepository
{
    Task<IEnumerable<Status>> GetAllStatusAsync();

    Task<Status?> GetStatusByIdAsync(int id);

    Task AddStatusAsync(Status status);

    Task UpdateStatusAsync(Status status);

    Task DeleteStatusAsync(int id);
}