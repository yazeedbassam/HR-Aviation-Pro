using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface INotificationService
    {
        Task ProcessLicenseExpiries();
    }
}