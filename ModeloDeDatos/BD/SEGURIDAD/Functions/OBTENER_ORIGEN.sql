
            CREATE FUNCTION [SEGURIDAD].[OBTENER_ORIGEN] (
            	@idUsuario int,
            	@idPermiso int
            )
            RETURNS VarChar(max)
            AS
            begin
            Declare @origen varchar(max)
            Declare @resultado varchar(max)
            DECLARE c CURSOR FOR select top(1) ORIGEN
                    from 
                    (
                    SELECT t4.NOMBRE + ' (Rol: ' + t6.NOMBRE + ')' as ORIGEN
                    from SEGURIDAD.USU_PUESTO t2
                    inner join SEGURIDAD.ROL_PUESTO T3 ON T3.IDPUESTO = T2.IDPUESTO
                    inner join SEGURIDAD.PUESTO t4 on t4.id = t3.IDPUESTO
                    INNER JOIN SEGURIDAD.ROL_PERMISO T5 ON T5.IDROL = T3.IDROL
                    inner join SEGURIDAD.ROL t6 on t6.ID = t5.IDROL
                    where t2.IDUSUA = @idUsuario and t5.IDPERMISO = @idPermiso
                    union
                    SELECT t4.NOMBRE + ' (Directo)' as ORIGEN
                    from SEGURIDAD.USU_PUESTO t2
                    inner join SEGURIDAD.PERMISO_DIRECTOS T3 ON T3.IDPUESTO = T2.IDPUESTO
                    inner join SEGURIDAD.PUESTO t4 on t4.id = t3.IDPUESTO
                    where t2.IDUSUA = @idUsuario and t3.IDPERMISO = @idPermiso
                    ) t1

            set @resultado = ''
            OPEN c
            FETCH NEXT FROM c INTO @origen
            WHILE @@fetch_status = 0
            BEGIN
			    if @resultado = ''  
                   set  @resultado = @origen 
				else 
                   set  @resultado = @resultado + ' - ' + @origen 
                FETCH NEXT FROM c INTO @origen
            END
            CLOSE c
            DEALLOCATE c
            
            return @resultado
            
            END

