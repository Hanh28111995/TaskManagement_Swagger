namespace NewJira.Application.DTOs.Common;   // hoặc DTOs/Task theo convention bạn

public class DashboardSummaryDto
{
    public int TotalProjects { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int MemberCount { get; set; }
    public List<StatusCountDto> ByStatus { get; set; } = new();    
}

public class StatusCountDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ProjectCompletedDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int Completed { get; set; }
}