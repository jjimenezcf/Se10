
               Create FUNCTION [SEGURIDAD].OBTENER_ROLES_DE_UN_PUESTO (
               	@idPuesto int
               )
               RETURNS VarChar(max)
               AS
               begin
               Declare @rol varchar(max)
               Declare @resultado varchar(max)
               DECLARE c CURSOR FOR select NOMBRE from SEGURIDAD.ROL_PUESTO t1
                                    inner join SEGURIDAD.ROL t2 on t2.ID = t1.IDROL
                                    where t1.IDPUESTO = @idPuesto
               
		       set @resultado = ''
               OPEN c
               FETCH NEXT FROM c INTO @rol
               WHILE @@fetch_status = 0
               BEGIN
			       if @resultado = ''  
                      set  @resultado = @rol 
			   	else 
                      set  @resultado = @resultado + ' - ' + @rol 
                   FETCH NEXT FROM c INTO @rol
               END
               CLOSE c
               DEALLOCATE c
               
               return @resultado
               
               END
               
            
