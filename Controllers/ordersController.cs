using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RawnaqProject.Data;
using RawnaqProject.Models;
using System.Text.Json;

namespace RawnaqProject.Controllers
{
    public class ordersController : Controller
    {
        private readonly RawnaqProjectContext _context;

        public ordersController(RawnaqProjectContext context)
        {
            _context = context;
        }

        //---------------------------------------------------------
        public async Task<IActionResult> CatalogueBuy()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "customer")
                return View(await _context.items.ToListAsync());
            else
                return RedirectToAction("Login", "usersaccounts");

        }


        //---------------------------------------------------------

        public async Task<IActionResult> ItemBuyDetail(int? id)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "customer")
            {
                var item = await _context.items.FindAsync(id);
                return View(item);
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        //---------------------------------------------------------

        List<buyitems> Bbks = new List<buyitems>();

        [HttpPost]
        public async Task<IActionResult> cartadd(int Id, int quantity, decimal price)
        {
            await HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "customer")
            {
                var sessionString = HttpContext.Session.GetString("Cart");
                if (sessionString is not null)
                {
                    Bbks = JsonSerializer.Deserialize<List<buyitems>>(sessionString);
                }

                var item = await _context.items.FromSqlRaw("select * from items  where Id= '" + Id + "'  ").FirstOrDefaultAsync();

                if (item == null)
                {
                    ViewData["message"] = "Item not found";
                    return View("ItemBuyDetail", item);

                }
                if (quantity > item.quantity)
                {
                    ViewData["message"] = "maxiumam order quantity should be " + item.quantity;
                    return View("ItemBuyDetail", item);
                }
                Bbks.Add(new buyitems
                {
                    Title = item.name,
                    price = price,
                    quant = quantity
                });

                item.quantity -= quantity;
                _context.Update(item);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(Bbks));
                return RedirectToAction("CartBuy");
            }
            else
            {
                return RedirectToAction("Login", "usersaccounts");

            }
        }



        //---------------------------------------------------------
        public async Task<IActionResult> CartBuy()
        {
            await HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "customer")
            {
                var sessionString = HttpContext.Session.GetString("Cart");
                if (sessionString is not null)
                {
                    Bbks = JsonSerializer.Deserialize<List<buyitems>>(sessionString);
                }
                return View(Bbks);
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }


        //---------------------------------------------------------

        public async Task<IActionResult> Buy()
        {
            /// make suru only customer can make orders
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "customer")
            {

                await HttpContext.Session.LoadAsync();
                var sessionString = HttpContext.Session.GetString("Cart");
                if (sessionString is not null)
                {
                    Bbks = JsonSerializer.Deserialize<List<buyitems>>(sessionString);
                }

                string ctname = HttpContext.Session.GetString("Name");
                orders order = new orders();
                order.total = 0;
                order.custname = ctname;
                order.orderdate = DateTime.Today;
                _context.orders.Add(order);
                await _context.SaveChangesAsync();
                var bord = await _context.orders.FromSqlRaw("select * from orders  where custname= '" + ctname + "' ").OrderByDescending(e => e.Id).FirstOrDefaultAsync();
                int ordid = bord.Id;
                decimal tot = 0;
                foreach (var bk in Bbks.ToList())
                {
                    orderline oline = new orderline();
                    oline.orderid = ordid;
                    oline.itemname = bk.Title;
                    oline.itemquant = bk.quant;
                    oline.itemprice = bk.price;
                    _context.orderline.Add(oline);
                    await _context.SaveChangesAsync();


                    var bkk = await _context.items.FromSqlRaw("select * from items  where name= '" + bk.Title + "' ").FirstOrDefaultAsync();
                    bkk.quantity = bkk.quantity - bk.quant;
                    _context.Update(bkk);
                    await _context.SaveChangesAsync();

                    tot = tot + (bk.quant * bk.price);
                }
                bord.total = Convert.ToInt16(tot);
                _context.Update(bord);
                await _context.SaveChangesAsync();

                ViewData["Message"] = "Thank you See you again";
                Bbks = new List<buyitems>();
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(Bbks));
                return RedirectToAction("MyOrder");
            }
            else
                return RedirectToAction("Login", "usersaccounts");
        }

        //---------------------------------------------------------
        public async Task<IActionResult> MyOrder()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "customer")
            {
                string ctname = HttpContext.Session.GetString("Name");
                return View(await _context.orders.FromSqlRaw("select * from orders  where custname = '" + ctname + "' ").ToListAsync());
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        //---------------------------------------------------------


        public async Task<IActionResult> Orderline(int? orid)
        {
            ViewData["role"] = HttpContext.Session.GetString("Role");

            var buybk = await _context.orderline.FromSqlRaw("select * from orderline  where orderid = '" + orid + "' ").ToListAsync();
            return View(buybk);
        }

        //---------------------------------------------------------

        public async Task<IActionResult> purchaseReport()
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                var orItems = await _context.orders.FromSqlRaw("select max(Id) as Id,max(orderdate) as orderdate," +
                    " custname, sum(total) as total from orders group by custname").ToListAsync();
                return View(orItems);
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        //---------------------------------------------------------

        public async Task<IActionResult> ordersdetail(string? custname)
        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                var orItems = await _context.orders.FromSqlRaw("select * from orders where custname = '" + custname + "' order by orderdate").ToListAsync();
                return View(orItems);
            }
            else
                return RedirectToAction("Login", "usersaccounts");

        }

        //---------------------------------------------------------

        // GET: orders
        public async Task<IActionResult> Index()
        {
            return _context.orders != null ?
                        View(await _context.orders.ToListAsync()) :
                        Problem("Entity set 'RawnaqProjectContext.orders'  is null.");
        }

        //---------------------------------------------------------
        // GET: orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.orders == null)
            {
                return NotFound();
            }

            var orders = await _context.orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        //---------------------------------------------------------
        // GET: orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,custname,orderdate,total")] orders orders)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orders);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        //---------------------------------------------------------
        // GET: orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.orders == null)
            {
                return NotFound();
            }

            var orders = await _context.orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            return View(orders);
        }

        // POST: orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,custname,orderdate,total")] orders orders)
        {
            if (id != orders.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ordersExists(orders.Id))
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
            return View(orders);
        }

        //---------------------------------------------------------
        // GET: orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.orders == null)
            {
                return NotFound();
            }

            var orders = await _context.orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.orders == null)
            {
                return Problem("Entity set 'RawnaqProjectContext.orders'  is null.");
            }
            var orders = await _context.orders.FindAsync(id);
            if (orders != null)
            {
                _context.orders.Remove(orders);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ordersExists(int id)
        {
            return (_context.orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
