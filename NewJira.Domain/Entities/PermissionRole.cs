using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NewJira.Domain.Entities;

public class PermissionRole
{
    public int Id { get; set; }            
    public int RoleId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }
    public int PermissionId { get; set; }
    [ForeignKey(nameof(PermissionId))]
    public Permission? Permission { get; set; }
}