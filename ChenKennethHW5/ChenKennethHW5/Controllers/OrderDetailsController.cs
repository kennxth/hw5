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

namespace ChenKennethHW5.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly AppDbContext _context;

        public OrderDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index()
        {
            return View(await _context.OrderDetails.ToListAsync());
        }

        // GET: OrderDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(m => m.OrderDetailID == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // GET: OrderDetails/Create
        [Authorize]
        public IActionResult Create(int orderId)
        {
            OrderDetail od = new OrderDetail();

            // Find the order using the ID passed in
            Order dbOrder = _context.Orders.Find(orderId);
            if (dbOrder == null)
            {
                return View("Error", new string[] { "Order not found." });
            }

            od.Order = dbOrder;

            ViewBag.AllProducts = GetAllProducts();
            return View(od);
        }


        // POST: OrderDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(OrderDetail orderDetail, int SelectedProductID)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllProducts = GetAllProducts();
                return View(orderDetail);
            }

            // Get the selected product
            Product selectedProduct = await _context.Products.FindAsync(SelectedProductID);
            if (selectedProduct == null)
            {
                return View("Error", new string[] { "Product not found." });
            }

            // Get the associated order
            Order dbOrder = await _context.Orders.FindAsync(orderDetail.Order.OrderID);
            if (dbOrder == null)
            {
                return View("Error", new string[] { "Order not found." });
            }

            // Assign properties
            orderDetail.Product = selectedProduct;
            orderDetail.ProductPrice = selectedProduct.Price;
            orderDetail.ExtendedPrice = orderDetail.Quantity * selectedProduct.Price;
            orderDetail.Order = dbOrder;

            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = dbOrder.OrderID });
        }


        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderDetailID,Quantity,ProductPrice,ExtendedPrice")] OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailExists(orderDetail.OrderDetailID))
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
            return View(orderDetail);
        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(m => m.OrderDetailID == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailID == id);
        }
        private SelectList GetAllProducts()
        {
            List<Product> productList = _context.Products.OrderBy(p => p.Name).ToList();
            return new SelectList(productList, "ProductID", "Name");
        }

    }
}
