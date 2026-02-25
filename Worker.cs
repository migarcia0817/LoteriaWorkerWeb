using Firebase.Database;
using Firebase.Database.Query;
using HtmlAgilityPack;
using LoteriaWorkerWeb.Helpers;

namespace LoteriaWorkerWeb
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly FirebaseClient firebaseClient;

        // Diccionario de mapeo entre nombre y clave
        private static readonly Dictionary<string, string> LoteriaClaves = new Dictionary<string, string>
    {
        { "Anguilla 08:00 AM", "Anguilla_800_AM" },
        { "Anguilla 09:00 AM", "Anguilla_900_AM" },
        { "Anguilla 10:00 AM", "Anguilla_1000_AM" },
        { "Anguilla 11:00 AM", "Anguilla_1100_AM" },
        { "Anguilla 12:00 PM", "Anguilla_1200_PM" },
        { "Anguilla 1:00 PM", "Anguilla_100_PM" },
        { "Anguilla 2:00 PM", "Anguilla_200_PM" },
        { "Anguilla 3:00 PM", "Anguilla_300_PM" },
        { "Anguilla 4:00 PM", "Anguilla_400_PM" },
        { "Anguilla 5:00 PM", "Anguilla_500_PM" },
        { "Anguilla 6:00 PM", "Anguilla_600_PM" },
        { "Anguilla 7:00 PM", "Anguilla_700_PM" },
        { "Anguilla 8:00 PM", "Anguilla_800_PM" },
        { "Anguilla 9:00 PM", "Anguilla_900_PM" },
        { "NY.Tarde 3:30 PM", "NYTarde_330_PM"},
        { "NY.Noche 11:30 PM", "NYNoche_1130_PM"},
        { "King Lotery 12:30 PM", "KingLot_1230_PM"},
        { "King Lotery 7:30 PM", "KingLot_730_PM"},
        { "Suerte 12:30 PM", "Suerte_1230_PM"},
        { "Suerte 6:00 PM", "Suerte_600_PM"},
        { "Primera 12 PM", "Primera_1200_PM"},
        { "Primera 7 PM", "Primera_700_PM"},
        { "Q.Real Tarde 12:55 PM", "QRealTarde_1255_PM" },
        { "FL.Tarde 2:30 PM", "FLTarde_230_PM" },
        { "FL.Noche 10:45 PM", "FLNoche_1045_PM" },
        { "Loteka 7:55 PM", "Loteka_755_PM"},
        { "Leisa 8:55 PM", "Leisa_855_PM"},
        { "Nac.Tarde 2:55 PM", "NacTarde_255_PM"},
        { "Nac.Noche 9:00 PM", "NacNoche_900_PM"}
    };

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            firebaseClient = new FirebaseClient("https://bancachupon-default-rtdb.firebaseio.com/");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Heartbeat log para confirmar que el Worker sigue activo
                _logger.LogInformation($"⏱️ Worker activo: {DateTime.Now}");

                _logger.LogInformation("Scrapeando resultados de lotería...");

                var resultados = await ObtenerNumerosGanadoresAsync();
                await GuardarResultadosEnFirebase(resultados);

                _logger.LogInformation("Resultados guardados en Firebase.");

                // Espera de 30 minutos antes del próximo ciclo
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);

                // Opciones alternativas (comentadas)
                // await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                // await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }


        private async Task<List<(string Loteria, string Fecha, string Hora, string Numero)>> ObtenerNumerosGanadoresAsync()
        {
            var url = "https://enloteria.com";
            using var client = new HttpClient();
            var html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var resultados = new List<(string Loteria, string Fecha, string Hora, string Numero)>();

            var loteriaBloques = doc.DocumentNode.SelectNodes("//div[contains(@class,'card-body')]");
            if (loteriaBloques != null)
            {
                foreach (var bloque in loteriaBloques)
                {
                    var nombre = bloque.SelectSingleNode(".//h5[contains(@class,'lottery-name')]")?.InnerText.Trim() ?? "Desconocida";
                    var fecha = bloque.SelectSingleNode(".//span[contains(@class,'result-date')]")?.InnerText.Trim() ?? "";
                    var hora = bloque.SelectSingleNode(".//span[contains(@class,'lottery-closing-time')]")?.InnerText.Trim() ?? "";

                    var numeros = bloque.SelectNodes(".//div[contains(@class,'result-number')]");
                    if (numeros != null)
                    {
                        foreach (var num in numeros)
                        {
                            resultados.Add((nombre, fecha, hora, num.InnerText.Trim()));
                        }
                    }
                }
            }

            return resultados;
        }

        private string? NormalizarNombre(string nombre, string hora)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null; // ⚠️ devolver null explícito si no hay nombre

            // Centralizar la normalización en HoraHelper
         //   string horaNormalizada = HoraHelper.Normalizar(hora);

            string horaNormalizada; 
            if (DateTime.TryParse(hora, out var dt))
                horaNormalizada = dt.ToString("h:mm tt"); // ejemplo: "8:00 AM"
                else horaNormalizada = hora.Trim();
            // Anguilla
            //  if (nombre.StartsWith("Anguilla"))
            //   {
            //        if (HoraHelper.AnguillaHoras.TryGetValue(horaNormalizada, out var horaFinal))
            //            return $"Anguilla {horaFinal}"; // ✅ usar el valor del diccionario
            //        else
            //            return null; // ⚠️ Hora inválida, se omite
            //    }
            if (nombre.StartsWith("Anguilla"))
            {
                if (horaNormalizada == "8:00 AM")
                    return "Anguilla 08:00 AM";
                if (horaNormalizada == "9:00 AM")
                    return "Anguilla 09:00 AM";
                if (horaNormalizada == "10:00 AM")
                    return "Anguilla 10:00 AM";
                if (horaNormalizada == "11:00 AM")
                    return "Anguilla 11:00 AM";
                if (horaNormalizada == "12:00 PM")
                    return "Anguilla 12:00 PM";
                if (horaNormalizada == "1:00 PM")
                    return "Anguilla 1:00 PM";
                if (horaNormalizada == "2:00 PM")
                    return "Anguilla 2:00 PM";
                if (horaNormalizada == "3:00 PM") 
                    return "Anguilla 3:00 PM";
                if (horaNormalizada == "4:00 PM")
                    return "Anguilla 4:00 PM";
                if (horaNormalizada == "5:00 PM")
                    return "Anguilla 5:00 PM";
                if (horaNormalizada == "6:00 PM")
                    return "Anguilla 6:00 PM";
                if (horaNormalizada == "7:00 PM")
                    return "Anguilla 7:00 PM";
                if (horaNormalizada == "8:00 PM")
                    return "Anguilla 8:00 PM";
                if (horaNormalizada == "9:00 PM") 
                    return "Anguilla 9:00 PM";


            }


            // La Primera
            if (nombre.StartsWith("La Primera"))
            {
                if (horaNormalizada == "12:00 PM")
                    return "Primera 12 PM";
                else if (horaNormalizada == "7:00 PM")
                    return "Primera 7 PM";
                else if (horaNormalizada == "8:00 PM")
                    return "Primera 8 PM";
            }

            // La Suerte
            if (nombre.StartsWith("La Suerte"))
            {
                if (horaNormalizada.Contains("12:30 PM") || horaNormalizada.Contains("12 PM"))
                    return "Suerte 12:30 PM";

                if (horaNormalizada.Contains("6:00 PM") || horaNormalizada.Contains("6PM"))
                    return "Suerte 6:00 PM";

                _logger.LogWarning($"⚠️ No se encontró clave para La Suerte ({horaNormalizada}), se omite.");
                return null;
            }

            // King Lottery Día/Noche
            if (nombre.StartsWith("King Lottery"))
            {
                if (nombre.Contains("Día") || horaNormalizada.Contains("12:30 PM"))
                    return "King Lotery 12:30 PM";
                if (nombre.Contains("Noche") || horaNormalizada.Contains("7:30 PM"))
                    return "King Lotery 7:30 PM";
            }

            // Real → Q.Real
            if (nombre.StartsWith("Real"))
            {
                if (horaNormalizada.Contains("1:00 PM")) return "Q.Real Tarde 1:00 PM";
                if (horaNormalizada.Contains("12:55 PM")) return "Q.Real Tarde 12:55 PM";

                _logger.LogWarning($"⚠️ No se encontró clave para Real ({horaNormalizada}), se omite.");
                return null;
            }

            // Florida
            if (nombre.StartsWith("Florida"))
            {
                if (horaNormalizada.Contains("2:30 PM"))
                    return "FL.Tarde 2:30 PM";
                else if (horaNormalizada.Contains("10:45 PM"))
                    return "FL.Noche 10:45 PM";
            }

            // New York
            if (nombre.StartsWith("New York"))
                return horaNormalizada.Contains("3:30 PM") ? "NY.Tarde 3:30 PM" : "NY.Noche 11:30 PM";

            // Loteka
            if (nombre.StartsWith("Loteka"))
                return "Loteka 7:55 PM";

            // Leidsa / Leisa
            //  if (nombre.StartsWith("Leidsa") || nombre.StartsWith("Leisa"))
            //    return "Leisa 8:55 PM";
            // Leidsa / Leisa
            if (nombre.StartsWith("Leidsa") || nombre.StartsWith("Leisa"))
            {
                var hoy = DateTime.Today.DayOfWeek;

                if (hoy == DayOfWeek.Sunday)
                {
                    // ✅ Los domingos se devuelve LeisaDomingo
                    return "LeisaDomingo_400_PM";
                }
                else
                {
                    // ✅ De lunes a sábado se mantiene la lógica normal
                    return "Leisa_855_PM";
                }
            }


            // Nacional
            // if (nombre.StartsWith("Nacional"))
            //   return horaNormalizada.Contains("2:55 PM") ? "Nac.Tarde 2:55 PM" : "Nac.Noche 9:00 PM";
            // Nacional
            if (nombre.StartsWith("Nacional"))
            {
                var hoy = DateTime.Today.DayOfWeek;

                if (hoy == DayOfWeek.Sunday)
                {
                    // ✅ Los domingos se devuelve NacDomingo
                    return "NacDomingo_600_PM";
                }
                else
                {
                    // ✅ De lunes a sábado se mantiene la lógica normal
                    return horaNormalizada.Contains("2:55 PM")
                        ? "Nac.Tarde 2:55 PM"
                        : "NacNoche_900_PM";
                }
            }


            // Gana Más → Nac.Tarde
            if (nombre.StartsWith("Gana Más"))
                return "Nac.Tarde 2:55 PM";

            // 🔹 Si no se reconoce, loguear y devolver null
            _logger.LogWarning($"⚠️ Nombre de lotería no reconocido: {nombre} ({horaNormalizada}), se omite.");
            return null;
        }





        private async Task GuardarResultadosEnFirebase(List<(string Loteria, string Fecha, string Hora, string Numero)> resultados)
        {
            int guardados = 0;
            int omitidosClaveNula = 0;
            int omitidosSinDiccionario = 0;

            foreach (var grupo in resultados.GroupBy(r => r.Loteria))
            {
                var loteriaNombre = grupo.Key;
                var fechaNormalizada = FechaHelper.GetFechaLocal();
                var hora = grupo.First().Hora;

                // Excluir loterías no deseadas
                if (loteriaNombre.Contains("Haiti Bolet") || loteriaNombre.Contains("LoteDom"))
                {
                    _logger.LogInformation($"⏭️ Excluyendo {loteriaNombre}");
                    continue;
                }

                // Normalizar nombre
                var nombreNormalizado = NormalizarNombre(loteriaNombre, hora);

                if (string.IsNullOrWhiteSpace(nombreNormalizado))
                {
                    omitidosClaveNula++;
                    _logger.LogWarning($"⚠️ Nombre normalizado nulo para {loteriaNombre} ({hora}), se omite.");
                    continue;
                }

                if (!LoteriaClaves.TryGetValue(nombreNormalizado, out var loteriaClave))
                {
                    omitidosSinDiccionario++;
                    _logger.LogWarning($"⚠️ No se encontró clave en el diccionario para {nombreNormalizado}, se omite.");
                    continue;
                }

                var numeros = grupo.Select(r => r.Numero).Take(3).ToList();
                var primerPremio = numeros.ElementAtOrDefault(0) ?? "";
                var segundoPremio = numeros.ElementAtOrDefault(1) ?? "";
                var tercerPremio = numeros.ElementAtOrDefault(2) ?? "";

                _logger.LogInformation($"✅ Guardando: {nombreNormalizado} ({fechaNormalizada}) - {primerPremio}, {segundoPremio}, {tercerPremio}");

                await firebaseClient
                    .Child("Resultados")
                    .Child(loteriaClave)
                    .Child(fechaNormalizada)
                    .PutAsync(new
                    {
                        FechaSorteo = fechaNormalizada,
                        LoteriaClave = loteriaClave,
                        LoteriaNombre = nombreNormalizado,
                        PrimerPremio = primerPremio,
                        SegundoPremio = segundoPremio,
                        TercerPremio = tercerPremio
                    });

                guardados++;
            }

            // 🔹 Resumen final del ciclo
            _logger.LogInformation($"📊 Resumen ciclo: Guardados={guardados}, OmitidosClaveNula={omitidosClaveNula}, OmitidosSinDiccionario={omitidosSinDiccionario}");
        }


    }
}