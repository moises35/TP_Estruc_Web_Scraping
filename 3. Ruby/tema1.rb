require 'nokogiri'
require 'open-uri'
require 'magic_cloud'

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

# Creamos el objeto Nokogiri
html = URI.open(URL).read
nokogiriScraper = Nokogiri::HTML(html)

# Obtenemos todos los div que tengan la clase "td-module-thumb" (contenedor de los artículos)
articles_container = nokogiriScraper.css('div.td-module-thumb')

# Obtenemos el link de cada articulo y lo guardamos en una lista
puts "🕸 Iniciando scraping en la pagina: #{URL}...\n "
links_articles = []
links_aux = []
articles_container.each do |article|
    title = article.css('a').attr('title').value
    link = article.css('a').attr('href').value
    # Si el link no está en la lista, lo agregamos
    unless links_aux.include?(link)
        links_articles << {'title' => title, 'link' => link}
        links_aux << link
    end
end

# Iteramos sobre la lista de links y sacamos el contenido de cada uno
all_text_articles = []
links_articles.each do |link|
    puts "⭕ Iniciando scraping en el articulo: #{link['title']}..."
    html = URI.open(link['link']).read
    nokogiriScraper = Nokogiri::HTML(html)

    # Obtenemos el contenido del articulo y extraemos cada parrafo
    article_content = nokogiriScraper.css('div.td-post-content')
    article_text_content = article_content.css('p')
    article_text_content.each do |paragraph|
        all_text_articles << paragraph.text
    end

    puts "✅ Articulo: #{link['title']} scrapeado con éxito!\n "
end

# Guardamos el contenido de los articulos en un archivo de texto
puts "📝 Se ha generado el archivo: #{OUTPUT_FILE_ARTICLES_CONTENT} con el contenido de todos los articulos scrapeados!"
File.open(OUTPUT_FILE_ARTICLES_CONTENT, 'w') do |file|
    all_text_articles.each do |text|
        file.puts text
    end
end

puts "-----------------------------------------------------------------------------------------"

puts '📊 Iniciando análisis de frecuencias de palabras...'

# Abrimos el archivo de texto y lo leemos
file_content = File.read(OUTPUT_FILE_ARTICLES_CONTENT, encoding: 'utf-8')

# Convertimos todo a minúsculas, eliminamos los caracteres especiales y separamos las palabras
file_content = file_content.downcase.gsub(/[^a-zñáéíóúü\s]/, '')
palabras = file_content.split

# Contamos las frecuencias de cada palabra
frecuencias = {}
palabras.each do |palabra|
  	next if WORDS_TO_DISMISS.include?(palabra)
	if frecuencias.key?(palabra)
		frecuencias[palabra] += 1
	else
		frecuencias[palabra] = 1
	end
end

# Ordenamos las frecuencias de mayor a menor
frecuencias_ordenadas = frecuencias.sort_by { |_, frecuencia| -frecuencia }

# Guardamos las frecuencias en un archivo de texto
File.open(OUTPUT_FILE_FREQUENCIES, 'w', encoding: 'utf-8') do |f|
	frecuencias_ordenadas.each do |palabra, frecuencia|
		f.write("#{palabra}:#{frecuencia}\n")
	end
end

puts "📝 Se ha generado el archivo: #{OUTPUT_FILE_FREQUENCIES} con las frecuencias de las palabras scrapeadas!"

# Imprimimos las 50 palabras más frecuentes
puts "📊 Las 50 palabras más frecuentes son: "
frecuencias_ordenadas.first(50).each do |palabra, frecuencia|
    puts "\t--> #{palabra}: #{frecuencia}"
end

puts "-----------------------------------------------------------------------------------------"

# Guardamos en una variables las primeras 50 palabras más frecuentes
words = []
frecuencias_ordenadas.first(50).each do |palabra, frecuencia|
  	words.push([palabra, frecuencia])
end

puts "☁ Creando nube de palabras..."
cloud = MagicCloud::Cloud.new(words, rotate: :free, scale: :log)
img = cloud.draw(960, 600)
img.write('nube_palabras.png')
puts "📝 Se ha generado el archivo: nube_palabras.png con la nube de palabras!"
puts "🎉 Proceso finalizado con éxito!"