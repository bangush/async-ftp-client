using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class MiddewareResponse
    {        
        private string estado;
        private string mensaje;
        private string descripcion;

        private Dictionary<string, object> data;

        public MiddewareResponse() { }

        public string Estado
        {
            get
            {
                return estado;
            }

            set
            {
                estado = value;
            }
        }

        public string Mensaje
        {
            get
            {
                return mensaje;
            }

            set
            {
                mensaje = value;
            }
        }

        public string Descripcion
        {
            get
            {
                return descripcion;
            }

            set
            {
                descripcion = value;
            }
        }

        public Dictionary<string, object> Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }
        
    }
}