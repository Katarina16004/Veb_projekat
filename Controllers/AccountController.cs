using System;
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
            SessionUser sessionUser = Session["CurrentUser"] as SessionUser;
            if (sessionUser != null && sessionUser.IsLoggedIn)
            {
                return RedirectToAction("Index", "Home");
            }

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

            SessionUser sessionUser = new SessionUser();
            sessionUser.Login(user);

            Session["CurrentUser"] = sessionUser;

            TempData["Success"] = $"Welcome {user.FirstName}!";

            switch (user.UserRole)
            {
                case RoleEnum.Tourist:
                    return RedirectToAction("MyReservations", "Tourist");
                case RoleEnum.Manager:
                    return RedirectToAction("Index", "Manager");
                case RoleEnum.Administrator:
                    return RedirectToAction("Index", "Admin");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        // Logout
        public ActionResult Logout()
        {
            SessionUser sessionUser = Session["CurrentUser"] as SessionUser;
            if (sessionUser != null)
            {
                sessionUser.Logout();
            }

            Session.Abandon();

            Response.Cache.SetNoStore();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now);
            Response.Cache.SetValidUntilExpires(true);

            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        public new ActionResult Profile()
        {
            SessionUser sessionUser = Session["CurrentUser"] as SessionUser;
            if (sessionUser == null || !sessionUser.IsLoggedIn)
            {
                return RedirectToAction("Login");
            }

            var user = UserRepository.GetByUsername(sessionUser.Username);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            return View(user);
        }
    }
}