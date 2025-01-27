using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Linq;

public class DuplicateRouteNameMiddleware
{
    private readonly RequestDelegate _next;

    public DuplicateRouteNameMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IActionDescriptorCollectionProvider actionDescriptorProvider)
    {
        var routeNames = actionDescriptorProvider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(ad => ad.AttributeRouteInfo?.Name != null)
            .Select(ad => ad.AttributeRouteInfo.Name)
            .ToList();

        var duplicates = routeNames
            .GroupBy(name => name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicates.Any())
        {
            throw new InvalidOperationException($"Duplicate Route Names detected: {string.Join(", ", duplicates)}");
        }

        await _next(context);
    }
}
