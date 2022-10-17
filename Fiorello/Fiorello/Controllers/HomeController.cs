using Fiorello.DAL;
using Fiorello.Helpers;
using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDBContext _db;
        public HomeController(AppDBContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new HomeVM
            {

                Products = await _db.Products.Where(x => !x.IsDeactive).ToListAsync(),
                Categories = await _db.Categories.Where(x => !x.IsDeactive).ToListAsync(),


            };


            return View(homeVM);
        }

        public async Task<IActionResult> Subscribe(string email)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (!Helper.IsValidEmail(email))
                {
                    return Content("Bu email adres deyil");
                }
                bool isExist = await _db.Subscribes.AnyAsync(x => x.Email == email);
                if (isExist)
                {
                    return Content("Bu email artiq abunedir");
                }
                await _db.Subscribes.AddAsync(new Subscribe { Email = email });
                await _db.SaveChangesAsync();
               
            }
            else
            {
               
                AppUser appUser = await _db.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
                await _db.Subscribes.AddAsync(new Subscribe { Email = appUser.Email });
                await _db.SaveChangesAsync();
            }
            return Content("Tebrikler siz abone oldunuz");

        }




        public IActionResult Error()
        {
            return View();
        }
    }
}
