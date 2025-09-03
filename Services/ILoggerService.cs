using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface ILoggerService
    {
        Task LogUserActivityAsync(int userId, string userName, string action, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null, bool isSuccessful = true, string? errorMessage = null);
        
        Task LogUserLoginAsync(int userId, string userName, string ipAddress, string userAgent, bool isSuccessful = true, string? errorMessage = null);
        
        Task LogUserLogoutAsync(int userId, string userName, string ipAddress, string userAgent);
        
        Task LogEntityCreationAsync(int userId, string userName, string entityType, string entityId, string? details = null, string? ipAddress = null, string? userAgent = null);
        
        Task LogEntityUpdateAsync(int userId, string userName, string entityType, string entityId, string? details = null, string? ipAddress = null, string? userAgent = null);
        
        Task LogEntityDeletionAsync(int userId, string userName, string entityType, string entityId, string? details = null, string? ipAddress = null, string? userAgent = null);
        
        Task LogEntityViewAsync(int userId, string userName, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null);
        
        Task LogExportAsync(int userId, string userName, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null);
        
        Task LogImportAsync(int userId, string userName, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null);
        
        Task LogErrorAsync(int userId, string userName, string action, string entityType, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null, string errorMessage = "");
    }
} 