using Microsoft.AspNetCore.Mvc;
using Template.Data;

namespace Template.Web.Views.Shared.Components;

[ViewComponent]
public class PaginatorViewComponent : ViewComponent
{
 
    public IViewComponentResult Invoke(PagedProps pages, int links=15)
    {       
        // extract existing query parameters except for page and size (these will be replaced with updated values)
        var existingParams = Request.Query
            .Where(e => e.Key.ToLower() != "page" && e.Key.ToLower() != "size")
            .Select(e =>  (e.Key.ToString(),e.Value.ToString())) 
            .ToList();
        
        // build paginator pages (page no and url)
        var pageLinks = new List<Page>();
        var (start,end) = LinkRange(pages.TotalPages, pages.CurrentPage, links);
        for(var p=1; p<=pages.TotalPages;  p++)
        {   
            // only show links within range or first and last pages
            if (p >= start && p <=end || p == 1 || p == pages.TotalPages)
            {
                pageLinks.Add( new Page(p, BuildUrl(p, pages.PageSize, existingParams)) ); 
            }
            else 
            {
                 pageLinks.Add( null );
            }
        }

        var props = new PaginatorProps {           
            Pages = pageLinks, 
            CurrentPage = pages.CurrentPage, 
            TotalRows = pages.TotalRows, 
            PageSize = pages.PageSize,
        };

        return View("Default", props);
    }

    // build a url for action with existing parameters plus updated page and size parameters
    private string BuildUrl(int page, int size, List<(string,string)> existingParameters)
    {
        // cast the ActionDescriptor to a ControllerActionDescriptor
        var actionDescriptor = ViewContext.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
      
        // Get the current action name
        string currentAction = actionDescriptor.ActionName;

        var paramsDict = new RouteValueDictionary();
        existingParameters.ForEach( p => paramsDict.Add(p.Item1,p.Item2));
        paramsDict.Add("page", page);
        paramsDict.Add("size", size); 
        return Url.Action(currentAction, paramsDict);
    }
    
    private static (int,int) LinkRange(int totalPages, int currentPage, int maxLinks=15)
    {
        int startPage;
        int endPage;

        if (totalPages <= maxLinks)
        {
            startPage = 1;
            endPage = totalPages;
        }
        else
        {
            int middleOffset = maxLinks / 2;

            if (currentPage <= middleOffset)
            {
                startPage = 1;
                endPage = maxLinks;
            }
            else if (currentPage + middleOffset >= totalPages)
            {
                startPage = totalPages - maxLinks + 1;
                endPage = totalPages;
            }
            else
            { 
                startPage = currentPage - middleOffset;
                endPage = currentPage + middleOffset;
            }
        }
        return (startPage, endPage);
    }

}

public record Page(int PageNo, string Url);

public class PaginatorProps 
{
    public int CurrentPage { get; set; } // 1..Pages.Count
    public List<Page> Pages { get; set; }
    public int TotalRows { get; set; }
    public int PageSize { get; set; }
    
    // Read only properties
    public int TotalPages => (int)Math.Ceiling(TotalRows / (decimal)PageSize);

    public Page NextPage => HasNextPage ? Pages[Index+1] : null;
    public Page PreviousPage => HasPreviousPage ? Pages[Index-1] : null;
    public Page FirstPage => Pages.FirstOrDefault();
    public Page LastPage => Pages.LastOrDefault();
 
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool AtLastPage => !HasNextPage;
    public bool AtFirstPage => !HasPreviousPage;

    private int Index => CurrentPage-1; // actual Pages index
}