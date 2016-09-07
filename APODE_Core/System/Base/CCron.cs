using System;
using System.Collections.Generic;
using System.Linq;

namespace APODE_Core
{
    public class CCron
    {
        #region Variables & events

        CGlobals Globals;
        CErrors errors;
        CDebug debug;
        Dictionary<String, System.Windows.Forms.Timer> program_list;         

        public delegate void program_controller(CEventDesc event_desc);
        public event program_controller runProgram;
        #endregion

        #region constructor

        /// <summary>
        /// Initialize cron
        /// </summary>
        /// <param name="interval_ms"></param>
        public CCron(CGlobals _globals, CErrors _errors, CDebug _debug)
        {
            Globals = _globals;
            errors = _errors;
            debug = _debug;
            program_list = new Dictionary<string, System.Windows.Forms.Timer>();
        }

        private void Tcron_Tick(object sender, EventArgs e)
        {
            // temporary desactive console dump
            debug.CONSOLE_DUMP = false;
            // run programs in list

            System.Windows.Forms.Timer tcron = (System.Windows.Forms.Timer)sender;
            tcron.Stop();

            if (runProgram != null)
            {
                CEventDesc desc = new CEventDesc("CRON CALL", "CRON CALL", "CRON CALL", "CRON CALL", tcron.Tag.ToString());
                runProgram(desc);
            }
            
            // restore console dump
            debug.CONSOLE_DUMP = true;
            // start cron
            if (program_list.Keys.Contains(tcron.Tag.ToString()))
            {
                tcron.Start();
            }
        }
        #endregion

        #region Manage cron


        #endregion

        #region Manage program list

        /// <summary>
        /// Add new program to Cron
        /// </summary>
        /// <param name="program_name"></param>
        /// <param name="eDesc"></param>
        public void addProgram(String program_name, int interval)
        {
            try
            {
                if (!program_list.Keys.Contains(program_name))
                {
                    System.Windows.Forms.Timer tcron = new System.Windows.Forms.Timer();
                    tcron.Interval = interval;
                    tcron.Tick += Tcron_Tick;
                    tcron.Tag = program_name;
                    program_list.Add(program_name, tcron);
                    tcron.Start();
                }
            }
            catch (Exception exc)
            {
                errors.add(String.Format("Error launching cron with program '{0}': {1}", program_name, exc.Message));
            }
        }

        /// <summary>
        /// remove a program, stop if program list is empty
        /// </summary>
        /// <param name="program_name"></param>
        public void removeProgram(String program_name)
        {
            try
            {
                if (program_list.Keys.Contains(program_name))
                {
                    program_list[program_name].Stop();
                    program_list.Remove(program_name);
                }  
            }
            catch (Exception exc)
            {
                errors.add(String.Format("Error stoping cron with program '{0}': {1}", program_name, exc.Message));
            }
        }
        #endregion

    }
}
