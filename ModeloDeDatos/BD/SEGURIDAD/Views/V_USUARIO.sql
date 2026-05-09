create view SEGURIDAD.V_USUARIO as
                                    select
                                       ID,
                                       LOGIN,
                                       APELLIDO,
                                       NOMBRE
                                    from
                                       ENTORNO.USUARIO