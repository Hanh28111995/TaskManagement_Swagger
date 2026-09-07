

using System.ComponentModel.DataAnnotations;

namespace NewJira.Application.DTOs.Comment
{
   public class CommentRequest
    {
        [Required(ErrorMessage = "TaskId không được để trống")]
        public int TaskId { get; set; }
        [Required(ErrorMessage = "ProjectId không được để trống")]
        public int ProjectId { get; set; }

    }
}
