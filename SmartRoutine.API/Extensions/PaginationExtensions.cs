using SmartRoutine.Application.DTOs.Common;

namespace SmartRoutine.API.Extensions;

public static class PaginationExtensions
{
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = source.Count();
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = await Task.FromResult(source.Count());
        var items = await Task.FromResult(source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList());

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    public static void AddPaginationHeader<T>(this HttpResponse response, PagedResult<T> pagedResult)
    {
        var paginationMetadata = new
        {
            totalCount = pagedResult.TotalCount,
            pageSize = pagedResult.PageSize,
            currentPage = pagedResult.PageNumber,
            totalPages = pagedResult.TotalPages,
            hasPreviousPage = pagedResult.HasPreviousPage,
            hasNextPage = pagedResult.HasNextPage
        };

        response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationMetadata));
    }
} 