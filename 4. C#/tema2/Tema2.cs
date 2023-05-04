using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.Text.RegularExpressions;

namespace WebScraper {
    public static class Constants {
        public const string URL = "https://cienciasdelsur.com/";
    } 

    class Program {
        static void Main(string[] args) {
            // Preguntamos al usuario que palabra compuesta quiere buscar
            Console.WriteLine("--> Ingrese la palabra compuesta de interes que desea buscar: ");
            string word_compound = Console.ReadLine() ?? "";

            if (word_compound == null) {
                Console.WriteLine("No se ingresó ninguna palabra compuesta.");
                return;
            }
            word_compound = word_compound.ToLower();
            word_compound = Regex.Replace(word_compound, @"[^a-zñáéíóúü\s]", "");
            string[] word_compound_split = word_compound.Split(" ");
            word_compound_split = word_compound_split.Where(palabra => !string.IsNullOrWhiteSpace(palabra))
                            .Select(palabra => palabra.Trim())
                            .ToArray();

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
            int count_word_compound = 0;
            foreach (var link in links_articles) {
                Console.WriteLine("* Iniciando scraping en el articulo: " + link["title"] + "...");
                var webpage2 = browser.NavigateToPage(new Uri(link["link"]));
                var article_content = webpage2.Html.CssSelect("div.td-post-content").Single();
                var article_text_content = article_content.CssSelect("p");
                var words_article = new List<string>();

                // Convertimos todo a minúsculas, eliminamos los caracteres especiales y separamos las palabras
                foreach (var paragraph in article_text_content) {
                    string text = paragraph.InnerText.ToLower();
                    text = Regex.Replace(text, @"[^a-zñáéíóúü\s]", "");
                    var words = text.Split(' ');
                    // Eliminamos los espacios en blanco 
                    words = words.Where(palabra => !string.IsNullOrWhiteSpace(palabra))
                            .Select(palabra => palabra.Trim())
                            .ToArray();
                    // Guardamos el array de palabras
                    words_article.AddRange(words);
                }

                // Iteramos sobre words_article y buscamos si word_compound_split está en words_article, la comparación se hace por palabras de forma exacta y no de forma parcial
                for (int i = 0; i < words_article.Count - word_compound_split.Length + 1; i++) {
                    bool match = true;
                    for (int j = 0; j < word_compound_split.Length; j++) {
                        if (words_article[i+j] != word_compound_split[j]) {
                            match = false;
                            break;
                        }
                    }
                    if (match) {
                        count_word_compound++;
                    }
                }

                Console.WriteLine($"- Se finalizo el scraping en el articulo: {link["title"]}\n");
            }

            Console.WriteLine($"Se encontraron {count_word_compound} ocurrencias de la palabra compuesta: {word_compound}");
        }
    }
}