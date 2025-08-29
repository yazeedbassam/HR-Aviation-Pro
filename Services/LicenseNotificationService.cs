using Microsoft.Data.SqlClient;
using System.Data;
using WebApplication1.DataAccess;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface ILicenseNotificationService
    {
        List<ControllerUser> GetControllersNeedingLicenses();
        List<ControllerUser> GetInactiveControllers();
        int GetControllersNeedingLicensesCount();
        int GetInactiveControllersCount();
    }

    public class LicenseNotificationService : ILicenseNotificationService
    {
        private readonly SqlServerDb _db;
        private readonly ILogger<LicenseNotificationService> _logger;

        public LicenseNotificationService(SqlServerDb db, ILogger<LicenseNotificationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// جلب المراقبين الذين يحتاجون رخص ولكن ليس لديهم رخص
        /// </summary>
        public List<ControllerUser> GetControllersNeedingLicenses()
        {
            try
            {
                const string sql = @"
                    SELECT DISTINCT
                        c.controllerid,
                        c.fullname,
                        c.username,
                        c.email,
                        c.phone_number,
                        c.job_title,
                        c.current_department,
                        c.NeedLicense,
                        c.IsActive,
                        a.airportname,
                        co.CountryName
                    FROM controllers c
                    LEFT JOIN airports a ON c.airportid = a.airportid
                    LEFT JOIN Countries co ON a.CountryId = co.CountryId
                    LEFT JOIN licenses l ON c.controllerid = l.controllerid
                    WHERE c.NeedLicense = 1 
                    AND c.IsActive = 1
                    AND l.licenseid IS NULL
                    ORDER BY c.fullname";

                var dt = _db.ExecuteQuery(sql);
                var controllers = new List<ControllerUser>();

                foreach (DataRow row in dt.Rows)
                {
                    controllers.Add(new ControllerUser
                    {
                        ControllerId = Convert.ToInt32(row["controllerid"]),
                        FullName = row["fullname"].ToString(),
                        Username = row["username"].ToString(),
                        Email = row["email"]?.ToString(),
                        PhoneNumber = row["phone_number"]?.ToString(),
                        JobTitle = row["job_title"]?.ToString(),
                        CurrentDepartment = row["current_department"]?.ToString(),
                        NeedLicense = Convert.ToBoolean(row["NeedLicense"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        AirportName = row["airportname"]?.ToString(),
                        CountryName = row["CountryName"]?.ToString()
                    });
                }

                return controllers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting controllers needing licenses");
                return new List<ControllerUser>();
            }
        }

        /// <summary>
        /// جلب المراقبين غير النشطين
        /// </summary>
        public List<ControllerUser> GetInactiveControllers()
        {
            try
            {
                const string sql = @"
                    SELECT 
                        c.controllerid,
                        c.fullname,
                        c.username,
                        c.email,
                        c.phone_number,
                        c.job_title,
                        c.current_department,
                        c.NeedLicense,
                        c.IsActive,
                        a.airportname,
                        co.CountryName
                    FROM controllers c
                    LEFT JOIN airports a ON c.airportid = a.airportid
                    LEFT JOIN Countries co ON a.CountryId = co.CountryId
                    WHERE c.IsActive = 0
                    ORDER BY c.fullname";

                var dt = _db.ExecuteQuery(sql);
                var controllers = new List<ControllerUser>();

                foreach (DataRow row in dt.Rows)
                {
                    controllers.Add(new ControllerUser
                    {
                        ControllerId = Convert.ToInt32(row["controllerid"]),
                        FullName = row["fullname"].ToString(),
                        Username = row["username"].ToString(),
                        Email = row["email"]?.ToString(),
                        PhoneNumber = row["phone_number"]?.ToString(),
                        JobTitle = row["job_title"]?.ToString(),
                        CurrentDepartment = row["current_department"]?.ToString(),
                        NeedLicense = Convert.ToBoolean(row["NeedLicense"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        AirportName = row["airportname"]?.ToString(),
                        CountryName = row["CountryName"]?.ToString()
                    });
                }

                return controllers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inactive controllers");
                return new List<ControllerUser>();
            }
        }

        /// <summary>
        /// عدد المراقبين الذين يحتاجون رخص
        /// </summary>
        public int GetControllersNeedingLicensesCount()
        {
            try
            {
                const string sql = @"
                    SELECT COUNT(DISTINCT c.controllerid)
                    FROM controllers c
                    LEFT JOIN licenses l ON c.controllerid = l.controllerid
                    WHERE c.NeedLicense = 1 
                    AND c.IsActive = 1
                    AND l.licenseid IS NULL";

                var result = _db.ExecuteScalar(sql);
                return result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting controllers needing licenses count");
                return 0;
            }
        }

        /// <summary>
        /// عدد المراقبين غير النشطين
        /// </summary>
        public int GetInactiveControllersCount()
        {
            try
            {
                const string sql = "SELECT COUNT(*) FROM controllers WHERE IsActive = 0";
                var result = _db.ExecuteScalar(sql);
                return result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inactive controllers count");
                return 0;
            }
        }
    }
} 