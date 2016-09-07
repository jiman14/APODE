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
