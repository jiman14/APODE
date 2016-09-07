using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APODE_Core;

namespace APODE_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            ACORE aCore = new ACORE(true);
            while (aCore.is_console_active) { }
        }
    }
}
