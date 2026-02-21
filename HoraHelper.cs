using System;
using System.Collections.Generic;

namespace LoteriaWorkerWeb.Helpers
{
    public static class HoraHelper
    {
        public static readonly Dictionary<string, string> AnguillaHoras = new Dictionary<string, string>
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

            // Asegurar espacio antes de AM/PM
            string horaNormalizada = hora.Replace("AM", " AM").Replace("PM", " PM").Trim();

            // Si es una hora de Anguilla, devolver la versión normalizada
            if (AnguillaHoras.TryGetValue(horaNormalizada, out var horaFinal))
                return horaFinal;

            // Si no es Anguilla, devolver tal cual
            return horaNormalizada;
        }
    }
}
