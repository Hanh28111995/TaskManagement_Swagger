// Decompiled with JetBrains decompiler
// Type: NewJira.Infrastructure.Repositories.TaskTypeRepository
// Assembly: NewJira.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6D2144EA-A17E-413C-8558-40342ACF835B
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Infrastructure.dll

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;

namespace NewJira.Infrastructure.Repositories
{
    public class TaskTypeRepository : ITaskTypeRepository
    {
        private readonly JiraDbContext _context;

        public TaskTypeRepository(JiraDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskType>> GetAllTaskTypesAsync()
        {
            return await _context.TaskTypes.ToListAsync();
        }

        public async Task<TaskType?> GetTaskTypeByIdAsync(int id)
        {
            return await _context.TaskTypes.FindAsync(id);
        }

        public async Task AddTaskTypeAsync(TaskType taskType)
        {
            await _context.TaskTypes.AddAsync(taskType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskTypeAsync(TaskType taskType)
        {
            _context.TaskTypes.Update(taskType);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskTypeAsync(int id)
        {
            var taskType = await _context.TaskTypes.FindAsync(id);
            if (taskType == null)
                return;

            _context.TaskTypes.Remove(taskType);
            await _context.SaveChangesAsync();
        }
    }
}