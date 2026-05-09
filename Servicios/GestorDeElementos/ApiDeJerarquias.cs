using System;
using System.Collections.Generic;
using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;

namespace GestorDeElementos
{
    public static class ApiDeJerarquias
    {
        public static JerarquiaDto EstructurarJerarquica(enumNegocio negocio, List<NodoDtm> nodosLeidosDtm, Type tipoDto)
        {
            var jerarquia = new JerarquiaDto();
            foreach (NodoDtm nodoDtm in nodosLeidosDtm)
            {
                if (nodoDtm.IdPadre != null) continue;
                var nodo = ApilarRaiz(negocio, jerarquia, nodoDtm, tipoDto.FullName);
                Hijos(nodo, nodosLeidosDtm);
            }
            return jerarquia;
        }

        public static JerarquiaDto Raices(enumNegocio negocio, List<NodoDtm> nodosLeidosDtm)
        {
            var jerarquia = new JerarquiaDto();
            foreach (NodoDtm nodoDtm in nodosLeidosDtm)
            {
                if (nodoDtm.TipoDtm == negocio.TipoDtm().FullName)
                {
                    NodoDeJerarquiaDto padre = new NodoDeJerarquiaDto(new NodoDto(nodoDtm, negocio.ToNombre(), negocio.TipoDto().FullName, nodoDtm.modoAcceso));
                    jerarquia.Ramas.Add(padre);
                }
            }
            return jerarquia;
        }

        public static void ApilarNodosComoJerarquiaEnRaiz(enumNegocio negocio, NodoDeJerarquiaDto raiz, List<NodoDtm> nodosDtm)
        {
            foreach(var nodoDtm in nodosDtm)
            {
                if (nodoDtm.IdPadre == null)
                {
                    NodoDto nodoDto = new NodoDto(nodoDtm,negocio.ToNombre(), negocio.TipoDto().FullName, raiz.Dto.ModoAcceso);
                    NodoDeJerarquiaDto hijo = new NodoDeJerarquiaDto(nodoDto);
                    raiz.Hijos.Add(hijo);
                    Hijos(hijo, nodosDtm);
                }
            }
        }

        private static void Hijos(NodoDeJerarquiaDto nodoPadre, List<NodoDtm> nodosLeidosDtm)
        {
            foreach (NodoDtm nodoDtm in nodosLeidosDtm)
            {
                if (nodoDtm.IdPadre != nodoPadre.Dto.Id)
                    continue;

                NodoDto nodoDto = new NodoDto(nodoDtm, nodoPadre.Dto.Negocio, nodoPadre.Dto.TipoDto, nodoDtm.modoAcceso);
                var nodoHijo = new NodoDeJerarquiaDto(nodoDto);
                nodoPadre.Hijos.Add(nodoHijo);
                Hijos(nodoHijo, nodosLeidosDtm);
            }
        }

        public  static JerarquiaDto EstructuraPlana(enumNegocio negocio, List<NodoDtm> nodosLeidosDtm, Type tipoDto)
        {
            var jerarquia = new JerarquiaDto();
            var leidos = nodosLeidosDtm.Count;
            do
            {
                for (int i = 0; i < nodosLeidosDtm.Count; i++)
                {
                    var nodoDtm = nodosLeidosDtm[i];
                    if (ExisteLaRaiz(jerarquia, nodoDtm.Id))
                        continue;
                    var nodoRaiz = ApilarRaiz(negocio, jerarquia, nodoDtm, tipoDto.FullName);
                    ApilarHijos(jerarquia, nodoRaiz, nodosLeidosDtm, tipoDto.FullName);
                }

                QuitarNodosApilados(nodosLeidosDtm, jerarquia, leidos);
            }
            while (jerarquia.Ramas.Count < leidos);
            return jerarquia;
        }

        /// <summary>
        /// Mira en una jerarquía si la raiz está ya añadida
        /// </summary>
        private static bool ExisteLaRaiz(JerarquiaDto jerarquia, int id)
        {
            foreach (var rama in jerarquia.Ramas)
                if (rama.Dto.Id == id)
                {
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Pone en una jerarquía el nodo pasado como una rama inicial
        /// </summary>
        private static NodoDeJerarquiaDto ApilarRaiz(enumNegocio negocio, JerarquiaDto jerarquia, NodoDtm nodoDtm, string tipoDto)
        {
            NodoDto nodoDto = new NodoDto(nodoDtm, negocio.ToNombre(), tipoDto, nodoDtm.modoAcceso);
            var nodoRaiz = new NodoDeJerarquiaDto(nodoDto);
            jerarquia.Ramas.Add(nodoRaiz);
            return nodoRaiz;
        }

       
        public static void ApilarNodosComoHijosDeLaRaiz(enumNegocio negocio, NodoDeJerarquiaDto padre, List<NodoDtm> nodosDtm)
        {
            foreach (var nodoDtm in nodosDtm)
                ApilarNodoDtm(negocio, padre, nodoDtm);
        }

        private static void ApilarNodoDtm(enumNegocio negocio, NodoDeJerarquiaDto padre, NodoDtm nodoDtm)
        {
            NodoDto nodoDto = new NodoDto(nodoDtm, negocio.ToNombre(), negocio.TipoDto().FullName, nodoDtm.modoAcceso);
            var nodoRaiz = new NodoDeJerarquiaDto(nodoDto);
            padre.Hijos.Add(nodoRaiz);
        }

        /// <summary>
        /// Recorre los nodo leidos, y de  ellos, los que dependen del nodo raiz y no están ya apilados, los apila como una rama nueva
        /// </summary>
        public static void ApilarHijos(JerarquiaDto jerarquia, NodoDeJerarquiaDto nodoRaiz, List<NodoDtm> nodosLeidosDtm, string tipoDto)
        {
            for (int i = 0; i < nodosLeidosDtm.Count; i++)
            {
                var nodoDtm = nodosLeidosDtm[i];
                if (nodoDtm.IdPadre != nodoRaiz.Dto.Id)
                    continue;

                if (ExisteLaRaiz(jerarquia, nodoDtm.Id))
                    continue;

                var nodo = ApilarRaiz(NegociosDeSe.ToEnumerado(nodoRaiz.Dto.Negocio), jerarquia, nodoDtm, tipoDto);
                ApilarHijos(jerarquia, nodo, nodosLeidosDtm, tipoDto);
            }
        }

        private static void QuitarNodosApilados(List<NodoDtm> nodosLeidosDtm, JerarquiaDto jerarquia, int leidos)
        {
            for (int i = leidos - 1; i >= 0; i--)
            {
                var id = nodosLeidosDtm[i].Id;
                foreach (var rama in jerarquia.Ramas)
                    if (rama.Dto.Id == id)
                    {
                        nodosLeidosDtm.RemoveAt(i);
                        break;
                    }
            }
        }

    }
}
