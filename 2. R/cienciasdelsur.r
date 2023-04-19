library(rvest)

# Definición de constantes
URL <- "https://cienciasdelsur.com/"
OUTPUT_FILE_ARTICLES_CONTENT <- "EXTRACCION_TEXTOS.txt"
OUTPUT_FILE_FREQUENCIES <- "FRECUENCIAS_PALABRAS.txt"

# Bloqueamos las "constantes" para evitar reasignaciones
lockBinding("URL", .GlobalEnv)
lockBinding("OUTPUT_FILE_ARTICLES_CONTENT", .GlobalEnv)
lockBinding("OUTPUT_FILE_FREQUENCIES", .GlobalEnv)

# Obtenemos todos los articulos del sitio y extraemos su link
webpage <- read_html(URL)
divs <- html_nodes(webpage, ".td-module-thumb")

links <- c()
for (i in 1:length(divs)) {
  link <- html_attr(html_node(divs[i], "a"), "href")
  if (length(which(links == link)) == 0) {
    links <- c(links, link)
  }
}
links <- unique(links)

# Extraemos el contenido de cada articulo
all_text_articles <- c()
for (i in 1:length(links)) {
  webpage <- read_html(links[i])
  divs <- html_nodes(webpage, ".td-post-content")
  p_tags <- html_nodes(divs, "p")
  for (j in 1:length(p_tags)) {
    texto <- html_text(p_tags[j])
    all_text_articles <- c(all_text_articles, texto)
  }
}

# Guardamos el all_text_articles en un archivo
write.table(all_text_articles, OUTPUT_FILE_ARTICLES_CONTENT, sep = "\n", row.names = FALSE, col.names = FALSE, quote = FALSE)


# ----------------------------------------------------------------------------------

# Abrimos el archivo creado y extraemos el texto
file_articles_content <- readLines(OUTPUT_FILE_ARTICLES_CONTENT, encoding = "UTF-8")

# Convertimos todo a minúsculas, eliminamos los caracteres especiales y separamos las palabras



