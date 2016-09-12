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
#endregion