using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DesarrolloFTP
{
    class ConexionBd
    {
        public DataTable vDataTable { get; set; }
        public int Codigo { get; set; }
        public string Mensaje { get; set; }
        public DataTable DatosDocumento { get; set; }

        String Query = "";
        

        public DataTable ConsultarSolicitudes(string CadenaConexion, DateTime FechaActual)
        {
            Codigo = 1;
            Mensaje = "Exitoso";
            string Procedimiento = "dbo.PruebaPoliza";

            vDataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(CadenaConexion))
            {
                try
                {
                    SqlCommand command = new SqlCommand(Procedimiento, connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Fecha", FechaActual);
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    da.Fill(vDataTable);
                }
                catch (Exception e)
                {
                    Codigo = 0;
                    Mensaje = e.Message;
                }
                connection.Close();
            }

            return vDataTable;
        }

        public DataTable ConsultarDocumentos(string CadenaConexion, string IdSolicitud)
        {
            Codigo = 1;
            Mensaje = "Exitoso";

            Query = "SELECT	TOP 1 D.IdTransaccion, D.IdCartridge, C.Ruta + CHAR(92) AS Ruta "
                    + "FROM	Documento AS D "
                    + "INNER JOIN Cartridge AS C ON D.IdCartridge = C.IdCartridge "
                    + "WHERE	IdTipoDocumento = 396 AND D.IdTransaccion = @IdSolicitud "
                    + "ORDER BY D.FechaModificacion DESC";

            DatosDocumento = new DataTable();

            using (SqlConnection connection = new SqlConnection(CadenaConexion))
            {
                try
                {
                    SqlCommand command = new SqlCommand(Query, connection);
                    command.Parameters.AddWithValue("@IdSolicitud", IdSolicitud);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    DatosDocumento.Load(reader);
                    reader.Close();
                }
                catch (Exception e)
                {
                    Codigo = 0;
                    Mensaje = e.Message;
                }
                connection.Close();
            }

            return DatosDocumento;
        }

        public DataTable ConsultarRutaArchivo(string CadenaConexion, string IdSolicitud)
        {
            string BaseDatosDocumentos = ConfigurationManager.AppSettings["BaseDatosDocumentos"]; 
            string IdTipoDocumento = ConfigurationManager.AppSettings["IdTipoDocumento"]; 
            Codigo = 1;
            Mensaje = "Exitoso";
            string Procedimiento = "dbo.spS_RutaDocumento";

            DatosDocumento = new DataTable();

            using (SqlConnection connection = new SqlConnection(CadenaConexion))
            {
                try
                {
                    SqlCommand command = new SqlCommand(Procedimiento, connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdSolicitud", IdSolicitud);
                    command.Parameters.AddWithValue("@TipoDocumento", IdTipoDocumento);
                    command.Parameters.AddWithValue("@NombreBaseDocs", BaseDatosDocumentos);
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    da.Fill(DatosDocumento);
                }
                catch (Exception e)
                {
                    Codigo = 0;
                    Mensaje = e.Message;
                }
                connection.Close();
            }

            return DatosDocumento;
        }


    }
}
