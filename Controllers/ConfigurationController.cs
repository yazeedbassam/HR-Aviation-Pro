using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(IConfigurationService configurationService, ILogger<ConfigurationController> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
        }

        #region Main Configuration Management

        // GET: Configuration
        public IActionResult Index()
        {
            try
            {
                var viewModel = new ConfigurationManagementViewModel
                {
                    Categories = _configurationService.GetAllCategories(),
                    Values = new List<ConfigurationValue>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration index");
                TempData["ErrorMessage"] = "An error occurred while loading the configuration.";
                return View(new ConfigurationManagementViewModel());
            }
        }

        // GET: Configuration/Category/{categoryName}
        public IActionResult Category(string categoryName)
        {
            try
            {
                var category = _configurationService.GetCategoryByName(categoryName);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                var values = _configurationService.GetValuesByCategory(categoryName);
                var viewModel = new ConfigurationManagementViewModel
                {
                    Categories = _configurationService.GetAllCategories(),
                    SelectedCategory = category,
                    Values = values
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category: {CategoryName}", categoryName);
                TempData["ErrorMessage"] = "An error occurred while loading the category.";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Category Management

        // GET: Configuration/CreateCategory
        public IActionResult CreateCategory()
        {
            return View(new ConfigurationCategoryViewModel());
        }

        // POST: Configuration/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(ConfigurationCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = new ConfigurationCategory
                    {
                        CategoryName = model.CategoryName,
                        DisplayName = model.DisplayName,
                        Description = model.Description,
                        IsActive = model.IsActiveString == "true" || model.IsActiveString == "on",
                        DisplayOrder = model.DisplayOrder
                    };

                    if (_configurationService.AddCategory(category))
                    {
                        TempData["SuccessMessage"] = "Category created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create category. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating category: {CategoryName}", model.CategoryName);
                    ModelState.AddModelError("", "An error occurred while creating the category.");
                }
            }

            return View(model);
        }

        // GET: Configuration/EditCategory/{id}
        public IActionResult EditCategory(int id)
        {
            try
            {
                var category = _configurationService.GetCategoryById(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ConfigurationCategoryViewModel
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    DisplayName = category.DisplayName,
                    Description = category.Description,
                    IsActiveString = category.IsActive ? "true" : "false",
                    DisplayOrder = category.DisplayOrder
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category for edit: {CategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the category.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Configuration/EditCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(ConfigurationCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = new ConfigurationCategory
                    {
                        CategoryId = model.CategoryId,
                        CategoryName = model.CategoryName,
                        DisplayName = model.DisplayName,
                        Description = model.Description,
                        IsActive = model.IsActiveString == "true" || model.IsActiveString == "on",
                        DisplayOrder = model.DisplayOrder
                    };

                    if (_configurationService.UpdateCategory(category))
                    {
                        TempData["SuccessMessage"] = "Category updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update category. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating category: {CategoryId}", model.CategoryId);
                    ModelState.AddModelError("", "An error occurred while updating the category.");
                }
            }

            return View(model);
        }

        // POST: Configuration/DeleteCategory/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id)
        {
            try
            {
                if (_configurationService.DeleteCategory(id))
                {
                    TempData["SuccessMessage"] = "Category deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete category.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the category.";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Value Management

        // GET: Configuration/CreateValue
        public IActionResult CreateValue(string categoryName = null)
        {
            try
            {
                var viewModel = new ConfigurationValueViewModel();
                
                if (!string.IsNullOrEmpty(categoryName))
                {
                    var category = _configurationService.GetCategoryByName(categoryName);
                    if (category != null)
                    {
                        viewModel.CategoryId = category.CategoryId;
                        viewModel.CategoryName = category.CategoryName;
                        viewModel.CategoryDisplayName = category.DisplayName;
                    }
                }

                ViewBag.Categories = new SelectList(_configurationService.GetAllCategories(), "CategoryId", "DisplayName");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create value form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Configuration/CreateValue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateValue(ConfigurationValueViewModel model)
        {
            _logger.LogInformation("CreateValue called with IsActiveString: {IsActiveString}", model.IsActiveString);
            
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? "System";
                    
                    var isActive = model.IsActiveString == "true" || model.IsActiveString == "on";
                    _logger.LogInformation("IsActive calculated as: {IsActive}", isActive);
                    
                    var value = new ConfigurationValue
                    {
                        CategoryId = model.CategoryId,
                        ValueKey = model.ValueKey,
                        ValueText = model.ValueText,
                        DisplayOrder = model.DisplayOrder,
                        IsActive = isActive,
                        CreatedBy = currentUser
                    };

                    _logger.LogInformation("About to add value: CategoryId={CategoryId}, ValueKey={ValueKey}, IsActive={IsActive}", 
                        value.CategoryId, value.ValueKey, value.IsActive);

                    if (_configurationService.AddValue(value))
                    {
                        _logger.LogInformation("Value added successfully");
                        TempData["SuccessMessage"] = "Value created successfully.";
                        
                        // Auto-redirect to the category page where the value was added
                        var category = _configurationService.GetCategoryById(model.CategoryId);
                        if (category != null)
                        {
                            return RedirectToAction(nameof(Category), new { categoryName = category.CategoryName });
                        }
                        
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogWarning("Failed to add value");
                        ModelState.AddModelError("", "Failed to create value. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating value: {ValueKey}", model.ValueKey);
                    ModelState.AddModelError("", "An error occurred while creating the value.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState is invalid: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            ViewBag.Categories = new SelectList(_configurationService.GetAllCategories(), "CategoryId", "DisplayName");
            return View(model);
        }

        // GET: Configuration/EditValue/{id}
        public IActionResult EditValue(int id)
        {
            try
            {
                var value = _configurationService.GetValueById(id);
                if (value == null)
                {
                    TempData["ErrorMessage"] = "Value not found.";
                    return RedirectToAction(nameof(Index));
                }

                var category = _configurationService.GetCategoryById(value.CategoryId);
                var viewModel = new ConfigurationValueViewModel
                {
                    ValueId = value.ValueId,
                    CategoryId = value.CategoryId,
                    ValueKey = value.ValueKey,
                    ValueText = value.ValueText,
                    DisplayOrder = value.DisplayOrder,
                    IsActiveString = value.IsActive ? "true" : "false",
                    CategoryName = category?.CategoryName,
                    CategoryDisplayName = category?.DisplayName
                };

                ViewBag.Categories = new SelectList(_configurationService.GetAllCategories(), "CategoryId", "DisplayName");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading value for edit: {ValueId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the value.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Configuration/EditValue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditValue(ConfigurationValueViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = User.FindFirstValue(ClaimTypes.Name) ?? "System";
                    
                    var value = new ConfigurationValue
                    {
                        ValueId = model.ValueId,
                        CategoryId = model.CategoryId,
                        ValueKey = model.ValueKey,
                        ValueText = model.ValueText,
                        DisplayOrder = model.DisplayOrder,
                        IsActive = model.IsActiveString == "true" || model.IsActiveString == "on",
                        ModifiedBy = currentUser
                    };

                    if (_configurationService.UpdateValue(value))
                    {
                        TempData["SuccessMessage"] = "Value updated successfully.";
                        return RedirectToAction(nameof(Category), new { categoryName = model.CategoryName });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update value. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating value: {ValueId}", model.ValueId);
                    ModelState.AddModelError("", "An error occurred while updating the value.");
                }
            }

            ViewBag.Categories = new SelectList(_configurationService.GetAllCategories(), "CategoryId", "DisplayName");
            return View(model);
        }

        // POST: Configuration/DeleteValue/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteValue(int id)
        {
            try
            {
                var value = _configurationService.GetValueById(id);
                if (value == null)
                {
                    TempData["ErrorMessage"] = "Value not found.";
                    return RedirectToAction(nameof(Index));
                }

                var category = _configurationService.GetCategoryById(value.CategoryId);
                
                if (_configurationService.DeleteValue(id))
                {
                    TempData["SuccessMessage"] = "Value deleted successfully.";
                    return RedirectToAction(nameof(Category), new { categoryName = category?.CategoryName });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete value.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting value: {ValueId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the value.";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Logs and Statistics

        // GET: Configuration/Logs
        public IActionResult Logs(int? valueId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var logs = _configurationService.GetLogs(valueId, fromDate, toDate);
                return View(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration logs");
                TempData["ErrorMessage"] = "An error occurred while loading the logs.";
                return View(new List<ConfigurationLog>());
            }
        }

        // GET: Configuration/Statistics
        public IActionResult Statistics()
        {
            try
            {
                var statistics = _configurationService.GetStatistics();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration statistics");
                TempData["ErrorMessage"] = "An error occurred while loading the statistics.";
                return View(new ConfigurationStatisticsViewModel());
            }
        }

        #endregion

        #region Cache Management

        // POST: Configuration/RefreshCache
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RefreshCache()
        {
            try
            {
                _configurationService.RefreshCache();
                TempData["SuccessMessage"] = "Cache refreshed successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache");
                TempData["ErrorMessage"] = "An error occurred while refreshing the cache.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Configuration/ClearCache
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCache()
        {
            try
            {
                _configurationService.ClearCache();
                TempData["SuccessMessage"] = "Cache cleared successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                TempData["ErrorMessage"] = "An error occurred while clearing the cache.";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region API Endpoints for AJAX

        // GET: Configuration/GetValues/{categoryName}
        [HttpGet]
        public IActionResult GetValues(string categoryName)
        {
            try
            {
                var values = _configurationService.GetDropdownValues(categoryName);
                return Json(values);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting values for category: {CategoryName}", categoryName);
                return Json(new List<SelectListItem>());
            }
        }

        // GET: Configuration/GetCategories
        [HttpGet]
        public IActionResult GetCategories()
        {
            try
            {
                var categories = _configurationService.GetAllCategories();
                return Json(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return Json(new List<ConfigurationCategory>());
            }
        }

        #endregion
    }
} 