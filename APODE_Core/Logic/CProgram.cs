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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using JSonUtil;
using APODE_Core;

namespace TLogic
{
    public class CProgram
    {
        #region constructor

        private CSystem sys;
        private JSonFile progInfo;                          // Program object descriptor
        public Dictionary<String, object> vars;             // Program vars
        public TDebug debug;                                // Debug object
        public delegate void RunProcess(TProcess prc);      // Run process delegate
        public event RunProcess ExecuteProcess;             // Execute process event

        public CProgram(CSystem csystem, String program)
        {
            sys = csystem;
            // Remove program queued flag
            program = (program.StartsWith("&")) ? program.Substring(1) : program;
            
            // Read program
            progInfo = new JSonFile(csystem.Globals.debug, csystem.Globals.AppData_path(dPATH.PROGRAM), program, true);

            if (!progInfo.hasErrors())
            {
                // Init csystem.ProgramErrors and debug for program execution
                csystem.NewProgram(program);
                csystem.ProgramErrors.Error = "";
                debug = csystem.ProgramDebug;

                csystem.ProgramErrors.Name = Name;
                csystem.ProgramErrors.Description = Description;
                debug.Name = Name;
                debug.Description = Description;

                // Init vars
                vars = new Dictionary<string, object>();
            }
            else
            {
                csystem.ProgramErrors.addError(progInfo.jErrors);
            }
        }
        /// <summary>
        /// Check queued program's flag
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        static public bool CheckQueuedFlag(String program)
        {
            return program.StartsWith("&");
        }

        /// <summary>
        /// Save debug and csystem.ProgramErrors
        /// </summary>
        public void ProgramClose()
        {
            if (debug != null) debug.Close();
            sys.ProgramErrors.Close();
        }       

        /// <summary>
        /// Has errors
        /// </summary>
        /// <returns></returns>
        public bool hasErrors()
        {
            return sys.ProgramErrors.hasErrors() || progInfo.hasErrors();
        }
        
        #endregion

        #region Public properties

        public JToken Name { get { return progInfo.get(dPROGRAM.NAME); } }
        public bool Parallel_execution {
            get
            {
                JToken pe = progInfo.get(dPROGRAM.PARALLEL_EXECUTION);
                return (pe != null)? pe.ToObject<bool>(): false;
            }
        }
        public JToken Description { get { return progInfo.get(dPROGRAM.DESCRIPTION); } }
        public JToken Configuration { get { return progInfo.get(dPROGRAM.CONFIGURATION); } }

        public JToken Logic
        {
            get { return (JArray)progInfo.get(dPROGRAM.LOGIC); }
            set { progInfo.set(dPROGRAM.LOGIC, value); }
        }
        #endregion

        #region Program logic

        public void Run()
        {
            #region Initalize program

            if ((String.IsNullOrEmpty(debug.Status)) || (debug.Status == TDebug.debugStatus.FINISHED.ToString()))
            {   
                debug.Status = TDebug.debugStatus.PROCESSING.ToString();
            }
            DateTime initTime = DateTime.Now;

            #endregion

            #region main bucle
            foreach (JObject prc_node in Logic)
            {
                execute_process(prc_node);
                if (sys.ProgramErrors.hasErrors())
                {
                    sys.ProgramErrors.addError(String.Format("Aborting program execution '{0}' due errors in: {1}", Name.ToString(), prc_node["Guid"].ToString()));
                    break;
                }
                if (sys.ProgramErrors.forceExitProgram)
                {
                    sys.debug.add("Force program exit flag actived by process");
                    break;
                }
            }
            #endregion

            #region end runing

            debug.Status = sys.ProgramErrors.ErrorStatus.ToString();
            debug.Total_time = Math.Round((DateTime.Now - initTime).TotalSeconds, 2);
            debug.Last_Execution_Time = DateTime.Now.ToString();

            #endregion
        }

        private TProcess execute_process(JObject prc_node)
        {
            TProcess proc = new TProcess(sys, ref vars, prc_node);

            // run recursive processes in inputs
            //for (int i = 0; i < proc.Inputs.Count(); i++)
            //{
            //    JToken jActiveInput = proc.Inputs.ElementAt(i);
            //    if ((jActiveInput != null) && (jActiveInput is JObject) && (jActiveInput[dPROCESS.GUID.ToString()] != null))
            //    {
            //        TProcess act_prc = execute_process(jActiveInput as JObject);
            //    }
            //}

            if (!sys.ProgramErrors.hasErrors())
            {
                // run current proccess
                if (ExecuteProcess != null)
                {
                    ExecuteProcess(proc);
                }

                debug.Processes.Add(proc.Data);
                return proc;
            }
            else return null;            
        }
        #endregion

    }
}
#endregion