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
        { "Primera 8 PM", "Primera_800_PM"},
        { "Q.Real Tarde 1:00 PM", "QRealTarde_100_PM"},
        { "FL.Tarde 1:30 PM", "FLTarde_130_PM"},
        { "FL.Noche 10:45 PM", "FLNoche_1045_PM"},
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
                _logger.LogInformation("Scrapeando resultados de lotería...");

                var resultados = await ObtenerNumerosGanadoresAsync();
                await GuardarResultadosEnFirebase(resultados);

                _logger.LogInformation("Resultados guardados en Firebase.");
                //cada 30 minuto se actualiza 
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);


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

        private string NormalizarNombre(string nombre, string hora)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return nombre;

            // Centralizar la normalización en HoraHelper
            string horaNormalizada = HoraHelper.Normalizar(hora);

            // Anguilla
            if (nombre.StartsWith("Anguilla"))
                return $"Anguilla {horaNormalizada}";

            // La Primera
            if (nombre.StartsWith("La Primera"))
                return horaNormalizada.Contains("12:00 PM") ? "Primera 12 PM" : "Primera 8 PM";

            // La Suerte
            if (nombre.StartsWith("La Suerte"))
            {
                if (horaNormalizada.Contains("12:00 PM")) return "Suerte 12:30 PM";
                if (horaNormalizada.Contains("6:00 PM")) return "Suerte 6:00 PM";
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
            if (nombre.StartsWith("Real") && horaNormalizada.Contains("1:00 PM"))
                return "Q.Real Tarde 1:00 PM";

            // Florida
            if (nombre.StartsWith("Florida"))
                return horaNormalizada.Contains("1:30 PM") ? "FL.Tarde 1:30 PM" : "FL.Noche 10:45 PM";

            // New York
            if (nombre.StartsWith("New York"))
                return horaNormalizada.Contains("3:30 PM") ? "NY.Tarde 3:30 PM" : "NY.Noche 11:30 PM";

            // Loteka
            if (nombre.StartsWith("Loteka"))
                return "Loteka 7:55 PM";

            // Leidsa / Leisa
            if (nombre.StartsWith("Leidsa") || nombre.StartsWith("Leisa"))
                return "Leisa 8:55 PM";

            // Nacional
            if (nombre.StartsWith("Nacional"))
                return horaNormalizada.Contains("2:55 PM") ? "Nac.Tarde 2:55 PM" : "Nac.Noche 9:00 PM";

            // Gana Más → Nac.Tarde
            if (nombre.StartsWith("Gana Más"))
                return "Nac.Tarde 2:55 PM";

            return nombre.Trim();
        }




        private async Task GuardarResultadosEnFirebase(List<(string Loteria, string Fecha, string Hora, string Numero)> resultados)
        {
            foreach (var grupo in resultados.GroupBy(r => r.Loteria))
            {
                var loteriaNombre = grupo.Key;
                var fechaTexto = grupo.First().Fecha;

                // Usa FechaHelper en vez de duplicar variable
                var fechaNormalizada = FechaHelper.GetFechaLocal();

                var hora = grupo.First().Hora;

                // Excluir loterías no deseadas
                if (loteriaNombre.Contains("Haiti Bolet") || loteriaNombre.Contains("LoteDom"))
                {
                    Console.WriteLine($"⏭️ Excluyendo {loteriaNombre}");
                    continue;
                }

                // Normalizar nombre con HoraHelper
                var nombreNormalizado = NormalizarNombre(loteriaNombre, hora);

                if (!LoteriaClaves.TryGetValue(nombreNormalizado, out var loteriaClave))
                {
                    Console.WriteLine($"⚠️ No se encontró clave para {nombreNormalizado}, se omite.");
                    continue;
                }

                var numeros = grupo.Select(r => r.Numero).Take(3).ToList();
                var primerPremio = numeros.ElementAtOrDefault(0) ?? "";
                var segundoPremio = numeros.ElementAtOrDefault(1) ?? "";
                var tercerPremio = numeros.ElementAtOrDefault(2) ?? "";

                Console.WriteLine($"Guardando: {nombreNormalizado} ({fechaNormalizada}) - {primerPremio}, {segundoPremio}, {tercerPremio}");

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
            }

        }
    }
}
