using Microsoft.AspNetCore.Mvc;

namespace Template.Web.Views.Shared.Components;

[ViewComponent]
public class SortOrderViewComponent : ViewComponent
{

    public IViewComponentResult Invoke(string field="Id", string orderby="Id", string direction = "asc")
    {      
        return View("Default", new SortOrderProps(field.ToLower(), orderby.ToLower(), direction.ToLower()) );
    }
}

public record SortOrderProps(string Field ="Id", string OrderBy="Id", string Direction="asc");
