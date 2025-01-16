using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RawnaqProject.Data;
using RawnaqProject.Models;
using System.Text.Json;

namespace RawnaqProject.Controllers
{
    public class itemsController : Controller
    {
        private readonly RawnaqProjectContext _context;

        public itemsController(RawnaqProjectContext context)
        {
            _context = context;
        }

        //---------------------------------------------------------

        public async Task<IActionResult> listItems()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                var item = await _context.items.OrderBy(i => i.category).ToListAsync();
                return View(item);
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        //---------------------------------------------------------

        public async Task<IActionResult> dashboard()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                string sql = "";

                var builder = WebApplication.CreateBuilder();
                string conStr = builder.Configuration.GetConnectionString("RawnaqProjectContext");
                SqlConnection conn = new SqlConnection(conStr);

                SqlCommand comm;
                conn.Open();
                sql = "SELECT COUNT( Id)  FROM items where category =1";
                comm = new SqlCommand(sql, conn);
                ViewData["d1"] = (int)comm.ExecuteScalar();

                sql = "SELECT COUNT( Id)  FROM items where category =2";
                comm = new SqlCommand(sql, conn);
                ViewData["d2"] = (int)comm.ExecuteScalar();

                sql = "SELECT COUNT( Id)  FROM items where category =3";
                comm = new SqlCommand(sql, conn);
                ViewData["d3"] = (int)comm.ExecuteScalar();
                return View();
            }
            else
                return RedirectToAction("Login", "usersaccounts");


        }

        //---------------------------------------------------------

        // GET: items
        public async Task<IActionResult> Index()
        {
            // Retrieve the user's role from session or another method
            ViewData["role"] = HttpContext.Session.GetString("Role");

            return _context.items != null ?
                          View(await _context.items.ToListAsync()) :
                          Problem("Entity set 'RawnaqProjectContext.items'  is null.");
        }

        //---------------------------------------------------------
        // GET: items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "customer")
            {
                var items = await _context.items
                    .FirstOrDefaultAsync(m => m.Id == id);

                return View(items);
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        //---------------------------------------------------------
        // GET: items/Create
        public IActionResult Create()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {

                ViewData["role"] = HttpContext.Session.GetString("Role");

                return View();
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        // POST: items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(IFormFile file, [Bind("Id,name,description,price,discount,pubdate,category,quantity")] items items)
        {
            ViewData["role"] = HttpContext.Session.GetString("Role");

            if (file != null)
            {
                string filename = file.FileName;
                //  string  ext = Path.GetExtension(file.FileName);
                string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images"));
                using (var filestream = new FileStream(Path.Combine(path, filename), FileMode.Create))
                { await file.CopyToAsync(filestream); }

                items.imgfile = filename;
            }

            _context.Add(items);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //---------------------------------------------------------
        // GET: items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                var items = await _context.items.FindAsync(id);
                return View(items);
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }
        // POST: items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile file, int id, [Bind("Id,name,description,price,discount,pubdate,category,quantity,imgfile")] items items)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            ViewData["role"] = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                if (id != items.Id)
                { return NotFound(); }

                if (file != null)
                {
                    string filename = file.FileName;
                    //  string  ext = Path.GetExtension(file.FileName);
                    string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images"));
                    using (var filestream = new FileStream(Path.Combine(path, filename), FileMode.Create))
                    { await file.CopyToAsync(filestream); }

                    items.imgfile = filename;
                }
                _context.Update(items);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
               // return View(items);
            }
            else
                return RedirectToAction("Login", "usersaccounts");
        }


        //---------------------------------------------------------
        // GET: items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {

                var items = await _context.items
                    .FirstOrDefaultAsync(m => m.Id == id);


                return View(items);
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        // POST: items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var items = await _context.items.FindAsync(id);
            _context.items.Remove(items);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool itemsExists(int id)
        {
            return (_context.items?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
