using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APODE_Core;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ACORE APODE_Core = new ACORE(true);
            while (APODE_Core.is_console_active) { }
        }
    }
}
