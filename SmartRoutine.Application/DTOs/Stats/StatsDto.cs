namespace SmartRoutine.Application.DTOs.Stats;

public class StatsDto
{
    public int TotalRoutines { get; set; }
    public int CompletedToday { get; set; }
    public double CompletionRate { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public Dictionary<string, int> WeeklyStats { get; set; } = new();
} 