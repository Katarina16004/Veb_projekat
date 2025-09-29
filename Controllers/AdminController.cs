using System;
using System.Linq;
using System.Web.Mvc;
using Veb_Projekat.Filters;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;
using Veb_Projekat.Services;

namespace Veb_Projekat.Controllers
{
    [AuthorizeSession(RoleEnum.Administrator)]
    public class AdminController : BaseController
    {
        

        public ActionResult Index()
        {
            var users = UserRepository.GetAll();

            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalTourists = users.Count(u => u.UserRole == RoleEnum.Tourist);
            ViewBag.TotalManagers = users.Count(u => u.UserRole == RoleEnum.Manager);
            ViewBag.TotalAdmins = users.Count(u => u.UserRole == RoleEnum.Administrator);

            return View();
        }


        // GET: ViewAllUsers
        public ActionResult ViewAllUsers(string searchName = "", string searchLastName = "",
            string roleFilter = "")
        {
            var users = UserRepository.GetAll();

            users = UserService.SearchUsers(users, searchName, searchLastName, roleFilter);

            ViewBag.AllUsers = users;
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalTourists = users.Count(u => u.UserRole == RoleEnum.Tourist);
            ViewBag.TotalManagers = users.Count(u => u.UserRole == RoleEnum.Manager);
            ViewBag.TotalAdmins = users.Count(u => u.UserRole == RoleEnum.Administrator);

            ViewBag.SearchName = searchName;
            ViewBag.SearchLastName = searchLastName;
            ViewBag.RoleFilter = roleFilter;

            return View();
        }


        // GET: RegisterManager
        public ActionResult RegisterManager()
        {
            return View();
        }

        // POST: RegisterManager
        [HttpPost]
        public ActionResult RegisterManager(string username, string password, string firstName,
            string lastName, GenderEnum gender, string email, DateTime dateOfBirth)
        {
            if (!UserService.ValidateManagerData(username, password, firstName, lastName, email, out string errorMessage))
            {
                ModelState.AddModelError("", errorMessage);
                return View();
            }

            if (UserRepository.GetByUsername(username) != null)
            {
                ModelState.AddModelError("", "Username already exists");
                return View();
            }

            var manager = new User
            {
                Username = username,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                Gender = gender,
                Email = email,
                DateOfBirth = dateOfBirth,
                UserRole = RoleEnum.Manager
            };

            try
            {
                UserRepository.Add(manager);
                TempData["Success"] = $"Manager '{username}' registered successfully!";
                return RedirectToAction("ViewAllUsers");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error registering manager: " + ex.Message);
                return View();
            }
        }

    }
}