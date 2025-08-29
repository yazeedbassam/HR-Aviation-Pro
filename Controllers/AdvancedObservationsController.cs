using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using WebApplication1.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Color = System.Drawing.Color;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class AdvancedObservationsController : Controller
    {
        private readonly SqlServerDb _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAdvancedPermissionService _permissionService;
        private readonly ILogger<AdvancedObservationsController> _logger;

        public AdvancedObservationsController(
            IConfiguration configuration, 
            IWebHostEnvironment webHostEnvironment,
            IAdvancedPermissionService permissionService,
            ILogger<AdvancedObservationsController> logger)
        {
            _db = new SqlServerDb(configuration);
            _webHostEnvironment = webHostEnvironment;
            _permissionService = permissionService;
            _logger = logger;
        }

        // GET: /AdvancedObservations/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if user has permission to view observations
                if (!await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_VIEW"))
                {
                    TempData["ErrorMessage"] = "ليس لديك صلاحية لعرض الملاحظات";
                    return RedirectToAction("Index", "Home");
                }

                // Get data with permission-based filtering
                var viewModel = new AdvancedObservationIndexViewModel
                {
                    ControllerObservations = await GetFilteredObservations("ControllerObservations", currentUserId),
                    AISObservations = await GetFilteredObservations("AISObservations", currentUserId),
                    CNSObservations = await GetFilteredObservations("CNSObservations", currentUserId),
                    AFTNObservations = await GetFilteredObservations("AFTNObservations", currentUserId),
                    ATFMObservations = await GetFilteredObservations("ATFMObservations", currentUserId),
                    OpsStaffObservations = await GetFilteredObservations("OpsStaffObservations", currentUserId)
                };

                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                ViewBag.CurrentUserId = currentUserId;
                ViewBag.CanCreate = await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_CREATE");
                ViewBag.CanEdit = await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_EDIT");
                ViewBag.CanDelete = await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_DELETE");
                ViewBag.CanExport = await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_EXPORT");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading advanced observations index");
                ViewBag.ErrorMessage = $"حدث خطأ أثناء تحميل البيانات: {ex.Message}";
                return View(new AdvancedObservationIndexViewModel());
            }
        }

        // GET: /AdvancedObservations/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if user has permission to create observations
                if (!await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_CREATE"))
                {
                    TempData["ErrorMessage"] = "ليس لديك صلاحية لإنشاء ملاحظات جديدة";
                    return RedirectToAction("Index");
                }

                LoadDropdowns(currentUserId);
                return View(new Observation());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create observation form");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل نموذج الإنشاء";
                return RedirectToAction("Index");
            }
        }

        // POST: /AdvancedObservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Observation observation, IFormFile UploadFile)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if user has permission to create observations
                if (!await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_CREATE"))
                {
                    TempData["ErrorMessage"] = "ليس لديك صلاحية لإنشاء ملاحظات جديدة";
                    return RedirectToAction("Index");
                }

                // Validate that user can access the target user's data
                if (observation.ControllerId.HasValue)
                {
                    if (!await _permissionService.CanAccessUserDataAsync(currentUserId, observation.ControllerId.Value))
                    {
                        ModelState.AddModelError("", "ليس لديك صلاحية لإضافة ملاحظات لهذا المراقب");
                    }
                }
                else if (observation.EmployeeId.HasValue)
                {
                    if (!await _permissionService.CanAccessUserDataAsync(currentUserId, observation.EmployeeId.Value))
                    {
                        ModelState.AddModelError("", "ليس لديك صلاحية لإضافة ملاحظات لهذا الموظف");
                    }
                }

                if (!observation.ControllerId.HasValue && !observation.EmployeeId.HasValue)
                {
                    ModelState.AddModelError("", "يجب اختيار مراقب أو موظف.");
                }

                ModelState.Remove(nameof(observation.FilePath));
                ModelState.Remove(nameof(observation.UploadFile));
                ModelState.Remove(nameof(observation.ObservationNo));

                if (ModelState.IsValid)
                {
                    string personIdentifier = null;
                    string personTypeFolder = "";

                    if (observation.ControllerId.HasValue)
                    {
                        var controllerTable = _db.GetControllerById(observation.ControllerId.Value);
                        if (controllerTable != null && controllerTable.Rows.Count > 0)
                        {
                            personIdentifier = controllerTable.Rows[0]["Username"].ToString();
                            personTypeFolder = "controllers";
                        }
                    }
                    else if (observation.EmployeeId.HasValue)
                    {
                        var employee = _db.GetEmployeeById(observation.EmployeeId.Value);
                        if (employee != null)
                        {
                            personIdentifier = employee.Username;
                            personTypeFolder = "employees";
                        }
                    }

                    if (UploadFile != null && UploadFile.Length > 0)
                    {
                        if (string.IsNullOrEmpty(personIdentifier))
                        {
                            ModelState.AddModelError("", "Cannot upload file because the associated person was not found.");
                        }
                        else
                        {
                            string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(UploadFile.FileName);
                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "observations", personTypeFolder, personIdentifier);
                            Directory.CreateDirectory(uploadsFolder);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await UploadFile.CopyToAsync(stream);
                            }
                            observation.FilePath = $"/observations/{personTypeFolder}/{personIdentifier}/{uniqueFileName}";
                        }
                    }

                    observation.ObservationNo = _db.GetNextObservationNumberForPerson(observation.ControllerId, observation.EmployeeId);

                    if (ModelState.ErrorCount == 0)
                    {
                        string sql = @"
                            INSERT INTO Observations (ControllerId, EmployeeId, ObservationNo, FlightNo, Duration_Days, TravelCountry, DepartDate, ReturnDate, LicenseNumber, FilePath, Notes)
                            VALUES (@ControllerId, @EmployeeId, @ObservationNo, @FlightNo, @DurationDays, @TravelCountry, @DepartDate, @ReturnDate, @LicenseNumber, @FilePath, @Notes)";

                        var parameters = new[]
                        {
                            new SqlParameter("@ControllerId", (object)observation.ControllerId ?? DBNull.Value),
                            new SqlParameter("@EmployeeId", (object)observation.EmployeeId ?? DBNull.Value),
                            new SqlParameter("@ObservationNo", (object)observation.ObservationNo ?? DBNull.Value),
                            new SqlParameter("@FlightNo", (object)observation.FlightNo ?? DBNull.Value),
                            new SqlParameter("@DurationDays", (object)observation.DurationDays ?? DBNull.Value),
                            new SqlParameter("@TravelCountry", (object)observation.TravelCountry ?? string.Empty),
                            new SqlParameter("@DepartDate", (object)observation.DepartDate ?? DBNull.Value),
                            new SqlParameter("@ReturnDate", (object)observation.ReturnDate ?? DBNull.Value),
                            new SqlParameter("@LicenseNumber", (object)observation.LicenseNumber ?? string.Empty),
                            new SqlParameter("@FilePath", (object)observation.FilePath ?? DBNull.Value),
                            new SqlParameter("@Notes", (object)observation.Notes ?? DBNull.Value)
                        };

                        _db.ExecuteNonQuery(sql, parameters);
                        TempData["SuccessMessage"] = "تم إضافة الملاحظة بنجاح!";
                        return RedirectToAction(nameof(Index));
                    }
                }

                LoadDropdowns(currentUserId);
                return View(observation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating observation");
                TempData["ErrorMessage"] = $"حدث خطأ أثناء إنشاء الملاحظة: {ex.Message}";
                LoadDropdowns(GetCurrentUserId());
                return View(observation);
            }
        }

        // GET: /AdvancedObservations/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if user has permission to edit observations
                if (!await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_EDIT"))
                {
                    TempData["ErrorMessage"] = "ليس لديك صلاحية لتعديل الملاحظات";
                    return RedirectToAction("Index");
                }

                var observation = _db.GetObservationById(id);
                if (observation == null)
                {
                    return NotFound();
                }

                // Check if user can access this observation's data
                if (observation.ControllerId.HasValue)
                {
                    if (!await _permissionService.CanAccessUserDataAsync(currentUserId, observation.ControllerId.Value))
                    {
                        TempData["ErrorMessage"] = "ليس لديك صلاحية لتعديل هذه الملاحظة";
                        return RedirectToAction("Index");
                    }
                }
                else if (observation.EmployeeId.HasValue)
                {
                    if (!await _permissionService.CanAccessUserDataAsync(currentUserId, observation.EmployeeId.Value))
                    {
                        TempData["ErrorMessage"] = "ليس لديك صلاحية لتعديل هذه الملاحظة";
                        return RedirectToAction("Index");
                    }
                }

                return View(observation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading observation for edit, ID: {Id}", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل الملاحظة للتعديل";
                return RedirectToAction("Index");
            }
        }

        // POST: /AdvancedObservations/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Observation observation, IFormFile UploadFile)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if user has permission to edit observations
                if (!await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_EDIT"))
                {
                    TempData["ErrorMessage"] = "ليس لديك صلاحية لتعديل الملاحظات";
                    return RedirectToAction("Index");
                }

                if (id != observation.ObservationId)
                {
                    return BadRequest();
                }

                // Check if user can access this observation's data
                var existingObs = _db.GetObservationById(id);
                if (existingObs == null)
                {
                    return NotFound();
                }

                if (existingObs.ControllerId.HasValue)
                {
                    if (!await _permissionService.CanAccessUserDataAsync(currentUserId, existingObs.ControllerId.Value))
                    {
                        TempData["ErrorMessage"] = "ليس لديك صلاحية لتعديل هذه الملاحظة";
                        return RedirectToAction("Index");
                    }
                }
                else if (existingObs.EmployeeId.HasValue)
                {
                    if (!await _permissionService.CanAccessUserDataAsync(currentUserId, existingObs.EmployeeId.Value))
                    {
                        TempData["ErrorMessage"] = "ليس لديك صلاحية لتعديل هذه الملاحظة";
                        return RedirectToAction("Index");
                    }
                }

                ModelState.Remove(nameof(observation.UploadFile));
                ModelState.Remove("UploadFile");
                ModelState.Remove(nameof(observation.PersonName));
                ModelState.Remove(nameof(observation.ControllerName));
                ModelState.Remove(nameof(observation.EmployeeName));
                ModelState.Remove(nameof(observation.ObservationNo));

                if (ModelState.IsValid)
                {
                    string currentFilePath = existingObs.FilePath;

                    // Process file upload if provided
                    if (UploadFile != null && UploadFile.Length > 0)
                    {
                        string personIdentifier = null;
                        string personTypeFolder = "";

                        if (existingObs.ControllerId.HasValue)
                        {
                            var controllerTable = _db.GetControllerById(existingObs.ControllerId.Value);
                            if (controllerTable != null && controllerTable.Rows.Count > 0)
                            {
                                personIdentifier = controllerTable.Rows[0]["Username"].ToString();
                                personTypeFolder = "controllers";
                            }
                        }
                        else if (existingObs.EmployeeId.HasValue)
                        {
                            var employee = _db.GetEmployeeById(existingObs.EmployeeId.Value);
                            if (employee != null)
                            {
                                personIdentifier = employee.Username;
                                personTypeFolder = "employees";
                            }
                        }

                        if (!string.IsNullOrEmpty(personIdentifier))
                        {
                            string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(UploadFile.FileName);
                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "observations", personTypeFolder, personIdentifier);
                            Directory.CreateDirectory(uploadsFolder);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await UploadFile.CopyToAsync(stream);
                            }
                            observation.FilePath = $"/observations/{personTypeFolder}/{personIdentifier}/{uniqueFileName}";
                        }
                    }
                    else
                    {
                        observation.FilePath = currentFilePath;
                    }

                    // Update the observation
                    string sql = @"
                        UPDATE Observations 
                        SET FlightNo = @FlightNo, Duration_Days = @DurationDays, TravelCountry = @TravelCountry, 
                            DepartDate = @DepartDate, ReturnDate = @ReturnDate, LicenseNumber = @LicenseNumber, 
                            FilePath = @FilePath, Notes = @Notes 
                        WHERE ObservationId = @ObservationId";

                    var parameters = new[]
                    {
                        new SqlParameter("@FlightNo", (object)observation.FlightNo ?? DBNull.Value),
                        new SqlParameter("@DurationDays", (object)observation.DurationDays ?? DBNull.Value),
                        new SqlParameter("@TravelCountry", (object)observation.TravelCountry ?? string.Empty),
                        new SqlParameter("@DepartDate", (object)observation.DepartDate ?? DBNull.Value),
                        new SqlParameter("@ReturnDate", (object)observation.ReturnDate ?? DBNull.Value),
                        new SqlParameter("@LicenseNumber", (object)observation.LicenseNumber ?? string.Empty),
                        new SqlParameter("@FilePath", (object)observation.FilePath ?? DBNull.Value),
                        new SqlParameter("@Notes", (object)observation.Notes ?? DBNull.Value),
                        new SqlParameter("@ObservationId", id)
                    };

                    _db.ExecuteNonQuery(sql, parameters);
                    TempData["SuccessMessage"] = "تم تحديث الملاحظة بنجاح!";
                    return RedirectToAction(nameof(Index));
                }

                // If model state is invalid, repopulate the person name and return to the view
                var person = _db.GetObservationById(id);
                if (person != null)
                {
                    observation.ControllerName = person.ControllerName;
                    observation.EmployeeName = person.EmployeeName;
                }
                return View(observation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating observation, ID: {Id}", id);
                TempData["ErrorMessage"] = $"حدث خطأ أثناء تحديث الملاحظة: {ex.Message}";
                return View(observation);
            }
        }

        // POST: /AdvancedObservations/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if user has permission to delete observations
                if (!await _permissionService.HasPermissionAsync(currentUserId, "OBSERVATIONS_DELETE"))
                {
                    TempData["ErrorMessage"] = "ليس لديك صلاحية لحذف الملاحظات";
                    return RedirectToAction("Index");
                }

                // Check if user can access this observation's data
                var observation = _db.GetObservationById(id);
                if (observation == null)
                {
                    return NotFound();
                }

                if (observation.ControllerId.HasValue)
                {
                    if (!await _permissionService.CanAccessUserDataAsync(currentUserId, observation.ControllerId.Value))
                    {
                        TempData["ErrorMessage"] = "ليس لديك صلاحية لحذف هذه الملاحظة";
                        return RedirectToAction("Index");
                    }
                }
                else if (observation.EmployeeId.HasValue)
                {
                    if (!await _permissionService.CanAccessUserDataAsync(currentUserId, observation.EmployeeId.Value))
                    {
                        TempData["ErrorMessage"] = "ليس لديك صلاحية لحذف هذه الملاحظة";
                        return RedirectToAction("Index");
                    }
                }

                _db.DeleteObservationById(id);
                TempData["SuccessMessage"] = "تم حذف الملاحظة بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting observation, ID: {Id}", id);
                TempData["ErrorMessage"] = $"حدث خطأ أثناء حذف الملاحظة: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private async Task<DataTable> GetFilteredObservations(string observationType, int currentUserId)
        {
            try
            {
                // Get accessible user IDs
                var accessibleUserIds = await _permissionService.GetAccessibleUserIdsAsync(currentUserId);
                
                if (!accessibleUserIds.Any())
                    return new DataTable();

                var userIds = string.Join(",", accessibleUserIds);
                var filterClause = $"UserId IN ({userIds})";

                // Apply filter based on observation type
                DataTable result = new DataTable();
                switch (observationType)
                {
                    case "ControllerObservations":
                        var controllerObs = _db.GetControllerObservations();
                        result = ConvertObservationsToDataTable(controllerObs);
                        break;
                    case "AISObservations":
                        var aisObs = _db.GetAISObservations();
                        result = ConvertObservationsToDataTable(aisObs);
                        break;
                    case "CNSObservations":
                        var cnsObs = _db.GetCNSObservations();
                        result = ConvertObservationsToDataTable(cnsObs);
                        break;
                    case "AFTNObservations":
                        var aftnObs = _db.GetAFTNObservations();
                        result = ConvertObservationsToDataTable(aftnObs);
                        break;
                    case "ATFMObservations":
                        var atfmObs = _db.GetATFMObservations();
                        result = ConvertObservationsToDataTable(atfmObs);
                        break;
                    case "OpsStaffObservations":
                        var opsObs = _db.GetOpsStaffObservations();
                        result = ConvertObservationsToDataTable(opsObs);
                        break;
                    default:
                        result = new DataTable();
                        break;
                }

                // Filter the results based on accessible users
                if (result.Columns.Contains("UserId"))
                {
                    var filteredRows = result.AsEnumerable()
                        .Where(row => accessibleUserIds.Contains(Convert.ToInt32(row["UserId"])))
                        .CopyToDataTable();
                    return filteredRows;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering observations for type {Type} and user {UserId}", observationType, currentUserId);
                return new DataTable();
            }
        }

        private void LoadDropdowns(int currentUserId)
        {
            try
            {
                // Get accessible user IDs
                var accessibleUserIds = _permissionService.GetAccessibleUserIdsAsync(currentUserId).GetAwaiter().GetResult();
                
                if (accessibleUserIds.Any())
                {
                    var userIds = string.Join(",", accessibleUserIds);
                    
                    // Load controllers
                    var controllersQuery = $"SELECT ControllerId, FullName FROM dbo.Controllers WHERE ControllerId IN ({userIds}) ORDER BY FullName";
                    ViewBag.Controllers = _db.ExecuteQuery(controllersQuery).AsEnumerable()
                                         .Select(row => new SelectListItem { Value = row["ControllerId"].ToString(), Text = row["FullName"].ToString() }).ToList();
                    
                    // Load employees
                    var employeesQuery = $"SELECT EmployeeID, FullName FROM dbo.Employees WHERE EmployeeID IN ({userIds}) ORDER BY FullName";
                    ViewBag.Employees = _db.ExecuteQuery(employeesQuery).AsEnumerable()
                                         .Select(row => new SelectListItem { Value = row["EmployeeID"].ToString(), Text = row["FullName"].ToString() }).ToList();
                }
                else
                {
                    ViewBag.Controllers = new List<SelectListItem>();
                    ViewBag.Employees = new List<SelectListItem>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dropdowns for user {UserId}", currentUserId);
                ViewBag.Controllers = new List<SelectListItem>();
                ViewBag.Employees = new List<SelectListItem>();
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;
            return 0;
        }

        private DataTable ConvertObservationsToDataTable(List<Observation> observations)
        {
            var dt = new DataTable();
            
            if (observations == null || !observations.Any())
                return dt;

            // Add columns based on the Observation model
            dt.Columns.Add("Obs#", typeof(int));
            dt.Columns.Add("PersonName", typeof(string));
            dt.Columns.Add("LicenseNumber", typeof(string));
            dt.Columns.Add("Notes", typeof(string));
            dt.Columns.Add("FlightNo", typeof(string));
            dt.Columns.Add("TravelCountry", typeof(string));
            dt.Columns.Add("DepartDate", typeof(DateTime));
            dt.Columns.Add("ReturnDate", typeof(DateTime));
            dt.Columns.Add("DurationDays", typeof(int));
            dt.Columns.Add("ControllerId", typeof(int));
            dt.Columns.Add("EmployeeId", typeof(int));

            // Add rows
            foreach (var obs in observations)
            {
                dt.Rows.Add(
                    obs.ObservationId,
                    obs.PersonName,
                    obs.LicenseNumber,
                    obs.Notes,
                    obs.FlightNo,
                    obs.TravelCountry,
                    obs.DepartDate,
                    obs.ReturnDate,
                    obs.DurationDays,
                    obs.ControllerId,
                    obs.EmployeeId
                );
            }

            return dt;
        }
    }

    public class AdvancedObservationIndexViewModel
    {
        public DataTable ControllerObservations { get; set; }
        public DataTable AISObservations { get; set; }
        public DataTable CNSObservations { get; set; }
        public DataTable AFTNObservations { get; set; }
        public DataTable ATFMObservations { get; set; }
        public DataTable OpsStaffObservations { get; set; }
    }
} 