using System;
using JSonUtil;
using Newtonsoft.Json.Linq;
using APODE_Core;

namespace TLogic
{
    public class TErrors
    {
        #region Vars

        bool Console_output = true;

        public enum errorStatus
        {
            OK,
            INPUTS,
            OUTPUTS,
            CONFIGURATION,
            PROCESS,
            UNHANDLED,
            INVALID_JSON
        }
        public errorStatus ErrorStatus;
        private JSonFile errorInfo;
        public bool forceExitProgram = false;

        #endregion

        #region Constructor

        public TErrors(CGlobals Globals, String Program)
        {
            errorInfo = new JSonFile(true, Globals.AppData_path(dPATH.ERRORS), Program);
            Console_output = Globals.get(dGLOBALS.SHOW_ERRORS_IN_CONSOLE).ToObject<bool>(); 
            ErrorStatus = errorStatus.OK;
        }

        #endregion

        #region Destructor

        public void Close()
        {
            errorInfo.Write();
        }

        #endregion

        #region Public methods

        public JObject get()
        {
            return errorInfo.jActiveObj;
        }
        public bool hasErrors()
        {
            return !((Error == null) || (Error.ToString() == ""));
        }

        #endregion

        #region Public properties

        public JToken Name
        {
            get { return errorInfo.get(dERROR.NAME); }
            set { errorInfo.set(dERROR.NAME, value); }
        }
        public JToken Description
        {
            get { return errorInfo.get(dERROR.DESCRIPTION); }
            set { errorInfo.set(dERROR.DESCRIPTION, value); }
        }
        public JToken Error
        {
            get { return errorInfo.get(dERROR.ERROR); }
            set
            {
                errorInfo.set(dERROR.ERROR, value);
                if (hasErrors())
                {
                    errorInfo.set(dERROR.ERROR_TIME, DateTime.Now.ToString());
                    if (Console_output) Console.WriteLine(value.ToString());
                }
            }
        }
        public JArray Processes
        {
            get { return errorInfo.get(dERROR.PROCESSES) as JArray; }
        }
        public void addError(JToken Value)
        {
            errorInfo.add(dERROR.PROCESSES, Value);
            if (!((Value == null) || (Value.ToString() == "")))
            {
                errorInfo.set(dERROR.ERROR_TIME, DateTime.Now.ToString());
                if (Console_output) Console.WriteLine(Value.ToString());
            }
        }
        public void addError(String Value)
        {
            errorInfo.add(dERROR.PROCESSES, Value);
            if (!String.IsNullOrEmpty(Value))
            {
                errorInfo.set(dERROR.ERROR_TIME, DateTime.Now.ToString());
                if (Console_output) Console.WriteLine(Value);
            }
        }
        public void cleanProcess()
        {
            errorInfo.set(dERROR.PROCESSES, new JArray());
        }
        public void removeErrors()
        {
            Error = "";
        }

        internal JToken noInputs
        {
            set
            {
                ErrorStatus = errorStatus.INPUTS;
                Error = value;
            }            
        }
        internal JToken noOutputs
        {
            set
            {
                ErrorStatus = errorStatus.OUTPUTS;
                Error = value;
            }
        }
        internal JToken noConfiguration
        {
            set
            {
                ErrorStatus = errorStatus.CONFIGURATION;
                Error = value;
            }
        }
        internal JToken processError
        {
            set
            {
                ErrorStatus = errorStatus.PROCESS;
                Error = value;
            }
        }
        internal JToken unhandledError
        {
            set
            {
                ErrorStatus = errorStatus.UNHANDLED;
                Error = value;
            }
        }
        internal JToken invalidJSON
        {
            set
            {
                ErrorStatus = errorStatus.INVALID_JSON;
                Error = value;
            }
        }



        #endregion

    }
}