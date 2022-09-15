using Fiorello.DAL;
using Fiorello.Helpers;
using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Fiorello.Helpers.Helper;

namespace Fiorello.Areas.admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDBContext _db;
        public UsersController(AppDBContext db,UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;

        }


        public async Task<IActionResult> Index()
        {
            List<AppUser> users = await _userManager.Users.ToListAsync();
            List<UserVM> userVMs = new List<UserVM>();
            foreach (AppUser user in users)
            {
                UserVM userVM = new UserVM 
                { 
                 FullName=user.FullName,
                 UserName=user.UserName,
                 Email=user.Email,
                 Id=user.Id,
                 IsDeactive=user.IsDeactive,
                 Role=(await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };
                userVMs.Add(userVM);


            }
            return View(userVMs);


        }
        public async Task<IActionResult> Activity(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            if (user.IsDeactive)
            {
                user.IsDeactive = false;
            }
            else
            {
                user.IsDeactive = true;
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterVM register)
        {
            AppUser appUser = new AppUser
            {
                FullName = register.FullName,
                UserName = register.UserName,
                Email = register.Email,

            };
            IdentityResult identityResult = await _userManager.CreateAsync(appUser, register.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            
            await _userManager.AddToRoleAsync(appUser, Helper.Roles.Admin.ToString());
            return RedirectToAction("Index", "Home");

            return View();
        }
        public async Task<IActionResult> Update(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            UpdateVM dbUpdateVM = new UpdateVM { 
             FullName=user.FullName,
             Email=user.Email,
             UserName=user.UserName,
             
            };

           
            return View(dbUpdateVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Update(string id,UpdateVM updateVM)
        {
         
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            UpdateVM dbUpdateVM = new UpdateVM
            {
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,

            };
            if (!ModelState.IsValid)
            {
                return View(dbUpdateVM);
            }
            //bool isExistName = await _db.Users.AnyAsync(x => x.UserName == updateVM.UserName || x.Email == updateVM.Email) /*|| (x.Email == updateVM.Email)*/;
            //if (isExistName)
            //{
            //    ModelState.AddModelError(" ", "This Username is already Exist");
            //    return View(dbUpdateVM);
            //}
            bool isExistName = await _db.Users.AnyAsync(x => x.UserName == updateVM.UserName && x.Id!=id);
            if (isExistName)
            {
                ModelState.AddModelError("UserName", "This Username is already exist");
                return View(dbUpdateVM);
            }
            bool isExistEmail = await _db.Users.AnyAsync(x => x.Email == updateVM.Email&&x.Id != id);
            if (isExistEmail)
            {
                ModelState.AddModelError("Email", "This Email is already exist");
                return View(dbUpdateVM);
            }
            user.FullName = updateVM.FullName;
            user.UserName = updateVM.UserName;
            user.Email = updateVM.Email;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");


        }
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }


            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id,ResetPasswordVM resetPasswordVM)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult identityResult = await _userManager.ResetPasswordAsync(user,token,resetPasswordVM.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                    
                }
                return View();
            }


            return RedirectToAction("Index");

        }
    } 
}
