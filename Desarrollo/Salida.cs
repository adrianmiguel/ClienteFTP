using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using Security.Bpm.Ws;
using System.Drawing;

namespace DesarrolloFTP
{
    class Salida
    {       
        public static string cadenaConexionBDMantiz { get; set; }
        public static string cadenaConexionBDDocumentos { get; set; }

        public Salida()
        {
            string userId = Cryptography.Decrypt(ConfigurationManager.AppSettings.Get("userId"), "BancoBogota");          //Usuario
            string key = Cryptography.Decrypt(ConfigurationManager.AppSettings.Get("key"), "BancoBogota");                //Clave
            string instancia = Cryptography.Decrypt(ConfigurationManager.AppSettings.Get("instancia"), "BancoBogota");    //Instancia SQL
            string dbMantiz = Cryptography.Decrypt(ConfigurationManager.AppSettings.Get("db"), "BancoBogota");            //Nombre Base de Datos
            cadenaConexionBDMantiz = "Server=" + instancia + ";Database=" + dbMantiz + ";User Id=" + userId + ";Password=" + key + ";";

            string ServerDocumentos = Cryptography.Decrypt(ConfigurationManager.AppSettings.Get("ServerDocumentos"), "BancoBogota");
            string DataBaseDocumentos = ConfigurationManager.AppSettings.Get("BaseDatosDocumentos2");
            string UserDocumentos = Cryptography.Decrypt(ConfigurationManager.AppSettings.Get("UsuarioBDDocumentos"), "BancoBogota");
            string PasswordDocumentos = Cryptography.Decrypt(ConfigurationManager.AppSettings.Get("PasswordDocumentos"), "BancoBogota");

            cadenaConexionBDDocumentos = "Data Source=" + ServerDocumentos + ";User ID=" + UserDocumentos + ";Password=" + PasswordDocumentos +
                                    ";Initial Catalog=" + DataBaseDocumentos + ";";
        }

        public void CreacionCarpeta()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string RutaSalidas_BB = appSettings["Ruta_Salidas_BB"];

            DateTime FechaActual = DateTime.Now;

            string Dia = FechaActual.ToString("dd");
            string Mes = FechaActual.ToString("MM");
            string Anio = FechaActual.ToString("yyyy");

            string NuevaCarpeta = Path.Combine(RutaSalidas_BB, Dia + Mes + Anio);

            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(NuevaCarpeta))
                {
                    Console.WriteLine("That path exists already.");
                    return;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(NuevaCarpeta);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(NuevaCarpeta));

                //// Delete the directory.
                //di.Delete();
                //Console.WriteLine("The directory was deleted successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        public void CreacionExcel()
        {
            var appSettings = ConfigurationManager.AppSettings;

            string RutaSalidas_BB = appSettings["Ruta_Salidas_BB"];
            string NombreHojaExcel = appSettings["NombreHojaExcel"];
            DataTable dt = new DataTable();
            DataTable dtDocumento = new DataTable();

            DateTime FechaActual = DateTime.Now;

            string Dia = FechaActual.ToString("dd");
            string Mes = FechaActual.ToString("MM");
            string Anio = FechaActual.ToString("yyyy");
            string Ruta_Archivo = Path.Combine(RutaSalidas_BB, "Poliza_Inc_" + Dia + Mes + Anio + ".xlsx");

            ConexionBd solicitudes = new ConexionBd();
            dt = solicitudes.ConsultarSolicitudes(cadenaConexionBDMantiz, FechaActual);            

            dt.TableName = "Poliza";
           
            dt.Columns.Add("Numero POLIZA");
            dt.Columns.Add("ASEGURADO");
            dt.Columns.Add("Riesgo");
            dt.Columns.Add("Nombre Arhivo Poliza");

            int inColumn = 0, inRow = 0;

            System.Reflection.Missing Default = System.Reflection.Missing.Value;
            //string strPath = Path.Combine(RutaSalidas_BB, "5.xlsx");

            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook excelWorkBook = excelApp.Workbooks.Add(1);

            //Create Excel WorkSheet
            Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add(Default, excelWorkBook.Sheets[excelWorkBook.Sheets.Count], 1, Default);
           // excelWorkSheet.Name = dt.TableName;//"Poliza";//Name worksheet

            //Write Column Name
            for (int i = 0; i < dt.Columns.Count; i++)
                excelWorkSheet.Cells[1, i + 1] = dt.Columns[i].ColumnName;//.ToUpper();

            //Write Rows
            for (int m = 0; m < dt.Rows.Count; m++)
            {
                for (int n = 0; n < dt.Columns.Count; n++)
                {
                    inColumn = n + 1;
                    inRow = 2 + m;//1 + 2 + m;
                    string Dato = dt.Rows[m].ItemArray[n].ToString();
                    excelWorkSheet.Cells[inRow, inColumn] = Dato;
                    if (n == 16)
                    {
                        dtDocumento = solicitudes.ConsultarRutaArchivo(cadenaConexionBDDocumentos, Dato);
                        string Ruta = dtDocumento.Rows[0]["RutaArchivo"].ToString();

                        string NombreArchivo = Path.GetFileName(Ruta);
                        string NombreArchivoSinExtension = Path.GetFileNameWithoutExtension(Ruta);
                        string ExtensionArchivo = Path.GetExtension(Ruta);

                        string RutaDestinoFile = Path.Combine(RutaSalidas_BB, NombreArchivo);

                        File.Copy(Ruta, RutaDestinoFile);
                    }
                    if (m % 2 == 0)
                        excelWorkSheet.get_Range("A" + inRow.ToString(), "Z" + inRow.ToString()).Interior.Color = System.Drawing.ColorTranslator.FromHtml("#D6EAF8");
                }
            }

            //Style table column names
            Excel.Range cellRang = excelWorkSheet.get_Range("A1", "W1");
            cellRang.Font.Bold = true;
            cellRang.Font.Color = ColorTranslator.ToOle(Color.White);
            cellRang.Interior.Color = ColorTranslator.FromHtml("#022C4D");
            cellRang.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

            cellRang = excelWorkSheet.get_Range("X1", "Z1");
            cellRang.Font.Bold = true;
            cellRang.Font.Color = ColorTranslator.ToOle(System.Drawing.Color.White);
            cellRang.Interior.Color = ColorTranslator.FromHtml("#69C0BC");

            excelWorkSheet.get_Range("F4").EntireColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            //Formate price column
            excelWorkSheet.get_Range("O2").EntireColumn.NumberFormat = "$#,##0.00_);[Red]($#,##0.00)"; //.NumberFormat = "0.00";
            excelWorkSheet.get_Range("O2").EntireColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            //Auto fit columns
            excelWorkSheet.Columns.AutoFit();

            //Delete First Page
            excelApp.DisplayAlerts = false;
            Microsoft.Office.Interop.Excel.Worksheet lastWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)excelWorkBook.Worksheets[1];
            lastWorkSheet.Delete();
            excelApp.DisplayAlerts = true;

            //Set Defualt Page
            (excelWorkBook.Sheets[1] as Excel._Worksheet).Activate();

            excelWorkBook.SaveAs(Ruta_Archivo, Default, Default, Default, false, Default, Excel.XlSaveAsAccessMode.xlNoChange, Default, Default, Default, Default, Default);
            excelWorkBook.Close();
            excelApp.Quit();        
        }

        public void BuscarArchivosMantiz()
        {

        }
    }
}
