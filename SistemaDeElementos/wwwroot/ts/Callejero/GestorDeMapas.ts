declare namespace L {
    function map(element: HTMLElement | string, options?: any): L.Map;
    function tileLayer(urlTemplate: string, options?: any): L.TileLayer;
    function marker(latlng: [number, number] | { lat: number; lng: number }, options?: any): L.Marker;

    function icon(options: any): L.Icon;

    interface Icon { }

    interface Map {
        setView(center: [number, number], zoom: number): this;
        fitBounds(bounds: [[number, number], [number, number]]): this;
        remove(): void;
        invalidateSize(): void;
    }
    interface TileLayer {
        addTo(map: L.Map): this;
    }
    interface Marker {
        addTo(map: L.Map): this;
        bindPopup(content: string): this;
        openPopup(): this;
    }
    type LatLngBoundsExpression = [[number, number], [number, number]];
}

namespace GestorDeMapas {

    export const ltrModoRenderizacion = {
        Frame: 'frame',
        Flet: 'flet'
    } as const;

    type ModoRenderizacion = typeof ltrModoRenderizacion[keyof typeof ltrModoRenderizacion];

    export const ltrDireccionEstructurada = {
        Calle: 'calle',
        Municipio: 'municipio',
        Provincia: 'provincia',
        Pais: 'pais'
    } as const;

    type NivelBusqueda = typeof ltrDireccionEstructurada[keyof typeof ltrDireccionEstructurada];

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

    export async function MostrarVisorDeOpenStreetView(
        panel: HTMLDivElement,
        pais: string,
        provincia: string,
        municipio: string,
        zona: string,
        tipoDeVia: string,
        calle: string,
        cp: string
    ) {
        const provinciaLimpia = provincia.replace(/\(\d+\)\s*/, '');
        const calleCompleta = `${tipoDeVia} ${calle}`.trim();

        const direccionEstructurada: IDireccionEstructurada = {
            calle: calleCompleta,
            municipio: municipio,
            provincia: provinciaLimpia,
            cp: cp,
            pais: pais
        };

        const popupHtml = `<b>${calleCompleta}</b><br>${municipio}, ${provinciaLimpia}`;
        GestorDeMapas.MostrarCalleEnStreetView(panel, direccionEstructurada, 0.002, GestorDeMapas.ltrModoRenderizacion.Flet, popupHtml);
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

    function RenderizarIframe(panel: HTMLDivElement, url: string): void {
        panel.innerHTML = "";
        const iframe = document.createElement('iframe');
        iframe.id = panel.id + '-iframe';
        iframe.setAttribute("src", url);
        iframe.style.width = "100%";
        iframe.style.height = "100%";
        iframe.style.border = "0";
        panel.appendChild(iframe);
        panel.style.display = 'block';
    }

    function RenderizarIframeOSM(panel: HTMLDivElement, lat: number, lon: number, delta: number): void {
        const bbox = `${lon - delta},${lat - delta},${lon + delta},${lat + delta}`;
        RenderizarIframe(panel, `https://www.openstreetmap.org/export/embed.html?bbox=${bbox}&layer=mapnik&marker=${lat},${lon}`);
    }

    const _leafletMaps = new WeakMap<HTMLDivElement, L.Map>();
    const _leafletObservers = new WeakMap<HTMLDivElement, ResizeObserver>();

    const _iconoMarcador = () => L.icon({
        iconUrl: '/lib/leaflet/dist/images/marker-icon.png',
        iconRetinaUrl: '/lib/leaflet/dist/images/marker-icon-2x.png',
        shadowUrl: '/lib/leaflet/dist/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });

    function AsegurarLeafletCargado(): Promise<void> {
        return new Promise((resolve) => {
            if (typeof L !== 'undefined') { resolve(); return; }
            if (!document.getElementById('leaflet-css')) {
                const link = document.createElement('link');
                link.id = 'leaflet-css';
                link.rel = 'stylesheet';
                link.href = '/lib/leaflet/dist/leaflet.css';
                document.head.appendChild(link);
            }
            const script = document.createElement('script');
            script.src = '/lib/leaflet/dist/leaflet.js';
            script.onload = () => resolve();
            document.head.appendChild(script);
        });
    }

    async function RenderizarLeaflet(panel: HTMLDivElement, lat: number, lon: number, delta: number, popupHtml?: string): Promise<void> {
        await AsegurarLeafletCargado();

        const observerExistente = _leafletObservers.get(panel);
        if (observerExistente) { observerExistente.disconnect(); _leafletObservers.delete(panel); }

        const mapaExistente = _leafletMaps.get(panel);
        if (mapaExistente) { mapaExistente.remove(); _leafletMaps.delete(panel); }

        panel.innerHTML = '';
        panel.style.display = 'block';

        const mapa = L.map(panel);
        _leafletMaps.set(panel, mapa);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(mapa);

        const bounds: L.LatLngBoundsExpression = [[lat - delta, lon - delta], [lat + delta, lon + delta]];
        mapa.fitBounds(bounds);

        if (lat !== 0 || lon !== 0) {
            const marker = L.marker([lat, lon], { icon: _iconoMarcador() }).addTo(mapa);
            if (!IsNullOrEmpty(popupHtml)) {
                marker.bindPopup(popupHtml).openPopup();
            }
        }

        const observer = new ResizeObserver(() => mapa.invalidateSize());
        observer.observe(panel);
        _leafletObservers.set(panel, observer);
    }

    async function RenderizarLeafletEspana(panel: HTMLDivElement): Promise<void> {
        await AsegurarLeafletCargado();

        const observerExistente = _leafletObservers.get(panel);
        if (observerExistente) { observerExistente.disconnect(); _leafletObservers.delete(panel); }

        const mapaExistente = _leafletMaps.get(panel);
        if (mapaExistente) { mapaExistente.remove(); _leafletMaps.delete(panel); }

        panel.innerHTML = '';
        panel.style.display = 'block';

        const mapa = L.map(panel);
        _leafletMaps.set(panel, mapa);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(mapa);

        mapa.fitBounds([[35.8, -9.5], [43.9, 4.5]]);

        const observer = new ResizeObserver(() => mapa.invalidateSize());
        observer.observe(panel);
        _leafletObservers.set(panel, observer);
    }

    // Definimos la interfaz para dar soporte de tipos en TypeScript
    export interface IDireccionEstructurada {
        calle: string;
        municipio: string;
        provincia: string;
        cp: string;
        pais: string;
    }

    export async function MostrarCalleEnStreetView(
        panel: HTMLDivElement,
        input: string | IDireccionEstructurada,
        deltaIn: number,
        modo: ModoRenderizacion,
        popupHtml?: string
    ): Promise<any | null> {

        // 🪜 ESTRATEGIA EN CASCADA
        // 1. Intentamos buscar por Calle (Dirección Completa)
        let datos = await ObtenerLatitudLongitud(input, ltrDireccionEstructurada.Calle);

        // 1b. Solo para estructuradas: si falla, reintentamos sin CP y validamos que el resultado
        //     sea del mismo municipio (el CP puede estar en null o ser incorrecto para Nominatim)
        if (!datos && typeof input !== 'string') {
            const sinCp: IDireccionEstructurada = { ...input, cp: '' };
            const datosSinCp = await ObtenerLatitudLongitud(sinCp, ltrDireccionEstructurada.Calle);
            if (datosSinCp && EsMismoMunicipio(datosSinCp, input)) {
                datos = datosSinCp;
            }
        }

        // 2. Si falla, intentamos por Municipio
        if (!datos) {
            console.warn("Nominatim no encontró la calle. Reintentando por Municipio...");
            datos = await ObtenerLatitudLongitud(input, ltrDireccionEstructurada.Municipio);
            deltaIn = 0.05;
            if (datos) MensajesSe.Info('No se localiza la calle, te muestro el municipio');
        }

        // 3. Si falla, intentamos por Provincia
        if (!datos) {
            console.warn("Nominatim no encontró el municipio. Reintentando por Provincia...");
            datos = await ObtenerLatitudLongitud(input, ltrDireccionEstructurada.Provincia);
            deltaIn = 0.5;
            if (datos) MensajesSe.Info('No se localiza la calle, ni el municipio, te muestro la provincia');
        }

        // 4. Si falla, intentamos por País
        if (!datos) {
            console.warn("Nominatim no encontró la provincia. Reintentando por País...");
            datos = await ObtenerLatitudLongitud(input, ltrDireccionEstructurada.Pais);
            deltaIn = 5;
            if (datos) MensajesSe.Info('Sólo se ha localizado el país, ni la calle ni el municipio ni la provincia');
        }

        // 🎯 Si en cualquiera de los niveles encontramos datos, pintamos el mapa correspondiente
        if (datos) {
            const lat = parseFloat(datos.lat);
            const lon = parseFloat(datos.lon);
            if (modo === ltrModoRenderizacion.Flet)
                await RenderizarLeaflet(panel, lat, lon, deltaIn, popupHtml);
            else
                RenderizarIframeOSM(panel, lat, lon, deltaIn);
            return datos;
        }

        // 🚨 FALLBACK CRÍTICO: Si absolutamente todo falla (sin internet, API caída...)
        console.error("Todos los niveles de búsqueda en cascada han fallado. Aplicando fallback España.");
        if (modo === ltrModoRenderizacion.Flet)
            await RenderizarLeafletEspana(panel);
        else
            RenderizarIframe(panel, `https://www.openstreetmap.org/export/embed.html?bbox=-9.5,35.8,4.5,43.9&layer=mapnik`);
        return null;
    }

    function EsMismoMunicipio(resultado: any, dir: IDireccionEstructurada): boolean {
        const addr = resultado.address;
        if (!addr) return true;
        const municipioResultado = (addr.city || addr.town || addr.village || addr.municipality || '').toLowerCase();
        const municipioEsperado = dir.municipio.toLowerCase();
        if (!municipioResultado || !municipioEsperado) return true;
        return municipioResultado.includes(municipioEsperado) || municipioEsperado.includes(municipioResultado);
    }

    async function ObtenerLatitudLongitud(
        input: string | IDireccionEstructurada,
        propiedad: NivelBusqueda
    ): Promise<any | null> {
        try {
            let datosBusqueda: IDireccionEstructurada;

            if (typeof input === 'string') {
                const partes = input.split(',').map(p => p.trim());
                const municipioRaw = partes.length >= 3 ? partes[partes.length - 3] : '';
                const municipioPartes = municipioRaw.split('-').map(p => p.trim());
                const municipio = municipioPartes.length > 1 ? municipioPartes.slice(1).join('-').trim() : municipioPartes[0];
                const cpDeMunicipio = municipioPartes.length > 1 ? municipioPartes[0] : '';
                datosBusqueda = {
                    pais: partes[partes.length - 1] || '',
                    provincia: partes[partes.length - 2] || '',
                    cp: cpDeMunicipio,
                    municipio: municipio,
                    calle: partes.slice(0, partes.length - 3).join(', ')
                };
                if (IsNullOrEmpty(datosBusqueda.cp)) {
                    const matchCP = input.match(/\b\d{5}\b/);
                    if (matchCP) datosBusqueda.cp = matchCP[0];
                }
            } else {
                datosBusqueda = { ...input };
            }

            let params: URLSearchParams;

            switch (propiedad) {
                case ltrDireccionEstructurada.Calle:
                    params = new URLSearchParams({
                        street: datosBusqueda.calle,
                        city: datosBusqueda.municipio,
                        county: datosBusqueda.provincia,
                        country: datosBusqueda.pais
                    });
                    // Solo añadimos postalcode si tiene valor, evitando enviar "null" o vacío
                    if (!IsNullOrEmpty(datosBusqueda.cp)) params.append('postalcode', datosBusqueda.cp);
                    break;
                case ltrDireccionEstructurada.Municipio:
                    params = new URLSearchParams({
                        city: datosBusqueda.municipio,
                        county: datosBusqueda.provincia,
                        country: datosBusqueda.pais
                    });
                    break;
                case ltrDireccionEstructurada.Provincia:
                    params = new URLSearchParams({
                        county: datosBusqueda.provincia,
                        country: datosBusqueda.pais
                    });
                    break;
                case ltrDireccionEstructurada.Pais:
                    params = new URLSearchParams({
                        country: datosBusqueda.pais
                    });
                    break;
            }

            params.append('format', 'json');
            params.append('limit', '1');
            params.append('addressdetails', '1');

            const response = await fetch(`https://nominatim.openstreetmap.org/search?${params.toString()}`, {
                headers: { 'Accept-Language': 'es', 'User-Agent': 'SistemaDeElementos/1.0' }
            });

            if (response.ok) {
                const datos = await response.json();
                if (datos && datos.length > 0) {
                    return datos[0];
                }
            }
        } catch (error) {
            console.error(`Error buscando por ${propiedad}:`, error);
        }
        return null;
    }

}