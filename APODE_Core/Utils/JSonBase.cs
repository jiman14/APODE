using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;


namespace JSonUtil
{
    public class JSonBase
    {
        String AppDataPath;
        DirectoryInfo dJSONPath;
        public JArray jErrors;
        public bool canWrite;
        String str_namespace;
        String file_name;
        bool file_must_exists = false;

        public JSonBase(DirectoryInfo dAppData_path, String NameSpace, bool fileMustExists)
        {
            try
            {
                jErrors = new JArray();
                canWrite = false;
                file_must_exists = fileMustExists;

                if (!String.IsNullOrEmpty(NameSpace))
                {
                    // Base app data path
                    AppDataPath = dAppData_path.FullName.Substring(0, dAppData_path.FullName.LastIndexOf("\\"));

                    str_namespace = NameSpace;

                    // Base path
                    dJSONPath = dAppData_path;

                    // Get into namespace folder
                    String[] namespaces = str_namespace.Split('.');
                    int i = 0;
                    for (i = 0; i < namespaces.Length - 1; i++)
                    {
                        if (dJSONPath.GetDirectories(namespaces[i].Trim(), SearchOption.TopDirectoryOnly).Count() > 0)
                        {
                            dJSONPath = dJSONPath.GetDirectories(namespaces[i].Trim(), SearchOption.TopDirectoryOnly)[0];
                        }
                        else
                        {
                            dJSONPath = dJSONPath.CreateSubdirectory(namespaces[i].Trim());
                        }
                    }

                    // Get filename
                    file_name = namespaces[i];
                    canWrite = true;
                }
            }
            catch (Exception exc)
            {
                jErrors.Add(String.Format("Error path not found '{0}\\{1}'. Details: {2}", dAppData_path, NameSpace.Replace(".", "\\"), exc.Message));
            }
        }
        public String ProgramDir
        {
            get
            {
                return dJSONPath.FullName;
            }
        }

        /// <summary>
        /// Read json file and return a JObject
        /// </summary>
        /// <param name="proc_guid"></param>
        /// <returns></returns>
        internal JObject readJsonFile(bool debug)
        {
            try
            {
                file_name = (file_name.ToLower().EndsWith(".json")) ? file_name : String.Format("{0}.json", file_name);
                canWrite = ! ((file_must_exists) && (dJSONPath.GetFiles(file_name).Count() == 0));                                    
                
                foreach (FileInfo fi in dJSONPath.GetFiles(file_name))
                {
                    StreamReader str = new StreamReader(fi.FullName);
                    JsonTextReader jsr = new JsonTextReader(str);
                    JObject JProc = null;
                    bool errors = false;

                    try
                    {
                        JsonSerializer jser = new JsonSerializer();
                        JProc = jser.Deserialize<JObject>(jsr);
                        if (JProc == null)
                        {
                            errors = true;
                        }
                    }
                    catch (Exception exc)
                    {
                        jErrors.Add(exc.Message);
                        errors = true;
                    }
                    finally
                    {
                        str.Close();
                    }
                    if ((debug) && (errors))
                    {
                        String filecontent = File.ReadAllText(fi.FullName);
                        String schema = "";
                        if (filecontent.Contains("$schema"))
                        {
                            schema = filecontent.Substring(filecontent.ToLower().IndexOf("schemas"));
                            schema = schema.Substring(0, schema.IndexOf("\"")).Replace("\\\\", "\\");
                        }
                        if ((schema != "") && (File.Exists(Path.Combine(AppDataPath, schema))))
                            try
                            {
                                JsonSchema4 JSchema = JsonSchema4.FromFile(Path.Combine(AppDataPath, schema));
                                foreach (ValidationError ve in JSchema.Validate(filecontent))
                                {
                                    jErrors.Add(String.Format("Error in {0} > {1}: {2}", ve.Path, ve.Property, ve.Kind));
                                }

                                errors = jErrors.Count() > 0;
                            }
                            catch (Exception exc)
                            {
                                jErrors.Add(String.Format("Error in json file '{0}': {1}", file_name, exc.Message));
                                canWrite = false;
                            }
                    }
                    return (!errors)? JProc : new JObject() ;
                }
            }
            catch (Exception exc)
            {
                jErrors.Add(String.Format("Error in '{0}': {1}", file_name, exc.Message));
                canWrite = false;
            }

            if (canWrite)
            {
                return new JObject();
            }
            else
            {
                jErrors.Add(String.Format("Program file not found: {0}", file_name));
                return null;
            }
            
        }

        /// <summary>
        /// Write json file
        /// </summary>
        /// <param name="jsonObj"></param>
        /// <param name="file_name"></param>
        internal void writeJsonFile(JObject jsonObj)
        {
            if (canWrite)
            {
                bool create_if_not_exists = true;
                file_name = (file_name.ToLower().EndsWith(".json")) ? file_name : String.Format("{0}.json", file_name);

                foreach (FileInfo fi in dJSONPath.GetFiles(file_name))
                {
                    File.WriteAllText(fi.FullName, jsonObj.ToString());
                    create_if_not_exists = false;
                }

                if (create_if_not_exists)
                {
                    File.WriteAllText(Path.Combine(dJSONPath.FullName, file_name), jsonObj.ToString());
                }
            }
        }
    }
}
