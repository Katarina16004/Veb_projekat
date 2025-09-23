using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
using Veb_Projekat.Repositories;

namespace Veb_Projekat.Controllers
{
    public class AccountController : Controller
    {
        // GET: Registration
        public ActionResult Register()
        {
            return View();
        }

        // POST: Registration
        [HttpPost]
        public ActionResult Register(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (UserRepository.GetByUsername(model.Username) != null)
            {
                ModelState.AddModelError("", "Username already exists");
                return View(model);
            }

            model.UserRole = RoleEnum.Tourist;

            UserRepository.Add(model);

            TempData["Success"] = "Registration successful! You can now log in";
            return RedirectToAction("Login");
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = UserRepository.GetByUsername(username);

            if (user == null || user.Password != password)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View();
            }

            // Save user in session
            Session["CurrentUser"] = user;

            TempData["Success"] = $"Welcome {user.FirstName}!";
            return RedirectToAction("Index", "Home");
        }

        // Logout
        public ActionResult Logout()
        {
            Session["CurrentUser"] = null;
            return RedirectToAction("Login");
        }
    }
}