using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Xml;
using Newtonsoft.Json;

namespace cache_redis_azure
{

    class Program
    {
        public static List<string> listacodigos = new List<string>();
        public static List<(string opcion, long tiempoRespuesta)> resultados = new List<(string, long)>();
        static void Main(string[] args)
        {
            // Crear las opciones de configuración para Redis
            ConfigurationOptions options = new ConfigurationOptions
            {
                EndPoints = { "" },
                Password = "",
                Ssl = true,
                AbortOnConnectFail = false,
                ConnectTimeout = 300, // Timeout extendido: 30 segundos
                SyncTimeout = 300    // Tiempo de espera para operaciones sincronas: 30 segundos
            };
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);
            IDatabase db = redis.GetDatabase();

            while (true)
            {
                try
                {
                    Console.WriteLine("1 -IngresarContratos");
                    Console.WriteLine("2 -ObtenerElementoAleatorio");
                    Console.WriteLine("3 -Resultados");

                    var opcionElegida = Console.ReadLine();

                    switch (opcionElegida)
                    {
                        case "1":
                            Console.WriteLine("IngresarContratos.");
                            Console.WriteLine("Inicio.");
                            var inicio = Console.ReadLine();
                            Console.WriteLine("FIN.");
                            var fin = Console.ReadLine();
                            IngresarContratos(db, Convert.ToInt32(inicio), Convert.ToInt32(fin));
                            break;

                        case "2":
                            Console.WriteLine("ObtenerElementoAleatorio. x10");
                            for (int i = 0; i < 10; i++)
                            {
                                var codigo = ObtenerElementoAleatorio(listacodigos);
                                var valor = RecuperarValor(db, codigo); 
                                Console.WriteLine(valor);
                            }
                            break;

                        case "3":
                            Console.WriteLine("Análisia de resultados");
                            AnalisisResultados();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RefrescarValor.{ex.Message}");
                }
            }



            //try
            //{
            //    // Establecer la conexión usando ConfigurationOptions



            //    // Lista para almacenar los tiempos de respuesta
            //    List<(string opcion, long tiempoRespuesta)> resultados = new List<(string, long)>();

            //    // Lista para almacenar las claves generadas
            //    List<string> clavesGeneradas = new List<string>();

            //    // Crear un generador aleatorio
            //    Random random = new Random();

            //    // Ejecutar 1 millón de peticiones
            //    for (int i = 0; i < 1000; i++)
            //    {
            //        // Elegir una opción aleatoria entre 1 y 5 (sin incluir la opción 6)
            //        string opcionElegida = (random.Next(1, 6)).ToString();

            //        // Medir el tiempo de respuesta
            //        Stopwatch stopwatch = Stopwatch.StartNew();
            //        try
            //        {

            //            switch (opcionElegida)
            //            {
            //                case "1":
            //                    IngresarValor(db, clavesGeneradas);
            //                    Console.SetCursorPosition(0, 1);
            //                    Console.WriteLine("IngresarValor.");
            //                    break;

            //                case "2":
            //                    RefrescarValor(db, clavesGeneradas);
            //                    Console.SetCursorPosition(0, 1);
            //                    Console.WriteLine("RefrescarValor.");
            //                    break;

            //                case "3":
            //                    EliminarValor(db, clavesGeneradas);
            //                    Console.SetCursorPosition(0, 1);
            //                    Console.WriteLine("EliminarValor.");
            //                    break;

            //                case "4":
            //                    EstablecerExpiracion(db, clavesGeneradas);
            //                    Console.SetCursorPosition(0, 1);
            //                    Console.WriteLine("EstablecerExpiracion.");
            //                    break;

            //                case "5":
            //                    RecuperarValor(db, clavesGeneradas);
            //                    Console.SetCursorPosition(0, 1);
            //                    Console.WriteLine("RecuperarValor.");
            //                    break;

            //                default:
            //                    Console.WriteLine("Opción no válida.");
            //                    break;
            //            }
            //        }
            //        catch (Exception)
            //        {
            //            stopwatch.Stop();
            //            Console.SetCursorPosition(0, 1);
            //            resultados.Add(("Error", stopwatch.ElapsedMilliseconds));
            //        }

            //        stopwatch.Stop();
            //        resultados.Add((opcionElegida, stopwatch.ElapsedMilliseconds));
            //        // Guardar la opción y el tiempo de respuesta

            //    }

            //    Console.SetCursorPosition(0, 3);
            //    Console.WriteLine("Se completaron 1000 peticiones.");

            //    redis.Dispose();



            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Error: {ex.Message}");
            //}
        }

        static void AnalisisResultados()
        {
            Console.WriteLine("Analizando resultados.");
            var tiemposPorOperacion = resultados
               .GroupBy(r => r.opcion) // Agrupar por la operación
               .Select(g => new
               {
                   Operacion = g.Key,
                   Promedio = g.Average(x => x.tiempoRespuesta),
                   Maximo = g.Max(x => x.tiempoRespuesta),
                   Minimo = g.Min(x => x.tiempoRespuesta),
                   Cantidad = g.Count(),
               })
               .OrderBy(x => x.Operacion) // Ordenar por tipo de operación
               .ToList();

            // Imprimir los resultados por operación
            Console.WriteLine("\nEstadísticas de tiempo por operación:");
            foreach (var item in tiemposPorOperacion)
            {
                Console.WriteLine($"Operación: {item.Operacion}");
                Console.WriteLine($"  Cantidad: {item.Cantidad}");
                Console.WriteLine($"  Promedio: {item.Promedio} ms");
                Console.WriteLine($"  Máximo: {item.Maximo} ms");
                Console.WriteLine($"  Mínimo: {item.Minimo} ms");
                Console.WriteLine();
            }



        }

        static string ObtenerElementoAleatorio(List<string> lista)
        {
            // Crear una instancia de Random
            Random random = new Random();

            // Obtener un índice aleatorio basado en el tamaño de la lista
            int indice = random.Next(lista.Count);

            // Retornar el elemento de la lista en ese índice
            return lista[indice];
        }

        static async void IngresarContratos(IDatabase db, int inicio, int fin)
        {
            //15834
            for (int codigoContrato = inicio; codigoContrato < fin; codigoContrato++)
            {
                try
                {

                    listacodigos.Add(codigoContrato.ToString());
                    Stopwatch sw = Stopwatch.StartNew();
                    sw.Restart();
                    sw.Start();
                    var respuesta = await ObtenerContratoPorCodigo(codigoContrato);
                    sw.Stop();
                    resultados.Add(("Consulta-Contratos", sw.ElapsedMilliseconds));
                    sw.Restart();
                    sw.Start();
                    if (respuesta != null)
                    {
                        IngresarValor(db, $"{codigoContrato}", respuesta);
                        sw.Stop();
                        resultados.Add(("Ingresar-Cache", sw.ElapsedMilliseconds));
                    }
                    // Imprimir la respuesta
                    Console.WriteLine($"Respuesta para código contrato {codigoContrato}:");
                    // Esperar un breve intervalo entre solicitudes para no sobrecargar el servidor (opcional)
                    await Task.Delay(500); // Delay de 500 ms
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Respuesta para código contrato {ex}:");
                }
            }

        }
        static void IngresarValor(IDatabase db, string codigoContrato, object valor)
        {
            var v = JsonConvert.SerializeObject(valor);
            db.StringSet(codigoContrato, v,,,,);
        }
        static void IngresarValor(IDatabase db, List<string> clavesGeneradas)
        {
            // Generar una clave única y guardarla
            string clave = "clave" + Guid.NewGuid().ToString();
            clavesGeneradas.Add(clave);

            string valor = "valor" + Guid.NewGuid().ToString();
            db.StringSet(clave, valor);
        }

        static void RefrescarValor(IDatabase db, List<string> clavesGeneradas)
        {
            if (clavesGeneradas.Count == 0)
                return;

            // Tomar una clave aleatoria de las generadas
            Random random = new Random();
            string clave = clavesGeneradas[random.Next(clavesGeneradas.Count)];

            string valor = "valor" + Guid.NewGuid().ToString();
            db.StringSet(clave, valor); // Actualizar el valor
        }

        static void EliminarValor(IDatabase db, List<string> clavesGeneradas)
        {
            if (clavesGeneradas.Count == 0)
                return;

            // Tomar una clave aleatoria de las generadas
            Random random = new Random();
            string clave = clavesGeneradas[random.Next(clavesGeneradas.Count)];

            db.KeyDelete(clave); // Eliminar la clave

            // Eliminar la clave de la lista interna
            clavesGeneradas.Remove(clave);
        }

        static void EstablecerExpiracion(IDatabase db, List<string> clavesGeneradas)
        {
            if (clavesGeneradas.Count == 0)
                return;

            // Tomar una clave aleatoria de las generadas
            Random random = new Random();
            string clave = clavesGeneradas[random.Next(clavesGeneradas.Count)];

            string valor = "valor" + Guid.NewGuid().ToString();
            db.StringSet(clave, valor);

            // Establecer tiempo de expiración
            db.KeyExpire(clave, TimeSpan.FromSeconds(60)); // Establecer expiración en 60 segundos
        }

        static void RecuperarValor(IDatabase db, List<string> clavesGeneradas)
        {
            if (clavesGeneradas.Count == 0)
                return;

            // Tomar una clave aleatoria de las generadas
            Random random = new Random();
            string clave = clavesGeneradas[random.Next(clavesGeneradas.Count)];

            string valor = db.StringGet(clave); // Intentar recuperar el valor de la clave
        }

        static string RecuperarValor(IDatabase db, string clave)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();
            sw.Start();
            var valor = db.StringGet(clave);
            sw.Stop();
            resultados.Add(("Recuperar-Cache", sw.ElapsedMilliseconds));
            return valor;
        }
        public static async Task<object> ObtenerContratoPorCodigo(int codigoContrato)
        {
            string url = "http://pruebas.servicios.saludsa.com.ec/ServicioContratos/api/contrato/ObtenerContratoPorCodigo";
            string incluirBeneficiarios = "true";
            string sistemaOperativo = "Saludsa";  // Fijo
            string dispositivoNavegador = "Saludsa";  // Fijo

            // Crear el cliente HTTP
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVVNSQUJFRkFSTSIsInN1YiI6IlVTUkFCRUZBUk0iLCJuYmYiOjE3MzUzMjczMTMsImV4cCI6MTczNTM2MzMxMywiaXNzIjoiU2FsdWQgUy5BLiIsImF1ZCI6IjhhM2U0ZDEwYjJiMjRkNmI5YzU1Yzg4YTk1ZmRjMzI0In0.3v3VI2MCwxDcHgLGEiOxguLQX8qICoFGAgYjFOuFzig");
            client.DefaultRequestHeaders.Add("CodigoAplicacion", "3");
            client.DefaultRequestHeaders.Add("CodigoPlataforma", "7");
            client.DefaultRequestHeaders.Add("SistemaOperativo", sistemaOperativo);
            client.DefaultRequestHeaders.Add("DispositivoNavegador", dispositivoNavegador);
            client.DefaultRequestHeaders.Add("DireccionIP", "10.10.1.1"); // Reemplazar por la IP real

            // Crear la solicitud GET con los parámetros
            var response = await client.GetAsync($"{url}?codigoContrato={codigoContrato}&incluirBeneficiarios={incluirBeneficiarios}");

            // Verificar la respuesta
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                // Deserializar la respuesta JSON en un objeto
                var resultado = JsonConvert.DeserializeObject<object>(responseData);
                return resultado;
            }
            else
            {
                Console.WriteLine($"Error para código contrato {codigoContrato}: {response.StatusCode}, {response.ReasonPhrase}");
                return null;
            }
        }
    }
}
