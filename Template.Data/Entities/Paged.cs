namespace Template.Data.Entities;

public class Paged<T> {        
    public List<T> Data { get; set;}
    public int TotalRows { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalRows / (decimal)PageSize);

    public string OrderBy { get; set; } = "id";
    public string Direction { get; set; } = "asc";
}