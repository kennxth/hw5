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
        [Authorize]
        public async Task<IActionResult> Index(int? orderId)
        {
            if (orderId == null)
            {
                return RedirectToAction("Index", "Orders");
            }

            var orderDetails = await _context.OrderDetails
                .Where(od => od.Order.OrderID == orderId)
                .Include(od => od.Product)
                .Include(od => od.Order)
                .ToListAsync();

            return View(orderDetails);
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
        public async Task<IActionResult> Create([Bind("Quantity,Order")] OrderDetail orderDetail, int SelectedProductID)
        {
            // Get the selected product
            Product selectedProduct = await _context.Products.FindAsync(SelectedProductID);
            if (selectedProduct == null)
            {
                ViewBag.AllProducts = GetAllProducts();
                ModelState.AddModelError("", "Please select a valid product.");
                return View(orderDetail);
            }

            // Get the associated order
            Order dbOrder = await _context.Orders.FindAsync(orderDetail.Order.OrderID);
            if (dbOrder == null)
            {
                return View("Error", new string[] { "Order not found." });
            }

            // Validate quantity
            if (orderDetail.Quantity < 1 || orderDetail.Quantity > 1000)
            {
                ViewBag.AllProducts = GetAllProducts();
                ModelState.AddModelError("Quantity", "Quantity must be between 1 and 1000.");
                orderDetail.Order = dbOrder;
                return View(orderDetail);
            }

            // Create a new order detail
            OrderDetail newOrderDetail = new OrderDetail
            {
                Order = dbOrder,
                Product = selectedProduct,
                Quantity = orderDetail.Quantity,
                ProductPrice = selectedProduct.Price,
                ExtendedPrice = orderDetail.Quantity * selectedProduct.Price
            };

            _context.OrderDetails.Add(newOrderDetail);
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

            var orderDetail = await _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.OrderDetailID == id);
                
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
        public async Task<IActionResult> Edit(int id, [Bind("OrderDetailID,Quantity,ProductPrice,ExtendedPrice,Order,Product")] OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID)
            {
                return NotFound();
            }

            // Get the original order detail from the database
            var dbOrderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .FirstOrDefaultAsync(od => od.OrderDetailID == id);

            if (dbOrderDetail == null)
            {
                return NotFound();
            }

            // Update the quantity
            dbOrderDetail.Quantity = orderDetail.Quantity;
            
            // Recalculate the product price and extended price
            dbOrderDetail.ProductPrice = dbOrderDetail.Product.Price;
            dbOrderDetail.ExtendedPrice = dbOrderDetail.Quantity * dbOrderDetail.ProductPrice;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dbOrderDetail);
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
                return RedirectToAction("Details", "Orders", new { id = dbOrderDetail.Order.OrderID });
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
            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.OrderDetailID == id);
                
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Orders", new { id = orderDetail.Order.OrderID });
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
