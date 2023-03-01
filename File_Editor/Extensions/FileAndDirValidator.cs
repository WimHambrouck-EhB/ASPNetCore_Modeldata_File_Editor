namespace File_Editor.Extensions
{
    public static class FileAndDirValidator
    {
        /// <summary>
        /// Controleert of de invoer niet leeg is, niet te lang is, geen ongeldige tekens bevat en geen gereserveerde systeemnaam is.
        /// Te gebruiken voor controle van geldigheid gebruikersnamen.
        /// </summary>
        /// <param name="invoer">Te controleren invoer.</param>
        /// <returns>true als aan de voorwaarden voldaan is, anders false</returns>
        public static bool IsValidFileOrDirName(this string invoer)
        {
            if (string.IsNullOrEmpty(invoer))
                return false;

            // idealiter doen we een call naar de windows-api om dit te achterhalen (want verschillend per versie van Windows), maar om de code wat in te korten een hardcoded lengte
            // meer info: https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation
            if (invoer.Length >= 255)
                return false;

            var reservedChars = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars());

            if (invoer.Any(c => reservedChars.Contains(c)))
                return false;

            // https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions
            var reservedNames = new string[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

            if (reservedNames.Any(invoer.ToUpperInvariant().Equals))
                return false;

            return true;
        }
    }

}
