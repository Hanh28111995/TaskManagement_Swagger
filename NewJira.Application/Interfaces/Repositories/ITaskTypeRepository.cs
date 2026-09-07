using System.Collections.Generic;
using System.Threading.Tasks;
using NewJira.Domain.Entities;

#nullable enable
namespace NewJira.Application.Interfaces.Repositories;

public interface ITaskTypeRepository
{
    Task<IEnumerable<TaskType>> GetAllTaskTypesAsync();

    Task<TaskType?> GetTaskTypeByIdAsync(int id);

    Task AddTaskTypeAsync(TaskType taskType);

    Task UpdateTaskTypeAsync(TaskType taskType);

    Task DeleteTaskTypeAsync(int id);
}