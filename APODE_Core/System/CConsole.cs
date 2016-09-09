using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TControls;

namespace AutaCore
{
    public class CConsole
    {
        CSystem system;
        bool exit = true;
        Task ctask = null;

        public delegate void program_controller(CEventDesc event_desc);
        public event program_controller runProgram;

        static string EXIT_COMMAND = "exit";

        public CConsole(CSystem sys)
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
