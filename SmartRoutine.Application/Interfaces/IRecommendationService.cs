namespace SmartRoutine.Application.Interfaces;

public interface IRecommendationService
{
    Task<string> GetRoutineSuggestionAsync(Guid userId);
} 