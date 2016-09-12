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
#endregion