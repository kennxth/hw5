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
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Suppliers) 
                .ToListAsync();
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Suppliers)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.AllSuppliers = GetAllSuppliers(); 
            ViewBag.ProductTypes = new SelectList(Enum.GetNames(typeof(ProductType)));
            return View();
        }

        /*  // POST: Products/Create
         // To protect from overposting attacks, enable the specific properties you want to bind to.
         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
         [HttpPost]
         [ValidateAntiForgeryToken]
         [Authorize(Roles = "Admin")]
         public async Task<IActionResult> Create([Bind("ProductID,Name,Description,Price,ProductType")] Product product)
         {
             if (ModelState.IsValid)
             {
                 _context.Add(product);
                 await _context.SaveChangesAsync();
                 return RedirectToAction(nameof(Index));
             }
             return View(product);
         }

         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ProductID,Name,Description,Price,ProductType")] Product product, int[] SelectedSupplierIDs)
        {
            if (ModelState.IsValid)
            {
                // Add the product to get its ProductID
                _context.Add(product);
                await _context.SaveChangesAsync();

                // Assign selected suppliers
                foreach (int supplierID in SelectedSupplierIDs)
                {
                    Supplier supplier = await _context.Suppliers.FindAsync(supplierID);
                    if (supplier != null)
                    {
                        product.Suppliers ??= new List<Supplier>();
                        product.Suppliers.Add(supplier);
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // repopulate dropdowns in case of error
            ViewBag.AllSuppliers = GetAllSuppliers();
            ViewBag.ProductTypes = new SelectList(Enum.GetNames(typeof(ProductType)));

            return View(product);
        }


        // GET: Products/Edit/5
        /* [Authorize(Roles = "Admin")]
         public async Task<IActionResult> Edit(int? id)
         {
             if (id == null)
             {
                 return NotFound();
             }

             var product = await _context.Products.FindAsync(id);
             if (product == null)
             {
                 return NotFound();
             }
             return View(product);
         }
         */
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                                        .Include(p => p.Suppliers)
                                        .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            // Get IDs of suppliers currently linked to this product
            int[] selectedSupplierIDs = product.Suppliers.Select(s => s.SupplierID).ToArray();

            ViewBag.AllSuppliers = new MultiSelectList(_context.Suppliers.OrderBy(s => s.SupplierName), "SupplierID", "SupplierName", selectedSupplierIDs);
            ViewBag.ProductTypes = new SelectList(Enum.GetNames(typeof(ProductType)));

            return View(product);
        }


        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Name,Description,Price,ProductType")] Product product)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
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
            return View(product);
        }
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Name,Description,Price,ProductType")] Product product, int[] SelectedSupplierIDs)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing product from DB including suppliers
                    var productToUpdate = await _context.Products
                                                        .Include(p => p.Suppliers)
                                                        .FirstOrDefaultAsync(p => p.ProductID == id);

                    if (productToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Update scalar properties
                    productToUpdate.Name = product.Name;
                    productToUpdate.Description = product.Description;
                    productToUpdate.Price = product.Price;
                    productToUpdate.ProductType = product.ProductType;

                    // Clear existing suppliers
                    productToUpdate.Suppliers.Clear();

                    // Add updated suppliers
                    foreach (int supplierID in SelectedSupplierIDs)
                    {
                        var supplier = await _context.Suppliers.FindAsync(supplierID);
                        if (supplier != null)
                        {
                            productToUpdate.Suppliers.Add(supplier);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
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

            // repopulate dropdowns on error
            ViewBag.AllSuppliers = new MultiSelectList(_context.Suppliers.OrderBy(s => s.SupplierName), "SupplierID", "SupplierName", SelectedSupplierIDs);
            ViewBag.ProductTypes = new SelectList(Enum.GetNames(typeof(ProductType)));

            return View(product);
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }

        private MultiSelectList GetAllSuppliers()
        {
            List<Supplier> supplierList = _context.Suppliers.OrderBy(s => s.SupplierName).ToList();
            return new MultiSelectList(supplierList, "SupplierID", "SupplierName");
        }


    }
}
