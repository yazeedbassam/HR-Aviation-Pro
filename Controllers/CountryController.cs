using Microsoft.AspNetCore.Mvc;
using WebApplication1.DataAccess;
using WebApplication1.Models;     // حتى نستعمل Country
using System.Data;

namespace WebApplication1.Controllers
{
    public class CountryController : Controller
    {
        private readonly SqlServerDb _db;

        public CountryController(IConfiguration configuration)
        {
            _db = new SqlServerDb(configuration);
        }

        public IActionResult Index()
        {
            List<Country> countries = new List<Country>();

            // جلب البيانات من قاعدة البيانات
            string query = "SELECT CountryId, CountryName FROM Countries";
            DataTable dt = _db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                countries.Add(new Country
                {
                    CountryId = Convert.ToInt32(row["CountryId"]),
                    CountryName = row["CountryName"].ToString()
                });
            }

            return View(countries);
        }


        // GET: Create Country
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Country
        [HttpPost]
        public IActionResult Create(Country country)
        {
            string query = $"INSERT INTO Countries (CountryName) VALUES ('{country.CountryName}')";
            int result = _db.ExecuteNonQuery(query);

            if (result > 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Failed to add country.");
                return View(country);
            }
        }



        // GET: Edit Country
        public IActionResult Edit(int id)
        {
            string query = $"SELECT CountryId, CountryName FROM Countries WHERE CountryId = {id}";
            DataTable dt = _db.ExecuteQuery(query);

            if (dt.Rows.Count == 0)
            {
                return NotFound();
            }

            var row = dt.Rows[0];
            var country = new Country
            {
                CountryId = Convert.ToInt32(row["CountryId"]),
                CountryName = row["CountryName"].ToString()
            };

            return View(country);
        }

        // POST: Edit Country
        [HttpPost]
        public IActionResult Edit(Country country)
        {
            string query = $"UPDATE Countries SET CountryName = '{country.CountryName}' WHERE CountryId = {country.CountryId}";
            int result = _db.ExecuteNonQuery(query);

            if (result > 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Update failed.");
                return View(country);
            }
        }

        // POST: Confirm Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            // تحقق إذا يوجد مطارات مرتبطة
            string checkQuery = $"SELECT COUNT(*) FROM Airports WHERE CountryId = {id}";
            DataTable dt = _db.ExecuteQuery(checkQuery);

            if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0)
            {
                TempData["Error"] = "Cannot delete Division because it has related airports.";
                return RedirectToAction("Index");
            }

            string query = $"DELETE FROM Countries WHERE CountryId = {id}";
            int result = _db.ExecuteNonQuery(query);

            return RedirectToAction("Index");
        }

    }
}
