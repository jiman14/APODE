using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using JSonUtil;
using TControls;
using APODE_Core;

namespace TLogic
{
    class CLogic
    {
        #region initialize

        CSystem sys;                    // System class mainly contains errors & debugs behind objects
        TErrors errors;                 // Program error object
        TDebug debug;                   // Debug object
        CViewsManager vm;               // View manager for access all views and controls
        CGlobals Globals;               // Global vars from Main form view
        TProcess prc;                   // Running process info
        CView view;                     // View from where launched the event
        CEventDesc event_desc;    // Event description struct
        Control mainCtrl;               // Main Form or User control in the view that launch the event
        Control fireCtrl;               // Control who fire the event
        object[] args;                  // Control event args 

        CView.CtrlStruct fireCtrl_str; // Controls struct who fire the event


        /// <summary>
        /// Load all information from UI to work with
        /// </summary>
        /// <param name="viewManager"></param>
        /// <param name="eventDesc"></param>
        public CLogic(CSystem csystem, CViewsManager views_manager, CEventDesc eventDesc)
        {
            sys = csystem;
            vm = views_manager;
            Globals = sys.Globals;
            errors = sys.ProgramErrors;
            debug = sys.ProgramDebug;
            event_desc = eventDesc;
            if (event_desc != null)
            {
                view = (event_desc.View_Guid != "") ? vm.getFirstView_byGuid(event_desc.View_Guid): null;
                if (view != null)
                {
                    mainCtrl = view.mainControl();
                    fireCtrl_str = view.getCtrlStruct(eventDesc.Control_Name);
                }
                fireCtrl = (Control) event_desc.fireCtrl;
                args = event_desc.args;                
            }
        }
        #endregion

        #region Variables

        #region Check in & out & config

        /// <summary>
        /// Check all inputs in process
        /// </summary>
        /// <returns></returns>
        private bool CheckAllVariables()
        {
            bool no_Input_errors = true;
            foreach (JToken input in prc.BaseInputs)
            {
                if ((!input.ToString().StartsWith("-")) && (prc.Inputs[input.ToString()] == null))
                {
                    no_Input_errors = false;
                    errors.noInputs = String.Format("Error: no input '{0}' found in current program.", input.ToString());
                }
            }
            bool no_configs_errors = true;
            foreach (JToken conf in prc.BaseConfiguration)
            {
                if ((!conf.ToString().StartsWith("-")) && (prc.Configuration[conf.ToString()] == null))
                {
                    no_configs_errors = false;
                    errors.noConfiguration = String.Format("Error: no configuration '{0}' found in current program.", conf.ToString());
                }
            }            

            bool no_outputs_errors = true;
            foreach (JToken output in prc.BaseOutputs)
            {
                if ((!output.ToString().StartsWith("-")) && (prc.Outputs[output.ToString()] == null))
                {
                    no_outputs_errors = false;
                    errors.noOutputs = String.Format("Error: no output '{0}' found out current program.", output.ToString());
                }
            }

            return (no_Input_errors && no_configs_errors && no_outputs_errors);

        }

        #endregion
        
        #region Inputs management

        /// <summary>
        /// Gets program & views variables by name as object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Input(String name)
        {
            return Input(name, false);
        }
        /// <summary>
        /// Gets program & views variables by name as object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Input_nullable(String name)
        {
            return Input(name, true);
        }
        /// <summary>
        /// Gets program & views variables in jtoken node as jtoken
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JToken _Input_j(bool nullable, params String[] property_list)
        {
            JToken jItem = null;
            foreach (String prop in property_list)
            {
                if (jItem == null)
                {
                    object res = Input(prop, nullable);
                    if ((res as JToken) != null) jItem = res as JToken;
                    else jItem = new JValue(res);
                }
                else jItem = jItem[prop];
            }
            return jItem;
        }
        /// <summary>
        /// Gets program & views variables in jtoken node as jtoken
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JToken Input_j(params String[] property_list)
        {            
            return _Input_j(false, property_list);
        }
        /// <summary>
        /// Gets program & views variables in jtoken node as jtoken
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JToken Input_j_nullable(params String[] property_list)
        {
            return _Input_j(true, property_list);
        }
        /// <summary>
        /// Gets program & views variables in jtoken node as jarray
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JArray Input_jarray(params String[] property_list)
        {
            return Input_j(property_list) as JArray;
        }
        /// <summary>
        /// Gets program & views variables in jtoken node as string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public String Input_str(params String[] property_list)
        {
            try
            {
                return Input_j(property_list).ToString();
            }
            catch (Exception exc)
            {
                errors.noInputs = property_list[0].ToString() + ": Error" + exc.Message;
                return "";
            }
        }
        /// <summary>
        /// Gets program & views variables by name, get error only if MustExists is true
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Input(String name, bool nullable)
        {
            String var_name = (prc.Inputs[name] != null)? prc.Inputs[name].ToString(): "";
            if (!String.IsNullOrEmpty(var_name))
            {
                if (var_name.Contains("."))
                {
                    String[] tname = var_name.Split('.');
                    String var = tname[tname.Length - 1];
                    String source = var_name.Replace("." + var, "");

                    if (source.StartsWith(dGLOBALS.GLOBALS))
                    {
                        return Globals.get(source, var);
                    }
                    else
                    {
                        CView cv = ((view != null) && (source == view.Name)) ? view : vm.getFirstView(source);

                        if (cv != null)
                        {
                            if (cv.vars.Keys.Contains(var))
                            {
                                return cv.vars[var];
                            }
                            else
                            {
                                if (!nullable) errors.noInputs = String.Format("Error: Var: '{0}' not found in view: {1}", var, source);
                            }
                        }
                        else
                        {
                            if (!nullable) errors.noInputs = String.Format("Error: view {0} not found, and must content var: {1}", source, var);
                        }
                    }
                }
                else
                {
                    if (prc.vars.Keys.Contains(var_name))
                    {
                        return prc.vars[var_name];
                    }
                    else
                    {
                        if (!nullable) errors.noInputs = String.Format("Error: program var: '{0}' not found", var_name);
                    }
                }
            }
            return null;
        }
        #endregion       

        #region Configuration

        /// <summary>
        /// Get active process configuration by name (must exists)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Config(String name)
        {
            return Config(name, false);
        }
        /// <summary>
        /// Get active process configuration by name (can be nullable)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Config_nullable(String name)
        {
            return Config(name, true);
        }
        /// <summary>
        /// Get active process configuration by name (checking if reports an error)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Config(String name, bool nullable)
        {
            if (prc.Configuration[name] != null)
            {
                return prc.Configuration[name];
            }
            else if (!nullable)
            {
                errors.noConfiguration = String.Format("Error: configuration var '{0}' not found", name);
                return null;
            }
            else { return null; }
        }
        /// <summary>
        /// Get active process configuration by name as string (not nullable)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public String Config_str(String name)
        {
            return Config(name, false).ToString();
        }
        /// <summary>
        /// Get active process configuration by name as string (nullable)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public String Config_str_nullable(String name)
        {
            return Config(name, true).ToString();
        }
        /// <summary>
        /// Get active process configuration by name as int
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int Config_int(String name)
        {
            try
            {
                return int.Parse(Config(name, false).ToString());
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// Get active process configuration by name as int (nullable)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int Config_int_nullable(String name)
        {
            try
            {
                return int.Parse(Config(name, true).ToString());
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Get active process configuration by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object Config(int index)
        {
            if (prc.Configuration[index] != null)
            {
                return prc.Configuration[index];
            }
            else
            {
                errors.noConfiguration = String.Format("Error: configuration var index '{0}' not found", index);
                return null;
            }
        }
        #endregion

        #region Outputs

        /// <summary>
        /// Sets program and view vars by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Outputs(String name, object value)
        {
            String var_name = prc.Outputs[name].ToString();
            if (!String.IsNullOrEmpty(var_name))
            {
                if (var_name.Contains("."))
                {
                    String[] tname = var_name.Split('.');
                    String var = tname[tname.Length-1];
                    String source = var_name.Replace("." + var, "");

                    CView cv = ((view != null) && (source == view.Name)) ? view : vm.getFirstView(source);
                    if (cv != null) {
                        cv.vars[var] = value;
                    }
                    else
                    {
                        errors.noOutputs = String.Format("Error: view {0} not found, and must content var: {1}", source, var);
                    }
                }
                else
                {
                    prc.vars[var_name] = value;
                }
            }
        }

        #endregion

        #endregion

        #region Run process

        /// <summary>
        /// Prepare for execute process
        /// </summary>
        /// <param name="prc"></param>
        /// <returns></returns>
        public void Run(TProcess proc)
        {
            try
            {
                prc = proc;

                // Check inputs first
                if (CheckAllVariables())
                {
                    // init timer
                    DateTime prcInitTime = DateTime.Now;

                    // Execute process
                    debug.cprint(String.Format("Running process: '{0}', from program {1} and view {2}", prc.Guid, event_desc.Program, event_desc.View_Name));
                    execute_process(prc.Guid);

                    // set process time
                    prc.Total_time = Math.Round((DateTime.Now - prcInitTime).TotalSeconds, 2);
                }
            }
            catch (Exception exc)
            {
                errors.unhandledError = String.Format("Error in '{0}->{1}': {2}", event_desc.Program, prc.Guid, exc.Message);
                errors.removeErrors();
            }
        }

        #endregion

        /// <summary>
        /// Run single process
        /// </summary>
        /// <param name="process_guid"></param>
        private void execute_process(String process_guid)
        {
            switch (process_guid)
            {                
                #region Console

                case "Version":
                    {                        
                        Console.WriteLine("Version: " + prc.Version.ToString());
                    }
                    break;

                case "NewProgram":
                    {
                        if (args.Length > 0)
                        {
                            Console.WriteLine("New program: " + args[0].ToString());
                        }
                        else
                        {
                            Console.WriteLine("Use c.new program_name");
                        }
                    }
                    break;
                #endregion

                #region System

                case "Call":
                    {
                        // Get program from config (first) or input
                        String Program = (Config_str("program") == "") ? Input_str("program"): Config_str("program");
                        if (Program != "") vm.CallProgram(event_desc, Program);                        
                    }
                    break;

                case "Cron add program":
                    {
                        String Program = Config_str("program");
                        int Interval = Config_int("interval");
                        sys.Cron.addProgram(Program, Interval);                        
                    }
                    break;

                case "Cron remove program":
                    {
                        String Program = Config_str("program");                        
                        sys.Cron.removeProgram(Program);
                    }
                    break;

                case "SetVar":
                    {
                        if (Config_nullable("value") != null) Outputs("variable", Config("value"));                        
                        else Outputs("variable", Input("value"));                        
                    }
                    break;

                case "KeyboardHook register key":
                    {
                        vm.getMainView().MethodCall("global HookKey", "Register code for restore");                        
                    }
                    break;

                #endregion

                #region Math process
                case "pow":
                    {
                        double _input1 = (Double) Config(0);
                        double _input2 = (Double) Config(1);
                        double res = 0;
                        try
                        {
                            res = Math.Pow(_input1, _input2);
                        }
                        catch (Exception exc)
                        {
                            errors.processError = exc.Message;
                        }
                        Outputs("res", res);
                    }
                    break;
                case "ope":
                    {
                        String _operator = Config(0).ToString();
                        if (_operator == "")
                        {
                            _operator = prc.Default_Configuration[0].ToString();
                        }

                        int _input1 = (int) Config(1);
                        int _input2 = (int) Config(2);
                        int res = 0;
                        try
                        {
                            switch (_operator)
                            {
                                case "sum":
                                    res = _input1 + _input2;
                                    break;
                            }
                        }
                        catch (Exception exc)
                        {
                            errors.processError = exc.Message;
                        }
                        Outputs("res", res);
                    }
                    break;
                #endregion

                #region Manage controls

                #region get / set control properties (generic)

                case "getControlProperty":
                    {
                        try
                        {
                            String control_name = Config_str("control_name");
                            String view_name = Config_str("view_name");
                            String property_name = Config_str("property");

                            Control ctrl = fireCtrl;
                            if ((control_name != "") && (view_name != ""))
                            {
                                // Get control from view
                                CView tempView = vm.getFirstView(view_name);
                                ctrl = tempView.getCtrl(control_name);
                            }

                            Type tc = ctrl.GetType();
                            PropertyInfo pi = tc.GetProperty(property_name);
                            if (pi != null)
                            {
                                object c = (object)ctrl;
                                Outputs("value", new JObject(pi.GetValue(c)));
                                Outputs("type", pi.PropertyType.ToString());
                            }
                            else
                            {
                                errors.addError(String.Format("property {0} not exists in control {1}", property_name, ctrl.Name));
                            }
                        }
                        catch (Exception exc)
                        {
                            errors.addError("Unhandled error: " + exc.Message);
                        }
                    }
                    break;

                case "setControlProperty":
                    {
                        try
                        {
                            String control_name = Config_str("control_name");
                            String view_name = Config_str("view_name");
                            String property_name = Config_str("property");

                            Control ctrl = fireCtrl;
                            if ((control_name != "") && (view_name != ""))
                            {
                                // Get control from view
                                CView tempView = vm.getFirstView(view_name);
                                ctrl = tempView.getCtrl(control_name);
                            }

                            // Dynamic set control property from JObject
                            new CView(sys).SetControlProperty(ref ctrl, property_name, Input_j("value"));
                        }
                        catch (Exception exc)
                        {
                            errors.addError("Unhandled error: " + exc.Message);
                        }

                    }
                    break;
                #endregion

                #region get / set Vars list in controls

                case "getVarFromControl":
                    {
                        String control_name = Config_str("control_name");
                        String view_name = Config_str("view_name");
                        String var_name = Config_str("var_name");

                        CView.CtrlStruct ctrl_str = fireCtrl_str;

                        if ((control_name != "") && (view_name != ""))
                        {
                            // Get control from view
                            CView tempView = vm.getFirstView(view_name);
                            ctrl_str = tempView.getCtrlStruct(control_name);
                        }

                        Outputs("value", new JObject(ctrl_str.vars[var_name]));

                    }
                    break;

                case "setVarInControl":
                    {
                        String control_name = Config_str("control_name");
                        String view_name = Config_str("view_name");
                        String var_name = Config_str("var_name");

                        CView.CtrlStruct ctrl_str = fireCtrl_str;

                        if ((control_name != "") && (view_name != ""))
                        {
                            // Get control from view
                            CView tempView = vm.getFirstView(view_name);
                            ctrl_str = tempView.getCtrlStruct(control_name);
                        }

                        ctrl_str.vars[var_name] = new JObject(Input("value"));

                    }
                    break;
                #endregion

                #endregion

                #region Notes Logic

                #region Get info from notes

                case "UpdateJson":
                    {
                        String property = Config_str("property");
                        (Input_j("jsonData"))[property] = Input_j("value");

                        Outputs("jsonData", Input_j("jsonData"));
                    }
                    break;
                case "Get notes":
                    {
                        Outputs("AppStatus", "LOADING");

                        String path = Input_str("path");                            
                        String fileName = Input_str("file name");
                        String status = Input_str("status");
                        JObject Notes = Input("Notes", true) as JObject;
                        String notes_array = (status == "Scheduled") ? "Active" : status;

                        if ((Notes == null) || Notes[notes_array] == null)
                        {
                            JSonFile jFile = new JSonFile(Globals.debug, Globals.AppData_path(path), fileName, true);
                            if (!jFile.hasErrors())
                            {
                                if (Notes == null) Notes = new JObject();
                                // Actives & scheduled where in same array                                                                
                                if (jFile.jActiveObj["Notes"].Count() > 0)
                                {
                                    Notes.Add(status, jFile.jActiveObj["Notes"][notes_array]);
                                }
                                else
                                {
                                    Notes.Add(status, new JArray());
                                }
                                Outputs("Notes", Notes);
                            }
                            else
                            {
                                errors.processError = jFile.jErrors;
                            }
                        }
                    }
                    break;
                case "Update note in notes":
                    {
                        JToken Note = Input_j_nullable("Note");
                        if (!String.IsNullOrEmpty(Note.ToString()))
                        {
                            JToken Notes = Input_j("Notes");
                            String noteGuid = Note["Guid"].ToString();
                            String status = Input_str("status");
                            String notes_array = (status == "Scheduled") ? "Active" : status;

                            // replace active note by guid                            
                            Notes[notes_array].SelectToken(String.Format("$[?(@.Guid == '{0}')]", noteGuid)).Remove();

                            String note_status = (Note["Status"].ToString() == "Scheduled") ? "Active" : Note["Status"].ToString();
                            if ((Notes[note_status] == null) || (Notes[note_status].ToString() == ""))
                            {
                                Notes[note_status] = new JArray(Note);
                            }
                            else
                            {
                                ((JArray)Notes[note_status]).Add(Note);
                            }

                            Outputs("Notes", Notes);
                        }
                    }
                    break;

                case "Get categories":
                    {
                        String path = Input_str("path");
                        String fileName = Input_str("file name");

                        JSonFile jFile =new JSonFile(Globals.debug, Globals.AppData_path(path), fileName, true);
                        JArray Categories = new JArray();
                        if (!jFile.hasErrors())
                        {
                            Categories = new JArray(jFile.jActiveObj.SelectToken("$.Categories").ToArray());                            
                        }
                        else
                        {
                            errors.processError = jFile.jErrors;
                        }
                        Outputs("Categories", Categories);
                    }
                    break;

                case "Set categories":
                    {
                        String path = Input_str("path");
                        String fileName = Input_str("file name");
                        JArray Categories = Input_jarray("Categories");

                        JSonFile jFile =new JSonFile(Globals.debug, Globals.AppData_path(path), fileName, true);
                        if (!jFile.hasErrors())
                        {
                            jFile.jActiveObj["Categories"] = Categories;
                            jFile.Write();
                        }
                        else
                        {
                            errors.processError = jFile.jErrors;
                        }
                    }
                    break;

                case "New note":
                    {                        
                        JObject Note = new JObject();
                        Note.Add("Guid", Guid.NewGuid().ToString());
                        Note.Add("Datecreated", DateTime.Now.ToString());
                        Note.Add("Datetime", DateTime.Now.ToString());
                        Note.Add("IdUser", "");
                        Note.Add("Title", "");                        
                        Note.Add("Note", "");
                        Note.Add("Order", -1);
                        Note.Add("Category", "");
                        Note.Add("Color", "White");
                        Note.Add("Status", "Active");
                        JObject Schedule = new JObject();
                        Schedule.Add("ActivationDate", "");
                        Schedule.Add("description", "");
                        Note.Add("Schedule", Schedule);

                        JToken Notes = Input_j("Notes");
                        ((JArray)Notes["Active"]).Add(Note);
                        Outputs("Notes", Notes);
                        Outputs("SaveFlag", bool.TrueString);
                    }
                    break;

                case "Delete note":
                    {
                        if (MessageBox.Show(view.mainControl(), "¿Desea eliminar esta nota?", "Confirmación", MessageBoxButtons.YesNo)
                            == DialogResult.Yes)
                        {
                            String status = Input_str("status");
                            JToken Notes = Input_j("Notes");
                            JToken Note = view.var_j("Note");
                            String NoteGuid = Note["Guid"].ToString();
                            String notes_array = (status == "Scheduled") ? "Active" : status;
                            Notes[notes_array].SelectToken(String.Format("$[?(@.Guid == '{0}')]", NoteGuid)).Remove();
                            Outputs("Notes", Notes);
                            Outputs("DeleteNoteGuid", NoteGuid);
                            Outputs("SaveFlag", bool.TrueString);
                        }
                    }
                    break;
                case "Free note":
                    {                        
                        if (Input("DeleteNoteGuid") != null)
                        {
                            String NoteGuid = Input_str("DeleteNoteGuid");
                            Dictionary<String, CView> NotesLoaded = (Dictionary<String, CView>)Input("NotesLoaded");

                            String DeleteNoteGuid = NotesLoaded[NoteGuid].guid;
                            NotesLoaded.Remove(NoteGuid);

                            vm.removeViews_byGuid(DeleteNoteGuid);
                            Outputs("NotesLoaded", NotesLoaded);
                        }
                        
                    }
                    break;
                #endregion

                #region Save note data
                case "Save notes":
                    {
                        bool SaveFlag = false;
                        SaveFlag = (bool.TryParse(Input_str("SaveFlag"), out SaveFlag))? SaveFlag: (Input_j("SaveFlag")).ToObject<bool>();
                        if (SaveFlag)
                        {
                            // Read inputs
                            String path = Input_str("path");
                            String fileName = Input_str("file name");
                            JToken Notes = Input_j("Notes");

                            JSonFile jFile =new JSonFile(Globals.debug, Globals.AppData_path(path), fileName, true);
                            if (jFile.hasErrors())
                            {
                                errors.processError = jFile.jErrors;
                            }
                            else 
                            {
                                jFile.jActiveObj["Notes"]["Active"] = Notes["Active"];
                                if (Notes["Completed"] != null)
                                {
                                    jFile.jActiveObj["Notes"]["Completed"] = Notes["Completed"]; 
                                }
                                jFile.Write();
                                Outputs("SaveFlag", bool.FalseString);                               
                            }
                        }
                        else
                        {
                            Outputs("Message", "");
                        }
                    }
                    break;

                #endregion

                #region Complete note
                case "Complete note":
                    {
                        String status = Input_str("status");
                        JToken Note = view.var_j("Note");
                        String noteGuid = Note["Guid"].ToString();

                        String ScheduleDate = Note["Schedule"]["ActivationDate"].ToString();
                        // Scheduled note free
                        if ((status != "Scheduled") || (MessageBox.Show((UserControl)view.mainControl(),
                            "Al completar la nota eliminará la activación programada el " + ScheduleDate,
                            "Confirmar completar nota", MessageBoxButtons.YesNo) == DialogResult.Yes))
                        {
                            // Free Scheduled data
                            JObject Schedule = new JObject();
                            Schedule.Add("ActivationDate", "");
                            Schedule.Add("description", "");
                            Note["Schedule"] = Schedule;

                            // Set modification time
                            Note["Datetime"] = DateTime.Now.ToString();

                            //Change status
                            Note["Status"] = (status == "Completed") ? "Active" : "Completed";

                            // hide scheduled note from active
                            Dictionary<String, CView> NotesLoaded = (Dictionary<String, CView>)Input("NotesLoaded");
                            NotesLoaded[noteGuid].mainControl().Visible = status == Note["Status"].ToString();
                            NotesLoaded[noteGuid].vars["Note"] = Note;                            

                            // Save note
                            Outputs("Note", Note);
                        }
                    }
                    break;
                #endregion

                #region Check scheduled notes 
                case "Check schedules notes":
                    {
                        JToken Notes = Input_j("Notes");
                        // Load form state
                        String FormState = Input_str("FormState");

                        List<JToken> ScheduledNotes = Notes["Active"].SelectTokens(String.Format("$[?(@.Status == 'Scheduled')]")).ToList();
                        int removedNotes = 0;

                        foreach (JToken Note in ScheduledNotes)
                        {
                            try
                            {
                                if (DateTime.Parse(Note["Schedule"]["ActivationDate"].ToString()) < DateTime.Now)
                                {
                                    removedNotes = 1;
                                    String noteGuid = Note["Guid"].ToString();
                                    // if form state ballon popup
                                    String desc = Note["Schedule"]["description"].ToString();

                                    if (FormState == "Minimize")
                                    {
                                        vm.getMainView().MethodCall("App task icon", "ShowBalloonTip");
                                    }
                                    else
                                    {
                                        String msg = "Se ha activado una nota programada.";
                                        if (desc != "") msg += "\n\nMensaje:\n" + desc;
                                        MessageBox.Show(msg, "Notificación programada");
                                    }
                                    Note["Schedule"]["ActivationDate"] = "";
                                    Note["Schedule"]["description"] = "";
                                    Note["Status"] = "Active";
                                    Note["Datetime"] = DateTime.Now.ToString();
                                    Note["Order"] = -1;

                                    Outputs("Note", Note);
                                    Outputs("SaveFlag", bool.TrueString);                             
                                }
                            }
                            catch { }
                        }

                        // stop cron if there isn't notes scheduled
                        if ((ScheduledNotes.Count - removedNotes) <= 0)
                        {
                            sys.Cron.removeProgram(event_desc.Program);
                        }

                    }
                    break;
                #endregion

                #endregion

                #region UI

                #region Generics

                case "Load Context Menu":
                    {
                        if (((view.Name == "SimpleNote") || (view.Name == "MainView")) && (view.getCtrl("contextMenu") == null))
                        {
                            ContextMenuStrip cMenu = (ContextMenuStrip)view.CreateControl("contextMenu");

                            // Load toolstrip menu
                            CView.CtrlStruct contextMenu_str = view.getCtrlStruct("contextMenu");

                            JArray jItems = (JArray)contextMenu_str.config["Menu items"];

                            foreach (JToken jItem in jItems)
                            {
                                ToolStripMenuItem tsmi = new ToolStripMenuItem(jItem["Name"].ToString());                                
                                object o = tsmi;            
                                
                                // Set image              
                                view.SetControlProperty(ref o, "Image", jItem["Image"].ToString());

                                // Set shortcut keys
                                if (jItem["ShortcutKeys"] != null)
                                {
                                    view.SetControlProperty(ref o, "ShortcutKeys", jItem["ShortcutKeys"]);
                                }

                                //region set event handler
                                view.Set_event_handler(tsmi, jItem["Name"].ToString(), "Click", jItem["Program"].ToString());

                                // Add menú item
                                cMenu.Items.Add(tsmi);                                
                            }
                            
                        }
                    }
                    break;

                case "Set shortcut keys":
                    {                        
                        String Program = "";
                        KeyEventArgs e = (KeyEventArgs)args[0];
                        KeysConverter kc = new KeysConverter();
                        
                        // Get actions and compare with event args
                        JObject jActions = Config("Actions") as JObject;

                        foreach (JProperty jp in jActions.Properties())
                        {
                            if (kc.ConvertToString(e.KeyCode) != "Menu")
                            {
                                Keys KeyCode = (Keys)kc.ConvertFromString(jp.Value["Keycode"].ToString());
                                Keys ModKey = (Keys)kc.ConvertFromString(jp.Value["Modifiers"].ToString());
                                
                                if (e.KeyData == (KeyCode | ModKey) )
                                {
                                    Program = jp.Value["Program"].ToString();
                                    break;
                                }
                            }
                        }
                        // Output program to call
                        Outputs("Program", Program);
                    }
                    break;

                #endregion

                #region Main app

                #region App manager
                case "ReloadApp":
                    {
                        vm.removeViews("SimpleNote");
                        vm.removeViews("NotesPanel");
                        view.getCtrl("main_panel").Controls.Clear();
                    }
                    break;

                case "Exit":
                    {
                        vm.removeViews("SimpleNote");
                        vm.removeViews("NotesPanel");
                        vm.getMainForm().Close();
                    }
                    break;
                #endregion

                #region Manage notes

                case "ClearNotes":
                    {
                        // Load current status
                        String status = Input_str("status");
                        // Load all notes and notes with controls loaded 
                        Dictionary<String, CView> NotesLoaded = (Input_nullable("NotesLoaded") == null) ?
                            new Dictionary<string, CView>() :
                            (Dictionary<String, CView>)Input("NotesLoaded");

                        foreach (CView vSimpleNote in NotesLoaded.Values.ToList())
                        {
                            vSimpleNote.mainControl().Visible = false;
                        }                        
                    }
                    break;
                case "CreateViewNotes":
                    {
                        // Load all notes and notes with controls loaded 
                        JToken Notes = Input_j("Notes");
                        Dictionary<String, CView> NotesLoaded = (Input_nullable("NotesLoaded") == null) ?
                            new Dictionary<string, CView>() :
                            (Dictionary<String, CView>)Input("NotesLoaded");
                        
                        // Load current status
                        String status = Input_str("status");

                        // get NotesPanel's view or create if not exists
                        CView vNotesPanel = vm.getFirstView("NotesPanel");
                        if (vNotesPanel == null)
                        {
                            vNotesPanel = vm.AddView("NotesPanel");
                            Control cMainPanel = vm.getMainView().getCtrl("main_panel");
                            cMainPanel.Controls.Add(vNotesPanel.mainControl());
                        }
                        FlowLayoutPanel cNotePanel = (FlowLayoutPanel) vNotesPanel.getCtrl("notes_panel");

                        // Show or create all notes from current status
                        String notes_array = (status == "Scheduled") ? "Active" : status;
                        int added = 0;
                        if (Notes[notes_array] != null)
                        {
                            foreach (JToken Note in Notes[notes_array].OrderBy(c => c["Order"]))
                            {
                                if (status == Note["Status"].ToString())
                                {
                                    CView vNotePanel = null;
                                    // Get or create simple note
                                    if (NotesLoaded.Keys.Contains(Note["Guid"].ToString()))
                                    {
                                        vNotePanel = NotesLoaded[Note["Guid"].ToString()];
                                        // Add Note as var
                                        vNotePanel.vars["Note"] = Note;
                                        // Show note
                                        vNotePanel.mainControl().Visible = status == Note["Status"].ToString();
                                        added += (vNotePanel.mainControl().Visible) ? 1 : 0;
                                    }
                                    else
                                    {
                                        vNotePanel = vm.AddView("SimpleNote");
                                        // Add Note as var
                                        vNotePanel.vars.Add("Note", Note);
                                        // Add simple note to notes panel container                                    
                                        cNotePanel.Controls.Add(vNotePanel.mainControl());
                                        // Add note to notes controls loaded list
                                        NotesLoaded.Add(Note["Guid"].ToString(), vNotePanel);
                                        added++;
                                    }
                                    // Visibilize complete button in note
                                    vNotePanel.getCtrl("bComplete").Visible = status != "Scheduled";
                                    // Visibilize schedule button in note
                                    vNotePanel.getCtrl("bSchedule").Visible = status != "Completed";
                                    // Load title
                                    ((Label)vNotePanel.getCtrl("label_head")).Text = Note["Title"].ToString();
                                    // Load text
                                    ((RichTextBox)vNotePanel.getCtrl("text_edit")).Rtf = Note["Note"].ToString();
                                    // Load context menu in rich text editor                                                                        
                                    ((RichTextBox)vNotePanel.getCtrl("text_edit")).ContextMenuStrip = (ContextMenuStrip)vNotePanel.getCtrl("contextMenu");
                                    // Load Color
                                    String str_color =
                                        (status == "Scheduled")? Input_str("Scheduled notes color"):
                                            (status == "Completed")? Input_str("Completed notes color"):
                                                Note["Color"].ToString();                                    
                                                                            
                                    ((RichTextBox)vNotePanel.getCtrl("text_edit")).BackColor = (str_color != "") ?                                       
                                        System.Drawing.ColorTranslator.FromHtml(str_color) :
                                        System.Drawing.Color.FromName("White");
                                }
                            }
                        }                        
                        // Visibilize empty notes panels
                        vNotesPanel.getCtrl("empty_note_completed").Visible = (status == "Completed") && ((Notes[notes_array] == null) || (Notes[notes_array].Count() == 0));
                        vNotesPanel.getCtrl("empty_note_scheduled").Visible = (status == "Scheduled") && (added == 0);                            
                        
                        // Set appstatus to ready
                        Outputs("AppStatus", "READY");
                        Outputs("NotesLoaded", NotesLoaded);
                    }
                    break;
                #endregion

                #region Menu, buttons & windows position

                case "ResetStatusButtons":
                    {
                        String status = Input_str("status");                        

                        // Set home button image
                        if (view.Name != "SimpleNote")
                        {
                            if (status == "Active") view.ConfigureControl("bHome", "Selected");
                            else view.ConfigureControl("bHome");
                        }
                        else
                        {
                            status = ((JObject)view.vars["Note"])["Status"].ToString();
                        }

                        // Set complete button image
                        if (status == "Completed") view.ConfigureControl("bComplete", "Selected");
                        else view.ConfigureControl("bComplete");

                        // Set scheduled button image
                        if (status == "Scheduled") view.ConfigureControl("bSchedule", "Selected");
                        else view.ConfigureControl("bSchedule");                      
                    }
                    break;
                case "Ver menú contextual principal":
                    {
                        ContextMenuStrip cMenu = (ContextMenuStrip)view.getCtrl("contextMenu");
                        cMenu.Show(fireCtrl, 0, fireCtrl.Height);
                    }
                    break;


                case "Change windows position":
                    {
                        Form mainForm = vm.getMainForm();
                        String State = (Config("State") != null) ? Config_str("State") : "";
                        if (State == "")
                        {
                            State = Input_str("FormState");
                        }
                        Outputs("FormState", State);

                        if (State == "Maximize")
                        {
                            CView vNotesPanel = vm.getFirstView("NotesPanel");
                            ((FlowLayoutPanel)vNotesPanel.getCtrl("notes_panel")).AutoSize = false;
                            ((FlowLayoutPanel)vNotesPanel.getCtrl("notes_panel")).Dock = DockStyle.Fill;

                            mainForm.Top = 0;
                            mainForm.Height = Screen.GetWorkingArea(mainForm).Height;
                            mainForm.Width = Screen.GetWorkingArea(mainForm).Width;
                        }
                        else if (State == "Minimize")
                        {
                            vm.getMainView().ConfigureObject("App task icon", "Icon visible");
                            vm.getMainView().MethodCall("App task icon", "Show minimize balloon tip");
                            ((Form)mainForm).Hide();
                        }
                        else
                        {
                            vm.getMainView().ConfigureObject("App task icon");
                            CView vNotesPanel = vm.getFirstView("NotesPanel");

                            FlowLayoutPanel notes_panel = ((FlowLayoutPanel)vNotesPanel.getCtrl("notes_panel"));                                
                            Panel menu_panel = ((Panel)vm.getMainView().getCtrl("menu_panel"));
                            notes_panel.AutoSize = true;                            
                            int total_width = menu_panel.Width + notes_panel.Width;

                            int SCROLL_SIZE = 18;
                            if (total_width > Screen.GetWorkingArea(mainForm).Width)
                            {
                                mainForm.Height = notes_panel.Height + SCROLL_SIZE;
                                mainForm.Top = Screen.GetWorkingArea(mainForm).Height - mainForm.Height;
                                mainForm.Left = 0;
                                mainForm.Width = Screen.GetWorkingArea(mainForm).Width;
                            }
                            else
                            {
                                mainForm.Height = notes_panel.Height;
                                mainForm.Top = (Screen.GetWorkingArea(mainForm).Height) - mainForm.Height;
                                mainForm.Left = 0;
                                mainForm.Width = total_width;
                            }

                            notes_panel.Dock = DockStyle.None;
                            if (!(vm.getMainForm().Visible))
                                vm.getMainForm().Show();
                        }
                    }

                    break;
                #endregion

                #endregion

                #region Simple Note UI

                #region Menu & buttons
                case "ResetNoteButtons":
                    {
                        String Note_status = (Input_j("Note"))["Status"].ToString();
                        
                        // Complete button
                        if (Note_status == "Scheduled")
                        {
                            // Scheduled notes can not be completed. Removing button
                            view.getCtrl("bComplete").Visible = false;                             
                        }
                        else
                        {
                            // Set active image
                            if (Note_status == "Completed") view.ConfigureControl("bComplete", "Selected");
                            else view.ConfigureControl("bComplete");
                            view.getCtrl("bComplete").Visible = true;
                        }

                        // Scheduled button                        
                        if (Note_status == "Completed")
                        {
                            // Completes notes can not Scheduled. Removing button
                            view.getCtrl("bSchedule").Visible = false;                             
                        }
                    }
                    break;
                case "Note ContextMenu Clicked":
                    {
                        MessageBox.Show((view.var_j("Note"))["Note"].ToString());
                    }
                    break;
                
                case "Ver menú contextual de nota":
                    {
                        ContextMenuStrip cMenu = (ContextMenuStrip)view.getCtrl("contextMenu");
                        cMenu.Show(fireCtrl, 0, fireCtrl.Height);
                    }
                    break;
                #endregion

                #region Set property

                case "Set category":
                    {                        
                        String noteGuid = Input_str("NoteGuid");
                        JToken note = (JToken)vm.getFirstView_byGuid(noteGuid).vars["Note"];

                        note["Category"] = ((TextBox)view.getCtrl("text_prop")).Text;
                        note["Datetime"] = DateTime.Now.ToString();
                        Outputs("Note", note);
                        ((Form)mainCtrl).Close();                                
                    }
                    break;

                case "Set title":
                    {
                        String noteGuid = Input_str("NoteGuid");
                        JToken note = (JToken)vm.getFirstView_byGuid(noteGuid).vars["Note"];

                        note["Title"] = ((TextBox)view.getCtrl("text_prop")).Text;
                        note["Datetime"] = DateTime.Now.ToString();
                        Outputs("Note", note);
                        ((Form)view.mainControl()).Close();
                    }
                    break;

                case "Text changed":
                    {
                        JToken Note = Input_j("Note");
                        if (Note["Note"].ToString() != ((RichTextBox)view.getCtrl("text_edit")).Rtf)
                        {
                            Note["Note"] = ((RichTextBox)view.getCtrl("text_edit")).Rtf;
                            Note["Datetime"] = DateTime.Now.ToString();
                            Outputs("SaveFlag", bool.TrueString);
                        }
                        Outputs("Note", Note);
                    }
                    break;
                #endregion

                #region Copy & Paste, Drag & Drop 

                case "Copy note text":
                    {
                        Clipboard.SetText(((RichTextBox)view.getCtrl("text_edit")).Rtf);
                    }
                    break;
                case "Copy note plain text":
                    {
                        Clipboard.SetText(((RichTextBox)view.getCtrl("text_edit")).Rtf);
                    }
                    break;
                case "Paste note text":
                    {
                    }
                    break;
                case "Begin drag":
                    {
                        DragEventArgs e = (DragEventArgs)args[0];
                        e.Effect = DragDropEffects.Move;
                    }
                    break;

                case "SimpleNote mouse move":
                    {
                        MouseEventArgs e = (MouseEventArgs)args[0];
                        if (e.Button == MouseButtons.Left)
                        {
                            Control p = fireCtrl.Parent;
                            while ((p != null) && (p.Name != "SimpleNote"))
                            {
                                p = p.Parent;
                            }
                            UserControl uNote = p as UserControl;

                            uNote.DoDragDrop(uNote, DragDropEffects.Move);
                        }
                    }
                    break;

                case "Drop":
                    {
                        DragEventArgs e = (DragEventArgs)args[0];
                        Control p = fireCtrl;
                        while ((p != null) && (p.Name != "SimpleNote"))
                        {
                            p = p.Parent;
                        }
                        if (p != null)
                        {
                            UserControl uNote = p as UserControl;
                            int targetIndex = (view.var_j("Note"))["Order"].ToObject<int>();
                            if (targetIndex != -1)
                            {
                                string TSimpleNote = typeof(UserControl).FullName;
                                if (e.Data.GetDataPresent(TSimpleNote))
                                {
                                    // get NotesPanel's view or create if not exists
                                    CView vNotesPanel = vm.getFirstView("NotesPanel");
                                    FlowLayoutPanel cNotePanel = (FlowLayoutPanel)vNotesPanel.getCtrl("notes_panel");
                                    UserControl source = e.Data.GetData(TSimpleNote) as UserControl;

                                    //int sourceIndex = this.FindPictureBoxIndex(source);

                                    if (targetIndex != -1)
                                        cNotePanel.Controls.SetChildIndex(source, targetIndex);
                                    
                                }
                            }
                        }

                    }
                    break;

                case "Save reordering notes":
                    {                        
                        JToken Notes = Input_j("Notes");
                        String status = Input_str("status");
                        Dictionary<String, CView> NotesLoaded = (Dictionary<String, CView>)Input("NotesLoaded");

                        CView vNotesPanel = vm.getFirstView("NotesPanel");
                        FlowLayoutPanel cNotePanel = (FlowLayoutPanel)vNotesPanel.getCtrl("notes_panel");

                        foreach (CView uNote in NotesLoaded.Values.ToList())
                        {
                            if (uNote.mainControl().Visible)
                            {
                                String NoteGuid = ((JToken)uNote.vars["Note"])["Guid"].ToString();
                                String notes_array = (status == "Scheduled") ? "Active" : status;

                                int orden = cNotePanel.Controls.GetChildIndex(uNote.mainControl());

                                Notes[notes_array].SelectToken(String.Format("$[?(@.Guid == '{0}')]", NoteGuid))["Order"] = orden;
                            }
                        }

                        Outputs("Notes", Notes);
                        Outputs("SaveFlag", bool.TrueString);
                    }
                    break;

                case "Reordering notes":
                    {
                        JToken Notes = Input_j("Notes");
                        String status = Input_str("status");
                        Dictionary<String, CView> NotesLoaded = (Dictionary<String, CView>)Input("NotesLoaded");

                        CView vNotesPanel = vm.getFirstView("NotesPanel");
                        FlowLayoutPanel cNotePanel = (FlowLayoutPanel)vNotesPanel.getCtrl("notes_panel");

                        JToken Note = Notes["Active"].SelectToken("$[?(@.Order == -1)]");
                        if (Note != null)
                        {
                            String NoteGuid = Note["Guid"].ToString();
                            cNotePanel.Controls.SetChildIndex(NotesLoaded[NoteGuid].mainControl(), 0);
                            foreach (CView uNote in NotesLoaded.Values.ToList())
                            {
                                if (uNote.mainControl().Visible)
                                {
                                    NoteGuid = ((JToken)uNote.vars["Note"])["Guid"].ToString();
                                    String notes_array = (status == "Scheduled") ? "Active" : status;

                                    int orden = cNotePanel.Controls.GetChildIndex(uNote.mainControl());

                                    Notes[notes_array].SelectToken(String.Format("$[?(@.Guid == '{0}')]", NoteGuid))["Order"] = orden;
                                }
                            }

                            Outputs("Notes", Notes);
                        }
                    }
                    break;
                #endregion

                #region Rich text box & send email

                case "RichTextBox link clicked":
                    {
                        LinkClickedEventArgs e = (LinkClickedEventArgs)args[0];
                        System.Diagnostics.Process.Start(e.LinkText);
                    }
                    break;

                case "Open note in word":
                    {
                        String path = Input_str("path");
                        String fileName = Input_str("file name");

                        String file_path = System.IO.Path.Combine(Globals.AppData_path(path).FullName, fileName);
                        String richText = ((RichTextBox)view.getCtrl("text_edit")).Rtf;
                        vm.getMainView().MethodCall("OpenXML Document", "Create word document",
                            new object[] { file_path, richText });
                    }
                    break;

                case "Send email from default client":
                    {
                        JToken Note = Input_j("Note");

                        System.Diagnostics.Process proc = new System.Diagnostics.Process();                        
                        proc.StartInfo.FileName = "mailto:?subject=Asunto de la nota: " + Note["Title"].ToString() + 
                            "&body=" + ((RichTextBox)view.getCtrl("text_edit")).Text;

                        proc.Start();
                    }
                    break;

                #endregion

                #endregion

                #region Auxiliar panels

                #region Info panel
                case "Load note info":
                    {
                        JToken note = Input_j("Note");

                        vm.removeViews("HelperForms.NoteInfo");
                        CView vInfoForm = vm.AddView("HelperForms.NoteInfo");

                        ((Label)vInfoForm.getCtrl("text_title")).Text = note["Title"].ToString();
                        ((Label)vInfoForm.getCtrl("text_fecha_c")).Text = note["Datecreated"].ToString();
                        ((Label)vInfoForm.getCtrl("text_fecha_m")).Text = note["Datetime"].ToString();
                        ((Label)vInfoForm.getCtrl("text_category")).Text = note["Category"].ToString();
                        ((Label)vInfoForm.getCtrl("text_status")).Text = note["Status"].ToString();

                        Form fInfo = (Form)vInfoForm.mainControl();
                        fInfo.Show();
                    }
                    break;
                #endregion

                #region Color panel
                case "Change note color":
                    {
                        JToken Note = Input_j("Note");

                        int argb_color = 0;
                        ColorDialog cd = new ColorDialog();

                        cd.Color = (Note["Color"].ToString() != "") ?
                            (
                                (int.TryParse(Note["Color"].ToString(), out argb_color)) ? System.Drawing.Color.FromArgb(argb_color) :
                                System.Drawing.Color.FromName(Note["Color"].ToString())
                            ) :
                            System.Drawing.Color.FromName("White");

                        if (cd.ShowDialog() == DialogResult.OK)
                        {
                            Note["Color"] = cd.Color.ToArgb();
                            Outputs("SaveFlag", bool.TrueString);
                        }
                        Outputs("Note", Note);
                    }
                    break;
                #endregion

                #region Title panel
                case "Load title panel":
                    {
                        JToken note = Input_j("Note");
                        vm.removeViews("HelperForms.SetTitlePanel");
                        CView vInfoForm = vm.AddView("HelperForms.SetTitlePanel");

                        ((Label)vInfoForm.getCtrl("label_prop")).Text = "Título";
                        ((TextBox)vInfoForm.getCtrl("text_prop")).Text = note["Title"].ToString();
                        ((TextBox)vInfoForm.getCtrl("text_prop")).MaxLength = 15;

                        Outputs("NoteGuid", view.guid);
                        Form fInfo = (Form)vInfoForm.mainControl();
                        fInfo.Show();
                    }
                    break;
                #endregion

                #region Schedule panel

                case "Load schedule panel":
                    {
                        JToken note = Input_j("Note");

                        vm.removeViews("HelperForms.SchedulePanel");
                        CView cSchedulePanel = vm.AddView("HelperForms.SchedulePanel");

                        ((TextBox)cSchedulePanel.getCtrl("text_fecha")).Text = (note["Schedule"]["ActivationDate"].ToString() != "") ?
                            DateTime.Parse(note["Schedule"]["ActivationDate"].ToString()).ToShortDateString() :
                            DateTime.Now.ToShortDateString();
                        ((TextBox)cSchedulePanel.getCtrl("text_hora")).Text = (note["Schedule"]["ActivationDate"].ToString() != "") ?
                            DateTime.Parse(note["Schedule"]["ActivationDate"].ToString()).ToShortTimeString() :
                            DateTime.Now.ToShortTimeString();
                        ((TextBox)cSchedulePanel.getCtrl("text_desc")).Text = note["Schedule"]["description"].ToString();

                        Outputs("NoteGuid", view.guid);

                        Form fSchedulePanel = (Form)cSchedulePanel.mainControl();
                        fSchedulePanel.Show();
                    }
                    break;

                case "Schedule note":
                    {
                        String SimpleNoteGuid = Input_str("NoteGuid");
                        JToken Note = (JToken)vm.getFirstView_byGuid(SimpleNoteGuid).vars["Note"];
                        String noteGuid = Note["Guid"].ToString();                        
                        String status = Input_str("status");
                        String action = Config_str("Action");

                        if (action == "Set")
                        {
                            String date = ((TextBox)view.getCtrl("text_fecha")).Text + " " + ((TextBox)view.getCtrl("text_hora")).Text;
                            DateTime dSchedule = DateTime.Now;
                            if (DateTime.TryParse(date, out dSchedule))
                            {
                                Note["Schedule"]["ActivationDate"] = date;
                            }
                            else
                            {
                                if (DateTime.TryParse(((TextBox)view.getCtrl("text_fecha")).Text, out dSchedule))
                                    MessageBox.Show("Fecha no válida. Formato (dd/mm/aaaa). Ej: 31/01/2016");
                                else MessageBox.Show("Formato de hora no válido. Ej: 17:45");
                            }
                            Note["Schedule"]["description"] = ((TextBox)view.getCtrl("text_desc")).Text;
                            Note["Status"] = "Scheduled";
                        }
                        else
                        {
                            Note["Schedule"]["ActivationDate"] = "";
                            Note["Schedule"]["description"] = "";
                            Note["Status"] = "Active";
                        }
                        Note["Datetime"] = DateTime.Now.ToString();

                        // hide scheduled note from active
                        Dictionary<String, CView> NotesLoaded = (Dictionary<String, CView>)Input("NotesLoaded");
                        NotesLoaded[noteGuid].mainControl().Visible = status == Note["Status"].ToString();
                        NotesLoaded[noteGuid].vars["Note"] = Note;

                        // Save notes
                        Outputs("Note", Note);

                        // close current note
                        ((Form)view.mainControl()).Close();
                        vm.removeViews("HelperForms.SchedulePanel");
                    }
                    break;

                #endregion

                #region Category panel

                case "Load category panel":
                    {
                        JToken note = Input_j("Note");
                        JArray Categories = Input_jarray("Categories");

                        vm.removeViews("HelperForms.SetCategoryPanel");
                        CView vInfoForm = vm.AddView("HelperForms.SetCategoryPanel");

                        ((TextBox)vInfoForm.getCtrl("text_prop")).Text = note["Category"].ToString();
                        ((TextBox)vInfoForm.getCtrl("text_prop")).MaxLength = 32;
                        ListBox lCategorias = (ListBox)vInfoForm.getCtrl("lista_categorias");
                        foreach (JToken cat in Categories)
                        {
                            lCategorias.Items.Add(cat.ToString());
                        }
                        lCategorias.SelectedIndex = lCategorias.Items.IndexOf(note["Category"].ToString());

                        Outputs("NoteGuid", view.guid);

                        Form fInfo = (Form)vInfoForm.mainControl();
                        fInfo.Show();
                    }
                    break;

                case "Set active category":
                    {
                        ListBox lCategorias = (ListBox)view.getCtrl("lista_categorias");
                        ((TextBox)view.getCtrl("text_prop")).Text = lCategorias.SelectedItem.ToString();

                    }
                    break;

                case "Remove category":
                    {
                        String cCat = Input_str("CurrentCategory");
                        JArray Categories = Input_jarray("Categories");
                        ListBox lCategorias = (ListBox)view.getCtrl("lista_categorias");
                        if (lCategorias.Items.IndexOf(cCat) >= 0)
                        {
                            Categories.RemoveAt(Categories.ToList().IndexOf(cCat));

                            lCategorias.Items.Remove(cCat);

                            Outputs("Categories", Categories);
                        }
                    }
                    break;

                case "Set current category":
                    {
                        if (((TextBox)view.getCtrl("text_prop")).Text != "")
                        {
                            Outputs("CurrentCategory", ((TextBox)view.getCtrl("text_prop")).Text);
                        }
                        else
                        {
                            ListBox lCategorias = (ListBox)view.getCtrl("lista_categorias");
                            Outputs("CurrentCategory", lCategorias.SelectedItem.ToString());
                        }
                    }
                    break;

                case "Add category":
                    {
                        String cCat = Input_str("CurrentCategory");
                        JArray Categories = Input_jarray("Categories");
                        ListBox lCategorias = (ListBox)view.getCtrl("lista_categorias");
                        if (lCategorias.Items.IndexOf(cCat) < 0)
                        {
                            Categories.Add(cCat);
                            Categories = new JArray(Categories.OrderBy(c => c));

                            lCategorias.Items.Add(cCat);

                            Outputs("Categories", Categories);
                        }
                    }
                    break;

                #endregion

                #region Foot toolbar messages
                case "Show toolbar message form":
                    {
                        String msg = Input_str("Message");

                        if (msg != "")
                        {
                            // remove if exists and create view for toolbar
                            vm.removeViews("HelperForms.FootbarMessageTool");
                            CView vFootbarMessage = vm.AddView("HelperForms.FootbarMessageTool");

                            // set message
                            ((Label)vFootbarMessage.getCtrl("label_message")).Text = msg;

                            // clear message
                            Outputs("Message", "");

                            // set position to left - bottom corner
                            Form fInfo = (Form)vFootbarMessage.mainControl();
                            fInfo.Top = Screen.GetWorkingArea(fInfo).Height - fInfo.Height;
                            fInfo.Left = 0;

                            // show message
                            fInfo.Show();
                        }
                    }
                    break;

                case "Hide toolbar message form":
                    {
                        // remove if exists
                        vm.removeViews("HelperForms.FootbarMessageTool");
                    }
                    break;
                #endregion

                #endregion

                #endregion
            }
        }
    }
}
