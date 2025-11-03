using HW03.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HW03.Controllers
{
    public class MaintainController : Controller
    {
        private readonly BikeStoresEntities db = new BikeStoresEntities();
        public async Task<ActionResult> Index(
       string brandFilter,
       string categoryFilter,
       int takeStaff = 10,
       int takeCustomers = 10,
       int takeProducts = 10)
        {
            // ✅ Load Stores for Staff Edit Dropdown
            ViewBag.Stores = await db.stores.ToListAsync();

            // ✅ Limit Staff
            ViewBag.Staffs = await db.staffs
                .OrderBy(s => s.last_name)
                .Take(takeStaff)
                .ToListAsync();

            // ✅ Limit Customers
            ViewBag.Customers = await db.customers
                .OrderBy(c => c.last_name)
                .Take(takeCustomers)
                .ToListAsync();

            // ✅ Products (with filtering)
            var products = db.products
                .Include(p => p.brand)
                .Include(p => p.category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(brandFilter))
                products = products.Where(p => p.brand.brand_name == brandFilter);
            if (!string.IsNullOrEmpty(categoryFilter))
                products = products.Where(p => p.category.category_name == categoryFilter);

            ViewBag.Products = await products
                .OrderBy(p => p.product_name)
                .Take(takeProducts)
                .ToListAsync();

            // ✅ Product count
            ViewBag.ProductCount = ((System.Collections.IEnumerable)ViewBag.Products)
                .Cast<object>().Count();

            // ✅ Load brand & category data once
            ViewBag.Brands = db.brands.Select(b => b.brand_name).Distinct().ToList();
            ViewBag.Categories = db.categories.Select(c => c.category_name).Distinct().ToList();

            // ✅ For edit modals (full objects)
            ViewBag.BrandList = await db.brands.ToListAsync();
            ViewBag.CategoryList = await db.categories.ToListAsync();

            // ✅ Load images dynamically
            string imageDir = Server.MapPath("~/Content/Images");
            ViewBag.ImageFiles = Directory.Exists(imageDir)
                ? Directory.GetFiles(imageDir, "*.jpeg")
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList()
                : new List<string>();

            return View();
        }



        //public async Task<ActionResult> Index(string brandFilter, string categoryFilter)
        //{
        //    // ✅ Load stores for staff edit dropdown
        //    ViewBag.Stores = await db.stores.ToListAsync();

        //    // ✅ Load images dynamically
        //    string imageDir = Server.MapPath("~/Content/Images");
        //    ViewBag.ImageFiles = Directory.Exists(imageDir)
        //        ? Directory.GetFiles(imageDir, "*.jpeg").Select(Path.GetFileNameWithoutExtension).ToList()
        //        : new List<string>();

        //    // ✅ Load brand & category filters
        //    ViewBag.Brands = await db.brands.Select(b => b.brand_name).ToListAsync();
        //    ViewBag.Categories = await db.categories.Select(c => c.category_name).ToListAsync();
        //    ViewBag.SelectedBrand = brandFilter;
        //    ViewBag.SelectedCategory = categoryFilter;

        //    // ✅ Load Staff and Customers
        //    ViewBag.Staffs = await db.staffs.OrderBy(s => s.last_name).ToListAsync();
        //    ViewBag.Customers = await db.customers.OrderBy(c => c.last_name).ToListAsync();

        //    // ✅ Filter Products
        //    var products = db.products.Include(p => p.brand).Include(p => p.category).AsQueryable();
        //    if (!string.IsNullOrEmpty(brandFilter))
        //        products = products.Where(p => p.brand.brand_name == brandFilter);
        //    if (!string.IsNullOrEmpty(categoryFilter))
        //        products = products.Where(p => p.category.category_name == categoryFilter);

        //    ViewBag.Products = await products.OrderBy(p => p.product_name).ToListAsync();
        //    ViewBag.ProductCount = await products.CountAsync();

        //    return View();
        //}



        // ---------- STAFF ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStaff(staff model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");
            db.Entry(model).State = EntityState.Modified;
            await db.SaveChangesAsync();
            TempData["Msg"] = "Staff updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteStaff(int staff_id)
        {
            var s = await db.staffs.FindAsync(staff_id);
            if (s != null)
            {
                db.staffs.Remove(s);
                await db.SaveChangesAsync();
            }
            TempData["Msg"] = "Staff deleted.";
            return RedirectToAction("Index");
        }

        // ---------- CUSTOMERS ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateCustomer(customer model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");
            db.Entry(model).State = EntityState.Modified;
            await db.SaveChangesAsync();
            TempData["Msg"] = "Customer updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteCustomer(int customer_id)
        {
            var c = await db.customers.FindAsync(customer_id);
            if (c != null)
            {
                db.customers.Remove(c);
                await db.SaveChangesAsync();
            }
            TempData["Msg"] = "Customer deleted.";
            return RedirectToAction("Index");
        }

        // ---------- PRODUCTS ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProduct(product model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");
            db.Entry(model).State = EntityState.Modified;
            await db.SaveChangesAsync();
            TempData["Msg"] = "Product updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteProduct(int product_id)
        {
            var p = await db.products.FindAsync(product_id);
            if (p != null)
            {
                db.products.Remove(p);
                await db.SaveChangesAsync();
            }
            TempData["Msg"] = "Product deleted.";
            return RedirectToAction("Index");
        }
    }
}
