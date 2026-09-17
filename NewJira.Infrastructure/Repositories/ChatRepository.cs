using Microsoft.EntityFrameworkCore;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;

namespace NewJira.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly JiraDbContext _context;

    public ChatRepository(JiraDbContext context)
    {
        _context = context;
    }

    // ---- PHÒNG ----

    // Tìm phòng Direct giữa 2 user (cả 2 đều là member, type Direct)
    public async Task<ChatRoom?> GetDirectRoomAsync(int userAId, int userBId)
    {
        return await _context.ChatRooms
            .Where(r => r.Type == "Direct" &&
                        r.Members.Any(m => m.UserId == userAId) &&
                        r.Members.Any(m => m.UserId == userBId))
            .Include(r => r.Members)          // Include: kéo theo danh sách member
                .ThenInclude(m => m.User)     // và mỗi member kèm luôn User
            .FirstOrDefaultAsync();
    }

    public async Task<ChatRoom> CreateDirectRoomAsync(int userAId, int userBId)
    {
        var room = new ChatRoom
        {
            Type = "Direct",
            Members = new List<ChatRoomMember>
            {
                new() { UserId = userAId },
                new() { UserId = userBId }
            }
        };
        _context.ChatRooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<ChatRoom> CreateGroupRoomAsync(string name, int creatorId, List<int> memberIds)
    {
        var room = new ChatRoom
        {
            Name = name,
            Type = "Group",
            CreatedById = creatorId,
            Members = memberIds.Select(uid => new ChatRoomMember { UserId = uid }).ToList()
        };
        _context.ChatRooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    // Danh sách phòng của user — Include Members (để FE hiện tên user kia khi DM)
    public async Task<List<ChatRoom>> GetRoomsForUserAsync(int userId)
    {
        return await _context.ChatRooms
            .Where(r => r.Members.Any(m => m.UserId == userId))
            .Include(r => r.Members)
                .ThenInclude(m => m.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatRoom?> GetRoomWithMembersAsync(int roomId)
    {
        return await _context.ChatRooms
            .Include(r => r.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(r => r.Id == roomId);
    }

    public async Task<bool> IsMemberAsync(int roomId, int userId)
    {
        return await _context.ChatRoomMembers
            .AnyAsync(m => m.RoomId == roomId && m.UserId == userId);
    }

    // ---- TIN NHẮN ----

    public async Task AddMessageAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
    }

    // Lịch sử tin — Include Sender để FE hiện avatar/tên người gửi
    public async Task<List<ChatMessage>> GetHistoryAsync(int roomId, int page, int pageSize)
    {
        return await _context.ChatMessages
            .Where(m => m.RoomId == roomId)
            .Include(m => m.Sender)           // kéo theo User gửi tin
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}