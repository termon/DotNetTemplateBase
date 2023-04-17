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

public static class PagedExtensions
{
    public static Paged<T> ToPaged<T>(this IQueryable<T> query, int page = 1, int size = 10, string orderBy = "id", string direction = "asc" )
    {       
        // determine total avilable rows
        var totalRows = query.Count();

        // slice page required         
        var data = query.Skip((page-1)*size).Take(size).ToList();
        
        // build paged result
        var paged = new Paged<T> {
            Data = data,
            TotalRows = totalRows,
            PageSize = size,
            CurrentPage = page,
            OrderBy = orderBy,
            //invert current direction for next query
            Direction = direction.ToLower() == "desc" ? "asc" : "desc"
        };
        
        return paged;
    }
}