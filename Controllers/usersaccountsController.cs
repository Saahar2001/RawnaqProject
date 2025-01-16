using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using RawnaqProject.Data;
using RawnaqProject.Models;
using System.Text.Json;

namespace RawnaqProject.Controllers
{
    public class usersaccountsController : Controller
    {
        private readonly RawnaqProjectContext _context;

        public usersaccountsController(RawnaqProjectContext context)
        {
            _context = context;
        }


        //---------------------------------------------------------
        //Log in 

        public IActionResult Login()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Name"))
                return View();
            else
            {
                string na = HttpContext.Request.Cookies["Name"].ToString();
                string ro = HttpContext.Request.Cookies["Role"].ToString();
                HttpContext.Session.SetString("Name", na);
                HttpContext.Session.SetString("Role", ro);

                if (ro == "customer")
                {
                    return RedirectToAction(nameof(customerHome));

                }
                else
                    return RedirectToAction("adminHome", "usersaccounts");
            }
        }

        [HttpPost, ActionName("Login")]
        public async Task<IActionResult> Login(string na, string pa, string auto)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            string paa = Encoding.ASCII.GetString(md5.ComputeHash(ASCIIEncoding.Default.GetBytes(pa)));

            var ur = await _context.usersaccounts.FromSqlRaw("SELECT * FROM usersaccounts where name ='" + na + "' and  pass ='" + pa + "' ").FirstOrDefaultAsync();

            if (ur != null)
            {
                int id = ur.Id;
                string na1 = ur.name;
                string ro = ur.role;
                HttpContext.Session.SetString("userid", Convert.ToString(id));
                HttpContext.Session.SetString("Name", na1);
                HttpContext.Session.SetString("Role", ro);
                if (auto == "on")
                {
                    HttpContext.Response.Cookies.Append("Name", na1);
                    HttpContext.Response.Cookies.Append("Role", ro);
                }

                if (ro == "customer")
                    return RedirectToAction("customerHome", "usersaccounts");
                else if (ro == "admin")
                    return RedirectToAction("adminHome", "usersaccounts");
                else
                    return View();
            }
            else
            {
                ViewData["Message"] = "wrong user name and password";
                return View();
            }
        }


        //---------------------------------------------------------
        //Logout action 

        public IActionResult Logout()
        {
            // Remove cookies
            HttpContext.Response.Cookies.Delete("Name");
            HttpContext.Response.Cookies.Delete("Role");

            HttpContext.Session.Clear();  // Clear all session variables


            // Redirect to Login page
            return RedirectToAction("Login", "usersaccounts");

        }

        //---------------------------------------------------------
        // Customer Home page 

        public async Task<IActionResult> customerHome()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "customer")
            {
                var a = HttpContext.Session.GetString("Name");
                ViewData["Message"] = a;

                var discountedItems = await _context.items.Where(i => i.discount == "yes").ToListAsync();


                return View(discountedItems);
            }
            else
                return RedirectToAction("Login", "usersaccounts");
        }


        //---------------------------------------------------------
        // Admin Home page 
        public async Task<IActionResult> adminHome()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                var a = HttpContext.Session.GetString("Name");
                ViewData["Message"] = a;
                return View();
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }


        //---------------------------------------------------------

        public IActionResult email()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
                return View();
            else
                return RedirectToAction("Login", "usersaccounts");
        }


        [HttpPost, ActionName("email")]
        [ValidateAntiForgeryToken]
        public IActionResult email(string address, string subject, string body)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss != "admin")
            {
                return RedirectToAction("Login", "usersaccounts");
            }

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            var mail = new MailMessage();
            mail.From = new MailAddress("saharismail2001@gmail.com");
            mail.To.Add(address); // receiver email address
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = body;
            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential("saharismail2001@gmail.com", "gtzu zjco sutt fbmq\r\n");
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
            ViewData["Message"] = "Email sent.";
            return View();
        }




        //---------------------------------------------------------
        //search for specific user only by admin
        public async Task<IActionResult> searchall()
        {

            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                usersaccounts users = new usersaccounts();

                return View(users);
            }
            else
                return RedirectToAction("Login", "usersaccounts");
        }

        [HttpPost]
        public async Task<IActionResult> searchall(string n)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss != "admin")
            {
                return RedirectToAction("Login", "usersaccounts");
            }

            var users = await _context.usersaccounts.FromSqlRaw("select * from usersaccounts where name = '" + n + "' ").FirstOrDefaultAsync();

            return View(users);
        }




        //---------------------------------------------------------

        public IActionResult registration()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
                return RedirectToAction("Login", "usersaccounts");
            else
                return View();
        }
        [HttpPost]
        public IActionResult registration([Bind("name,email,job,gender,married, location")] customer cust, [Bind("name,pass,role")] usersaccounts acc)
        {
            var builder = WebApplication.CreateBuilder();
            string conStr = builder.Configuration.GetConnectionString("RawnaqProjectContext");
            SqlConnection conn = new SqlConnection(conStr);
            MD5 md5 = new MD5CryptoServiceProvider();
            string paa = Encoding.ASCII.GetString(md5.ComputeHash(ASCIIEncoding.Default.GetBytes(acc.pass)));

            conn.Open();
            string sql = "select * from usersaccounts  where name = '" + acc.name + "'";
            SqlCommand comm = new SqlCommand(sql, conn);
            SqlDataReader reader = comm.ExecuteReader();
            if (reader.Read())
            {
                ViewData["message"] = "name and password already exists";
                reader.Close();
            }
            else
            {
                reader.Close();
                sql = "insert into customer (name,email,job,married,gender,location)  values  ('" + cust.name + "','" + cust.email + "','" + cust.job + "','" + cust.married + "' ,'" + cust.gender + "','" + cust.location + "')";
                comm = new SqlCommand(sql, conn);
                comm.ExecuteNonQuery();

                acc.role = "customer";
                sql = "insert into usersaccounts (name,pass,role)  values  ('" + acc.name + "','" + paa + "','" + acc.role + "')";
                comm = new SqlCommand(sql, conn);


                string id = acc.Id.ToString();
                string na1 = acc.name;
                string ro = acc.role;
                HttpContext.Session.SetString("Name", na1);
                HttpContext.Session.SetString("Role", ro);
                HttpContext.Session.SetString("Id", id);


                HttpContext.Response.Cookies.Append("Name", na1);
                HttpContext.Response.Cookies.Append("Role", ro);
                HttpContext.Response.Cookies.Append("Id", id);


                comm.ExecuteNonQuery();
                conn.Close();


                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                var mail = new MailMessage();
                mail.From = new MailAddress("saharismail2001@gmail.com");
                mail.To.Add(cust.email); // receiver email address
                mail.Subject = "Welcome to our Rawnaq shop!";
                mail.IsBodyHtml = true;
                mail.Body = "You are very welcome  " + cust.name + "  to our website Rawnaq, have a nice shooping :)";
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential("saharismail2001@gmail.com", "gtzu zjco sutt fbmq\r\n");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                ViewData["message"] = "Sucessfully added";

                return RedirectToAction("customerHome", "usersaccounts");

            }
            conn.Close();
            return View();
        }


        //---------------------------------------------------------

        public IActionResult addAdmin()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
                return View();
            else
                return RedirectToAction("Login", "usersaccounts");
        }

        [HttpPost]
        public IActionResult addAdmin([Bind("name,pass,role")] usersaccounts cli)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            var builder = WebApplication.CreateBuilder();
            string conStr = builder.Configuration.GetConnectionString("RawnaqProjectContext");
            SqlConnection conn = new SqlConnection(conStr);
            MD5 md5 = new MD5CryptoServiceProvider();
            string paa = Encoding.ASCII.GetString(md5.ComputeHash(ASCIIEncoding.Default.GetBytes(cli.pass)));

            conn.Open();
            string sql = "select * from usersaccounts  where name = '" + cli.name + "'";
            SqlCommand comm = new SqlCommand(sql, conn);
            SqlDataReader reader = comm.ExecuteReader();
            if (reader.Read())
            {
                ViewData["message"] = "name already exists";
                reader.Close();
                conn.Close();
                return View();
            }
            else
            {
                reader.Close();
                cli.role = "admin";
                sql = "insert into usersaccounts (name,pass,role)  values  ('" + cli.name + "','" + paa + "','" + cli.role + "')";
                comm = new SqlCommand(sql, conn);
                comm.ExecuteNonQuery();
                ViewData["message"] = "Sucessfully added";
                conn.Close();
                return RedirectToAction("Index", "usersaccounts");
            }
        }

        //---------------------------------------------------------
        // GET: usersaccounts
        public async Task<IActionResult> Index()
        {
            /// make suru only admin can edit orders
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {

                return _context.usersaccounts != null ?
                          View(await _context.usersaccounts.ToListAsync()) :
                          Problem("Entity set 'RawnaqProjectContext.usersaccounts'  is null.");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //---------------------------------------------------------
        // GET: usersaccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                var usersaccounts = await _context.usersaccounts
                               .FirstOrDefaultAsync(m => m.Id == id);

                return View(usersaccounts);
            }
            else
                return RedirectToAction("Login", "usersaccounts");


        }

        //---------------------------------------------------------
        // GET: usersaccounts/Create
        public IActionResult Create()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
                return View();
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        // POST: usersaccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,name,pass,role")] usersaccounts usersaccounts)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usersaccounts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usersaccounts);
        }

        //---------------------------------------------------------
        // GET: usersaccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                if (id == null || _context.usersaccounts == null)
                {
                    return NotFound();
                }
                var usersaccounts = await _context.usersaccounts.FindAsync(id);
                if (usersaccounts == null)
                {
                    return NotFound();
                }
                return View(usersaccounts);
            }
            else
                return RedirectToAction("Login", "usersaccounts");
        }
        // POST: usersaccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,pass,role")] usersaccounts usersaccounts)
        {
            if (id != usersaccounts.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usersaccounts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!usersaccountsExists(usersaccounts.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usersaccounts);
        }

        //---------------------------------------------------------
        // GET: usersaccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {

                var usersaccounts = await _context.usersaccounts
                    .FirstOrDefaultAsync(m => m.Id == id);


                return View(usersaccounts);
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        // POST: usersaccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.usersaccounts == null)
            {
                return Problem("Entity set 'RawnaqProjectContext.usersaccounts'  is null.");
            }
            var usersaccounts = await _context.usersaccounts.FindAsync(id);
            if (usersaccounts != null)
            {
                _context.usersaccounts.Remove(usersaccounts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool usersaccountsExists(int id)
        {
            return (_context.usersaccounts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
