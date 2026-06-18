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

    export function MostrarFrameOpenStreetView(panel: HTMLDivElement, pais: string, provincia: string, municipio: string, zona: string, tipoDeVia: string, calle: string, cp: string) {
        const params = new URLSearchParams();
        const direccionCompleta = ComponerDireccion(tipoDeVia, calle, zona, municipio, provincia, cp, pais);
        params.append('q', direccionCompleta);
        params.append('format', 'html');
        params.append('limit', '1');
        params.append('addressdetails', '1');
        params.append('zoom', '17');
        const link = `https://nominatim.openstreetmap.org/ui/search.html?${params.toString()}`;
        RenderizarIframe(panel, link);
    }

    function ComponerDireccion(tipoDeVia: string, calle: string, zona: string, municipio: string, provincia: string, cp: string, pais: string): string {
        const partesDireccion = [
            `${tipoDeVia} ${calle}`,
            zona,
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

    export async function MostrarFrameStreetViewPorTexto(panel: HTMLDivElement, texto: string, deltaIn: number): Promise<void> {
        try {
            const params = new URLSearchParams({ q: texto, format: 'json', limit: '1' });
            const response = await fetch(`https://nominatim.openstreetmap.org/search?${params.toString()}`, {
                headers: { 'Accept-Language': 'es' }
            });
            if (response.ok) {
                const datos = await response.json();
                if (datos && datos.length > 0) {
                    const lat = parseFloat(datos[0].lat);
                    const lon = parseFloat(datos[0].lon);
                    const delta = deltaIn;
                    const bbox = `${lon - delta},${lat - delta},${lon + delta},${lat + delta}`;
                    RenderizarIframe(panel, `https://www.openstreetmap.org/export/embed.html?bbox=${bbox}&layer=mapnik&marker=${lat},${lon}`);
                    AjustarAlturaIframe(panel);
                    return;
                }
            }
        } catch { }
        // Fallback: España completa
        RenderizarIframe(panel, `https://www.openstreetmap.org/export/embed.html?bbox=-9.5,35.8,4.5,43.9&layer=mapnik`);
        AjustarAlturaIframe(panel);
    }


}