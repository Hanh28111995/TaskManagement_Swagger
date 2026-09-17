using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewJira.Application.DTOs.Chat
{
    // CreateDirectRoomDto.cs
    public class CreateDirectRoomDto
    {
        public int UserId { get; set; }
    }

    // CreateGroupRoomDto.cs
    public class CreateGroupRoomDto
    {
        public string Name { get; set; } = string.Empty;
        public List<int> MemberIds { get; set; } = new();
    }

    // SendMessageDto.cs
    public class SendMessageDto
    {
        public string Content { get; set; } = string.Empty;
    }
}
