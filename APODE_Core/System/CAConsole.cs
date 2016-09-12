#region Copyright © 2015, 2016 MJM [info.apode@gmail.com]
/*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.

* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
* GNU General Public License for more details.

* You should have received a copy of the GNU General Public License
* along with this program.If not, see<http:* www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TControls;

namespace APODE_Core
{
    public class CAConsole
    {
        CSystem system;
        bool exit = true;
        Task ctask = null;

        public delegate void program_controller(CEventDesc event_desc);
        public event program_controller runProgram;

        static string EXIT_COMMAND = "exit";

        public CAConsole(CSystem sys)
        {
            Console.WriteLine("Running console...");
            system = sys;
        }

        public void start()
        {
            exit = false;
            ctask = new Task(run);
            ctask.Start();
        }
        public void stop()
        {
            exit = true;
            if ((ctask != null) && (ctask.Status == TaskStatus.Running)) {
                ctask.Wait();
            }
        }
        public bool isActive
        {
            get { return !exit; }
        }
        private void run()
        {
            String cmd = "";
            while ((cmd != EXIT_COMMAND) && (!exit))
            {
                cmd = Console.ReadLine();

                if (!String.IsNullOrEmpty(cmd))
                {
                    String[] command = cmd.Split(' ');
                    String program_name = command[0];
                    CEventDesc eDesc = new CEventDesc("", "", "", "", program_name);
                    eDesc.addArgs(new object[] { cmd.Replace(program_name + " ", "") });
                    runProgram(eDesc);
                }
            }
            exit = true;
        }
    }
}
#endregion