# Importamos los módulos necesarios
import requests
import re
import matplotlib.pyplot as plt
from bs4 import BeautifulSoup
from wordcloud import WordCloud

# Declaración de constantes
URL = 'https://cienciasdelsur.com/'
OUTPUT_FILE_ARTICLES_CONTENT = 'EXTRACCION_TEXTOS.txt'
OUTPUT_FILE_FREQUENCIES = 'FRECUENCIAS_PALABRAS.txt'
WORDS_TO_DISMISS = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 
    'r', 's', 't', 'u', 'v', 'y', 'x', 'w', 'z', 'y', 'o', 'no', 'el', 'la', 'los', 'las', 
    'un', 'una', 'es', 'su', 'unos', 'unas' , 'pero', 'a', 'ante', 'bajo', 'cabe', 'con', 'contra', 'de', 
    'desde', 'durante', 'en', 'entre', 'hacia', 'hasta', 'mediante', 'para', 'por', 'según', 
    'sin', 'so', 'sobre', 'tras', 'como', 'que', 'se', 'del', 'al', 'lo', 'como', 'esta', 
    'está', 'este', 'estos', 'estas', 'ha', 'han', 'porque', 'si', 'sí', 'sino', 'más', 'menos', 
    'ser', 'estar', 'fue', 'son', 'sin', 'tal', 'también', 'todo', 'todos', 'cual', 'cuales', 'cuya', 'cuyos'
]

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

    print(f'✅ Articulo: {link["title"]} scrapeado con éxito!\n')

# Guardamos el contenido de todos los articulos en un archivo de texto
with open(OUTPUT_FILE_ARTICLES_CONTENT, 'w', encoding='utf-8') as f:
    for text in all_text_articles:
        f.write(text + ' ') 

print(f'📝 Se ha generado el archivo: {OUTPUT_FILE_ARTICLES_CONTENT} con el contenido de todos los articulos scrapeados!')
print('-----------------------------------------------------------------------------------------')
print('📊 Iniciando análisis de frecuencias de palabras...')

# Abrimos el archivo de texto y lo leemos
with open(OUTPUT_FILE_ARTICLES_CONTENT, 'r', encoding='utf-8') as f:
    file_content = f.read()

# Convertimos todo a minúsculas, eliminamos los caracteres especiales y separamos las palabras
file_content = file_content.lower()
file_content = re.sub(r'[^a-zñáéíóúü\s]', '', file_content)
palabras = file_content.split()

# Contamos las frecuencias de cada palabra
frecuencias = {}
for palabra in palabras:
    if palabra not in WORDS_TO_DISMISS:
        if palabra in frecuencias:
            frecuencias[palabra] += 1
        else:
            frecuencias[palabra] = 1

# Ordenamos las frecuencias de mayor a menor
frecuencias_ordenadas = sorted(frecuencias.items(), key=lambda x: x[1], reverse=True)

# Guardamos las frecuencias en un archivo de texto
with open(OUTPUT_FILE_FREQUENCIES, 'w', encoding='utf-8') as f:
    for palabra, frecuencia in frecuencias_ordenadas:
        f.write(f'{palabra}:{frecuencia}\n') 

print(f'📝 Se ha generado el archivo: {OUTPUT_FILE_FREQUENCIES} con las frecuencias de las palabras scrapeadas!')
print('-----------------------------------------------------------------------------------------')

# Imprimimos las 50 palabras más frecuentes
print('--> Las 50 palabras más frecuentes son: ')
for palabra, frecuencia in frecuencias_ordenadas[:50]:
    print(f'\t - {palabra} : {frecuencia}')  

frecuencias = dict(frecuencias_ordenadas[:50])

# Crear la nube de palabras
wordcloud = WordCloud(width=800, height=800, background_color='white').generate_from_frequencies(frecuencias)

# Visualizar la nube de palabras
plt.figure(figsize=(8, 8), facecolor=None)
plt.imshow(wordcloud)
plt.axis("off")
plt.tight_layout(pad=0)
plt.show()
print('🎉 Proceso finalizado con éxito!')