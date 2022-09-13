using Fiorello.DAL;
using Fiorello.Helpers;
using Fiorello.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fiorello.Areas.admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDBContext _db;
        private readonly IWebHostEnvironment _env;
        public ProductsController(AppDBContext db,IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _db.Products.Include(x=>x.Category).ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product,int catId)
        {

            ViewBag.Categories = await _db.Categories.ToListAsync();
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool isExist = await _db.Products.AnyAsync(x => x.Name == product.Name);
            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu product movcuddur");
                return View();
            }
            if (product.Photo == null)
            {
                ModelState.AddModelError("Photo", "Zehmet olmasa bir sekil elave edin");
                return View();
            }
            if (!product.Photo.IsImage())
            {
                ModelState.AddModelError("Photo", " sekil elave edin");
                return View();
            }
            if (product.Photo.IsOlder1MB())
            {
                ModelState.AddModelError("Photo", " 1 mb");
                return View();
            }
            string folder = Path.Combine(_env.WebRootPath, "img");
            product.Image = await product.Photo.SaveFileAsync(folder);

            product.CategoryId = catId;
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            if (id == null)
            {
                return NotFound();
            }
            Product dbProduct = await _db.Products.Include(x=>x.productDetail).FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }
            return View(dbProduct);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id,Product product,int catId)
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            if (id == null)
            {
                return NotFound();
            }
            Product dbProduct = await _db.Products.Include(x => x.productDetail).FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return View(dbProduct);
            }
            bool isExist = await _db.Products.AnyAsync(x => x.Name == product.Name&&x.Id!=id);
            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu product movcuddur");
                return View(dbProduct);
            }
            if (product.Photo != null)
            {
                if (!product.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", " sekil elave edin");
                    return View(dbProduct);
                }
                if (product.Photo.IsOlder1MB())
                {
                    ModelState.AddModelError("Photo", " 1 mb");
                    return View(dbProduct);
                }
                string folder = Path.Combine(_env.WebRootPath, "img");
                string path = Path.Combine(folder, dbProduct.Image);
                if (System.IO.File.Exists(folder))
                {
                    System.IO.File.Delete(path);
                }
                dbProduct.Image = await product.Photo.SaveFileAsync(folder);
                dbProduct.Image = await product.Photo.SaveFileAsync(folder);
            }
            dbProduct.CategoryId = catId;
            dbProduct.Name = product.Name;
            dbProduct.Price = product.Price;
            dbProduct.productDetail.Description = product.productDetail.Description;
            await _db.SaveChangesAsync();
            
       
            return RedirectToAction("Index");


        }
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return BadRequest();
            }
            if (product.IsDeactive)
            {
                product.IsDeactive = false;
            }
            else
            {
                product.IsDeactive = true;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
