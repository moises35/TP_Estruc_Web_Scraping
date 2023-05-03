using System;
using HtmlAgilityPack;

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
            // Creamos el objeto HttpClient y hacemos la petición
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = httpClient.GetAsync(Constants.URL).Result;
            string content = response.Content.ReadAsStringAsync().Result;
            
            // Creamos el objeto HtmlDocument con el contenido de la página
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);

            // Obtenemos todos los div que tengan la clase "td-module-thumb" (contenedor de los artículos)
            var articlesContainer = doc.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "").Equals("td-module-thumb"));

            // Obtenemos el link de cada artículo y lo guardamos en una lista
            List<Dictionary<string, string>> linksArticles = new List<Dictionary<string, string>>();
            List<string> linksAux = new List<string>();
            foreach (var article in articlesContainer) {
                string title = article.Descendants("a").First().GetAttributeValue("title", "");
                string link = article.Descendants("a").First().GetAttributeValue("href", "");

                // Si el link no está en la lista, lo agregamos
                if (!linksAux.Contains(link)) {
                    Dictionary<string, string> articleLink = new Dictionary<string, string>();
                    articleLink.Add("title", title);
                    articleLink.Add("link", link);
                    linksArticles.Add(articleLink);
                    linksAux.Add(link);
                }
            }
            // Imprimimos los links de los artículos con sus titulos
            foreach (var article in linksArticles) {
                Console.WriteLine(article["title"] + " - " + article["link"]);
            }

        }
    }
}