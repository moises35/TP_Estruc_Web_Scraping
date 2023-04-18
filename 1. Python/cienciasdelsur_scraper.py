# Importamos los módulos necesarios
import requests
from bs4 import BeautifulSoup

# Definimos la URL que queremos scrapear y hacemos la petición 
URL = 'https://cienciasdelsur.com/'
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
for link in links_articles:
    print("⭕ Iniciando scraping en el articulo: " + link['title'] + "...")
    response = requests.get(link['link'])
    soup = BeautifulSoup(response.text, 'html.parser')
    print("✅ Articulo: " + link['title'] + " scrapeado con éxito!\n")

    
