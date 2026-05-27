using System.Security.Claims;

namespace TodoApp.Api;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? user.FindFirstValue("sub")
              ?? user.FindFirstValue("nameid");

        if (!int.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("Invalid user identifier in token.");

        return userId;
    }
}
