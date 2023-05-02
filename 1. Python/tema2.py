# Importamos los módulos necesarios
import requests
from bs4 import BeautifulSoup

# Definimos la URL que queremos scrapear y hacemos la petición 
URL = 'https://cienciasdelsur.com/'
response = requests.get(URL)

# Preguntamos al usuario que palabra compuesta quiere buscar
word_compound = input('--> Ingrese la palabra compuesta de interes que desea buscar: ')

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

    print(f'✅ Articulo: {link["title"]} scrapeado con éxito!\n')

# Buscamos la cantidad de veces que aparece la palabra compuesta en el contenido de los articulos
word_compound_count = 0
for text in all_text_articles:
    if word_compound.lower() in text.lower():
        word_compound_count += 1

print(f'🔍 La palabra compuesta "{word_compound}" aparece {word_compound_count} veces en el contenido de los articulos scrapeados!')

