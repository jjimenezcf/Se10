

            CREATE FUNCTION SEGURIDAD.OBTENER_ORIGEN_PUESTO_PERMISO (
            	@idPuesto int,
            	@idPermiso int
            )
            RETURNS VarChar(max)
            AS
            begin
            Declare @origen varchar(max)
            Declare @resultado varchar(max)
            DECLARE c CURSOR FOR SELECT t2.NOMBRE
                      from SEGURIDAD.ROL_PUESTO T1
                      inner join seguridad.rol t2 on t2.id = t1.IDROL 
                      INNER JOIN SEGURIDAD.ROL_PERMISO T3 ON T3.IDROL = T2.ID
                      where t1.IDPUESTO = @idPuesto and t3.IDPERMISO = @idPermiso
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
