using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService? _productService;

        public ProductController(IProductService? productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

#pragma warning disable CS1998 // O método assíncrono não possui operadores 'await' e será executado de forma síncrona
        public async Task<IActionResult> ProductIndex()
#pragma warning restore CS1998 // O método assíncrono não possui operadores 'await' e será executado de forma síncrona
        {
            var products = _productService!.FindAllProducts();
            return View(products);
        }
    }
}
