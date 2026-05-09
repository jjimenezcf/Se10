namespace Callejero {

    export const restrictor = {
        codigoPostal: "codigopostal",
        provincia: "idprovincia",
        municipio: "idmunicipio"
    };

    export const controlador = {
        provincia: ltrControladores.Callejero.Provincia,
        municipio: ltrControladores.Callejero.Municipio
    };

    export const atributo = {
        propiedad: {
            idpais: "idpais",
            idprovincia: "idprovincia",
            idmunicipio: "idmunicipio"
        },
        guardarEn: {
            idpais: "idpais",
            idprovincia: "idprovincia"
        }
    };

    export const objeto = {
        municipioDto: {
            idpais: "idpais",
            pais: "pais",
            idprovincia: "idprovincia",
            provincia: "provincia"
        },

        provinciaDto: {
            idpais: "idpais",
            pais: "pais"
        }
    };

}
