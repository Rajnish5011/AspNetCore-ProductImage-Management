using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.Models;
using ProductManagement.ViewModels;

namespace ProductManagement.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _db.Products
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

     
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

     
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var product = new Product
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                CreatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();


            if (vm.NewImages != null && vm.NewImages.Any())
            {
                await SaveUploadedImages(product.Id, vm.NewImages);
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int id)
        {
            var p = await _db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();

            var vm = new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ExistingImages = p.Images.Select(i => new ProductImageDto { Id = i.Id, ImageUrl = i.ImageUrl }).ToList()
            };

            return View(vm);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            var product = await _db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.ExistingImages = product.Images.Select(i => new ProductImageDto { Id = i.Id, ImageUrl = i.ImageUrl }).ToList();
                return View(vm);
            }

            product.Name = vm.Name;
            product.Description = vm.Description;
            product.Price = vm.Price;

            _db.Update(product);
            await _db.SaveChangesAsync();


            if (vm.NewImages != null && vm.NewImages.Any())
            {
                await SaveUploadedImages(product.Id, vm.NewImages);
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return View(p);
        }


        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var p = await _db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();

       
            foreach (var img in p.Images)
            {
                DeletePhysicalFile(img.ImageUrl);
            }

            _db.Products.Remove(p);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var img = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == id);
            if (img == null) return NotFound();

            DeletePhysicalFile(img.ImageUrl);

            _db.ProductImages.Remove(img);
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }

        private async Task SaveUploadedImages(int productId, Microsoft.AspNetCore.Http.IFormFile[] files)
        {
            if (files == null || files.Length == 0) return;

            var uploadRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "products");
            if (!Directory.Exists(uploadRoot)) Directory.CreateDirectory(uploadRoot);

            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var physicalPath = Path.Combine(uploadRoot, fileName);

                using (var fs = new FileStream(physicalPath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                var relativeUrl = $"/images/products/{fileName}";

                var img = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = relativeUrl
                };

                _db.ProductImages.Add(img);
            }

            await _db.SaveChangesAsync();
        }

        private void DeletePhysicalFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            var webRoot = _env.WebRootPath ?? "wwwroot";
            var p = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var full = Path.Combine(webRoot, p);
            if (System.IO.File.Exists(full))
            {
                try { System.IO.File.Delete(full); } catch { }
            }
        }
    }
}
