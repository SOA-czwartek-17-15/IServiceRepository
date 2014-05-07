using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceRepository
{
    /**
     *  Klasa reprezentująca serwis w bazie
     * */
    public class Service
    {
        /**
         *  Nazwa serwisu
         * */
        public String Name { get; set; }

        /**
         * Adres serwisu
         * */
        public String Adress { get; set; }

    }
}
