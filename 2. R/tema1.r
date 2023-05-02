library(rvest)
library(wordcloud)

# Definición de constantes
URL <- "https://cienciasdelsur.com/"
OUTPUT_FILE_ARTICLES_CONTENT <- "EXTRACCION_TEXTOS.txt"
OUTPUT_FILE_FREQUENCIES <- "FRECUENCIAS_PALABRAS.txt"
WORDS_TO_DISMISS <- c("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "p", "q", 
  "r", "s", "t", "u", "v", "y", "x", "w", "z", "y", "o", "no", "el", "la", "los", "las", 
  "un", "una", "es", "su", "unos", "unas" , "pero", "a", "ante", "bajo", "cabe", "con", "contra", "de", 
  "desde", "durante", "en", "entre", "hacia", "hasta", "mediante", "para", "por", "según", 
  "sin", "so", "sobre", "tras", "como", "que", "se", "del", "al", "lo", "como", "esta", 
  "está", "este", "estos", "estas", "ha", "han", "porque", "si", "sí", "sino", "más", "menos", 
  "ser", "estar", "fue", "son", "sin", "tal", "también", "todo", "todos", "cual", "cuales", "cuya", "cuyos"
)

# Bloqueamos las "constantes" para evitar reasignaciones
lockBinding("URL", .GlobalEnv)
lockBinding("OUTPUT_FILE_ARTICLES_CONTENT", .GlobalEnv)
lockBinding("OUTPUT_FILE_FREQUENCIES", .GlobalEnv)
lockBinding("WORDS_TO_DISMISS", .GlobalEnv)

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
file_articles_content <- tolower(file_articles_content)
file_articles_content <- gsub("[^[:alpha:][:space:]ñáéíóúü]", "", file_articles_content)
words <- unlist(strsplit(file_articles_content, "\\s+"))
filtered_words <- words[!words %in% WORDS_TO_DISMISS]

# Hallamos la frecuencia, ordenamos y guardamos en el archivo
word_counts <- table(filtered_words)
sorted_word_counts <- sort(word_counts, decreasing = TRUE)
write.table(sorted_word_counts, OUTPUT_FILE_FREQUENCIES, sep = "\t", quote = FALSE)

# Imprimir las 50 primeras frecuencias 
cat("\nLas 50 palabras más frecuentes son:\n")
for (i in 1:50) {
  cat(paste(names(sorted_word_counts)[i], ": ", sorted_word_counts[i], "\n", sep = ""))
}


# ----------------------------------------------------------------------------------

# Creamos la nube de palabras
wordcloud(words = names(sorted_word_counts)[1:50], freq = sorted_word_counts[1:50], scale = c(5, 0.5), min.freq = 1, max.words = 50, random.order = FALSE, rot.per = 0, colors = brewer.pal(8, "Dark2"))

