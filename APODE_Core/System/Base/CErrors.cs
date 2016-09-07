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
