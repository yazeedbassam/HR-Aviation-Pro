using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using WebApplication1.ViewModels; // Ensure this using statement is present

namespace WebApplication1.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly SqlServerDb _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProjectsController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _db = new SqlServerDb(configuration);
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Projects
        public IActionResult Index()
        {
            try
            {
                var allProjects = _db.GetAllProjects();
                var allParticipants = _db.GetAllProjectParticipants();
                var allDivisions = _db.GetAllProjectDivisions();

                var viewModel = new ProjectIndexViewModel();

                foreach (var project in allProjects)
                {
                    var projectCard = new ProjectViewModel
                    {
                        Project = project,
                        Participants = allParticipants.Contains(project.ProjectId) ? allParticipants[project.ProjectId].ToList() : new List<string>(),
                        Divisions = allDivisions.Contains(project.ProjectId) ? allDivisions[project.ProjectId].ToList() : new List<string>()
                    };
                    viewModel.Projects.Add(projectCard);
                }

                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred while fetching projects: {ex.Message}";
                return View(new ProjectIndexViewModel());
            }
        }

        // GET: /Projects/Create
        public IActionResult Create()
        {
            var viewModel = new ProjectCreateViewModel();
            LoadDropdownsForCreate(viewModel);
            return View(viewModel);
        }

        // POST: /Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProjectCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string safeProjectName = Regex.Replace(viewModel.Project.ProjectName, @"[^\w\s-]", "").Replace(" ", "_");
                    string folderName = $"{DateTime.Now:yyyyMMdd}_{safeProjectName}";
                    string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "projects", folderName);

                    Directory.CreateDirectory(folderPath);
                    viewModel.Project.FolderPath = $"/projects/{folderName}";

                    int newProjectId = _db.AddProject(viewModel.Project);

                    var controllerIds = viewModel.SelectedParticipantIds?.Where(id => id.StartsWith("c-")).Select(id => int.Parse(id.Substring(2))).ToList() ?? new List<int>();
                    var employeeIds = viewModel.SelectedParticipantIds?.Where(id => id.StartsWith("e-")).Select(id => int.Parse(id.Substring(2))).ToList() ?? new List<int>();

                    _db.AddProjectParticipants(newProjectId, controllerIds, employeeIds);
                    _db.AddProjectDivisions(newProjectId, viewModel.SelectedDivisionIds ?? new List<int>());

                    TempData["SuccessMessage"] = $"Project '{viewModel.Project.ProjectName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while creating the project: {ex.Message}");
                }
            }
            LoadDropdownsForCreate(viewModel);
            return View(viewModel);
        }

        // GET: Projects/Details/5
        public IActionResult Details(int id)
        {
            var project = _db.GetProjectById(id);
            if (project == null) return NotFound();

            var viewModel = new ProjectDetailViewModel
            {
                Project = project,
                Participants = _db.GetParticipantsByProjectId(id),
                Divisions = _db.GetDivisionsByProjectId(id),
                Files = new List<ProjectFileViewModel>()
            };

            if (!string.IsNullOrEmpty(project.FolderPath))
            {
                string physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, project.FolderPath.TrimStart('/', '\\'));
                if (Directory.Exists(physicalPath))
                {
                    var directoryInfo = new DirectoryInfo(physicalPath);
                    foreach (var file in directoryInfo.GetFiles().OrderBy(f => f.Name))
                    {
                        viewModel.Files.Add(new ProjectFileViewModel
                        {
                            FileName = file.Name,
                            FilePath = $"{project.FolderPath}/{file.Name}".Replace('\\', '/'),
                            FormattedFileSize = FormatFileSize(file.Length),
                            FileIconClass = GetIconForFile(file.Extension)
                        });
                    }
                }
            }
            return View(viewModel);
        }

        // GET: Projects/Edit/5
        public IActionResult Edit(int id)
        {
            var project = _db.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new ProjectEditViewModel
            {
                ProjectId = id,
                Project = project,
                SelectedParticipantIds = _db.GetSelectedParticipantIds(id),
                SelectedDivisionIds = _db.GetSelectedDivisionIds(id)
            };

            // --- FIX IS HERE: Populate the dropdown lists directly on the viewModel ---
            var controllers = _db.GetAllControllersForSelectList().AsEnumerable()
                .Select(row => new SelectListItem { Value = "c-" + row["ControllerId"].ToString(), Text = row["FullName"].ToString() + " (ATC)" });
            var employees = _db.GetAllEmployeesForSelectList().AsEnumerable()
                .Select(row => new SelectListItem { Value = "e-" + row["EmployeeID"].ToString(), Text = row["FullName"].ToString() + " (Employee)" });

            viewModel.AllParticipants = new List<SelectListItem>();
            viewModel.AllParticipants.AddRange(controllers);
            viewModel.AllParticipants.AddRange(employees);
            viewModel.AllDivisions = _db.GetAllDivisionsForSelectList();
            // --- END OF FIX ---

            return View(viewModel);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProjectEditViewModel viewModel)
        {
            if (id != viewModel.ProjectId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    viewModel.Project.ProjectId = id;
                    _db.UpdateProject(viewModel.Project);

                    var controllerIds = viewModel.SelectedParticipantIds?.Where(pId => pId.StartsWith("c-")).Select(pId => int.Parse(pId.Substring(2))).ToList() ?? new List<int>();
                    var employeeIds = viewModel.SelectedParticipantIds?.Where(pId => pId.StartsWith("e-")).Select(pId => int.Parse(pId.Substring(2))).ToList() ?? new List<int>();

                    _db.UpdateProjectParticipants(id, controllerIds, employeeIds);
                    _db.UpdateProjectDivisions(id, viewModel.SelectedDivisionIds ?? new List<int>());

                    TempData["SuccessMessage"] = "Project updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the project: " + ex.Message);
                }
            }

            // If model state is invalid, you must re-populate the dropdowns
            var ctrls = _db.GetAllControllersForSelectList().AsEnumerable().Select(row => new SelectListItem { Value = "c-" + row["ControllerId"].ToString(), Text = row["FullName"].ToString() + " (ATC)" });
            var emps = _db.GetAllEmployeesForSelectList().AsEnumerable().Select(row => new SelectListItem { Value = "e-" + row["EmployeeID"].ToString(), Text = row["FullName"].ToString() + " (Employee)" });
            viewModel.AllParticipants = new List<SelectListItem>();
            viewModel.AllParticipants.AddRange(ctrls);
            viewModel.AllParticipants.AddRange(emps);
            viewModel.AllDivisions = _db.GetAllDivisionsForSelectList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile file, int projectId)
        {
            var project = _db.GetProjectById(projectId);
            if (project == null || string.IsNullOrEmpty(project.FolderPath))
            {
                TempData["FileMessage"] = "Error: Project or project folder not found.";
                return RedirectToAction("Index");
            }

            if (file == null || file.Length == 0)
            {
                TempData["FileMessage"] = "Error: Please select a file to upload.";
                return RedirectToAction("Details", new { id = projectId });
            }

            try
            {
                string physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, project.FolderPath.TrimStart('/', '\\'));
                Directory.CreateDirectory(physicalPath);
                var finalFilePath = Path.Combine(physicalPath, file.FileName);
                using (var stream = new FileStream(finalFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                TempData["FileMessage"] = $"File '{file.FileName}' uploaded successfully!";
            }
            catch (Exception ex)
            {
                TempData["FileMessage"] = $"Error uploading file: {ex.Message}";
            }
            return RedirectToAction("Details", new { id = projectId });
        }

        // Helper method for Create action
        private void LoadDropdownsForCreate(ProjectCreateViewModel viewModel)
        {
            var controllers = _db.GetAllControllersForSelectList().AsEnumerable()
                .Select(row => new SelectListItem { Value = "c-" + row["ControllerId"].ToString(), Text = row["FullName"].ToString() + " (ATC)" });
            var employees = _db.GetAllEmployeesForSelectList().AsEnumerable()
                .Select(row => new SelectListItem { Value = "e-" + row["EmployeeID"].ToString(), Text = row["FullName"].ToString() + " (Employee)" });

            viewModel.AllParticipants = new List<SelectListItem>();
            viewModel.AllParticipants.AddRange(controllers);
            viewModel.AllParticipants.AddRange(employees);
            viewModel.AllDivisions = _db.GetAllDivisionsForSelectList();
        }

        // Helper method for Details action
        private string GetIconForFile(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "fa-solid fa-file-pdf text-danger",
                ".doc" or ".docx" => "fa-solid fa-file-word text-primary",
                ".xls" or ".xlsx" => "fa-solid fa-file-excel text-success",
                ".ppt" or ".pptx" => "fa-solid fa-file-powerpoint text-warning",
                ".zip" or ".rar" => "fa-solid fa-file-zipper text-secondary",
                ".jpg" or ".jpeg" or ".png" or ".gif" => "fa-solid fa-file-image text-info",
                ".txt" => "fa-solid fa-file-lines text-muted",
                _ => "fa-solid fa-file text-dark",
            };
        }

        // Helper method for Details action
        private string FormatFileSize(long bytes)
        {
            var unit = 1024;
            if (bytes < unit) return $"{bytes} B";
            var exp = (int)(Math.Log(bytes) / Math.Log(unit));
            var pre = "KMGTPE"[exp - 1];
            return $"{bytes / Math.Pow(unit, exp):F1} {pre}B";
        }


        [HttpPost("Projects/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var project = _db.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }

            try
            {
                _db.DeleteProject(id);

                if (!string.IsNullOrEmpty(project.FolderPath))
                {
                    string physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, project.FolderPath.TrimStart('/', '\\'));
                    if (Directory.Exists(physicalPath))
                    {
                        Directory.Delete(physicalPath, true);
                    }
                }

                TempData["SuccessMessage"] = $"Project '{project.ProjectName}' and all its files were deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the project: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
        }

        // --- NEW ACTION TO DELETE A FILE ---
        // POST: /Projects/DeleteFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFile(int projectId, string fileName)
        {
            var project = _db.GetProjectById(projectId);
            if (project == null || string.IsNullOrEmpty(project.FolderPath))
            {
                TempData["FileMessage"] = "Error: Project or project folder not found.";
                return RedirectToAction("Details", new { id = projectId });
            }

            if (string.IsNullOrEmpty(fileName))
            {
                TempData["FileMessage"] = "Error: Invalid file name.";
                return RedirectToAction("Details", new { id = projectId });
            }

            try
            {
                // Construct the full physical path to the file
                string physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, project.FolderPath.TrimStart('/', '\\'));
                string filePath = Path.Combine(physicalPath, fileName);

                // Check if the file exists before attempting to delete
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    TempData["FileMessage"] = $"File '{fileName}' was deleted successfully.";
                }
                else
                {
                    TempData["FileMessage"] = $"Error: File '{fileName}' not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["FileMessage"] = $"An error occurred while deleting the file: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = projectId });
        }
    }
}
