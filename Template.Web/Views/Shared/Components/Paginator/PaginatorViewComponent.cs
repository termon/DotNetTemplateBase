using Microsoft.AspNetCore.Mvc;
using Template.Data.Models;

namespace Template.Web.Views.Shared.Components;

[ViewComponent]
public class PaginatorViewComponent : ViewComponent
{

    public IViewComponentResult Invoke(string url, int rows, int page, int size=10)
    {
        // round totalPages up
        var totalPages = (int)Math.Ceiling(rows / (decimal)size);
        var pages = new List<Page>();
        for(var p=1; p<=totalPages;  p++)
        {
            pages.Add(new Page(p, $"{url}?page={p}&size={size}"));
        }

        return View("Default", new PaginatorProps {           
            Pages = pages, 
            CurrentPage = page, 
            TotalRows = rows, 
            PageSize = size
         });
    }
}

public record Page(int pageNo, string url);

public class PaginatorProps 
{
    // private Pages index
    private int index => (CurrentPage>=1 && CurrentPage<=TotalPages) ? CurrentPage-1 : 0;
 
    public List<Page> Pages { get; set; }
    public int CurrentPage { get; set; }
    public int TotalRows { get; set; }
    public int PageSize { get; set; }

    // read-only props
    public int TotalPages => Pages.Count;
    public Page NextPage => HasNextPage ? Pages[index+1] : Pages[index]; 
    public Page PreviousPage => HasPreviousPage ? Pages[index-1] : Pages[index];
    public Page FirstPage => Pages.FirstOrDefault();
    public Page LastPage => Pages.LastOrDefault();
 
    public bool HasPreviousPage => CurrentPage > 1 && CurrentPage <= TotalPages;
    public bool HasNextPage => CurrentPage >=1 && CurrentPage < TotalPages;
    public bool AtLastPage => !HasNextPage;
    public bool AtFirstPage => !HasPreviousPage;
}