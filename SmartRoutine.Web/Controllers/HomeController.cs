using Microsoft.AspNetCore.Mvc;
using SmartRoutine.Web.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartRoutine.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/api";
        base.OnActionExecuting(context);
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        // API çağrısı client-side JS ile yapılacak
        return View(model);
    }

    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        // API çağrısı client-side JS ile yapılacak
        return View(model);
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult Routines()
    {
        return View();
    }

    public IActionResult Stats()
    {
        return View();
    }

    [HttpGet]
    public IActionResult UpdateProfile()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        // API çağrısı ile profil güncelle
        // ... API entegrasyonu burada olacak ...
        await Task.CompletedTask;
        ViewBag.Success = true;
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
} 