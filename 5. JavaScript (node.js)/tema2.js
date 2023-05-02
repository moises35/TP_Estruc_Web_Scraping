// Importar módulos	
const axios = require('axios');
const cheerio = require('cheerio');
const readline = require('readline');

// Declaración de constantes
const URL = 'https://cienciasdelsur.com/';

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
const get_articles_content = async (links_articles, palabra_compuesta) => {
    try {
        let count_wc = 0;
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
            const split_aux_text = aux_text.toLowerCase().replace(/[^a-zñáéíóúü ]/g, "").split(/\s+/);

            // Buscamos todas las palabras compuestas dentro de aux_text y sumamos la cantidad de ocurrencias que encontremos. 
            const split_palabra_compuesta = palabra_compuesta.split(/\s+/);

            for (let i = 0; i < split_aux_text.length; i++) {
                if (split_aux_text[i] === split_palabra_compuesta[0]) {
                    let aux_count = 0;
                    for (let j = 0; j < split_palabra_compuesta.length; j++) {
                        if (split_aux_text[i + j] === split_palabra_compuesta[j]) {
                            aux_count++;
                        }
                    }
                    if (aux_count === split_palabra_compuesta.length) {
                        count_wc++;
                    }
                }
            }

            response ? console.log(`✅ Finalizado scraping en el articulo: ${links_articles[i].title}\n`) : console.log(`❌ Error al realizar scraping en el articulo: ${links_articles[i].title}\n`);
        }

        return count_wc;
    } catch (error) {
        console.error(error);
    }
}

const main = async () => {
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout
    });

    // Pedimos al usuario que ingrese la palabra compuesta a buscar
    const question = () => {
        return new Promise((resolve) => {
            rl.question('--> Ingrese la palabra compuesta de interés que desea buscar: ', (answer) => {
                resolve(answer);
            });
        });
    }

    const word_compound = await question();

    // Obtenemos los enlaces de los artículos y su título
    const links_articles = await get_links_articles();

    // Obtenemos la cantidad de ocurrencias de la palabra compuesta en los artículos
    const count_word_compound = await get_articles_content(links_articles, word_compound);
    console.log(`--> La palabra compuesta ${word_compound} aparece ${count_word_compound} veces en los artículos.`);

    rl.close();
}

main();
