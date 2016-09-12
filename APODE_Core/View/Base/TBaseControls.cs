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
    public class TBaseControls
    {
        #region constructor

        private JSonFile prcInfo;
        public JArray jErrors;

        /// <summary>
        /// Get controls in view from file
        /// </summary>
        /// <param name="Globals"></param>
        /// <param name="view"></param>
        /// <param name="control_guid"></param>
        public TBaseControls(CGlobals Globals, String view)
        {
            jErrors = new JArray();
            prcInfo =new JSonFile(Globals.debug, Globals.AppData_path(dCONTROLS.PATH), view, true);
            if (!prcInfo.hasErrors())
            {
                jErrors = prcInfo.jErrors;
            }
        }
        /// <summary>
        /// Initialize controls from view controls jobject
        /// </summary>
        /// <param name="view_controls"></param>
        /// <param name="control_guid"></param>
        public void ActiveControl(String control_guid)
        {
            if (control_guid.Contains("."))
            {
                control_guid = control_guid.Substring(control_guid.LastIndexOf(".") + 1);
            }
            prcInfo.setActiveNode(String.Format("$.{0}[?(@.{1} == '{2}')]", dCONTROLS.CONTROLS, dCONTROLS.GUID.ToString(), control_guid));
        }
        public bool hasErrors()
        {
            return jErrors.Count() > 0;
        }

        #endregion

        #region public properties

        public JArray Controls
        {
            get { return (prcInfo.get(dCONTROLS.CONTROLS) as JArray != null)? prcInfo.get(dCONTROLS.CONTROLS) as JArray: new JArray(); }
            set { prcInfo.set(dCONTROLS.CONTROLS, value); }
        }
        public JToken Configuration
        {
            get { return prcInfo.get(dCONTROLS.CONFIGURATION); }
            set { prcInfo.set(dCONTROLS.CONFIGURATION, value); }
        }
        public JToken Events
        {
            get { return prcInfo.get(dCONTROLS.EVENTS); }
            set { prcInfo.set(dCONTROLS.EVENTS, value); }
        }
        public String Type_str
        {
            get { return prcInfo.get(dCONTROLS.TYPE).ToString(); }
            set { prcInfo.set(dCONTROLS.TYPE, value); }
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