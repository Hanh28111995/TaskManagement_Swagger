// Decompiled with JetBrains decompiler
// Type: NewJira.Infrastructure.Data.JiraDbContext
// Assembly: NewJira.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6D2144EA-A17E-413C-8558-40342ACF835B
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Infrastructure.dll

using Microsoft.EntityFrameworkCore;
using NewJira.Domain.Entities;
using System.Linq.Expressions;

#nullable enable
namespace NewJira.Infrastructure.Data;

public class JiraDbContext(DbContextOptions<JiraDbContext> options) : DbContext((DbContextOptions)options)
{
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();

    public DbSet<ChatRoomMember> ChatRoomMembers => Set<ChatRoomMember>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<PermissionRole> PermissionRoles => Set<PermissionRole>();

    public DbSet<User> Users => this.Set<User>();

    public DbSet<Role> Roles => Set<Role>();    

    public DbSet<Project> Projects => this.Set<Project>();

    public DbSet<TaskItem> TaskItems => this.Set<TaskItem>();

    public DbSet<Status> Statuses => this.Set<Status>();

    public DbSet<Priority> Priorities => this.Set<Priority>();

    public DbSet<Category> Categories => this.Set<Category>();

    public DbSet<TaskType> TaskTypes => this.Set<TaskType>();

    public DbSet<Comment> Comments => this.Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChatRoomMember>(entity =>
        {
            entity.HasKey(m => new { m.RoomId, m.UserId });

            entity.HasOne(m => m.Room).WithMany(r => r.Members)
                  .HasForeignKey(m => m.RoomId).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.User).WithMany()
                  .HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasOne(m => m.Room).WithMany(r => r.Messages)
                  .HasForeignKey(m => m.RoomId).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Sender).WithMany()
                  .HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(m => new { m.RoomId, m.SentAt });
        });

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasOne(r => r.CreatedBy).WithMany()
                  .HasForeignKey(r => r.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PermissionRole>(entity =>
        {
            entity.HasOne(pr => pr.Role).WithMany(r => r.PermissionRoles)
                  .HasForeignKey(pr => pr.RoleId).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pr => pr.Permission).WithMany()
                  .HasForeignKey(pr => pr.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.Role)
                  .WithMany()
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(t => t.TokenHash).IsUnique();
            entity.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasOne(t => t.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectUser>(entity =>
        {
            entity.HasKey(pu => new { pu.MembersId, pu.ProjectsId });

            entity.HasOne(pu => pu.Member)
                  .WithMany()
                  .HasForeignKey(pu => pu.MembersId);

            entity.HasOne(pu => pu.Project)
                  .WithMany(p => p.ProjectUsers)
                  .HasForeignKey(pu => pu.ProjectsId);
        });

        modelBuilder.Entity<TaskItem>().Property(t => t.EstimateHours).HasPrecision(18, 2);
        modelBuilder.Entity<TaskItem>().Property(t => t.TimeTrackingSpentHours).HasPrecision(18, 2);
        modelBuilder.Entity<TaskItem>().Property(t => t.TimeTrackingRemainingHours).HasPrecision(18, 2);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Creator)
            .WithMany()
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Project>()
    .HasOne(p => p.Category)
    .WithMany()
    .HasForeignKey(p => p.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);
    }
}
