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

}