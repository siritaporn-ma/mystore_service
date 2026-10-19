namespace MyApi.DTO.Models.ResponseModels;

// ผลลัพธ์แบบแบ่งหน้า ใช้ซ้ำได้กับทุก entity
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}
