using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class CertificateIndexViewModel
    {
        public List<CertificateViewModel> ControllerCertificates { get; set; }
        public List<CertificateViewModel> AISCertificates { get; set; }
        public List<CertificateViewModel> CNSCertificates { get; set; }
        public List<CertificateViewModel> AFTNCertificates { get; set; }
        public List<CertificateViewModel> OpsStaffCertificates { get; set; }

        public CertificateIndexViewModel()
        {
            ControllerCertificates = new List<CertificateViewModel>();
            AISCertificates = new List<CertificateViewModel>();
            CNSCertificates = new List<CertificateViewModel>();
            AFTNCertificates = new List<CertificateViewModel>();
            OpsStaffCertificates = new List<CertificateViewModel>();
        }
    }
}
