using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using JSonUtil;
using APODE_Core;

namespace TLogic
{
    public class TProcess 
    {
        #region constructor

        CSystem sys;
        private JSonFile pActivePrc;
        private JSonFile pBasePrc;
        public Dictionary<String, object> vars;

        public TProcess(CSystem csystem, ref Dictionary<String, Object> program_vars, JObject prc)
        {
            sys = csystem;
            vars = program_vars;
            pActivePrc = new JSonFile(prc);
            loadBaseProcess();
        }
        private void loadBaseProcess()
        {            
            pBasePrc = new JSonFile(sys.Globals.debug, sys.Globals.AppData_path(dPATH.PROCESSES), Namespace, true);
            if (!pBasePrc.hasErrors())
            {
                pBasePrc.setActiveNode(String.Format("$.{0}[?(@.{1} == '{2}')]", dPROCESS.PROCESSES, dPROCESS.GUID.ToString(), Guid));
            }
            else
            {
                sys.ProgramErrors.invalidJSON = 
                    String.Format("Error loading json file {0}.{1}. Details: {2}",
                        dPATH.PROCESSES,
                        Namespace,
                        pBasePrc.jErrors.ToString());
            }
        }

        #endregion

        #region public properties

        public JToken Data
        {
            get { return pActivePrc.jActiveObj; }
        }

        public String Namespace
        {
            get { return pActivePrc.get(dPROCESS.NAMESPACE).ToObject<String>(); }
        }
        public String Guid
        {
            get { return pActivePrc.get(dPROCESS.GUID).ToObject<String>(); }
        }
        public JToken Name
        {
            get { return pActivePrc.get(dPROCESS.NAME); }
            set { pActivePrc.set(dPROCESS.NAME, value); }
        }
        public JToken Description
        {
            get { return pActivePrc.get(dPROCESS.DESCRIPTION); }
            set { pActivePrc.set(dPROCESS.DESCRIPTION, value); }
        }
        public JToken Version
        {
            get { return pBasePrc.get(dPROCESS.VERSION); }
            set { pBasePrc.set(dPROCESS.VERSION, value); }
        }
        public JToken Default_Configuration
        {
            get { return pActivePrc.get(dPROCESS.DEFAULT_CONFIGURATION); }
            set { pActivePrc.set(dPROCESS.DEFAULT_CONFIGURATION, value); }
        }
        public JToken BaseConfiguration
        {
            get { return pBasePrc.get(dPROCESS.CONFIGURATION); }
            set { pBasePrc.set(dPROCESS.CONFIGURATION, value); }
        }
        public JToken Configuration
        {
            get { return pActivePrc.get(dPROCESS.CONFIGURATION); }
            set { pActivePrc.set(dPROCESS.CONFIGURATION, value); }
        }
        public JToken Inputs
        {
            get { return pActivePrc.get(dPROCESS.INPUTS); }
            set { pActivePrc.set(dPROCESS.INPUTS, value); }
        }
        public JToken BaseInputs
        {
            get { return pBasePrc.get(dPROCESS.INPUTS); }
            set { pBasePrc.set(dPROCESS.INPUTS, value); }
        }
        public JToken Outputs
        {
            get { return pActivePrc.get(dPROCESS.OUTPUTS); }
            set { pActivePrc.set(dPROCESS.OUTPUTS, value); }
        }
        public JToken BaseOutputs
        {
            get { return pBasePrc.get(dPROCESS.OUTPUTS); }
            set { pBasePrc.set(dPROCESS.OUTPUTS, value); }
        }
        public JToken Total_time
        {
            get { return pActivePrc.get(dPROCESS.TOTAL_TIME); }
            set { pActivePrc.set(dPROCESS.TOTAL_TIME, value); }
        }
        public JToken Debug_info
        {
            get { return pActivePrc.get(dPROCESS.DEBUG_INFO); }
            set { pActivePrc.set(dPROCESS.DEBUG_INFO, value); }
        }

        #endregion

    }
}
