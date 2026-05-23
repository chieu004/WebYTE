namespace WebYTE.Application.DTOs.Admin;

public class SearchFilterDto
{
    public string? SearchTerm { get; set; }
    public string? SpecialtyId { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
