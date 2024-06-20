using Microsoft.AspNetCore.Mvc;

namespace Template.Web.Views.Shared.Components;

[ViewComponent]
public class SortOrderViewComponent : ViewComponent
{

    public IViewComponentResult Invoke(string field="id") //, string orderby=null, string direction="asc")
    {    
        var order = Request.Query.Where(e => e.Key.ToLower() == "order").Select(e =>  e.Value.ToString()).FirstOrDefault() ?? "id";  
        var direction = Request.Query.Where(e => e.Key.ToLower() == "direction").Select(e =>  e.Value.ToString()).FirstOrDefault() ?? "asc";  
        
        return View("Default", new SortOrderProps( field?.ToLower(), order.ToLower(), direction.ToLower()) );
    }
}

public record SortOrderProps(string Field, string OrderBy, string Direction);
