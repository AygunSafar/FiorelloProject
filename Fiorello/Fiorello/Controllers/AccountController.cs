using Fiorello.Helpers;
using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fiorello.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<AppUser> userManager,
                                  SignInManager<AppUser> signInManager,
                                  RoleManager<IdentityRole> roleManager)
        {
            _userManager= userManager;
            _signInManager= signInManager;
            _roleManager = roleManager;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAsync(RegisterVM register)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }
            AppUser appUser = new AppUser
            {
                FullName = register.FullName,
                UserName=register.UserName,
                Email=register.Email,
               
            };
           IdentityResult identityResult= await _userManager.CreateAsync(appUser, register.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            await _signInManager.SignInAsync(appUser,true);
            await _userManager.AddToRoleAsync(appUser, Helper.Roles.Member.ToString());
            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser user =await _userManager.FindByNameAsync(loginVM.UserName);
            if (user==null)
            {
                ModelState.AddModelError("", "Incorrect username or password specified.");
                return View();
            }
            if (user.IsDeactive)
            {
                ModelState.AddModelError("UserName", "Sorry, this user account is suspended. ");
                return View();
            }
            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(user,loginVM.Password,true,true);
            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError("UserName", "Your access to the site has been blocked for a minute");
                return View();
            }
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError(" ", "Incorrect username or password specified.");
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        //public async Task CreateRoles()
        //{
        //    if(!await _roleManager.RoleExistsAsync(Helper.Roles.Admin.ToString()))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole(Helper.Roles.Admin.ToString()));
        //    }
        //    if (!await _roleManager.RoleExistsAsync(Helper.Roles.Member.ToString()))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole(Helper.Roles.Member.ToString()));
        //    }
        //}
    }
}
