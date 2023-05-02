library(rvest)
library(stringr)
library(svDialogs)

# Definición de constantes
URL <- "https://cienciasdelsur.com/"

# Bloqueamos las "constantes" para evitar reasignaciones
lockBinding("URL", .GlobalEnv)

# Preguntamos al usuario que palabra compuesta quiere buscar
palabra_compuesta <- tolower(dlgInput(message="Ingrese la palabra compuesta que desea buscar: ")$res)

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
    texto <- tolower(html_text(p_tags[j]))
    texto <- gsub("[^[:alpha:][:space:]ñáéíóúü]", "", texto)
    if (grepl(paste0("\\b", palabra_compuesta, "\\b"), texto)) {
      all_text_articles <- c(all_text_articles, texto)
    }
  }
}

palabras_compuestas_contadas <- str_count(all_text_articles, paste0("\\b", palabra_compuesta, "\\b"))
cat(sprintf("La palabra compuesta '%s' aparece %d veces en los artículos.\n", palabra_compuesta, sum(palabras_compuestas_contadas)))