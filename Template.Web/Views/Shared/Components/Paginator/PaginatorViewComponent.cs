using Microsoft.AspNetCore.Mvc;

namespace Template.Web.Views.Shared.Components;

[ViewComponent]
public class PaginatorViewComponent : ViewComponent
{
    
    public IViewComponentResult Invoke(string action, int rows, int pages, int current, int size=10)
    {       
        // extract existing query parameters except for page and size (these will be replaced with updated values)
        var existingParams = Request.Query
            .Where(e => e.Key.ToLower() != "page" && e.Key.ToLower() != "size")
            .Select(e =>  (e.Key.ToString(),e.Value.ToString())) 
            .ToList();
        
        // build paginator pages (page no and url)
        var pageLinks = new List<Page>();
        for(var p=1; p<=pages;  p++)
        {       
            pageLinks.Add( new Page(p, BuildUrl(action, p, size, existingParams)) ); 
        }

        var props = new PaginatorProps {           
            Pages = pageLinks, 
            CurrentPage = current, 
            TotalRows = rows, 
            PageSize = size,
         };
        return View("Default", props);
    }

    // build a url for action with existing parameters plus updated page and size parameters
    private string BuildUrl(string action, int page, int size, List<(string,string)> existingParameters)
    {
        var paramsDict = new RouteValueDictionary();
        existingParameters.ForEach( p => paramsDict.Add(p.Item1,p.Item2));
        paramsDict.Add("page", page);
        paramsDict.Add("size", size); 
        return Url.Action(action, paramsDict);
    }
}

public record Page(int pageNo, string url);

public class PaginatorProps 
{
    private int Index => CurrentPage-1; // actual Pages index

    public int CurrentPage { get; set; } // 1..Pages.Count
    public List<Page> Pages { get; set; }
    public int TotalRows { get; set; }
    public int PageSize { get; set; }

    // read-only props
    public int TotalPages => Pages.Count;
    public Page NextPage => HasNextPage ? Pages[Index+1] : null;
    public Page PreviousPage => HasPreviousPage ? Pages[Index-1] : null;
    public Page FirstPage => Pages.FirstOrDefault();
    public Page LastPage => Pages.LastOrDefault();
 
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool AtLastPage => !HasNextPage;
    public bool AtFirstPage => !HasPreviousPage;
}