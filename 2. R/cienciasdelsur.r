library(rvest)

# Definición de constantes
URL <- "https://cienciasdelsur.com/"

# Bloqueamos las "constantes" para evitar reasignaciones
lockBinding("URL", .GlobalEnv)

# Obtenemos todos los div con la clase 'td-module-thumb'
webpage <- read_html(URL)
divs <- html_nodes(webpage, ".td-module-thumb")