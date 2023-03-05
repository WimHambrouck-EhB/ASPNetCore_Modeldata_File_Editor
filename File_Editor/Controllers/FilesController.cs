using File_Editor.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace File_Editor.Controllers
{
    public class FilesController : Controller
    {
        public const string KEY_SESSION_USER_PATH = "_UserPath";
        private const string KEY_ERROR_SESSION = "_SessionError";
        private const string KEY_ERROR_FILE = "_FileError";
        private const string KEY_MESSAGE = "Message";

        public IActionResult Manager()
        {
            // check of gebruiker nog bestaat in de sessie, indien niet: terugsturen naar login
            if (IsSessionUserPathExpired(out string? userPath))
            {
                return RedirectToAction(nameof(AccountsController.Login), nameof(AccountsController).Replace("Controller", ""));
            }

            var files = new List<string>();

            // alle bestandsnamen uit de gebruikersmap ophalen om te kunnen weergeven in de view
            foreach (var file in Directory.GetFiles(userPath!))
            {
                FileInfo fileInfo = new(file);
                files.Add(fileInfo.Name);
            }

            // gebruikersnaam uit sessie halen
            ViewBag.Username = userPath![(userPath!.LastIndexOf("\\") + 1)..];
            ViewBag.Files = files;
            ViewBag.Error = TempData[KEY_ERROR_FILE]; // voor het geval dat een andere actie doorstuurt met een foutmelding (vb: als er iets fout gaat in /Create, stuurt die de gebruiker terug naar /Manager)

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string filename)
        {
            if (!filename.IsValidFileOrDirName()) // zie: ExtensionMethods
            {
                // als bestandsnaam niet correct is, terugsturen naar /Manager waar dan een foutmelding zal verschijnen
                TempData[KEY_ERROR_FILE] = "Invalid file name!";
                return RedirectToAction(nameof(Manager));
            }
            else
            {
                // check of gebruiker nog bestaat in de sessie
                if (IsSessionUserPathExpired(out string? userPath))
                {
                    return RedirectToAction(nameof(AccountsController.Login), nameof(AccountsController).Replace("Controller", ""));
                }

                var filePath = $"{userPath}\\{filename}.txt";

                if (System.IO.File.Exists(filePath))
                {
                    // bestand bestaat al, foutmelding in tempdata en doorsturen naar /Manager
                    TempData[KEY_ERROR_FILE] = "This file already exists.";
                    return RedirectToAction(nameof(Manager));
                }

                // nieuw bestand aanmaken en File handle meteen daarna sluiten (anders IOException als we het later proberen uitlezen/aanpassen)
                System.IO.File.Create(filePath).Close();
                // doorsturen naar /Edit en bestandsnaam van net aangemaakte bestand meesturen
                return RedirectToAction(nameof(Edit), new { filename = $"{filename}.txt" });
            }
        }

        public IActionResult Edit(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename) || filename.Contains("/") || filename.Contains("\\")) // check op (back)slash voor het geval iemand probeert in het bestandssysteem te navigeren (vb: Edit?filename=../Ann/test.txt)
            {
                return RedirectToAction(nameof(Manager));
            }

            // check of gebruiker nog bestaat in de sessie
            if (IsSessionUserPathExpired(out string? userPath))
            {
                return RedirectToAction(nameof(AccountsController.Login), nameof(AccountsController).Replace("Controller", ""));
            }

            var filePath = $"{userPath}\\{filename}";

            // check of bestand bestaat
            if (!System.IO.File.Exists(filePath))
                return RedirectToAction(nameof(Manager));

            ViewBag.Filename = filename;
            ViewBag.Inhoud = System.IO.File.ReadAllText(filePath);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(string filename, string filecontent)
        {
            return SaveFile(filename, filecontent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveAndQuit(string filename, string filecontent)
        {
            return SaveFile(filename, filecontent, true);
        }

        private IActionResult SaveFile(string filename, string filecontent, bool quit = false)
        {
            if (IsSessionUserPathExpired(out string? userPath))
            {
                return RedirectToAction(nameof(AccountsController.Login), nameof(AccountsController).Replace("Controller", ""));
            }

            var filePath = $"{userPath}\\{filename}";

            System.IO.File.WriteAllText(filePath, filecontent);

            if (quit)
            {
                return RedirectToAction(nameof(Manager));
            }

            TempData[KEY_MESSAGE] = "Saved!";
            return RedirectToAction(nameof(Edit), new { filename });
        }

        public IActionResult Delete(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename) || filename.Contains('/') || filename.Contains('\\')) // check op (back)slash voor het geval iemand probeert via de url in het bestandssysteem te navigeren (vb: ../Ann/test.txt)
            {
                return RedirectToAction(nameof(Manager));
            }

            if (IsSessionUserPathExpired(out string? userPath))
            {
                return RedirectToAction(nameof(AccountsController.Login), nameof(AccountsController).Replace("Controller", ""));
            }

            var filePath = $"{userPath}\\{filename}";

            if (!System.IO.File.Exists(filePath))
                return RedirectToAction(nameof(Manager));

            ViewBag.Filename = filename;

            return View();
        }

        /// <summary>
        /// User krijgt eerste GET /Delete/{filename} te zien met een bevestiging, als daar een POST gebeurt, komen we hier terecht.
        /// ActionName doorbeekt de conventie ASP.NET dat naam van de methode == naam van de actie. Dit omdat we anders 2 methodes Delete hebben met dezelfde signatuur, waarmee de compiler niet blij zal zijn.
        /// We willen echter dat zowel de GET- als de POST-actie om een bestand te verwijderen allebei "Delete" heten.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string filename)
        {
            if (IsSessionUserPathExpired(out string userPath))
            {
                return RedirectToAction(nameof(AccountsController.Login), nameof(AccountsController).Replace("Controller", ""));
            }

            var filePath = $"{userPath}\\{filename}";
            System.IO.File.Delete(filePath);

            return RedirectToAction(nameof(Manager));
        }

        /// <summary>
        /// Controleert of de sessie nog bestaat a.d.h.v. <see cref="SessionKeyUserPath"/> en geeft user path terug.
        /// </summary>
        /// <param name="sessionUserPath">Pad naar userfiles wordt hier ingevuld.</param>
        /// <returns>True als sessie verlopen is, false indien niet.</returns>
        private bool IsSessionUserPathExpired(out string? sessionUserPath)
        {
            sessionUserPath = HttpContext.Session.GetString(KEY_SESSION_USER_PATH);

            if (string.IsNullOrEmpty(sessionUserPath))
            {
                TempData[KEY_ERROR_SESSION] = "Session expired. Try logging in again.";
                return true;
            }

            return false;
        }

    }
}
