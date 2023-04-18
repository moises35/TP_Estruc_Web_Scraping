# Importamos los módulos necesarios
import requests
from bs4 import BeautifulSoup

# Declaración de constantes
URL = 'https://cienciasdelsur.com/'
OUTPUT_FILE_ARTICLES_CONTENT = 'EXTRACCION_TEXTOS.txt'

# Definimos la URL que queremos scrapear y hacemos la petición 
response = requests.get(URL)

# Creamos el objeto BeautifulSoup
soup = BeautifulSoup(response.text, 'html.parser')

# Obtenemos todos los div que tengan la clase "td-module-thumb" (contenedor de los artículos)
articles_container = soup.find_all('div', {'class': 'td-module-thumb'})

# Obtenemos el link de cada articulo y lo guardamos en una lista
links_articles = []
links_aux = []
for article in articles_container:
    title = article.find('a')['title']
    link = article.find('a')['href']
    # Si el link no está en la lista, lo agregamos
    if link not in links_aux:
        links_articles.append({'title': title, 'link': link})
        links_aux.append(link)

# Iteramos sobre la lista de links y sacamos el contenido de cada uno
all_text_articles = []
for link in links_articles:
    print("⭕ Iniciando scraping en el articulo: " + link['title'] + "...")
    response = requests.get(link['link'])
    soup = BeautifulSoup(response.text, 'html.parser')

    # Obtenemos el contenido del articulo y extraemos cada parrafo
    article_content = soup.find('div', {'class': 'td-post-content'}) 
    article_text_content = article_content.find_all('p')
    for paragraph in article_text_content:
        all_text_articles.append(paragraph.text)

    print("✅ Articulo: " + link['title'] + " scrapeado con éxito!\n")

# Guardamos el contenido de todos los articulos en un archivo de texto
with open(OUTPUT_FILE_ARTICLES_CONTENT, 'w', encoding='utf-8') as f:
    for text in all_text_articles:
        f.write(text + ' ') 

print("📝 Se ha generado el archivo: " + OUTPUT_FILE_ARTICLES_CONTENT + " con el contenido de todos los articulos scrapeados!")