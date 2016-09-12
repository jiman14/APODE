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

    public class CErrors
    {
        private JArray jErrors;
        bool Console_output = true;
        bool there_is_error = false;

        public CErrors(CGlobals Globals)
        {
            jErrors = new JArray();
            Console_output = Globals.get(dGLOBALS.SHOW_ERRORS_IN_CONSOLE).ToObject<bool>();
        }
        public CErrors(CGlobals Globals, JToken base_error)
        {
            jErrors = (JArray) base_error;
            Console_output = Globals.get(dGLOBALS.SHOW_ERRORS_IN_CONSOLE).ToObject<bool>();
        }
        public bool hasErrors()
        {
            return there_is_error;
        }

        /// <summary>
        /// Reset UI errors
        /// </summary>
        public void reset_UI_errors()
        {
            there_is_error = false;
            jErrors = new JArray();
        }

        public void add(String error)
        {
            if (error != "")
            {
                there_is_error = true;
                if (Console_output)
                {
                    Console.WriteLine(error.ToString());
                }
                jErrors.Add(error);
            }
        }
        public void add(JToken error)
        {
            if (error is JArray)
            {
                foreach (JToken jerror in error)
                {
                    if (jerror is JArray)
                    {
                        add(jerror);
                    }
                    else
                    {
                        add(jerror.ToString());
                    }
                }
            }
            else {
                add(error.ToString());
            }
        }
        public JToken getErrors()
        {
            return jErrors;
        }

        /// <summary>
        /// print errors in console
        /// </summary>
        /// <param name="jError"></param>
        private void print_errors(object jError)
        {
            if (jError is JArray)
            {
                foreach (object jerror in jError as JArray)
                {
                    if (jerror is JArray)
                    {
                        print_errors(jerror);
                    }
                    else if (jerror is JObject)
                    {
                        print_errors(((JObject)jerror).Value<JObject>());
                    }
                    else
                    {
                        Console.WriteLine(jerror.ToString());
                    }
                }
            }
            else if (jError is JObject)
            {
                print_errors(((JObject)jError).Value<JObject>());
            }
            else
            {
                Console.WriteLine(jError.ToString());
            }
        }
    }

}
#endregion