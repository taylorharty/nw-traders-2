using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;


public class ProductController : Controller
{
  // this controller depends on the NorthwindRepository
  private DataContext _dataContext;
  public ProductController(DataContext db) => _dataContext = db;
  public IActionResult Category() => View(_dataContext.Categories.OrderBy(c => c.CategoryName));
  public IActionResult Index(int id)
  {
    ViewBag.id = id;
    return View(_dataContext.Categories.OrderBy(c => c.CategoryName));
  }
  [Authorize(Roles = "northwind-employee")]
  public ActionResult Discounts() => View(_dataContext.Discounts.Include("Product"));
  [Authorize(Roles = "northwind-employee")]
  public async Task<IActionResult> DeleteDiscount(int id)
  {
    var selectedDiscount = _dataContext.Discounts.FirstOrDefault(d => d.DiscountId == id);
    _dataContext.RemoveDiscount(selectedDiscount);
    return RedirectToAction("Discounts", "Product");
  }
  
  [Authorize(Roles = "northwind-employee")]
  public async Task<IActionResult> AddDiscount()
  {
    ViewBag.Products = _dataContext.Products;
    return View();
  }
  
  [Authorize(Roles = "northwind-employee")]
  [HttpPost]
  public async Task<IActionResult> AddDiscount(Discount discount)
  {
    Random random = new Random();
    int fourDigitNumber = random.Next(1000, 10000);
    Discount NewDiscount = new Discount
    {
      Title = discount.Title,
      Description = discount.Description,
      DiscountPercent = discount.DiscountPercent / 100,
      ProductId = Convert.ToInt32(discount.ProductId),
      StartTime = discount.StartTime,
      EndTime = discount.EndTime,
      Code = fourDigitNumber,
    };
    _dataContext.AddDiscount(NewDiscount);
    return RedirectToAction("Discounts", "Product");
  }

  [Authorize(Roles = "northwind-employee")]
  public IActionResult EditDiscount(int id)
  {
    var discount = _dataContext.Discounts.Include("Product").FirstOrDefault(d => d.DiscountId == id);
    if (discount == null) return NotFound();
    ViewBag.Products = _dataContext.Products;
    return View(discount);
  }

  [Authorize(Roles = "northwind-employee")]
  [HttpPost]
  public IActionResult EditDiscount(Discount discount)
  {
    var discountToUpdate = _dataContext.Discounts.FirstOrDefault(d => d.DiscountId == discount.DiscountId);
    if (discountToUpdate == null) return NotFound();

    discountToUpdate.Title = discount.Title;
    discountToUpdate.Description = discount.Description;
    // incoming DiscountPercent in the form is a percentage (e.g. 25 for 25%) — convert to decimal like AddDiscount
    discountToUpdate.DiscountPercent = discount.DiscountPercent / 100;
    discountToUpdate.StartTime = discount.StartTime;
    discountToUpdate.EndTime = discount.EndTime;

    _dataContext.SaveChanges();

    return RedirectToAction("Discounts", "Product");
  }
}

