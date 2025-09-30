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
        public ActionResult Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            if (UserRepository.GetByUsername(user.Username) != null)
            {
                ModelState.AddModelError("", "Username already exists");
                return View(user);
            }

            user.UserRole = RoleEnum.Tourist;
            UserRepository.Add(user);

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

            // Proveri cookie i popuni username
            HttpCookie rememberCookie = Request.Cookies["RememberUser"];
            if (rememberCookie != null && !string.IsNullOrEmpty(rememberCookie.Values["username"]))
            {
                ViewBag.RememberedUsername = rememberCookie.Values["username"];
            }

            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password, bool rememberMe = false)
        {
            // Prvo probamo sa form parametrima
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                // Regularni login sa formom
                var user = UserRepository.GetByUsername(username);
                if (user == null || user.Password != password)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View();
                }

                SessionUser sessionUser = new SessionUser();
                sessionUser.Login(user);
                Session["CurrentUser"] = sessionUser;

                // Kreiramo novi cookie za ovog korisnika
                if (rememberMe)
                {
                    HttpCookie rememberCookie = new HttpCookie("RememberUser");
                    rememberCookie.Values["username"] = user.Username;
                    rememberCookie.Expires = DateTime.Now.AddHours(12);
                    Response.Cookies.Add(rememberCookie);
                }
                else
                {
                    // Brisemo cookie ako nije cekiran Remember Me
                    HttpCookie oldCookie = Request.Cookies["RememberUser"];
                    if (oldCookie != null)
                    {
                        oldCookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(oldCookie);
                    }
                }

                TempData["Success"] = $"Welcome {user.FirstName}!";
                return RedirectBasedOnRole(user.UserRole);
            }
            else
            {
                // Auto-login sa cookie ako je forma prazna
                HttpCookie rememberCookie = Request.Cookies["RememberUser"];
                if (rememberCookie != null && !string.IsNullOrEmpty(rememberCookie.Values["username"]))
                {
                    string cookieUsername = rememberCookie.Values["username"];
                    var rememberedUser = UserRepository.GetByUsername(cookieUsername);

                    if (rememberedUser != null)
                    {
                        SessionUser sessionUser = new SessionUser();
                        sessionUser.Login(rememberedUser);
                        Session["CurrentUser"] = sessionUser;

                        TempData["Success"] = $"Welcome back {rememberedUser.FirstName}!";
                        return RedirectBasedOnRole(rememberedUser.UserRole);
                    }
                }
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View();
        }

        private ActionResult RedirectBasedOnRole(RoleEnum role)
        {
            return RedirectToAction("Index", "Home");
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

            // Brisemo cookie
            HttpCookie rememberCookie = Request.Cookies["RememberUser"];
            if (rememberCookie != null)
            {
                rememberCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(rememberCookie);
            }

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

        // GET: Profile (Edit Profile)
        public ActionResult EditProfile()
        {
            var currentUser = Session["CurrentUser"] as SessionUser;
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var user = UserRepository.GetByUsername(currentUser.Username);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        // POST: EditProfile
        [HttpPost]
        public ActionResult EditProfile(User updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedUser);
            }

            var currentUser = Session["CurrentUser"] as SessionUser;
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var user = UserRepository.GetByUsername(currentUser.Username);
            if (user == null)  
                return HttpNotFound();

            try
            {
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Password= updatedUser.Password;
                user.Email = updatedUser.Email;
                user.Gender = updatedUser.Gender;

                UserRepository.Update(user);

                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("EditProfile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating profile: " + ex.Message);
                return View(updatedUser);
            }
        }
    }
}