using File_Editor.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace File_Editor.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public static string SimpleName => nameof(FilesController).Replace("Controller", "");
        public static Dictionary<string, string> Users { get; } = new()
        {
            { "Wim", "123" },
            { "Jan", "456" },
            { "Ann", "789" }
        };

        public const string KEY_SESSION_ERROR = "_SessionError";
        public const string USERFOLDER = "\\userfiles\\";

        public AccountsController(IWebHostEnvironment webHostEnvironment)
        {
            // HostingEnvironment opvragen om later lokale pad van de applicatie op te vragen
            _webHostEnvironment = webHostEnvironment;
        }

        // deze action is de default route van de applicatie (zie Program.cs, onderaan)
        public IActionResult Login()
        {
            if (TempData.ContainsKey(KEY_SESSION_ERROR)) // in het geval dat FileController de gebruiker doorstuurt met foutmelding
            {
                ViewBag.Error = TempData[KEY_SESSION_ERROR];
                ViewBag.AlertType = "warning"; // voor lay-out in bootstrap
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please fill out both fields.";
            }
            else if (!username.IsValidFileOrDirName()) // zie: ExtensionMethods
            {
                ViewBag.Error = "Username invalid.";
            }
            else if (!(Users.TryGetValue(username, out string? savedPassword) && savedPassword == password)) // check of gebruiker bestaat in users Dictionary, indien ja: wachtwoord ophalen en checken of dit correct is
            {
                ViewBag.Error = "Username or password incorrect.";
            }
            else
            {
                // bestaande gebruiker, dus lokaal pad naar hun bestanden opvragen, dit opslaan in de sessie en vervolgens doorsturen naar de file manager (/File/Manager)
                var userPath = GetUserPath(_webHostEnvironment, username);
                HttpContext.Session.SetString(FilesController.KEY_SESSION_USER_PATH, userPath);
                return RedirectToAction(nameof(FilesController.Manager), FilesController.SimpleName);
            }
    
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Error = "Please fill in a username";
            }
            else if (!username.IsValidFileOrDirName()) // zie: Extensions
            {
                ViewBag.Error = "Username invalid.";
            }
            else
            {
                // map naar de bestanden van de opgegeven gebruiker opvragen
                string userPath = GetUserPath(_webHostEnvironment, username);

                // als de map al bestaat, is dit reeds een bestaande gebruiker
                if (Directory.Exists(userPath))
                {
                    ViewBag.Error = "User already exists, try again!";
                }
                else
                {
                    // map bestaat nog niet, dus we maken het aan, slaan het pad op in de sessie en sturen de gebruiker door naar de file manager (/Files/Manage)
                    Directory.CreateDirectory(userPath);
                    HttpContext.Session.SetString(FilesController.KEY_SESSION_USER_PATH, userPath);
                    return RedirectToAction(nameof(FilesController.Manager), FilesController.SimpleName);
                }
            }

            return View();
        }


        /// <summary>
        /// Geeft het lokale pad naar de bestanden van de opgegeven gebruiker
        /// </summary>
        /// <param name="hostingEnvironment"><see cref="IHostingEnvironment"/> om WebRootPath te kunnen opvragen</param>
        /// <param name="username">Gebruikersnaam (tevens de naam van de map waarin de bestanden van deze gebruiker staan)</param>
        /// <returns>Absoluut pad naar userfiles van specifieke gebruiker</returns>
        private static string GetUserPath(IWebHostEnvironment webHostEnvironment, string username)
        {
            return webHostEnvironment.WebRootPath + USERFOLDER + username;
        }
    }
}
