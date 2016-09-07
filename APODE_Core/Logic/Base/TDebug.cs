using System;
using Newtonsoft.Json.Linq;
using JSonUtil;
using APODE_Core;

namespace TLogic
{
    public class TDebug
    {
        #region Vars and enumerations
        CGlobals Globals;
        public enum debugStatus
        {
            PROCESSING,
            WAIT_FOR_INPUT,
            WAIT_FOR_CONFIGURATION,
            UNHANDLED_ERROR,
            PROCESS_ERROR,
            FINISHED
        }
        private bool Show_debug_in_console = false;
        #endregion

        #region constructor

        private JSonFile debugInfo;

        public TDebug(CGlobals globals, String Program)
        {
            Globals = globals;
            debugInfo = new JSonFile(true, Globals.AppData_path(dPATH.DEBUG), Program);
            Show_debug_in_console = Globals.get(dGLOBALS.SHOW_DEBUG_IN_CONSOLE).ToObject<bool>();
        }
        #endregion

        #region Destructor

        public void Close()
        {
            if (Globals.debug)
            {
                debugInfo.Write();
            }
        }

        #endregion

        #region Public properties

        public JToken Name
        {
            get { return debugInfo.get(dDEBUG.NAME); }
            set { debugInfo.set(dDEBUG.NAME, value); }
        }
        public JToken Description
        {
            get { return debugInfo.get(dDEBUG.DESCRIPTION); }
            set { debugInfo.set(dDEBUG.DESCRIPTION, value); }
        }
        public String Status
        {
            get { return (debugInfo.get(dDEBUG.STATUS) != null)? debugInfo.get(dDEBUG.STATUS).ToString(): ""; }
            set { debugInfo.set(dDEBUG.STATUS, value); }
        }
        /// <summary>
        /// total program time in seconds
        /// </summary>
        public JToken Total_time
        {
            get { return debugInfo.get(dDEBUG.TOTAL_TIME); }
            set { debugInfo.set(dDEBUG.TOTAL_TIME, value); }
        }
        public JToken Last_Execution_Time
        {
            get { return debugInfo.get(dDEBUG.LAST_EXECUTION_TIME); }
            set { debugInfo.set(dDEBUG.LAST_EXECUTION_TIME, value); }
        }

        internal JToken getOutput(string process_guid)
        {            
            return debugInfo.getNode(String.Format("$.Processes[?(@.Guid == '{0}')]", process_guid));            
        }

        public JArray Processes
        {
            get { return debugInfo.get(dDEBUG.PROCESSES) as JArray; }

        }
        public void cleanProcess()
        {
            debugInfo.set(dDEBUG.PROCESSES, new JArray());
        }
        public void cprint(JToken message)
        {
            Console.WriteLine(message.ToString());
        }
        
        #endregion
       
    }
}
