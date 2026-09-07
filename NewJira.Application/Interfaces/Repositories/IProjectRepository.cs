using NewJira.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewJira.Application.Interfaces.Repositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();

        Task<Project?> GetProjectByIdAsync(int id);

        Task AddProjectAsync(Project project);

        Task UpdateProjectAsync(Project project);

        Task DeleteProjectAsync(int id);

        Task<bool> SaveProjectChangesAsync();
    }
}
