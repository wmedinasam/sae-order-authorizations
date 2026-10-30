using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ORD_SAE.DAO
{
    class DaoParameter
    {
        /// <summary>
        /// Nombre del parametro.
        /// </summary>
        public string Name;

        /// <summary>
        /// Valor del parametro.
        /// </summary>
        public Object Value;

        /// <summary>
        /// Tipo de dato del parametro.
        /// </summary>
        public System.Data.DbType Type;

        /// <summary>
        /// True si es un parametro de salida del procedimiento almacenado.
        /// </summary>
        public bool Out;
    }
}
