require 'nokogiri'
require 'open-uri'

# Declaración de constantes
URL = 'https://cienciasdelsur.com/'

# Pedir una palabra compuesta al usuario
print '--> Ingrese la palabra compuesta de interes que desea buscar: '
palabra_compuesta = gets.chomp

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
all_article_content = ''
links_articles.each do |link|
    puts "⭕ Iniciando scraping en el articulo: #{link['title']}..."
    html = URI.open(link['link']).read
    nokogiriScraper = Nokogiri::HTML(html)

    # Obtenemos el contenido del articulo y extraemos cada parrafo
    article_content = nokogiriScraper.css('div.td-post-content')
    article_text_content = article_content.css('p')
    article_text_content.each do |paragraph|
        all_article_content += paragraph.text
    end

    puts "✅ Articulo: #{link['title']} scrapeado con éxito!\n "
end


# Convertimos todo a minúsculas, eliminamos los caracteres especiales y separamos las palabras eliminando los espacios
all_article_content = all_article_content.downcase.gsub(/[^a-zñáéíóúü\s]/, '')
all_article_content_split = all_article_content.split.reject { |palabra| palabra.empty? }
all_article_content_split.map! { |palabra| palabra.strip }
palabra_compuesta = palabra_compuesta.downcase.gsub(/[^a-zñáéíóúü\s]/, '')
palabra_compuesta_split = palabra_compuesta.split.reject { |palabra| palabra.empty? }
palabra_compuesta_split.map! { |palabra| palabra.strip }

# Contamos la cantidad de veces que aparece la palabra compuesta en los articulos
cantidad_palabra_compuesta = 0
all_article_content_split.each_with_index do |word, i|
    if word == palabra_compuesta_split[0]
        aux_count = 0
        palabra_compuesta_split.each_with_index do |pc_word, j|
            if all_article_content_split[i + j] == pc_word
                aux_count += 1
            end
        end
        if aux_count == palabra_compuesta_split.length
            cantidad_palabra_compuesta += 1
        end
    end
end

# Mostramos la cantidad de veces que aparece la palabra compuesta en el archivo de texto
puts "--> La palabra compuesta '#{palabra_compuesta}' aparece #{cantidad_palabra_compuesta} veces en el archivo de texto."
