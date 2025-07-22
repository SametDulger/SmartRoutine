using SmartRoutine.Application.Interfaces;

namespace SmartRoutine.Application.Services;

public class AIRecommendationService : IRecommendationService
{
    public async Task<string> GetRoutineSuggestionAsync(Guid userId)
    {
        // AI ile öneri üretme örneği
        return await Task.FromResult("Daha fazla su içmeyi unutma!");
    }
} 