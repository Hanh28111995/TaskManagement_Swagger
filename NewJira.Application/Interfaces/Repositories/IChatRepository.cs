using System.Collections.Generic;
using System.Threading.Tasks;
using NewJira.Domain.Entities;

#nullable enable
namespace NewJira.Application.Interfaces.Repositories;

public interface IChatRepository
{
    Task<ChatRoom?> GetDirectRoomAsync(int userAId, int userBId);
    Task<ChatRoom> CreateDirectRoomAsync(int userAId, int userBId);    
    Task<ChatRoom> CreateGroupRoomAsync(string name, int creatorId, List<int> memberIds);
    Task<List<ChatRoom>> GetRoomsForUserAsync(int userId);      
    Task<ChatRoom?> GetRoomWithMembersAsync(int roomId);
    Task<bool> IsMemberAsync(int roomId, int userId);
    Task AddMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetHistoryAsync(int roomId, int page, int pageSize);
    Task SaveChangesAsync();
}