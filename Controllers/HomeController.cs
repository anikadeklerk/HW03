using HW03.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HW03.Controllers
{
    public class HomeController : Controller
    {
        private readonly BikeStoresEntities db = new BikeStoresEntities();

        public async Task<ActionResult> Index(string brandFilter, string categoryFilter)
        {
            try
            {
               
                ViewBag.StoreCount = await db.stores.CountAsync();
                ViewBag.StaffCount = await db.staffs.CountAsync();
                ViewBag.CustomerCount = await db.customers.CountAsync();
                ViewBag.ProductCount = await db.products.CountAsync();
                ViewBag.OrderCount = await db.orders.CountAsync();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error loading dashboard: " + ex.Message;
            }

            ViewBag.Stores = await db.stores.ToListAsync();
            var staffs = await db.staffs.ToListAsync();
            var customers = await db.customers.ToListAsync();

            var products = db.products.Include(p => p.brand).Include(p => p.category);
            if (!string.IsNullOrEmpty(brandFilter))
                products = products.Where(p => p.brand.brand_name == brandFilter);
            if (!string.IsNullOrEmpty(categoryFilter))
                products = products.Where(p => p.category.category_name == categoryFilter);

            ViewBag.Brands = db.brands.Select(b => b.brand_name).Distinct().ToList();
            ViewBag.Categories = db.categories.Select(c => c.category_name).Distinct().ToList();

            ViewBag.Staffs = staffs;
            ViewBag.Customers = customers;
            ViewBag.Products = await products.ToListAsync();

            string imageDir = Server.MapPath("~/Content/Images");
            var imageFiles = Directory.GetFiles(imageDir, "*.jpeg")
                                      .Select(Path.GetFileNameWithoutExtension)
                                      .ToList();
            ViewBag.ImageFiles = imageFiles;

            return View();
        }


        // ===== CREATE STAFF  =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateStaff(staff model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            if (model.store_id == 0)
            {
                TempData["Msg"] = "Please select a valid store.";
                return RedirectToAction("Index");
            }
            model.store_id = 1; // default store id
            db.staffs.Add(model);
            await db.SaveChangesAsync();

            TempData["Msg"] = "Staff created successfully.";
            return RedirectToAction("Index");
        }


        // ===== CREATE CUSTOMER  =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCustomer(customer model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");
            db.customers.Add(model);
            await db.SaveChangesAsync();
            TempData["Msg"] = "Customer created successfully.";
            return RedirectToAction("Index");
        }
    }
}
