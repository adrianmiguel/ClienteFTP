using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace ClienteFTP
{
    class Conexion
    {
        Uri uri;
        FtpWebRequest clienteRequest;
        NetworkCredential credenciales;
        string Ip = "192.168.0.14";
        string Puerto = "2121";
        public void Conectarse()
        {           
            try
            {                            
                //uri = new Uri("ftp://" + Ip + ":" + Puerto);
                uri = new Uri("ftp://" + Ip + ":" + Puerto + "/RutaFtp");

                clienteRequest = (FtpWebRequest)WebRequest.Create(uri);

                credenciales = new NetworkCredential("adrian", "adrian9110");

                clienteRequest.Credentials = credenciales;
                clienteRequest.EnableSsl = false;
                clienteRequest.Method = WebRequestMethods.Ftp.ListDirectory;// .ListDirectoryDetails;
                //clienteRequest.Method = WebRequestMethods.Http.Get;
                clienteRequest.KeepAlive = true;
                clienteRequest.UsePassive = true;

                FtpWebResponse respuesta = (FtpWebResponse)clienteRequest.GetResponse();

                StreamReader sr = new StreamReader(respuesta.GetResponseStream(), Encoding.ASCII);

                string resultado = sr.ReadToEnd();
                string mensaje = respuesta.WelcomeMessage; 
                respuesta.Close();

                //List<Archivo> archivos = ObtieneLsita(resultado);

                //foreach (Archivo item in archivos)
                //{
                //    //string lista = item.nombre;
                //    Console.WriteLine(item.nombre);
                //    Console.ReadKey();
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void cmdDescarga()
        {
            //string archivoescarga = "IMG_20180217_193256.jpg";
            //uri = new Uri("ftp://" + Ip + ":" + Puerto + "/DCIM/Camera/IMG_20180217_193256.jpg");
            string archivodescarga = "9781540407306(1).pdf";
            uri = new Uri("ftp://" + Ip + ":" + Puerto + "/documents/9781540407306(www.ebook-dl.com)/9781540407306(1).pdf");
            clienteRequest = (FtpWebRequest)WebRequest.Create(uri);

            credenciales = new NetworkCredential("adrian", "adrian9110");
            clienteRequest.Credentials = credenciales;

            clienteRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            FtpWebResponse respuesta = (FtpWebResponse)clienteRequest.GetResponse();
            Stream s = respuesta.GetResponseStream();
            FileStream fs = new FileStream(@"C:\Users\adria\Documents\Visual Studio 2017\Projects\ClienteFTP\ArchivosDescargados\" + archivodescarga, FileMode.Create, FileAccess.Write);
            crearArhivo(s, fs);
        }

        public void cmdCargar()
        {
            string archivocarga = "Object_Oriented_Programming_C_Sharp_Succinctly.pdf";
            uri = new Uri("ftp://" + Ip + ":" + Puerto + "/documents/Object_Oriented_Programming_C_Sharp_Succinctly.pdf");
            clienteRequest = (FtpWebRequest)WebRequest.Create(uri);

            credenciales = new NetworkCredential("adrian", "adrian9110");
            clienteRequest.Credentials = credenciales;

            clienteRequest.Method = WebRequestMethods.Ftp.UploadFile;

            Stream destino = clienteRequest.GetRequestStream();
            FileStream origen = new FileStream(@"C:\Users\adria\Documents\Visual Studio 2017\Projects\ClienteFTP\ArchivosDescargados\" + archivocarga, FileMode.Open, FileAccess.Read);
            crearArhivo(origen, destino);
        }

        private void crearArhivo(Stream origen, Stream destino)
        {
            byte[] buffer = new byte[1024];
            int bytesLeidos = origen.Read(buffer, 0, 1024);
            while (bytesLeidos != 0)
            {
                destino.Write(buffer, 0, bytesLeidos);
                bytesLeidos = origen.Read(buffer, 0, 104);
            }
            origen.Close();
            destino.Close();
        }

        public void cmdBorrar()
        {
            string archivocarga = "cover.jpg";
            uri = new Uri("ftp://" + Ip + ":" + Puerto + "/documents/9781540407306(www.ebook-dl.com)");
            clienteRequest = (FtpWebRequest)WebRequest.Create(uri);

            credenciales = new NetworkCredential("adrian", "adrian9110");
            clienteRequest.Credentials = credenciales;

            clienteRequest.Method = WebRequestMethods.Ftp.DeleteFile;

            FtpWebResponse response = (FtpWebResponse)clienteRequest.GetResponse();
            Console.WriteLine("Delete status: {0}", response.StatusDescription);
            response.Close();
        }

        private List<Archivo> ObtieneLsita(string datos)
        {
            List<Archivo> retorno = new List<Archivo>();
            string[] registros = datos.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string procesaItem = "";
            string fechastr = "";
            string horastr = "";
            foreach (string item in registros)
            {
                Archivo archivo = new Archivo();
                archivo.nombre = "..";

                procesaItem = item.Trim();
                fechastr = procesaItem.Substring(0, 8);
                procesaItem = (procesaItem.Substring(8, procesaItem.Length - 8)).Trim();
                horastr = procesaItem.Substring(0, 7);
                procesaItem = (procesaItem.Substring(7, procesaItem.Length - 7)).Trim();

                archivo.fecha = fechastr + " " + horastr;

                if (procesaItem.Substring(0, 5) == "<DIR>")
                {
                    archivo.bDirectorio = true;
                    procesaItem = (procesaItem.Substring(5, procesaItem.Length - 5)).Trim();
                }
                else
                {
                    string[] strs = procesaItem.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    archivo.tamanio = Int64.Parse(strs[0]);
                    procesaItem = String.Join(" ", strs, 1, strs.Length - 1);
                    archivo.bDirectorio = false;
                }
                archivo.nombre = procesaItem;

                if(archivo.nombre != "" && archivo.nombre != "." && archivo.nombre != "..")
                {
                    retorno.Add(archivo);
                }
            }

            return retorno;
        }

        public struct Archivo
        {
            public string nombre;
            public bool bDirectorio;
            public Int64 tamanio;
            public string fecha;
        }
            
    }
}
