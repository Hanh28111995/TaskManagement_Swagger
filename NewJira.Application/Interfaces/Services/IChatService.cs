namespace NewJira.Application.Services
{
    public interface IChatRealtimeService
    {
        Task SendMessageNotificationAsync(int roomId, int senderId, string messageContent);
    }
}