using Microsoft.AspNetCore.Mvc;

namespace File_Editor.Controllers
{
    public class FilesController : Controller
    {
        public const string KEY_SESSION_USER_PATH = "_UserPath";

        public IActionResult Index()
        {
            return View();
        }
    }
}
