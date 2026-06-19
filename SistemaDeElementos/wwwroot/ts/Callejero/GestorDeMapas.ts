namespace GestorDeMapas {

    export function VisualizarMapaConGoogle(mapa: HTMLDivElement, pais: string, provincia: string, municipio: string, zona: string, tipoDeVia: string, calle: string, cp: string) {
        let posicion = `${tipoDeVia} ${calle}${(IsNullOrEmpty(zona) ? "" : "," + zona)}${(IsNullOrEmpty(cp) ? "" : "," + cp)}, ${municipio}, ${provincia}, ${pais}`;

        var geocoder = new google.maps.Geocoder();
        // GeocoderStatus.OK pasa a ser GeocoderStatus.OK (sigue igual)
        // pero el callback ahora recibe GeocoderResult[] | null y GeocoderStatus
        geocoder.geocode({ address: posicion }, function (results, status) {
            if (status === google.maps.GeocoderStatus.OK && results && results.length > 0) {
                var mapOptions: google.maps.MapOptions = {
                    center: results[0].geometry.location,
                    mapTypeId: google.maps.MapTypeId.ROADMAP as string
                };
                // google.maps.Map sigue igual
                let map = new google.maps.Map(mapa, mapOptions);
                map.fitBounds(results[0].geometry.viewport);

                // google.maps.Marker sigue igual pero está deprecado en favor de AdvancedMarkerElement
                // Para no romper nada lo dejamos como está
                var markerOptions: google.maps.MarkerOptions = {
                    position: results[0].geometry.location,
                    title: posicion
                };
                var marker = new google.maps.Marker(markerOptions);
                marker.setMap(map);
            }
        });
    }

    export function MostrarFrameGoogleMaps(panel: HTMLDivElement, pais: string, provincia: string, municipio: string, zona: string, tipoDeVia: string, calle: string, cp: string) {
        const direccionCompleta = ComponerDireccion(tipoDeVia, calle, zona, municipio, provincia, cp, pais);
        const encodedDireccion = encodeURIComponent(direccionCompleta);
        const link = `https://www.google.com/maps?q=${encodedDireccion}&output=embed`;
        RenderizarIframe(panel, link);
    }

    export async function MostrarFrameOpenStreetView(
        panel: HTMLDivElement,
        pais: string,
        provincia: string,
        municipio: string,
        zona: string, // Ignorada según tus indicaciones
        tipoDeVia: string,
        calle: string,
        cp: string
    ) {
        // 1. Limpiamos la provincia eliminando los paréntesis con números y espacios
        const provinciaLimpia = provincia.replace(/\(\d+\)\s*/, '');

        // 2. Concatenamos el tipo de vía con la calle para el parámetro 'street' de Nominatim
        // Ej: "Camino de la" + " " + "Torrecica" = "Camino de la Torrecica"
        const calleCompleta = `${tipoDeVia} ${calle}`.trim();

        // 3. Creamos el objeto con la estructura que necesita tu nueva función
        const direccionEstructurada = {
            calle: calleCompleta,
            municipio: municipio,
            provincia: provinciaLimpia,
            cp: cp,
            pais: pais
        };

        // 4. Llamamos a tu gestor de mapas pasando el objeto estructurado
        // Nota: Asegúrate de actualizar 'MostrarFrameStreetView' en tu GestorDeMapas 
        // para que acepte este objeto y monte los parámetros estructurados (street, city, etc.)
        GestorDeMapas.MostrarFrameStreetView(panel, direccionEstructurada, 0.002);
    }

    function ComponerDireccion(tipoDeVia: string, calle: string, zona: string, municipio: string, provincia: string, cp: string, pais: string): string {
        const partesDireccion = [
            `${tipoDeVia} ${calle}`,
            pais === ltrValores.Maestros.Callejero.Paises.Espana ? zona : null,
            municipio,
            provincia,
            cp,
            pais
        ];
        return partesDireccion.filter(Boolean).join(', ');
    }

    //function RenderizarIframe(panel: HTMLDivElement, url: string): void {
    //    panel.style.display = 'none';
    //    panel.innerHTML = "";

    //    const iframe = document.createElement('iframe');
    //    iframe.id = panel.id + '-iframe';
    //    iframe.setAttribute("src", url);
    //    iframe.style.width = "100%";
    //    iframe.style.height = "400px";
    //    iframe.style.border = "0";

    //    panel.appendChild(iframe);

    //    iframe.addEventListener("load", () => {
    //        panel.style.display = 'block';
    //    });
    //}

    function AjustarAlturaIframe(panel: HTMLDivElement): void {
        const iframe = panel.querySelector('iframe') as HTMLIFrameElement;
        if (iframe) iframe.style.height = '100%';
    }

    function RenderizarIframe(panel: HTMLDivElement, url: string): void {
        panel.innerHTML = "";

        const iframe = document.createElement('iframe');
        iframe.id = panel.id + '-iframe';
        iframe.setAttribute("src", url);
        iframe.style.width = "100%";
        iframe.style.height = "400px";
        iframe.style.border = "0";

        panel.appendChild(iframe);

        // Mostrar directamente sin esperar al evento load
        // que Google Maps y OpenStreetMap bloquean
        panel.style.display = 'block';
    }

    export function MostrarFrameGoogleMapsPorTexto(panel: HTMLDivElement, texto: string): void {
        const link = `https://www.google.com/maps?q=${encodeURIComponent(texto)}&output=embed`;
        RenderizarIframe(panel, link);
    }
    // Definimos la interfaz para dar soporte de tipos en TypeScript
    export interface IDireccionEstructurada {
        calle: string;
        municipio: string;
        provincia: string;
        cp: string;
        pais: string;
    }

    export async function MostrarFrameStreetView(
        panel: HTMLDivElement,
        input: string | IDireccionEstructurada,
        deltaIn: number
    ): Promise<any | null> {

        // 🪜 ESTRATEGIA EN CASCADA
        // 1. Intentamos buscar por Calle (Dirección Completa)
        let datos = await ObtenerLatitudLongitud(input, 'calle');

        // 2. Si falla, intentamos por Municipio
        if (!datos) {
            console.warn("Nominatim no encontró la calle. Reintentando por Municipio...");
            datos = await ObtenerLatitudLongitud(input, 'municipio');
            deltaIn = 0.05;
            if (datos) MensajesSe.Info('No se localiza la calle, te muestro el municipio');
        }

        // 3. Si falla, intentamos por Provincia
        if (!datos) {
            console.warn("Nominatim no encontró el municipio. Reintentando por Provincia...");
            datos = await ObtenerLatitudLongitud(input, 'provincia');
            deltaIn = 0.5;
            if (datos) MensajesSe.Info('No se localiza la calle, ni el municipio, te muestro la provincia');
        }

        // 4. Si falla, intentamos por País
        if (!datos) {
            console.warn("Nominatim no encontró la provincia. Reintentando por País...");
            datos = await ObtenerLatitudLongitud(input, 'pais');
            deltaIn = 5;
            if (datos) MensajesSe.Info('Sólo se ha localizado el país, ni la calle ni el municipio ni la provincia');
        }

        // 🎯 Si en cualquiera de los niveles encontramos datos, pintamos el mapa correspondiente
        if (datos) {
            const lat = parseFloat(datos.lat);
            const lon = parseFloat(datos.lon);
            const delta = deltaIn;
            const bbox = `${lon - delta},${lat - delta},${lon + delta},${lat + delta}`;

            RenderizarIframe(panel, `https://www.openstreetmap.org/export/embed.html?bbox=${bbox}&layer=mapnik&marker=${lat},${lon}`);
            AjustarAlturaIframe(panel);

            return datos; // Devolvemos el registro que ha rescatado la búsqueda
        }

        // 🚨 FALLBACK CRÍTICO: Si absolutamente todo falla (sin internet, API caída...)
        console.error("Todos los niveles de búsqueda en cascada han fallado. Aplicando fallback España.");
        RenderizarIframe(panel, `https://www.openstreetmap.org/export/embed.html?bbox=-9.5,35.8,4.5,43.9&layer=mapnik`);
        AjustarAlturaIframe(panel);
        if (datos) MensajesSe.Info('No se localiza la calle indicada');

        return null;
    }

    async function ObtenerLatitudLongitud(
        input: string | IDireccionEstructurada,
        propiedad: 'calle' | 'municipio' | 'provincia' | 'pais'
    ): Promise<any | null> {
        try {
            let datosBusqueda: IDireccionEstructurada;

            if (typeof input === 'string') {
                // 🔍 INTUICIÓN POR REGEX: Separamos por comas y limpiamos espacios
                // Espera un formato aprox: "Calle, Municipio, Provincia, CP, Pais" o similar
                const partes = input.split(',').map(p => p.trim());

                // Reconstrucción estimada basada en la posición desde el final (más seguro)
                datosBusqueda = {
                    pais: partes[partes.length - 1] || '',
                    provincia: partes[partes.length - 2] || '',
                    cp: '', // El CP es difícil de asegurar por posición exacta en string plano
                    municipio: partes.length >= 3 ? partes[partes.length - 3] : '',
                    calle: partes.slice(0, partes.length - 3).join(', ') // Todo lo anterior es la calle
                };

                // Intento de limpiar el CP si viene pegado en la provincia (ej: "30430 Cehegín")
                const matchCP = input.match(/\b\d{5}\b/);
                if (matchCP) datosBusqueda.cp = matchCP[0];
            } else {
                // Si ya es estructurada, la usamos directamente
                datosBusqueda = { ...input };
            }

            // 🛠️ Construimos los parámetros de Nominatim según el nivel (propiedad) solicitado
            let params: URLSearchParams;

            switch (propiedad) {
                case 'calle':
                    // Búsqueda completa (Máxima precisión)
                    params = new URLSearchParams({
                        street: datosBusqueda.calle,
                        city: datosBusqueda.municipio,
                        county: datosBusqueda.provincia,
                        postalcode: datosBusqueda.cp,
                        country: datosBusqueda.pais
                    });
                    break;
                case 'municipio':
                    // Bajamos un escalón: Buscamos solo el municipio dentro de su provincia/país
                    params = new URLSearchParams({
                        city: datosBusqueda.municipio,
                        county: datosBusqueda.provincia,
                        country: datosBusqueda.pais
                    });
                    break;
                case 'provincia':
                    // Bajamos otro escalón: Solo la provincia dentro del país
                    params = new URLSearchParams({
                        county: datosBusqueda.provincia,
                        country: datosBusqueda.pais
                    });
                    break;
                case 'pais':
                    // Último recurso dirigido: El país completo
                    params = new URLSearchParams({
                        country: datosBusqueda.pais
                    });
                    break;
            }

            // Añadimos el formato obligatorio de respuesta
            params.append('format', 'json');
            params.append('limit', '1');

            const response = await fetch(`https://nominatim.openstreetmap.org/search?${params.toString()}`, {
                headers: { 'Accept-Language': 'es', 'User-Agent': 'SistemaDeElementos/1.0' }
            });

            if (response.ok) {
                const datos = await response.json();
                if (datos && datos.length > 0) {
                    return datos[0]; // Devolvemos el primer resultado encontrado
                }
            }
        } catch (error) {
            console.error(`Error buscando por ${propiedad}:`, error);
        }
        return null;
    }

}