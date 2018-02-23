using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClienteFTP
{
    class Program
    {
        static void Main(string[] args)
        {

            Conexion conexion = new Conexion();
            conexion.cmdBorrar();
        }
    }
}
