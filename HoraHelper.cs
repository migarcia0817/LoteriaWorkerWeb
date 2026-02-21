using System;
using System.Collections.Generic;

namespace LoteriaWorkerWeb.Helpers
{
    public static class HoraHelper
    {
        public static readonly Dictionary<string, string> AnguillaHoras =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "8:00 AM", "08:00 AM" },
    { "9:00 AM", "09:00 AM" },
    { "10:00 AM", "10:00 AM" },
    { "11:00 AM", "11:00 AM" },
    { "12:00 PM", "12:00 PM" },
    { "1:00 PM", "1:00 PM" },
    { "2:00 PM", "2:00 PM" },
    { "3:00 PM", "3:00 PM" },
    { "4:00 PM", "4:00 PM" },
    { "5:00 PM", "5:00 PM" },
    { "6:00 PM", "6:00 PM" },
    { "7:00 PM", "7:00 PM" },
    { "8:00 PM", "8:00 PM" },
    { "9:00 PM", "9:00 PM" }
};


        public static string Normalizar(string hora)
        {
            if (string.IsNullOrWhiteSpace(hora)) return hora;

            string horaNormalizada = hora.Trim();

            // Reemplazar espacios invisibles y tabs
            horaNormalizada = horaNormalizada
                .Replace("\u00A0", " ") // non-breaking space
                .Replace("\u2007", " ") // figure space
                .Replace("\u202F", " ") // narrow no-break space
                .Replace("\t", " ")
                .Trim();

            // Insertar espacio antes de AM/PM si falta
            if (horaNormalizada.EndsWith("AM") && !horaNormalizada.EndsWith(" AM"))
                horaNormalizada = horaNormalizada.Substring(0, horaNormalizada.Length - 2) + " AM";
            else if (horaNormalizada.EndsWith("PM") && !horaNormalizada.EndsWith(" PM"))
                horaNormalizada = horaNormalizada.Substring(0, horaNormalizada.Length - 2) + " PM";

            // Casos especiales de Anguilla
            if (horaNormalizada.Equals("8 AM", StringComparison.OrdinalIgnoreCase))
                horaNormalizada = "8:00 AM";
            else if (horaNormalizada.Equals("9 AM", StringComparison.OrdinalIgnoreCase))
                horaNormalizada = "9:00 AM";

            Console.WriteLine($"[DEBUG] Normalizada limpia: '{horaNormalizada}' (Length={horaNormalizada.Length})");

            // Si es una hora de Anguilla, devolver la versión normalizada
            if (AnguillaHoras.TryGetValue(horaNormalizada, out var horaFinal))
                return horaFinal;

            return horaNormalizada;
        }





    }
}
