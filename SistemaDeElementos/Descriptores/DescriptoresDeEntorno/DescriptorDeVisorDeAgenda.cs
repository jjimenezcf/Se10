using System;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDelVisorDeAgenda : DescriptorDeCrud<EventoDeAgendaDto>
    {
        public DescriptorDelVisorDeAgenda(ContextoSe contexto, ModoDescriptor modo, DateTime fecha)
        : base(contexto: contexto, nameof(VisorDeAgendaController),nameof(VisorDeAgendaController.VisorDeAgenda),modo, enumNameSpaceTs.Entorno, enumNegocio.EventoDeAgenda)
        {
            Fecha = fecha;
        }

        public DateTime Fecha { get; }

        public override string RenderControl()
       {
            var ano = Fecha.Year;
            var mes = Fecha.Month -1;
            var dia = Fecha.Day;

            //<script src=¨../Fuentes/dhtmlxScheduler/dhtmlxscheduler.js¨></script> 
            //https://cdn.dhtmlx.com/scheduler/edge/dhtmlxscheduler.js
            //https://cdn.dhtmlx.com/scheduler/edge/dhtmlxscheduler_material.css
            var render =
                      $@"
                      <link href=¨../Fuentes/dhtmlxScheduler/dhtmlxscheduler_material_v_6.css¨ rel=¨stylesheet¨ type=¨text/css¨ />
                      <script src=¨../Fuentes/dhtmlxScheduler/dhtmlxscheduler_v_6.js¨></script> 
                      <script src=¨../Fuentes/dhtmlxScheduler/locale/locale.js¨ charset=¨utf-8¨></script>
                      <script src=¨../js/Entorno/Agendas.js¨ charset=¨utf-8¨></script>
                      <script>
                          document.addEventListener(¨DOMContentLoaded¨, function(event) {{
 
                              // Solo lectura y permito la serializacion para el formato iCal
                              scheduler.plugins({{readonly: true }});
                              scheduler.plugins({{serialize: true}});


                              //scheduler.config.readonly = true;
                              scheduler.config.readonly_form = true;

		                      scheduler.config.lightbox.sections = [
		                      	{{name: ¨description¨, height:32, map_to: ¨text¨, type: ¨textarea¨, focus: true}},
		                      	{{name: ¨time¨, height: 32, type: ¨time¨, map_to: ¨auto¨}}
		                      ];
                              
                             
                              scheduler.attachEvent(¨onLightbox¨, function(){{var section = scheduler.formSection(¨description¨); section.control.disabled = true;}});
                              
                              scheduler.attachEvent(¨onBeforeDrag¨,function(){{return false;}})
		                      scheduler.attachEvent(¨onClick¨,function(){{return false;}})
		                      scheduler.config.details_on_dblclick = true;
		                      scheduler.config.dblclick_create = false;

                              scheduler.init(¨scheduler_here¨, new Date({ano},{mes},{dia}), ¨week¨);  
                              scheduler.setLoadMode(¨week¨);

                      
                              // initiating data loading
                              scheduler.load(¨/VisorDeAgenda/LeerEventos¨, Entorno.DespuesDeMostrarAgenda());
                              scheduler.attachEvent(¨onLoadEnd¨, function() {{Entorno.DespuesDeMapearEventos();}});

                              scheduler.attachEvent(¨onViewChange¨, function (new_mode , new_date){{Entorno.DespuesDeMapearEventos(); 
                              }});

                              scheduler.attachEvent(¨onDblClick¨, function (id, e){{
                                  //any custom logic here
                                  return false;
                              }})

                              scheduler.attachEvent(¨onAfterSchedulerResize¨, function(){{
                                 Entorno.DespuesDeMapearEventos();
                              }});

                              // initializing dataProcessor
                              //var dp = scheduler.createDataProcessor(¨/VisorDeAgenda/LeerEventos¨);
                              // and attaching it to scheduler
                              //dp.init(scheduler);
                              // setting the REST mode for dataProcessor
                              //dp.setTransactionMode(¨REST¨);
                              //
                              let shedulerHere = document.getElementById('scheduler_here');
                              shedulerHere.style.height = 'inherit';
                          }});
                      </script>
       
                      <div id=¨scheduler_here¨ class=¨dhx_cal_container¨ style='width:100%; height:100vh;'>
                            <div class=¨dhx_cal_navline¨>
                                <div class=¨dhx_cal_prev_button¨>&nbsp;</div>
                                <div class=¨dhx_cal_next_button¨>&nbsp;</div>
                                <div class=¨dhx_cal_today_button¨></div>
                                <div class=¨dhx_cal_date¨></div>
                                <div class=¨dhx_cal_tab¨ name=¨day_tab¨></div>
                                <div class=¨dhx_cal_tab¨ name=¨week_tab¨></div>
                                <div class=¨dhx_cal_tab¨ name=¨month_tab¨></div>
                            </div>
                            <div class=¨dhx_cal_header¨></div>
                            <div class=¨dhx_cal_data¨></div>
                      </div>
                       ";
       
           return PanelDeControl.RenderPagina(Contexto, render.Render(), "cuerpo-de-agenda");
       }
    }

}
