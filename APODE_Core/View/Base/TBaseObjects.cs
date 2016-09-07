using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using JSonUtil;
using APODE_Core;

namespace TControls
{
    public class TBaseObjects
    {
        #region constructor

        private JSonFile prcInfo;
        public JArray jErrors;

        /// <summary>
        /// Get objects in view from file
        /// </summary>
        /// <param name="Globals"></param>
        /// <param name="view"></param>
        /// <param name="object_guid"></param>
        public TBaseObjects(CGlobals Globals, String view)
        {
            jErrors = new JArray();
            prcInfo = new JSonFile(Globals.debug, Globals.AppData_path(dCONTROLS.PATH), view, true);
            if (prcInfo.hasErrors())            
            {
                jErrors = prcInfo.jErrors;
            }
        }

        /// <summary>
        /// Initialize from object view jobject
        /// </summary>
        /// <param name="view_objects"></param>
        /// <param name="object_guid"></param>
        public void ActiveObject(JToken active_Object)
        {
            prcInfo.jActiveObj = active_Object as JObject;
        }
        public bool hasErrors()
        {
            return jErrors.Count() > 0;
        }

        #endregion

        #region public properties

        public JArray Object_list { get { return (prcInfo.get(dCONTROLS.OBJECTS) as JArray != null)? prcInfo.get(dCONTROLS.OBJECTS) as JArray: new JArray(); } }

        public String GUID
        {
            get { return prcInfo.get(dCONTROLS.GUID).ToString(); }
        }
        public String Assembly
        {
            get { return prcInfo.get(dCONTROLS.ASSEMBLY).ToString(); }
            set { prcInfo.set(dCONTROLS.ASSEMBLY, value); }
        }
        public String Object_name
        {
            get { return prcInfo.get(dCONTROLS.OBJECT_NAME).ToString(); }
        }                
        public JToken Configuration
        {
            get { return prcInfo.get(dCONTROLS.CONFIGURATION); }
            set { prcInfo.set(dCONTROLS.CONFIGURATION, value); }
        }
        public JToken Methods
        {
            get { return prcInfo.get(dCONTROLS.METHODS); }
            set { prcInfo.set(dCONTROLS.METHODS, value); }
        }
        public JToken Events
        {
            get { return prcInfo.get(dCONTROLS.EVENTS); }
            set { prcInfo.set(dCONTROLS.EVENTS, value); }
        }
        public JToken Description
        {
            get { return prcInfo.get(dCONTROLS.DESCRIPTION); }
            set { prcInfo.set(dCONTROLS.DESCRIPTION, value); }
        }
        public JToken Version
        {
            get { return prcInfo.get(dCONTROLS.VERSION); }
            set { prcInfo.set(dCONTROLS.VERSION, value); }
        }

        #endregion

    }
}
