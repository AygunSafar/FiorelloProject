using Fiorello.DAL;
using Fiorello.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Areas.admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDBContext _db;
        
        public CategoryController(AppDBContext db)
        {
            _db = db;
           
        }
        public IActionResult Index()
        {
            List<Category> categories = _db.Categories.ToList();
            return View(categories);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
       [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool isExist = await _db.Categories.AnyAsync(x => x.Name == category.Name);
            if (isExist)
            {
                ModelState.AddModelError("Name", "This category is already exist");
                return View();
            }
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
