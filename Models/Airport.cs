namespace WebApplication1.Models // تأكد الاسم يطابق مشروعك
{
    public class Airport
    {
        public int AirportId { get; set; }
        public string? AirportName { get; set; }
        public int? CountryId { get; set; }

        // الخاصية الجديدة لاسم الدولة
        public string? CountryName { get; set; }
        public string? IcaoCode { get; set; }
    }
}
