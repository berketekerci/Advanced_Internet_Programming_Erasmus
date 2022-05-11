using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SchoolRegister.Model.DataModels;
using SchoolRegister.Services.Interfaces;
using SchoolRegister.ViewModels.VM;

namespace SchoolRegister.Web.Controllers
{
    public class StudentController : BaseController
    {
        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;
        private readonly UserManager<User> _userManager;
        private readonly IGradeService _gradeService;

        public StudentController(ILogger logger,
            IMapper mapper,
            IStringLocalizer localizer,
            IStudentService studentService,
            IGroupService groupService,
            UserManager<User> userManager,
            IGradeService gradeService) : base(logger, mapper, localizer)
        {
            _studentService = studentService;
            _groupService = groupService;
            _userManager = userManager;
            _gradeService = gradeService;
        }

        [Authorize(Roles = "Teacher, Admin, Parent")]
        public IActionResult Index(string filterValue = null)
        {
            Expression<Func<Student, bool>> filterExpression = null;
            if (!string.IsNullOrWhiteSpace(filterValue))
                filterExpression = s => s.LastName.Contains(filterValue);
            bool isAjaxRequest = HttpContext.Request
            .Headers["x-requested-with"] == "XMLHttpRequest";
            var user = _userManager.GetUserAsync(User).Result;

            IEnumerable<StudentVm> studentsVm = null;
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Parent"))
            {
                var parent = _userManager.GetUserAsync(User).Result;
                studentsVm = _studentService.GetStudents(s => s.ParentId == parent.Id);
            }
            else if (_userManager.IsInRoleAsync(user, "Teacher").Result)
            {
                
                studentsVm = _studentService.GetStudents(filterExpression);
                //var subjectVms = _subjectService.GetSubjects(filterExpression);
                if (isAjaxRequest)
                    return PartialView("_StudentsTableDataPartial", studentsVm);
                return View(studentsVm);

            }
            else if (_userManager.IsInRoleAsync(user, "Admin").Result)
            {
                studentsVm = _studentService.GetStudents(filterExpression);
                //var subjectVms = _subjectService.GetSubjects(filterExpression);
                if (isAjaxRequest)
                    return PartialView("_StudentsTableDataPartial", studentsVm);
                return View(studentsVm);

            }
            return View(studentsVm);
        }

        [Authorize(Roles = "Teacher, Admin, Parent, Student")]
        public IActionResult Details(int studentId)
        {
            var getGradesDto = new GetGradesReportVm
            {
                StudentId = studentId,
                GetterUserId = _userManager.GetUserAsync(User).Result.Id
            };
            var studentGradesReport = _gradeService.GetGradesReportForStudent(getGradesDto);
            if (studentGradesReport == null) return View("Error");
            return View(studentGradesReport);
        }

        [Authorize(Roles = "Teacher, Admin")]
        public IActionResult AttachStudentToGroup(int studentId)
        {
            ViewBag.ActionType = Localizer["Attach"];
            return AttachDetachGetView(studentId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher, Admin")]
        public IActionResult AttachStudentToGroup(AttachDetachStudentToGroupVm attachDetachStudentToGroupVm)
        {
            if (ModelState.IsValid)
            {
                _groupService.AttachStudentToGroup(attachDetachStudentToGroupVm);
                return RedirectToAction("Index");
            }
            return View("AttachDetachStudentToGroup");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher, Admin")]
        public IActionResult DetachStudentToGroup(AttachDetachStudentToGroupVm attachDetachStudentToGroupVm)
        {
            if (ModelState.IsValid)
            {
                _groupService.DetachStudentFromGroup(attachDetachStudentToGroupVm);
                return RedirectToAction("Index");
            }
            return View("AttachDetachStudentToGroup");
        }

        [Authorize(Roles = "Teacher, Admin")]
        private IActionResult AttachDetachGetView(int studentId)
        {
            var students = _studentService.GetStudents()
                                        .ToList();
            var groups = _groupService.GetGroups();
            var currentStudent = students.FirstOrDefault(x => x.Id == studentId);
            if (currentStudent == null)
            {
                throw new ArgumentNullException($"studentId not exists.");
            }
            var attachDetachStudentToGroupDto = new AttachDetachStudentToGroupVm
            {
                StudentId = currentStudent.Id
            };
            ViewBag.SubjectList = new SelectList(students.Select(s => new
            {
                Text = $"{s.FirstName} {s.LastName}",
                Value = s.Id,
                Selected = s.Id == currentStudent.Id
            }), "Value", "Text");
            ViewBag.GroupList = new SelectList(groups.Select(s => new
            {
                Text = s.Name,
                Value = s.Id
            }), "Value", "Text");
            return View("AttachDetachStudentToGroup", attachDetachStudentToGroupDto);
        }
    }

}