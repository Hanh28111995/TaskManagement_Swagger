using NewJira.Domain.Entities;

#nullable enable
namespace NewJira.Application.Interfaces.Repositories;

    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<Permission?> GetByIdAsync(int id);
        Task AddAsync(Permission permission);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }

