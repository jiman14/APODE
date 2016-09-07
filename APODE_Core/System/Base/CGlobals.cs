using System;
using System.IO;
using Newtonsoft.Json.Linq;
using JSonUtil;
using System.Reflection;

namespace APODE_Core
{
    public class CGlobals
    {
        #region constructor

        private JSonFile pInfo;
        private JToken globals;
        DirectoryInfo dApp_path;
        DirectoryInfo dAppData_path;
        DirectoryInfo dResources_path;
        DirectoryInfo dAssemblies_path;

        String errors = "";
        private bool _debug = false;
        public bool debug
        {
            get { return _debug; }
        }

        /// <summary>
        /// Load Globals vars
        /// </summary>
        /// <param name="GLOBALS"></param>
        public CGlobals()
        {
            // Get AppData location
            dApp_path = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            String sAppData_path = dGLOBALS.APPDATA_PATH;
            while ((dApp_path != null) && (!Directory.Exists(Path.Combine(dApp_path.FullName, sAppData_path))))
            {
                dApp_path = dApp_path.Parent;
            }
            if (dApp_path != null)
            {
                dAppData_path = dApp_path.GetDirectories(sAppData_path)[0];
                dResources_path = dApp_path.GetDirectories(dGLOBALS.RESOURCES_PATH)[0];
                dAssemblies_path = dAppData_path.GetDirectories(dGLOBALS.ASSEMBLIES_PATH)[0];
            }
            else errors = "Error loading AppData path";

            if (errors == "")
            {
                pInfo = new JSonFile(true, AppData_path(""), dGLOBALS.GLOBALS);
                if (!pInfo.hasErrors())
                {
                    globals = pInfo.get(dGLOBALS.GLOBALS);
                    _debug = get(dGLOBALS.DEBUG_MODE).ToObject<bool>();
                }
            }
        }

        /// <summary>
        /// Return errors
        /// </summary>
        public String Errors
        {
            get { return errors; }
        }

        /// <summary>
        /// Get single var in Globals
        /// </summary>true
        /// <param name="variable"></param>
        /// <returns></returns>
        public JToken get(String variable)
        {
            return globals[variable];
        }
        /// <summary>
        /// Get single var in Globals
        /// </summary>true
        /// <param name="variable"></param>
        /// <returns></returns>
        public JToken get(String path, String variable)
        {
            JToken jnode = null;
            foreach (string node in path.Split('.'))
            {
                if (node != dGLOBALS.GLOBALS)
                {
                    if (jnode == null) jnode = globals[node];
                    else jnode = jnode[node];
                }                        
            }
            return (jnode != null)? jnode[variable]: globals[variable];
        }

        /// <summary>
        /// Get all globals vars
        /// </summary>
        public JToken Globals
        {
            get { return globals; }
        }

        /// <summary>
        /// Return AppData_path or a section of it
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public DirectoryInfo AppData_path(String section)
        {            
            DirectoryInfo dtemp = new DirectoryInfo(dAppData_path.FullName);
            String[] sections = section.Split('.');
            foreach (String sec in sections) {
                if ((sec != "") && (dtemp != null))
                {
                    dtemp = (dtemp.GetDirectories(sec).Length > 0) ? dtemp.GetDirectories(sec)[0] : null;
                }
            }
            return dtemp;
        }

        /// <summary>
        /// Resources string path
        /// </summary>
        public String Resources_path
        {
            get { return dResources_path.FullName; }
        }

        /// <summary>
        /// Get assembly full path
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public string getAssembly(string assembly)
        {
            DirectoryInfo dtemp = new DirectoryInfo(dAssemblies_path.FullName);
            String[] sections = assembly.Split('.');
            int nSec = sections.Length;
            for (int i=0; i<nSec-2; i++)
            {
                if ((sections[i] != "") && (dtemp != null))
                {
                    dtemp = (dtemp.GetDirectories(sections[i]).Length > 0) ? dtemp.GetDirectories(sections[i])[0] : null;
                }
            }
            return Path.Combine(dtemp.FullName, sections[nSec-2] + "." + sections[nSec-1]);
        }
        #endregion
    }
}