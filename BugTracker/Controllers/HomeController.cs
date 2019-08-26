using BugTracker.Helpers;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //use for login
        public ActionResult Login()
        {
            return View();
        }

        //use for registration
        public ActionResult Register()
        {
            return View();
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult UserProfile()
        {
            var userId = User.Identity.GetUserId();
            var user = new UserProfileViewModel
            {
                Id = userId,
                AvatarUrl = db.Users.Find(userId).AvatarUrl
            };
            return View(user);
        }

        //
        // POST: /Home/AddAvatarUrl
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAvatarUrl(UserProfileViewModel model, HttpPostedFileBase avatar)
        {
            if (ModelState.IsValid)
            {
                //Validator
                if (FileHelper.IsValidAttachment(avatar))
                {
                    var fileName = Path.GetFileName(avatar.FileName);
                    avatar.SaveAs(Path.Combine(Server.MapPath("~/Images/"), fileName));
                    var user = db.Users.Find(model.Id);
                    user.AvatarUrl = "/Images/" + fileName;
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult DemoUser()
        {
            return View();
        }
    }
}