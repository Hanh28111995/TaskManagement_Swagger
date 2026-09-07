using System.Collections.Generic;
using System.Threading.Tasks;
using NewJira.Domain.Entities;

#nullable enable
namespace NewJira.Application.Interfaces.Repositories;

public interface IProjectUserRepository
{
    Task<IEnumerable<ProjectUser>> GetAllProjectUsersAsync();

    Task<ProjectUser?> GetByIdsAsync(int membersId, int projectsId);

    Task AddProjectUserAsync(ProjectUser projectUser);

    Task DeleteProjectUserAsync(int membersId, int projectsId);
}