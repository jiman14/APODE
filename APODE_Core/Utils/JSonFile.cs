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
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JSonUtil
{
    public class JSonFile: JSonBase
    {
        JObject jObj;
        JObject jActiveNode;

        /// <summary>
        /// Create json object from file and manage it
        /// </summary>
        /// <param name="json_dir_path"></param>
        /// <param name="json_file"></param>
        public JSonFile(bool debug, DirectoryInfo json_dir_path, String NameSpace, bool fileMustExists) :
            base(json_dir_path, NameSpace, fileMustExists)
        {
            // string current location
            jObj = readJsonFile(debug);
            jActiveNode = jObj;
        }
        /// <summary>
        /// Create json object from file and manage it
        /// </summary>
        /// <param name="json_dir_path"></param>
        /// <param name="json_file"></param>
        public JSonFile(bool debug, DirectoryInfo json_dir_path, String NameSpaces) :
            base(json_dir_path, NameSpaces, false)
        {
            // string current location
            jObj = readJsonFile(debug);
            jActiveNode = jObj;
        }
        /// <summary>
        /// Constructor for json object from arguments
        /// </summary>
        /// <param name="jobj"></param>
        public JSonFile(JObject jobj):
            base(null, null, false)           
        {            
            jObj = jobj;
            jActiveNode = jObj;
        }
        public bool hasErrors()
        {
            return jErrors.Count() > 0;
        }
        public void setActiveNode(String path)
        {
            if (path != "")
            {
                jActiveNode = getNode(path) as JObject;
            }
        }
        public JToken getNode(String path)
        {
            return jObj.SelectToken(path);
        }
        public JObject jActiveObj
        {
            get {
                if (jActiveNode != null)
                   return jActiveNode;
                else
                   return jObj;
            }
            set {
                jActiveNode = value;
            }
        }

        public void setByGuid(String Guid, JToken jtoken)
        {
            JToken node = getNode(String.Format("$.Notes[?(@.Guid == '{0}')]", Guid));
            if (node != null)
            {
                node.Replace(jtoken);
                Write();
            }
        }
        /// <summary>
        /// set jobject
        /// </summary>
        /// <param name="jobject"></param>
        public void set(JObject jobject)
        {
            jObj = jobject;
        }
        public void set(String property_name, JToken Value)
        {
            jActiveObj[property_name] = Value;
        }
        public void set(String property_name, String Value)
        {
            jActiveObj[property_name] = Value;
        }
        public void set(String property_name, int Value)
        {
            jActiveObj[property_name] = Value;
        }
        public void set(String property_name, double Value)
        {
            jActiveObj[property_name] = Value;
        }
        public void set(String property_name, bool Value)
        {
            jActiveObj[property_name] = Value;
        }
        public void set(String property_name, long Value)
        {
            jActiveObj[property_name] = Value;
        }
        public void set(String property_name, JObject Value)
        {
            jActiveObj[property_name] = Value;
        }
        public void add(String property_name, JToken Value)
        {
            (jActiveObj[property_name] as JArray).Add(Value);
        }
        public void add(String property_name, JObject Value)
        {
            if (jActiveObj[property_name] == null)
            {
                jActiveObj[property_name] = new JArray();
            }
            (jActiveObj[property_name] as JArray).Add(Value);
        }
        public void add(String property_name, object Value)
        {
            (jActiveObj[property_name] as JArray).Add(Value);
        }
        public void add(String property_name, String Value)
        {
            (jActiveObj[property_name] as JArray).Add(Value);
        }
        public void add(String property_name, int Value)
        {
            (jActiveObj[property_name] as JArray).Add(Value);
        }
        public void add(String property_name, bool Value)
        {
            (jActiveObj[property_name] as JArray).Add(Value);
        }
        public void add(String property_name, double Value)
        {
            (jActiveObj[property_name] as JArray).Add(Value);
        }
        public void add(String property_name, long Value)
        {
            (jActiveObj[property_name] as JArray).Add(Value);
        }
        /// <summary>
        /// get jobject
        /// </summary>
        /// <returns></returns>
        public JToken get(String property_name)
        {
            return ((property_name != "") && (jActiveObj[property_name] != null))? jActiveObj[property_name]: new JObject();
        }
        /// <summary>
        /// get jobject
        /// </summary>
        /// <returns></returns>
        public JObject get()
        {
            return jObj;
        }
        /// <summary>
        /// write json object in json file
        /// </summary>
        public void Write()
        {
            writeJsonFile(jObj);
        }

        
    }
}

#endregion