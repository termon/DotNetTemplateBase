namespace Template.Data;

// Paged class to hold paged data and paging information
public class Paged<T> : PagedProps {  

    public List<T> Data { get; set; } = new();
  
    public PagedProps Pages =>  new PagedProps {
        TotalRows = TotalRows,
        CurrentPage = CurrentPage,
        PageSize = PageSize,       
    };
}

// used to pass paging information round the application
public class PagedProps
{
    public int TotalRows { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalRows / (decimal)PageSize);
}

// Extension method to add paging to IQueryable
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
        };
        
        return paged;
    }
}