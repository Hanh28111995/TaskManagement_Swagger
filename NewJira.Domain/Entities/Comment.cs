using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace NewJira.Domain.Entities
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public string ContentComment { get; set; } = string.Empty;

        public int TaskItemId { get; set; }

        public int UserId { get; set; }

        public virtual User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }        
    }
}