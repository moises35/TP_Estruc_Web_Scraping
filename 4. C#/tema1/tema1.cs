﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;


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
        }
    }
}