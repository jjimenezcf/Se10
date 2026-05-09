using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.SistemaDocumental;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers;

public class CarpetasController : JerarquiaController<ContextoSe>
{
    private readonly GestorDeCarpetas _gestorDeCarpetas;

    public CarpetasController(GestorDeCarpetas gestor, GestorDeErrores gestorDeErrores)
        : base(gestor.Contexto
            , gestor.Mapeador
            , gestorDeErrores)
    {
        _gestorDeCarpetas = gestor;
    }


    public IActionResult CrudCarpetas(int idarchivador)
    {
        try
        {
            return ViewFormulario(new DescriptorDeCarpetas(_gestorDeCarpetas, "carpeta", "Carpetas de un archivador", nameof(CarpetasController), nameof(CrudCarpetas), idarchivador));
        }
        catch (Exception e)
        {
            return RenderMensaje(GestorDeErrores.Mensaje(e));
        }
    }

    public override JsonResult epLeerJerarquia(string negocio, int idPadre, string filtrosJson)
    {
        return LeerJerarquia(negocio, idPadre, () => GestorDeCarpetas.LeerJerarquia(Contexto, idPadre, filtrosJson));
    }


    public JsonResult epCrearNodo(string negocio, string json)
    {
        var gestor = GestorDeCarpetas.Gestor(Contexto, Contexto.Mapeador);
        var carpetaDto = JsonConvert.DeserializeObject<CarpetaDto>(json);

        return PersistirElemento(negocio
            , enumTipoOperacion.Insertar.ToString()
            , () => gestor.PersistirElementoDto(carpetaDto, new ParametrosDeNegocio(enumTipoOperacion.Insertar))
        );
    }

    public JsonResult epPersistirNodo(string negocio, string json, string operacion)
    {
        var gestor = GestorDeCarpetas.Gestor(Contexto, Contexto.Mapeador);
        var carpetaDto = JsonConvert.DeserializeObject<CarpetaDto>(json);

        return PersistirElemento(negocio
            , operacion.ToTipoOperacion().ToString()
            , () => gestor.PersistirElementoDto(carpetaDto, new ParametrosDeNegocio(operacion.ToTipoOperacion()))
        );
    }

    protected override IEnumerable<CarpetaDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
    {
        var carpetas = ApiController.LeerElementos(_gestorDeCarpetas, posicion, cantidad, filtros, orden, opcionesDeMapeo);
        return carpetas;
    }

    public JsonResult epLeerNodoSeleccionado(string negocio, int id, string filtrosJson)
    {
        var parametros = new Dictionary<string, object> { { ltrParametrosDto.DescargarGestionDocumental, true } };
        return LeerNodoSeleccionado(negocio, id, () => _gestorDeCarpetas.LeerElementoPorId(id, parametros));
    }

    [HttpGet("movil/[controller]/archivador/{id:int}")]
    [Authorize]
    public JsonResult LeerCarpetasJerarquia([FromRoute] int id)
    {

        var filtros = new List<object>
        {
            new List<object> { "idarchivador", id},
            new List<object> { "mostrarJerarquia", true}
        };


        var filtrosJson = JsonConvert.SerializeObject(filtros);
        return LeerJerarquia(enumNegocio.Carpeta.ToString(), 0, () => GestorDeCarpetas.LeerJerarquia(Contexto, 0, filtrosJson));
    }

}