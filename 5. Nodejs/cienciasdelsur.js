// Importar módulos	
const axios = require('axios');
const cheerio = require('cheerio');
const fs = require('fs');

// Declaración de constantes
const URL = 'https://cienciasdelsur.com/'
const OUTPUT_FILE_ARTICLES_CONTENT = 'EXTRACCION_TEXTOS.txt'
const OUTPUT_FILE_FREQUENCIES = 'FRECUENCIAS_PALABRAS.txt'
const WORDS_TO_DISMISS = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'p', 'q',
    'r', 's', 't', 'u', 'v', 'y', 'x', 'w', 'z', 'y', 'o', 'no', 'el', 'la', 'los', 'las',
    'un', 'una', 'es', 'su', 'unos', 'unas', 'pero', 'a', 'ante', 'bajo', 'cabe', 'con', 'contra', 'de',
    'desde', 'durante', 'en', 'entre', 'hacia', 'hasta', 'mediante', 'para', 'por', 'según',
    'sin', 'so', 'sobre', 'tras', 'como', 'que', 'se', 'del', 'al', 'lo', 'como', 'esta',
    'está', 'este', 'estos', 'estas', 'ha', 'han', 'porque', 'si', 'sí', 'sino', 'más', 'menos',
    'ser', 'estar', 'fue', 'son', 'sin', 'tal', 'también', 'todo', 'todos', 'cual', 'cuales', 'cuya', 'cuyos'
]

// Obtenemos los divs con la clase 'td-module-thumb' y luego recorremos para extraer los enlaces de los articulos
const get_links_articles = async () => {
    try {
        let aux_links_articles = [];
        const response = await axios.get(URL);
        const $ = cheerio.load(response.data);
        $('div.td-module-thumb').each((i, el) => {
            const tag_a = $(el).find('a');
            const link = tag_a.attr('href');
            const title = tag_a.attr('title');
            aux_links_articles.push({
                link,
                title
            });
        });
        // Eliminamos los enlaces duplicados
        const links_articles_unique = aux_links_articles.filter((link, index, self) =>
            index === self.findIndex((t) => (
                t.link === link.link
            ))
        );
        return links_articles_unique;
    } catch (error) {
        console.error(error);
    }
}

// Obtenemos el contenido de los articulos
const get_articles_content = async (links_articles) => {
    try {
        let aux_articles_content = [];
        for (let i = 0; i < links_articles.length; i++) {
            console.log(`⭕ Iniciando scraping en el articulo: ${links_articles[i].title}`);
            const response = await axios.get(links_articles[i].link);
            const $ = cheerio.load(response.data);
            const article_content = $('.td-post-content');
            // Extraemos el texto de todos los parrafos
            let aux_text = '';
            article_content.find('p').each((i, el) => {
                // Extraer solamente el texto e ignorar las etiquetas de imagen
                if ($(el).find('img').length === 0) {
                    aux_text += $(el).text() + '\n';
                }
            });
            aux_articles_content.push(aux_text);
            response ? console.log(`✅ Finalizado scraping en el articulo: ${links_articles[i].title}\n`) : console.log(`❌ Error al realizar scraping en el articulo: ${links_articles[i].title}\n`);
        }

        return aux_articles_content;
    } catch (error) {
        console.error(error);
    }
}

const main = async () => {
    // Se obtienen los enlaces de los articulos y su titulo
    const links_articles = (await Promise.all([get_links_articles()]))[0];

    // Se extrae el contenido de los articulos
    const articles_content = (await Promise.all([get_articles_content(links_articles)]))[0];

    // Escribimos el contenido de los articulos en un archivo de texto
    fs.writeFile(OUTPUT_FILE_ARTICLES_CONTENT, articles_content.join('\n'), (err) => {
        if (err) throw err;
        console.log(`📝 Archivo ${OUTPUT_FILE_ARTICLES_CONTENT} creado correctamente`);
    });

    // Leemos el archivo de texto con el contenido de los articulos
    const data = fs.readFileSync(OUTPUT_FILE_ARTICLES_CONTENT, 'utf8');

    // Se convierte el texto a minusculas, se eliminan los caracteres especiales y se separa por palabras
    const words = data.toLowerCase().replace(/[^a-zñáéíóúü ]/g, "").split(/\s+/);

    // Se eliminan las palabras que no se desean tener en cuenta
    const words_filtered = words.filter(word => !WORDS_TO_DISMISS.includes(word));

    // Se crea un objeto con las palabras y sus frecuencias
    const words_frequencies = words_filtered.reduce((acc, word) => {
        acc[word] = acc[word] ? acc[word] + 1 : 1;
        return acc;
    }, {});

    // Se ordenan las palabras por frecuencia y se exporta a un archivo
    const words_frequencies_sorted = Object.entries(words_frequencies).sort((a, b) => b[1] - a[1]);
}


main();