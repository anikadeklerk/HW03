using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HW03.Models;

namespace HW03.Controllers
{
    public class ReportsController : Controller
    {
        private readonly BikeStoresEntities db = new BikeStoresEntities();
        private string RepoPath => Server.MapPath("~/App_Data/Reports");

        public class SalesRow
        {
            public string Customer { get; set; }
            public string Staff { get; set; }
            public string Product { get; set; }
            public string Brand { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal { get; set; }
            public DateTime OrderDate { get; set; }
        }

     
        public async Task<ActionResult> Index()
        {
            // Join order_items  orders  customers  staffs  products -> 
            var q = from oi in db.order_items
                    join o in db.orders on oi.order_id equals o.order_id
                    join c in db.customers on o.customer_id equals c.customer_id
                    join s in db.staffs on o.staff_id equals s.staff_id
                    join p in db.products on oi.product_id equals p.product_id
                    join b in db.brands on p.brand_id equals b.brand_id
                    select new SalesRow
                    {
                        Customer = c.first_name + " " + c.last_name,
                        Staff = s.first_name + " " + s.last_name,
                        Product = p.product_name,
                        Brand = b.brand_name,
                        Quantity = oi.quantity,
                        LineTotal = oi.list_price * oi.quantity * (1 - oi.discount),
                        OrderDate = o.order_date
                    };

            var rows = await q.OrderByDescending(r => r.OrderDate).ToListAsync();

            // Aggregate for chart
            var brandAgg = rows
                .GroupBy(r => r.Brand)
                .Select(g => new { Brand = g.Key, Total = g.Sum(x => x.LineTotal) })
                .OrderByDescending(x => x.Total)
                .ToList();

            ViewBag.Sales = rows;
            ViewBag.BrandLabels = brandAgg.Select(x => x.Brand).ToArray();
            ViewBag.BrandTotals = brandAgg.Select(x => Math.Round(x.Total, 2)).ToArray();

            // Archive list
            Directory.CreateDirectory(RepoPath);
            var files = Directory.GetFiles(RepoPath)
                                 .Select(Path.GetFileName)
                                 .OrderBy(f => f)
                                 .ToList();
            ViewBag.Files = files;

            return View();
        }

        // ===== Save posted  report as file + description =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(string fileName, string fileType, string base64Data, string description)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(fileType) || string.IsNullOrWhiteSpace(base64Data))
            {
                TempData["Msg"] = "Missing save parameters.";
                return RedirectToAction("Index");
            }

            Directory.CreateDirectory(RepoPath);

            var parts = base64Data.Split(',');
            var meta = parts[0];
            var b64 = parts.Length > 1 ? parts[1] : "";
            var bytes = Convert.FromBase64String(b64);

            var safeName = Path.GetFileNameWithoutExtension(fileName);
            var ext = fileType.ToLower() == "pdf" ? ".pdf" : ".png";

            var fullPath = Path.Combine(RepoPath, safeName + ext);
            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true))
            {
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }

            
          
            var descPath = Path.Combine(RepoPath, safeName + ".txt");
            using (var writer = new StreamWriter(descPath, false, Encoding.UTF8))
            {
                await writer.WriteAsync(description ?? "");
            }

            TempData["Msg"] = $"Report saved as {safeName + ext}.";
            return RedirectToAction("Index");

        }

        // ===== Download from archive =====
        public ActionResult Download(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return HttpNotFound();
            var full = Path.Combine(RepoPath, Path.GetFileName(name));
            if (!System.IO.File.Exists(full)) return HttpNotFound();
            var mime = name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? "application/pdf"
                : "image/png";
            return File(full, mime, name);
        }

        // ===== Delete from archive =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return RedirectToAction("Index");
            var safe = Path.GetFileNameWithoutExtension(name);
            var full = Path.Combine(RepoPath, Path.GetFileName(name));
            var txt = Path.Combine(RepoPath, safe + ".txt");
            if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
            if (System.IO.File.Exists(txt)) System.IO.File.Delete(txt);
            TempData["Msg"] = "File deleted.";
            return RedirectToAction("Index");
        }
    }
}
