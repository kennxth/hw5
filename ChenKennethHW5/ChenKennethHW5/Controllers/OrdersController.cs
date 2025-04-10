using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChenKennethHW5.DAL;
using ChenKennethHW5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ChenKennethHW5.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private SelectList GetAllProducts()
        {
            List<Product> productList = _context.Products.OrderBy(p => p.Name).ToList();
            return new SelectList(productList, "ProductID", "Name");
        }



        public OrdersController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: Orders
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Orders.ToListAsync());
            }
            else // must be Customer
            {
                var userEmail = User.Identity.Name;
                var userOrders = await _context.Orders
                    .Where(o => o.Email == userEmail)
                    .ToListAsync();

                return View(userOrders);
            }
        }


        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)  
                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }

            // Role check to prevent customers viewing others' orders
            if (User.IsInRole("Customer") && order.Email != User.Identity.Name)
            {
                return View("Error", new string[] { "You are not authorized to view this order." });
            }

            return View(order);
        }



        // GET: Orders/Create
        [Authorize(Roles = "Customer")]
        public IActionResult Create(int orderId)
        {
            // a. Create a new instance of OrderDetail
            OrderDetail od = new OrderDetail();

            // b. Find the associated order
            Order dbOrder = _context.Orders.Find(orderId);

            if (dbOrder == null)
            {
                return View("Error", new string[] { "Order not found." });
            }

            // c. Set the order property
            od.Order = dbOrder;

            // d. Populate the ViewBag with the product list
            ViewBag.AllProducts = GetAllProducts();

            // e. Return the view with the order detail
            return View(od);
        }


        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        /*public async Task<IActionResult> Create([Bind("OrderID,OrderNumber,OrderDate,OrderNotes")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }*/

        public async Task<IActionResult> Create([Bind("OrderNotes")] Order order)
        {
            // 1. Generate the next order number
            order.OrderID = Order.nextOrderNumber++;

            // 2. Set the current date
            order.OrderDate = DateTime.Now;

            // 3. Get the current logged-in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            order.Email = user.Email;

            // 4. Save to the database
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();

                // 5. Redirect to OrderDetails Create (to add products)
                return RedirectToAction("Create", "OrderDetails", new { orderId = order.OrderID });
            }

            return View(order);
        }


        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,OrderNumber,OrderDate,OrderNotes")] Order order)
        {
            if (id != order.OrderID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderID))
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
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}
