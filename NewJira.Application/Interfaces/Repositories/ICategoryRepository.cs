
using System.Collections.Generic;
using System.Threading.Tasks;
using NewJira.Domain.Entities;

#nullable enable
namespace NewJira.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllProjectCategoriesAsync();

    Task<Category?> GetProjectCategoryByIdAsync(int id);

    Task AddProjectCategoryAsync(Category projectCategory);

    Task UpdateProjectCategoryAsync(Category projectCategory);

    Task DeleteProjectCategoryAsync(int id);
}