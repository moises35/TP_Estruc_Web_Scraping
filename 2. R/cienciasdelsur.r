library(rvest)

# Definición de constantes
URL <- "https://cienciasdelsur.com/"

# Bloqueamos las "constantes" para evitar reasignaciones
lockBinding("URL", .GlobalEnv)

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

print(links)