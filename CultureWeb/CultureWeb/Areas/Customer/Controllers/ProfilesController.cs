﻿
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using System.Data;

using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using CultureWeb;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CultureWeb.Areas.Admin.Models;

using CultureWeb.Data;
using CultureWeb.Models;
using CultureWeb.Utility;
using System.Net;
using X.PagedList;
using Newtonsoft.Json.Schema;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Security.Claims;

namespace CultureWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfilesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _hostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user's ID

            if (userId == null)
            {
                return NotFound(); // Handle the case where the user is not found
            }

            var user = _userManager.FindByIdAsync(userId).Result as ApplicationUser; // Find the user by ID

            if (user == null)
            {
                return NotFound(); // Handle the case where the user is not found
            }

            var profileViewModel = new ApplicationUser
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Image = user.Image,
                // Map other properties as needed
            };

            return View(profileViewModel);
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return NotFound();
            }

            var user = _userManager.FindByIdAsync(userId).Result as ApplicationUser;

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public IActionResult EditProfile(ApplicationUser model)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return NotFound();
            }

            var user = _userManager.FindByIdAsync(userId).Result as ApplicationUser;

            if (user == null)
            {
                return NotFound();
            }
            // Check if a new image has been uploaded
            if (model.ImageFile != null)
            {
                // Upload the new image and set the model's Image property to the new file name
                string uniqueFileName = UploadFile(model);
                model.Image = uniqueFileName;
            }
            else
            {
                // Keep the existing image file name in the model's Image property
                var existingUser = _context.ApplicationUsers.AsNoTracking().FirstOrDefault(m => m.Id == user.Id);
                if (existingUser != null)
                {
                    model.Image = existingUser.Image;
                }
            }

            // Update the user's profile properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;
            user.Image = model.Image;



            // Update other properties as needed

            // Save the changes to the user's profile
            var result = _userManager.UpdateAsync(user).Result;

            if (result.Succeeded)
            {
                return RedirectToAction("Profile");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        private string UploadFile(ApplicationUser model)
        {
            string uniqueFileName = null;
            if (model.ImageFile != null)
            {
                string uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "Images/User/");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImageFile.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

    }
}
