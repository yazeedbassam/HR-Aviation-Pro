using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WebApplication1.Services
{
    public static class UserActivityHelper
    {
        public static string GetUserIpAddress(HttpContext httpContext)
        {
            try
            {
                // Try to get IP from various headers (for proxy/load balancer scenarios)
                var forwardedHeader = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedHeader))
                {
                    // X-Forwarded-For can contain multiple IPs, take the first one
                    var firstIp = forwardedHeader.Split(',')[0].Trim();
                    if (IsValidIpAddress(firstIp))
                        return firstIp;
                }

                var realIpHeader = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIpHeader) && IsValidIpAddress(realIpHeader))
                    return realIpHeader;

                // Fall back to connection remote IP
                var remoteIp = httpContext.Connection.RemoteIpAddress;
                if (remoteIp != null)
                    return remoteIp.ToString();

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string GetUserAgent(HttpContext httpContext)
        {
            try
            {
                return httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static int GetCurrentUserId(ClaimsPrincipal user)
        {
            try
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    return userId;

                // Fallback to UserId claim
                var userIdClaim2 = user.FindFirst("UserId");
                if (userIdClaim2 != null && int.TryParse(userIdClaim2.Value, out int userId2))
                    return userId2;

                return 0; // Anonymous user
            }
            catch
            {
                return 0;
            }
        }

        public static string GetCurrentUserName(ClaimsPrincipal user)
        {
            try
            {
                return user.Identity?.Name ?? user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string GetCurrentUserRole(ClaimsPrincipal user)
        {
            try
            {
                return user.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static bool IsValidIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            // Basic IP validation
            var parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                    return false;
            }

            return true;
        }

        public static string GetEntityTypeFromController(string controllerName)
        {
            return controllerName.Replace("Controller", "").Replace("s", "");
        }

        public static string FormatEntityDetails(string entityType, string entityId, string? additionalInfo = null)
        {
            var details = $"{entityType} ID: {entityId}";
            if (!string.IsNullOrEmpty(additionalInfo))
                details += $" - {additionalInfo}";
            return details;
        }
    }
} 