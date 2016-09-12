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

namespace APODE_Core
{
    public class CDebug
    {
        private JArray jDebug;
        
        bool Console_output = true;

        public CDebug(CGlobals Globals)
        {
            jDebug = new JArray();
            Console_output = Globals.get(dGLOBALS.SHOW_DEBUG_IN_CONSOLE).ToObject<bool>(); 
        }
        public CDebug(CGlobals Globals, JToken base_debug)
        {
            jDebug = (JArray)base_debug;
            Console_output = Globals.get(dGLOBALS.SHOW_DEBUG_IN_CONSOLE).ToObject<bool>();
        }
        /// <summary>
        /// Active / Desactive console output
        /// </summary>
        public bool CONSOLE_DUMP
        {
            get { return Console_output; }
            set { Console_output = value; }
        } 
        public void add(String message)
        {
            if (Console_output)
            {
                Console.WriteLine(message.ToString());
            }
            jDebug.Add(message);
        }
        public JToken getDebug()
        {
            return jDebug;
        }
        public JToken getChildDebug()
        {
            JArray jchild = new JArray();
            jDebug.Add(jchild);
            return jchild;
        }

        private void print_debug(object jdbg)
        {
            if (jdbg is JArray)
            {
                foreach (object jd in jdbg as JArray)
                {
                    if (jd is JArray)
                    {
                        print_debug(jd);
                    }
                    else if (jd is JObject)
                    {
                        print_debug(((JObject)jd).Value<JObject>());
                    }
                    else
                    {
                        Console.WriteLine(jd.ToString());
                    }
                }
            }
            else if (jdbg is JObject)
            {
                print_debug(((JObject)jdbg).Value<JObject>());
            }
            else
            {
                Console.WriteLine(jdbg.ToString());
            }
        }
    }
}
#endregion