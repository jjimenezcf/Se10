using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Negocio;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeSociedades : DescriptorDeCrud<SociedadDto>
    {
        public DescriptorDeSociedades(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDeSociedades(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
              , controlador: nameof(SociedadesController)
              , vista: nameof(SociedadesController.CrudSociedades)
              , modo: modo
              , rutaBase: enumNameSpaceTs.Terceros)
        {
            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), "Sociedad", "Buscar por nif, apellido, nombre, mail, teléfono");
            Mnt.Filtro.FiltroDeNombre.CambiarPropiedad(nameof(SociedadDto.Expresion));
            RecolocarControl(Mnt.Filtro.FiltroPorBaja, new Posicion(0, 1));
            DefinirFiltrosEnModal();
            DefinirMfMnt(menuIndividual, Mnt.OpcionesPorElemento);
            DefinirMfEdt(menuEdicion, Editor.OpcionesMf);
            DescriptorDeParametrosDeMiSociedad();
            DescriptorDeCuentasDeMiSociedad();
            DescriptorDeTarjetasDeMiSociedad();
            DescriptorDeFacturador();
            DescriptorDeBuzones();
            Editor.Expanes.Insert(5, DescriptorDeExpansorContactos(Editor));

            DescriptorDeDireccion(Ampliaciones.Sociedad.DireccionAlCrear);
            Mnt.OrdenacionInicial = $"{nameof(SociedadDto.Nombre)}:{nameof(ElementoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
        }

        private void DefinirFiltrosEnModal()
        {
            var modal = new ModalDeFiltrado<SociedadDto>(Mnt.Filtro, "filtros-de-sociedades", "Filtrar sociedades", "Seleccione que sociedades mostrar");
            Mnt.Filtro.Modales.Add(modal);

            modal.ControlesDeFiltrado.Add(new CheckFiltro<SociedadDto>(modal,
                etiqueta: "Es interlocutor",
                filtrarPor: nameof(SociedadDto.EsInterlocutor),
                ayuda: "Sólo las sociedades que son interlocutoras",
                valorInicial: false,
                filtrarPorFalse: false,
                posicion: new Posicion(0, 1)));

            modal.ControlesDeFiltrado.Add(new CheckFiltro<SociedadDto>(modal,
                etiqueta: "Con contactos",
                filtrarPor: nameof(ltrDeSociedad.ConContactos),
                ayuda: "Sólo las sociedades con contactos",
                valorInicial: false,
                filtrarPorFalse: false,
                posicion: new Posicion(1, 1)));

            modal.ControlesDeFiltrado.Add(new CheckFiltro<SociedadDto>(modal,
                etiqueta: "Con centros gestores",
                filtrarPor: nameof(ltrDeSociedad.FiltoPorCg),
                ayuda: "Sólo las sociedades con CG",
                valorInicial: false,
                filtrarPorFalse: false,
                posicion: new Posicion(1, 2)));

            modal.ControlesDeFiltrado.Add(new CheckFiltro<SociedadDto>(modal,
                etiqueta: "Con certificado",
                filtrarPor: nameof(ltrDeSociedad.ConCertificado),
                ayuda: "Sólo las sociedades con certificados",
                valorInicial: false,
                filtrarPorFalse: false,
                posicion: new Posicion(1, 2)));
        }

        private void DefinirMfMnt(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.1' accion-menu='{eventosDeMf.CentrosGestores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Centros Gestores</li>");
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.3' accion-menu='{eventosDeMf.Interlocutores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Gestionar interlocutores</li>");
        }

        private void DefinirMfEdt(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.1' accion-menu='{eventosDeMf.CentrosGestores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Centros Gestores</li>");
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.3' accion-menu='{eventosDeMf.Interlocutores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Gestionar interlocutores</li>");
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.5' accion-menu='{eventosDeMf.Soc_CuentasBancarias}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Cuentas bancarias</li>");
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.6' accion-menu='{eventosDeMf.Soc_TarjetasBancarias}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Tarjetas bancarias</li>");
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.7' accion-menu='{eventosDeMf.Soc_facturador}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador, false)}>Facturador</li>");

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CertificadoDeUnaSociedadDto), eventosDeMf.AsociarCertificado, "Certificado de la sociedad"));
            DescriptorDeEdicion<SociedadDto>.IncluirMfIndividual(opciones, $"<li id='{menuIndividual}.{eventosDeMf.AsociarCertificado}' accion-menu='{eventosDeMf.AsociarCertificado}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador, false)}>Asociar certificado</li>");

            var idsDeUsuarios = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar, valorPorDefecto: Literal.Cero).Valor.ToLista<int>();
            if (idsDeUsuarios.Contains(Contexto.DatosDeConexion.IdUsuario))
            {
                Editor.IncluirMfIndividual("Crear lote terceros", eventosDeMf.Spr_LoteDeTerceros, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
            }
            if (Contexto.SePuedeParametrizar())
            {
                Editor.IncluirMfIndividual("Activar verifactu", eventosDeMf.Soc_ActivarVerifactu, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
                Editor.IncluirMfIndividual("Recomponer BlockChain", eventosDeMf.Soc_RecomponerBlockChain, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);

                if (ExtensorDeExpedientes.HayTiposJuridicos(Contexto))
                {
                    Editor.IncluirMfIndividual("Catálogos judiciales", eventosDeMf.Soc_CatalogosJudiciales, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
                }
            }
        }

        private DescriptorDeExpansor DescriptorDeExpansorContactos(DescriptorDeEdicion<SociedadDto> editor)
        {
            var expansor = new DescriptorDeExpansor(editor, $"{editor.Id}-contactos", "Contactos", mostrarPlegado: true, "Contactos de la sociedad");

            //Definimos el grid de detalles del cuerpo
            var columnasDeZonas = new DescriptorDeColumnas("contactos");
            columnasDeZonas.Add(titulo: "Contacto", propiedad: nameof(ContactoDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeZonas.Add(titulo: "Teléfono", propiedad: nameof(ContactoDto.Telefono), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeZonas.Add(titulo: "e-Mail", propiedad: nameof(ContactoDto.eMail), alineacion: enumAliniacion.izquierda, mostrar: true);
            //columnasDeZonas.Add(titulo: "Baja", propiedad: nameof(ContactoDto.EstaDeBaja), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeZonas.Add(titulo: "Es Interlocutor", propiedad: nameof(ContactoDto.EsInterlocutor), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnasDeZonas.Add(titulo: "Id", propiedad: nameof(ContactoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(ContactosController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,ContactoDtm, ContactoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ContactoDto.IdElemento) }
              , { nameof(GridDeRelacion.IdNegocio), NegociosDeSe.IdNegocio(enumNegocio.Contacto) }
            };

            var grid = new GridDeRelacion(expansor, columnasDeZonas, parametros);
            grid.acciones.Add(new ColumnaAccion { accion = Referencia.DarDeAlta(expansor), titulo = "Dar de alta", tamano = 130, visible = true });

            expansor.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(ContactoDto), typeof(ContactosController), nameof(ContactoDto.IdElemento), "Añadir contacto");
            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(editor.Crud.Contexto, typeof(ContactoDto), typeof(ContactosController), "Editar el contacto", false);
            modalDeEdicion.AccionTrasAbrirModal =
                $"javascript:{RutaBase}.{enumGestorDeEventos.EventosDeSociedad}('{eventosDeSociedad.EditarContacto}','{grid.idModalParaEditar.ToLower()}');";
            return expansor;
        }

        private void DescriptorDeCuentasDeMiSociedad()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-CuentasDeMiSociedad", "Cuentas bancarias", true, "Cuentas bancarias de la sociedad");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("CuentasDeMiSociedad");
            columnas.Add(titulo: "Banco", propiedad: nameof(CuentaDeMiSociedadDto.Banco), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Alias", propiedad: nameof(CuentaDeMiSociedadDto.Alias), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Cuenta", propiedad: nameof(CuentaDeMiSociedadDto.Cuenta), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Clase", propiedad: nameof(CuentaDeMiSociedadDto.Clase), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Activa", propiedad: nameof(CuentaDeMiSociedadDto.Activa), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(CuentaDeMiSociedadDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(CuentaDeMiSociedadDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(CuentaDeMiSociedadDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CuentasDeMiSociedadController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(CuentasDeMiSociedadController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CuentaDeMiSociedadDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(CuentaDeMiSociedadDto), typeof(CuentasDeMiSociedadController), nameof(CuentaDeMiSociedadDto.IdElemento), "Añadir cuenta");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Sociedad_InicializarModalParaCrearCuentas}('{modalDeCreacion.IdHtml}')";
            modalDeCreacion.AccionTrasCrear = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Sociedad_RecargarGridDeArchivos}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(CuentaDeMiSociedadDto), typeof(CuentasDeMiSociedadController), "Consultar cuenta", soloConsulta: false);
            modalDeEdicion.AccionTrasModificar = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Sociedad_RecargarGridDeArchivos}()";
        }


        private void DescriptorDeTarjetasDeMiSociedad()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-TarjetasDeMiSociedad", "Tarjetas bancarias", true, "Tarjetas bancarias de la sociedad");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(1, expansor);

            var columnas = new DescriptorDeColumnas("TarjetasDeMiSociedad");
            columnas.Add(titulo: "Alias", propiedad: nameof(TarjetaDeMiSociedadDto.Alias), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Numero", propiedad: nameof(TarjetaDeMiSociedadDto.Numero), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Cuenta", propiedad: nameof(TarjetaDeMiSociedadDto.CuentaDeCargo), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Clase", propiedad: nameof(TarjetaDeMiSociedadDto.Clase), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Modo", propiedad: nameof(TarjetaDeMiSociedadDto.Modo), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Activa", propiedad: nameof(TarjetaDeMiSociedadDto.Activa), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(TarjetaDeMiSociedadDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(TarjetaDeMiSociedadDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(TarjetaDeMiSociedadDto.Alias)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(TarjetasDeMiSociedadController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(TarjetasDeMiSociedadController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(TarjetaDeMiSociedadDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(TarjetaDeMiSociedadDto), typeof(TarjetasDeMiSociedadController), nameof(TarjetaDeMiSociedadDto.IdElemento), "Añadir tarjeta");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Sociedad_InicializarModalParaCrearTarjetas}('{modalDeCreacion.IdHtml}')";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(TarjetaDeMiSociedadDto), typeof(TarjetasDeMiSociedadController), "Consultar tarjeta", soloConsulta: false);
        }

        private void DescriptorDeFacturador()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-Facturador", "Datos de facturacion", true, "Información para la facturación de sociedades o series externas");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(1, expansor);

            var columnas = new DescriptorDeColumnas("FacturadorDeSociedad");
            columnas.Add(titulo: "Centro Gestor", propiedad: nameof(FacturadorDeSociedadDto.Cg), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Tipo", propiedad: nameof(FacturadorDeSociedadDto.TipoDeFactura), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "ApiKey", propiedad: nameof(FacturadorDeSociedadDto.Apikey), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Activa", propiedad: nameof(FacturadorDeSociedadDto.Activa), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(FacturadorDeSociedadDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(FacturadorDeSociedadDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(FacturadorDeSociedadesController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(FacturadorDeSociedadesController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(FacturadorDeSociedadDto.IdElemento) }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(FacturadorDeSociedadDto), typeof(FacturadorDeSociedadesController), nameof(FacturadorDeSociedadDto.IdElemento), "Añadir facturador");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Sociedad_InicializarModalParaFacturador}('{modalDeCreacion.IdHtml}')";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(FacturadorDeSociedadDto), typeof(FacturadorDeSociedadesController), "Consultar facturador", soloConsulta: false);
        }

        private void DescriptorDeBuzones()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-buzones", "Buzones", true, "Buzones de la sociedad");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(2, expansor);

            var columnas = new DescriptorDeColumnas("buzones");
            columnas.Add(titulo: "eMail", alineacion: enumAliniacion.derecha, tamano: 250, mostrar: true);
            columnas.Add(titulo: "Buzón", propiedad: nameof(BuzonDeMiSociedadDto.Buzon), tamano: 200, alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Permiso", nameof(BuzonDeMiSociedadDto.Permiso), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "IdElemento", mostrar: false);
            columnas.Add(titulo: "IdPermiso", mostrar: false);
            columnas.Add(titulo: "Id", mostrar: false);

            var orden = $"{nameof(BuzonDeMiSociedadDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(BuzonesDeMiSociedadController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BuzonesDeMiSociedadController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(BuzonDeMiSociedadDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(BuzonDeMiSociedadDto), typeof(BuzonesDeMiSociedadController), nameof(BuzonDeMiSociedadDto.IdElemento), "Añadir buzón");
            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(BuzonDeMiSociedadDto), typeof(BuzonesDeMiSociedadController), "Consultar buzón", soloConsulta: false);
        }

        private void DescriptorDeParametrosDeMiSociedad()
        {
            var parametros = new AmpliacionDeEdicion(Editor, Ampliaciones.Sociedad.Parametros, "Parámetros", new Dimension(2, 2), ayuda: "Parámetros de la sociedad");
            parametros.Dto = typeof(ParametrosDeMiSociedadDto);
            parametros.Controlador = nameof(ParametrosDeMiSociedadController);
            Editor.Ampliaciones.Add(parametros);
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;


            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                      $@"<script src=¨../../js/{RutaBase}/Sociedades.js?v={System.DateTime.Now.Ticks}¨></script>
                         <script src=¨../../js/{RutaBase}/Interlocutores.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeSociedades('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                     ";
            ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
            return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
        }

    }
}

/*
 * 
                    
*/
