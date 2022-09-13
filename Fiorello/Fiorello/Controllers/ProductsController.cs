using Fiorello.DAL;
using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Controllers
{
    public class ProductsController : Controller
    {


        private readonly AppDBContext _db;
        public ProductsController(AppDBContext db)
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
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product product = await _db.Products.Include(x=>x.productDetail).FirstOrDefaultAsync(x=>x.Id==id);
            if (product == null)
            {
                return BadRequest();
            }
            return View(product);
        }


    }
}
