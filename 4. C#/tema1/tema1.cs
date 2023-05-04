using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using KnowledgePicker.WordCloud;
using KnowledgePicker.WordCloud.Coloring;
using KnowledgePicker.WordCloud.Drawing;
using KnowledgePicker.WordCloud.Layouts;
using KnowledgePicker.WordCloud.Primitives;
using KnowledgePicker.WordCloud.Sizers;
using SkiaSharp;

namespace WebScraper {
    // Declaracion de constantes
    public static class Constants {
        public const string URL = "https://cienciasdelsur.com/";
        public const string OUTPUT_FILE_ARTICLES_CONTENT = "EXTRACCION_TEXTOS.txt";
        public const string OUTPUT_FILE_FREQUENCIES = "FRECUENCIAS_PALABRAS.txt";
        public static readonly List<string> WORDS_TO_DISMISS = new List<string> {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "p", "q", 
            "r", "s", "t", "u", "v", "y", "x", "w", "z", "y", "o", "no", "el", "la", "los", "las", 
            "un", "una", "es", "su", "unos", "unas" , "pero", "a", "ante", "bajo", "cabe", "con", "contra", "de", 
            "desde", "durante", "en", "entre", "hacia", "hasta", "mediante", "para", "por", "según", 
            "sin", "so", "sobre", "tras", "como", "que", "se", "del", "al", "lo", "como", "esta", 
            "está", "este", "estos", "estas", "ha", "han", "porque", "si", "sí", "sino", "más", "menos", 
            "ser", "estar", "fue", "son", "sin", "tal", "también", "todo", "todos", "cual", "cuales", "cuya", "cuyos"
        };
    }

    class Program {
        static void Main(string[] args) {
            // Creamos la instancia de la clase ScrapingBrowser
            var browser = new ScrapingBrowser();

            // Hacemos la petición y obtenemos el objeto WebPage
            WebPage webpage = browser.NavigateToPage(new Uri(Constants.URL));
            HtmlNode html = webpage.Html;

            // Obtenemos todos los div que tengan la clase "td-module-thumb" (contenedor de los artículos)
            var articles_container = html.CssSelect("div.td-module-thumb");

            // Obtenemos el link de cada articulo y lo guardamos en una lista
            var links_articles = new List<Dictionary<string, string>>();
            var links_aux = new List<string>();
            foreach (var article in articles_container) {
                string title = article.CssSelect("a").Single().GetAttributeValue("title", "");
                string link = article.CssSelect("a").Single().GetAttributeValue("href", "");

                // Si el link no está en la lista, lo agregamos
                if (!links_aux.Contains(link)) {
                    var dict = new Dictionary<string, string>();
                    dict.Add("title", title);
                    dict.Add("link", link);
                    links_articles.Add(dict);
                    links_aux.Add(link);
                }
            }

            // Iteramos sobre la lista de links y sacamos el contenido de cada uno
            var all_text_articles = new List<string>();
            foreach (var link in links_articles) {
                Console.WriteLine("* Iniciando scraping en el articulo: " + link["title"] + "...");
                var webpage2 = browser.NavigateToPage(new Uri(link["link"]));
                var article_content = webpage2.Html.CssSelect("div.td-post-content").Single();
                var article_text_content = article_content.CssSelect("p");
                foreach (var paragraph in article_text_content) {
                    all_text_articles.Add(paragraph.InnerText);
                }

                Console.WriteLine($"- Se finalizo el scraping en el articulo: {link["title"]}\n");
            }

            // Guardamos el contenido de todos los articulos en un archivo de texto
            using (StreamWriter file = new StreamWriter(Constants.OUTPUT_FILE_ARTICLES_CONTENT)) {
                foreach (var text in all_text_articles) {
                    file.WriteLine(text + " ");
                }
            }

            Console.WriteLine($"** Se ha generado el archivo: {Constants.OUTPUT_FILE_ARTICLES_CONTENT} con el contenido de todos los articulos scrapeados!");
            Console.WriteLine("-----------------------------------------------------------------------------------------");
            // Leemos el archivo de texto generado utilizando la codificacion UTF-8
            Console.WriteLine("**  Iniciando análisis de frecuencias de palabras...");
            string text_file = File.ReadAllText(Constants.OUTPUT_FILE_ARTICLES_CONTENT, System.Text.Encoding.UTF8);

            // Convertimos todo a minúsculas, eliminamos los caracteres especiales y separamos las palabras
            text_file = text_file.ToLower();
            text_file = Regex.Replace(text_file, @"[^a-zñáéíóúü\s]", "");
            var palabras = text_file.Split(' ');

            // Eliminamos todos los espacios en blanco
            palabras = palabras.Where(palabra => !string.IsNullOrWhiteSpace(palabra))
                            .Select(palabra => palabra.Trim())
                            .ToArray();

            // Contamos las frecuencias de cada palabra, sin contar las palabras que no nos interesan
            Dictionary<string, int> frecuencias = new Dictionary<string, int>();
            foreach (var palabra in palabras) {
                if (!(Constants.WORDS_TO_DISMISS.Contains(palabra))) {
                    if (frecuencias.ContainsKey(palabra)) {
                        frecuencias[palabra]++;
                    } else {
                        frecuencias[palabra] = 1;
                    }
                }
            }

            // Ordenamos las frecuencias de mayor a menor
            var frecuencias_ordenadas = frecuencias.OrderByDescending(x => x.Value);

            // Guardamos las frecuencias en un archivo de texto
            using (StreamWriter file = new StreamWriter(Constants.OUTPUT_FILE_FREQUENCIES, false, System.Text.Encoding.UTF8)) {
                foreach (var item in frecuencias_ordenadas) {
                    file.WriteLine($"{item.Key}:{item.Value}");
                }
            }

            // Mostramos las 50 palabras más frecuentes
            Dictionary<string, int> frecuencias_ordenadas_50 = new Dictionary<string, int>();
            int contador = 0;
            foreach (var item in frecuencias_ordenadas) {
                if (contador == 50) {
                    break;
                }
                frecuencias_ordenadas_50[item.Key] = item.Value;
                contador++;
            }
            Console.WriteLine($"** Las 50 palabras más frecuentes son:");
            foreach (var item in frecuencias_ordenadas_50) {
                Console.WriteLine($"\t--> {item.Key}:{item.Value}");
            }

            Console.WriteLine($"** Se ha generado el archivo: {Constants.OUTPUT_FILE_FREQUENCIES} con las frecuencias de palabras!");
            Console.WriteLine("-----------------------------------------------------------------------------------------");

            // Generar la nube
            Console.WriteLine("** Generando nube de palabras...");
            
            // Configuración de la nube de palabras.
            const int k = 6; // scale
            var wordCloud = new WordCloudInput(frecuencias_ordenadas_50.Select(p => new WordCloudEntry(p.Key, p.Value))) {
                Width = 1024 * k,
                Height = 256 * k,
                MinFontSize = 16 * k,
                MaxFontSize = 32 * k
            };

            var sizer = new LogSizer(wordCloud);
            using var engine = new SkGraphicEngine(sizer, wordCloud);
            var layout = new SpiralLayout(wordCloud);
            var colorizer = new RandomColorizer();
            var wcg = new WordCloudGenerator<SKBitmap>(wordCloud, engine, layout, colorizer);

            // Dibuja la nube de palabras.
            using var final = new SKBitmap(wordCloud.Width, wordCloud.Height);
            using var canvas = new SKCanvas(final);
            canvas.Clear(SKColors.White);
            using var bitmap = wcg.Draw();
            canvas.DrawBitmap(bitmap, 0, 0);

            // Guardar la nube de palabras.
            using var data = final.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite("NUBE_PALABRAS.png");
            data.SaveTo(stream);
            Console.WriteLine($"** Se ha generado el archivo: NUBE_PALABRAS.png con la nube de palabras!");
        }
    }
}