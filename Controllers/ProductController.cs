using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Data;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductRepository repository = new ProductRepository();

        // GET: /Product or /
        public IActionResult Index()
        {
            var products = repository.GetAll();
            return View(products);
        }

        // GET: /Product/Create
        public IActionResult Create()
        {
            return View("Form", new Product());
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Name,Category,Quantity,Price")] Product product)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", product);
            }

            repository.Add(product);
            TempData["Success"] = "Product added successfully.";
            return RedirectToAction("Index");
        }

        // GET: /Product/Edit/5
        public IActionResult Edit(int id)
        {
            var product = repository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View("Form", product);
        }

        // POST: /Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Name,Category,Quantity,Price")] Product product)
        {
            product.Id = id;
            if (!ModelState.IsValid)
            {
                return View("Form", product);
            }

            if (!repository.Update(product))
            {
                return NotFound();
            }

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: /Product/Delete/5
        public IActionResult Delete(int id)
        {
            var product = repository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: /Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!repository.Delete(id))
            {
                return NotFound();
            }

            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
