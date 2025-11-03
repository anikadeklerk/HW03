using Antlr.Runtime;
using HW03.Models;
using System;
using System.Data.Entity;
using System.EnterpriseServices.CompensatingResourceManager;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace HW03.Controllers
{
    public class HomeController : Controller
    {
        private readonly BikeStoresEntities db = new BikeStoresEntities();

        public async Task<ActionResult> Index(string brandFilter, string categoryFilter)
        {
            try
            {
                //displays live statistics
                //Each number is calculated in the controller using CountAsync(), then passed to the view through ViewBag.
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


            //These dropdowns are automatically populated from the database, so they always reflect the current data in those tables.
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

// It first checks if the input data is valid.
//If it is, it adds a new product entity to the database, saves the changes asynchronously, and refreshes the page.
//It’s a clean pattern that uses Entity Framework’s Add() method for inserting new records.
//And again, I use TempData["Msg"] to show a success message at the top of the page.
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
