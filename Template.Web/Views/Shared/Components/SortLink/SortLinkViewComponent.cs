using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc;

namespace Template.Web.Views.Shared.Components;

[ViewComponent]
public class SortLinkViewComponent : ViewComponent
{

    public IViewComponentResult Invoke(string column="id")
    {    
        // cast the ActionDescriptor to a ControllerActionDescriptor
        var actionDescriptor = ViewContext.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
      
        // Get the current action name
        string currentAction = actionDescriptor.ActionName;

        // retrieve current sort order and direction from the request or set to defaults
        var order = Request.Query.Where(e => e.Key.ToLower() == "order").Select(e =>  e.Value.ToString()).FirstOrDefault() ?? "id";  
        var direction = Request.Query.Where(e => e.Key.ToLower() == "direction").Select(e =>  e.Value.ToString()).FirstOrDefault() ?? "asc";  

        // Get the query parameters from the request excluding the order and direction parameters (if present)
        var queryParamsDict = Request.Query
            .Where(e => e.Key.ToLower() != "order" && e.Key.ToLower() != "direction")
            .Select(e =>  (e.Key.ToString(),e.Value.ToString())) 
            .ToDictionary();

        // check if the column is the same as the current order column
        if (column == order)
        {
            // set order and toggle the direction
            queryParamsDict["order"] = order;           
            queryParamsDict["direction"] = direction == "asc" ? "desc" : "asc";   
        }
        else
        {
            // set the order column to the new column and default direction 
            queryParamsDict["order"] = column;
            queryParamsDict["direction"] = direction;
        }
       

        // Generate the URL using Url.Action and including the existing query parameters
        string url = Url.Action(currentAction, queryParamsDict);

        return View("Default", new SortLinkProps( url, column?.ToLower(), order.ToLower(), direction.ToLower()) );
    }
}

public record SortLinkProps(string Url, string Column, string Order, string Direction);