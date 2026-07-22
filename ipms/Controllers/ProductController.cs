using System.Security.Claims;
using IPMS.BAL.IService;
using IPMS.DTO;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }


    // Any signed-in user can browse the catalogue.
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProductsDto>>> GetProducts()
    {
        ProductsDto result = await _service.GetProductsAsync();
        return Ok(ApiResponse.Ok(result, "Products retrieved."));
    }


    [HttpGet("{product_id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductById(Guid product_id)
    {
        ProductDto result = await _service.GetProductByIdAsync(product_id);
        return Ok(ApiResponse.Ok(result, "Product retrieved."));
    }


    // Insurance agents own the product catalogue.
    [HttpPost]
    [Authorize(Roles = Roles.InsuranceAgent)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(CreateProductDto payload)
    {
        ProductDto result = await _service.CreateProductAsync(GetUserId(), payload);
        return Ok(ApiResponse.Ok(result, "Product created."));
    }


    [HttpPatch("{product_id}")]
    [Authorize(Roles = Roles.InsuranceAgent)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProductById(
        Guid product_id,
        UpdateProductDto payload)
    {
        ProductDto result = await _service.UpdateProductAsync(product_id, payload);
        return Ok(ApiResponse.Ok(result, "Product updated."));
    }


    [HttpDelete("{product_id}")]
    [Authorize(Roles = Roles.InsuranceAgent)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteProductById(Guid product_id)
    {
        await _service.DeleteProductAsync(product_id);
        return Ok(ApiResponse.Ok(string.Empty, "Product deleted."));
    }


    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
