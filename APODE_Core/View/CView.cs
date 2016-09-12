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
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;
using APODE_Core;

namespace TControls
{
    public class CView
    {
        #region Variables & events

        CGlobals Globals;
        CErrors errors;
        CDebug debug;
        String View_guid = "";
        String View_name = "";
        Dictionary<String, CtrlStruct> ctrls;
        Dictionary<String, ObjStruct> objects;
        public Dictionary<String, object> vars;

        public delegate void generic_event(CctrlEventDesc desc, object sender, object [] args);
        public event generic_event on_any_event;
        #endregion

        #region manage variables

        /// <summary>
        /// Return var as JToken from property list
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JToken var_j(params String[] property_list)
        {
            JToken jItem = null;
            foreach (String prop in property_list)
            {
                if (jItem == null)
                {
                    object res = vars[prop];
                    if ((res as JToken) != null) jItem = res as JToken;
                    else jItem = new JValue(res);
                }
                else jItem = jItem[prop];
            }
            return jItem;
        }
        /// <summary>
        /// Return var as String
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public String var_str(params String[] property_list)
        {
            try
            {
                return var_j(property_list).ToString();
            }
            catch
            {
                return "";
            }
        }
       
        /// <summary>
        /// Return var as JArray
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JArray var_jarray(params String[] property_list)
        {
            try
            {
                return var_j(property_list) as JArray;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Return var as int
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int var_int(params String[] property_list)
        {
            try
            {
                return var_j(property_list).ToObject<int>();
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// Return var as JToken
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double var_double(params String[] property_list)
        {
            try
            {
                return var_j(property_list).ToObject<double>();
            }
            catch
            {
                return -1.0;
            }
        }

        #endregion

        #region Initialize view

        /// <summary>
        /// Primary constructor to call from view manager
        /// </summary>
        /// <param name="base_errors"></param>
        /// <param name="base_debug"></param>
        /// <param name="view_name"></param>
        public CView(CSystem csystem, String view_name)
        {
            Globals = csystem.Globals;
            errors = csystem.errors;
            debug = csystem.debug;
            vars = new Dictionary<string, object>();
            View_name = view_name;
            View_guid = Guid.NewGuid().ToString();
            ctrls = new Dictionary<String, CtrlStruct>();
            objects = new Dictionary<String, ObjStruct>();

            // Create main control and descendants
            CreateControl(view_name);

            // Create other objects
            CreateObjects(view_name);
        }
        /// <summary>
        /// Simple use for access to util functions
        /// </summary>
        public CView(CSystem csystem)
        {
            Globals = csystem.Globals;
            errors = csystem.errors;
            debug = csystem.debug;
            View_name = "";
        }
        #endregion

        #region Destroy view

        /// <summary>
        /// Destroy current view with her controls
        /// </summary>
        public void Free()
        {
            Control main = mainControl();
            while (ctrls.Count() > 0)
            {
                RemoveControl(ctrls.ElementAt(0).Key);
            }
            if (main is Form) ((Form)main).Close();
            vars.Clear();
        }
        #endregion

        #region Control & Object struct class

        /// <summary>
        /// Struct for all controls
        /// </summary>
        public class CtrlStruct
        {
            public String control_name;
            public Control ctrl;
            public JToken config;
            public JToken events;
            public JArray child_controls;
            public Dictionary<String, object> vars;
            public CtrlStruct(String s, Control c, JToken cfg, JToken evt, JArray cc) { control_name = s; ctrl = c; config = cfg; events = evt; child_controls = cc; vars = new Dictionary<string, object>(); }
        }
        /// <summary>
        /// Struct for no UI objects
        /// </summary>
        public class ObjStruct
        {
            public String object_name;
            public Object obj;
            public JToken config;
            public JToken events;
            public JToken methods;
            public Dictionary<String, object> vars;
            public ObjStruct(String s, Object o, JToken cfg, JToken evt, JToken met) { object_name = s; obj = o; config = cfg; events = evt; methods = met; vars = new Dictionary<string, object>(); }
        }
        #endregion

        #region View properties

        public String Name { get { return View_name; } }
        public String guid { get { return View_guid; } }
        #endregion

        #region Configuration manager


        /// <summary>
        /// Configure control events and properties
        /// </summary>
        /// <param name="control_guid"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        private JToken Configure(TBaseControls bCtrl, Control ctrl)
        {
            JToken Config = bCtrl.Configuration;
            object obj = (object)ctrl;            
            set_properties(ref obj, Config);
            Set_event_handler(bCtrl.Events, ref obj);

            return Config;
        }
        /// <summary>
        /// Configure object events and properties
        /// </summary>
        /// <param name="_view_name"></param>
        /// <param name="_control_guid"></param>
        /// <param name="ctrl"></param>
        public JToken Configure(TBaseObjects bObj, ref object obj)
        {            
            JToken Config = bObj.Configuration;                        
            set_properties(ref obj, Config);
            Set_event_handler(bObj.Events, ref obj);

            return Config;
        }
        #endregion

        #region Control properties

        /// <summary>
        /// Set control property
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="prop_name"></param>
        /// <param name="value"></param>
        public void SetControlProperty(ref Control ctrl, String prop_name, object value)
        {
            object o = ctrl;
            SetControlProperty(ref o, prop_name, value);
        }

        /// <summary>
        /// Set generic control property
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="control_name"></param>
        /// <param name="prop_name"></param>
        /// <param name="value"></param>
        public void SetControlProperty(ref object ctrl, String prop_name, object value)
        {
            try
            {
                // Get type of object and property info
                Type tc = ctrl.GetType();
                PropertyInfo pi = tc.GetProperty(prop_name);

                if (pi != null)
                {
                    // Set property 
                    String property_path = "";
                    SetPropertyInfo(ref ctrl, pi, (value as JToken == null) ? new JValue(value) : value as JToken, ref property_path);
                }
                else
                {
                    errors.add(String.Format("Configure error in control property '{0}' not found", prop_name));
                }
            }
            catch (Exception exc)
            {
                errors.add(String.Format("Configure error in control property '{0}':  {1}", prop_name, exc.Message));                
            }
        }
        /// <summary>
        /// Set object and controls properties
        /// </summary>
        /// <param name="c"></param>
        /// <param name="Config"></param>
        private void set_properties(ref object c, JToken Config)
        {
            String property_path = "";
            set_properties_recursive(ref c, Config, ref property_path);
        }

        /// <summary>
        /// Set generic properties recursively
        /// </summary>
        /// <param name="c"></param>
        /// <param name="Config"></param>
        /// <param name="property_path"></param>
        private void set_properties_recursive(ref object c, JToken Config, ref String property_path)
        {
            try
            {
                Type tc = c.GetType();

                foreach (PropertyInfo pi in tc.GetProperties())
                {
                    if (Config[pi.Name] != null)
                    {
                        SetPropertyInfo(ref c, pi, Config[pi.Name], ref property_path);
                    }
                }
            }
            catch (Exception exc)
            {
                errors.add(String.Format("Configure error in property '{0}': {1}", property_path, exc.Message));
            }
        }

        /// <summary>
        /// Set control property 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pi"></param>
        /// <param name="Config"></param>
        /// <param name="config_var"></param>
        /// <param name="property_path"></param>
        private void SetPropertyInfo(ref object c, PropertyInfo pi, JToken ConfigVar, ref String property_path)
        {
            property_path += pi.Name + ".";

            if ((pi.PropertyType == typeof(Color)) && (!(ConfigVar is JObject)))
            {
                pi.SetValue(c, ColorTranslator.FromHtml(ConfigVar.ToString()));
            }
            // set keys 
            else if (pi.PropertyType == typeof(Keys))
            {
                KeysConverter kc = new KeysConverter();
                Keys modkey = ((ConfigVar["Modifiers"] == null) || (ConfigVar["Modifiers"].ToString() == ""))? 
                                Keys.None: 
                                (Keys)kc.ConvertFromString(ConfigVar["Modifiers"].ToString());
                Keys key = (Keys)kc.ConvertFromString(ConfigVar["Keycode"].ToString());

                pi.SetValue(c, modkey | key);
            }
            else if (pi.PropertyType == typeof(Image))
            {
                String imagePath = ConfigVar.ToString();
                try
                {
                    imagePath = imagePath.Replace("%" + dGLOBALS.RESOURCES_PATH + "%", Globals.Resources_path);
                    if (System.IO.File.Exists(imagePath))
                    {
                        Image img = Image.FromFile(imagePath);
                        pi.SetValue(c, img);
                    }
                    else {
                        debug.add(String.Format("Image not found '{0}'", imagePath));
                    }                    
                }
                catch (Exception exc) {
                    errors.add(String.Format("Error loading image '{0}', error message: {1}", imagePath, exc.Message));
                }
            }
            else if (pi.PropertyType == typeof(Icon))
            {
                String imagePath = ConfigVar.ToString();
                try
                {
                    imagePath = imagePath.Replace("%" + dGLOBALS.RESOURCES_PATH + "%", Globals.Resources_path);
                    if (System.IO.File.Exists(imagePath))
                    {
                        Icon iconImg = new Icon(imagePath);                        
                        pi.SetValue(c, iconImg);
                    }
                    else
                    {
                        debug.add(String.Format("Image not found '{0}'", imagePath));
                    }
                }
                catch (Exception exc)
                {
                    errors.add(String.Format("Error loading image '{0}', error message: {1}", imagePath, exc.Message));
                }
            }
            else if (pi.PropertyType == typeof(Font))
            {
                FontStyle fs = (ConfigVar["FontStyle"] != null) ? (FontStyle)ConfigVar["FontStyle"].ToObject(typeof(FontStyle)) : FontStyle.Regular;
                String familyName = (ConfigVar["familyName"] != null) ? ConfigVar["familyName"].ToString() : mainControl().Font.FontFamily.Name;
                float emSize = (ConfigVar["emSize"] != null) ? ConfigVar["emSize"].ToObject<float>() : mainControl().Font.Size;
                Font f = new Font(familyName, emSize, fs);
                pi.SetValue(c, f);
            }
            else if ((pi.PropertyType.IsValueType) || (pi.PropertyType.FullName == typeof(String).FullName))
            {
                if ((pi.PropertyType.GetProperties().Count() == 0) || (pi.PropertyType.FullName == typeof(String).FullName))
                {
                    pi.SetValue(c, ConfigVar.ToObject(pi.PropertyType));
                }
                else
                {
                    var struct_obj = Activator.CreateInstance(pi.PropertyType);

                    foreach (PropertyInfo pi_child in pi.PropertyType.GetProperties())
                    {
                        if (ConfigVar[pi_child.Name] != null)
                        {
                            PropertyInfo struct_pinfo = pi.PropertyType.GetProperty(pi_child.Name);
                            struct_pinfo.SetValue(struct_obj, ConfigVar[pi_child.Name].ToObject(pi_child.PropertyType));
                        }
                    }

                    pi.SetValue(c, struct_obj);
                }
            }
            else
            {
                object c1 = pi.GetValue(c);
                set_properties_recursive(ref c1, ConfigVar, ref property_path);
            }
            
        }

        #endregion

        #region Control events

        /// <summary>
        /// Add generic event handler
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="EventName"></param>
        public void Set_event_handler(object ctrl, String ControlName, String EventName, String Program)
        {
            Type tc = ctrl.GetType();
            EventInfo ei = tc.GetEvent(EventName);
            ei.AddEventHandler(ctrl, BuildDynamicHandle(ei.EventHandlerType,
                                event_target_method, View_guid, View_name, ControlName, ei.Name, Program));
        }
        /// <summary>
        /// Set generic event from configuration
        /// </summary>
        /// <param name="Events"></param>
        /// <param name="ctrl"></param>
        private void Set_event_handler(JToken Events, ref object ctrl)
        {
            if (Events != null)
            {
                Type tc = ctrl.GetType();
                foreach (EventInfo ei in tc.GetEvents())
                {
                    if (Events[ei.Name] != null)
                    {
                        ei.AddEventHandler(ctrl, BuildDynamicHandle(ei.EventHandlerType, event_target_method,
                                            View_guid, View_name, (ctrl is Control)? ((Control)ctrl).Name: "", ei.Name, Events[ei.Name].ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// Default delegate for generic event 
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public object event_target_method(object[] sender)
        {
            if (on_any_event != null)
            {
                object []args = new object[sender.Length - 2];

                Array.Copy(sender, 2, args as Array, 0, sender.Length - 2);

                on_any_event(sender[0] as CctrlEventDesc, sender[1], args);
            }

            return null;
        }
        /// <summary>
        /// Event desc class for generic event delegate
        /// </summary>
        public class CctrlEventDesc : CEventDesc
        {

            public CctrlEventDesc(String _view_guid, String _view_name, String _control_name, String _event_name, String _program_name)
            : base(_view_guid, _view_name, _control_name, _event_name, _program_name)
            { 
            }

            public void setEventArgs(Control fired_control, object [] fired_args)
            {
                fireCtrl = fired_control;
                args = fired_args;
            }
        }
        /// <summary>
        /// Dynamic construc delegate using expressions
        /// </summary>
        /// <param name="delegateType"></param>
        /// <param name="func"></param>
        /// <param name="control_name"></param>
        /// <param name="event_name"></param>
        /// <param name="program"></param>
        /// <returns></returns>
        public static Delegate BuildDynamicHandle(Type delegateType, Func<object[], object> func, String view_guid, String view_name, String control_name, String event_name, String program)
        {
            var invokeMethod = delegateType.GetMethod("Invoke");
            ParameterExpression[] parms = invokeMethod.GetParameters().Select(parm => Expression.Parameter(parm.ParameterType, parm.Name)).ToArray();
            
            CctrlEventDesc eDesc = new CctrlEventDesc(view_guid, view_name, control_name, event_name, program);

            UnaryExpression[] typeAsExpression = new UnaryExpression[] { Expression.TypeAs(System.Linq.Expressions.Expression.Constant(eDesc, typeof(CctrlEventDesc)), typeof(CctrlEventDesc)) };

            var instance = func.Target == null ? null : Expression.Constant(func.Target);

            //var converted = parms.Select(parm => Expression.Convert(parm, typeof(object))).Concat(typeAsExpression);
            var converted = typeAsExpression.Concat(parms.Select(parm => Expression.Convert(parm, typeof(object))));
            NewArrayExpression paramArray = Expression.NewArrayInit(typeof(object), converted);

            var call = Expression.Call(instance, func.Method, paramArray);

            var body =
                invokeMethod.ReturnType == typeof(void) ? (Expression)call : Expression.Convert(call, invokeMethod.ReturnType);
            var expr = Expression.Lambda(delegateType, body, new ParameterExpression[] { parms[0], parms[1] }); // parms
            return expr.Compile();
        }
        #endregion

        #region Manage control struct

        #region Manage UI controls

        /// <summary>
        /// Generic new control from type
        /// </summary>
        /// <param name="control_guid"></param>
        /// <param name="type_control"></param>
        /// <returns></returns>
        public Control CreateControl(String control_guid)
        {
            TBaseControls bCtrl = new TBaseControls(Globals, View_name);
            return NewControl(control_guid, bCtrl);
        }

        /// <summary>
        /// Generic new control from type
        /// </summary>
        /// <param name="control_guid"></param>
        /// <param name="type_control"></param>
        /// <returns></returns>
        private Control NewControl(String control_guid, TBaseControls bCtrl)
        {
            try
            {
                debug.add("New control: " + control_guid);

                // Active control by guid
                bCtrl.ActiveControl(control_guid);
                if (bCtrl.Type_str == "")
                {
                    errors.add(String.Format("Error: control guid '{0}' not found in view: {1}", control_guid, Name));
                }

                if (bCtrl.hasErrors())
                {
                    errors.add(bCtrl.jErrors);

                    return null;
                }
                else
                {
                    // create typed control
                    Control typed_control = null;
                    Type type_control = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(type => type.FullName == "System.Windows.Forms." + bCtrl.Type_str)).FirstOrDefault();
                    if (type_control == null)
                    {
                        type_control = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(type => type.FullName == bCtrl.Type_str)).FirstOrDefault();
                    }
                    if (type_control == null)
                    {
                        if (bCtrl.Type_str != null)
                        {
                            errors.add(String.Format("Error creating control '{0}' from type '{1}'.", control_guid, bCtrl.Type_str));
                        }
                        else
                        {
                            errors.add(String.Format("Error creating control '{0}' from empty type.", control_guid));
                        }
                        return null;
                    }
                    else
                    {                        
                        typed_control = (Control)Activator.CreateInstance(type_control);
                        typed_control.Name = control_guid;

                        // Configure control
                        JToken cfg = Configure(bCtrl, typed_control);

                        // add control to list                        
                        ctrls.Add(control_guid, new CtrlStruct(control_guid, typed_control, cfg, bCtrl.Events, bCtrl.Controls));

                        foreach (String child_control in bCtrl.Controls)
                        {
                            typed_control.Controls.Add(NewControl(child_control, bCtrl));
                        }
                    }
                    return typed_control;
                }
            }
            catch (Exception exc)
            {
                errors.add(String.Format("Unhandled error creating control '{0}': {1}", control_guid, exc.Message));
                return null;
            }
        }

        /// <summary>
        /// Remove all controls from controls struct with that guid
        /// </summary>
        /// <param name="control_guid"></param>
        public void RemoveControl(String control_guid)
        {
            if (ctrls.Keys.Contains(control_guid))
            {
                // get first control with that guid
                CtrlStruct cstr = ctrls[control_guid];

                // clear vars
                cstr.vars.Clear();

                // remove it from parent control
                Control cparent = cstr.ctrl.Parent;
                if (cparent != null) cparent.Controls.Remove(cstr.ctrl);

                // remove from controls struct
                ctrls.Remove(control_guid);
            }            
        }

        /// <summary>
        /// Configure a control with the configuration node path
        /// </summary>
        /// <param name="object_guid"></param>
        /// <param name="config_path"></param>
        public void ConfigureControl(String control_guid, params String[] config_path)
        {
            CtrlStruct cStr = getCtrlStruct(control_guid);

            JToken jConfig = cStr.config;
            foreach (String cfg_node in config_path)
            {
                jConfig = jConfig[cfg_node];
            }
            object o = cStr.ctrl;
            set_properties(ref o, jConfig);
        }

        #endregion

        #region Control struct access

        /// <summary>
        /// Get first control from control guid
        /// </summary>
        /// <param name="control_guid"></param>
        /// <returns></returns>
        public Control getCtrl(String control_guid)
        {
            try
            {
                return (ctrls.Keys.Contains(control_guid))? ctrls[control_guid].ctrl: null;
            }
            catch {
                debug.add(String.Format("Control not found: {0}, in view: {1}", control_guid, View_name));
            }
            return null;
        }

        /// <summary>
        /// Get first control from control guid
        /// </summary>
        /// <param name="control_guid"></param>
        /// <returns></returns>
        public Control mainControl()
        {
            return getCtrl(View_name);
        }

        /// <summary>
        /// Get first control from control guid
        /// </summary>
        /// <param name="control_guid"></param>
        /// <returns></returns>
        public CtrlStruct getCtrlStruct(String control_guid)
        {
            try
            {
                return (ctrls.Keys.Contains(control_guid)) ? ctrls[control_guid]: null ;
            }
            catch {
                debug.add(String.Format("Control not found: {0}, in view: {1}", control_guid, View_name));
            }
            return null;
        }
        #endregion

        #region Manage control struct vars

        /// <summary>
        /// Get variables from control struct
        /// </summary>
        /// <param name="control_guid"></param>
        /// <returns></returns>
        public Dictionary<String, object> CtrlVars(String control_guid)
        {
            return (ctrls.Keys.Contains(control_guid)) ? ctrls[control_guid].vars: null;
        }

        /// <summary>
        /// Add a variable to control struct
        /// </summary>
        /// <param name="control_guid"></param>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        public bool addCtrlVar(String control_guid, String variable, object value)
        {
            if (ctrls.Keys.Contains(control_guid))
            {
                ctrls[control_guid].vars.Add(variable, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a variable from control struct
        /// </summary>
        /// <param name="control_guid"></param>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        public bool removeCtrlVar(String control_guid, String variable)
        {
            if (ctrls.Keys.Contains(control_guid))
            {
                ctrls[control_guid].vars.Remove(variable);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clear all variables in control struct
        /// </summary>
        /// <param name="control_guid"></param>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        public bool clearCtrlVars(String control_guid)
        {
            if (ctrls.Keys.Contains(control_guid))
            {
                ctrls[control_guid].vars.Clear();
                return true;
            }
            return false;
        }
        #endregion

        #endregion

        #region Manage objects struct

        #region Manage UI objects

        /// <summary>
        /// Generic new control from type
        /// </summary>
        /// <param name="control_guid"></param>
        /// <param name="type_control"></param>
        /// <returns></returns>
        private void CreateObjects(String main_view)
        {
            TBaseObjects bObj = new TBaseObjects(Globals, main_view);
            try
            {
                if (bObj.hasErrors()) errors.add(bObj.jErrors);                
                else
                {
                    JArray Obj_list = bObj.Object_list;
                    foreach (JToken _bObj in Obj_list)
                    {
                        // Active object
                        bObj.ActiveObject(_bObj);

                        // display debug
                        debug.add("New control: " + bObj.GUID); 

                        // create typed control
                        Type type_control = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(type => type.FullName == bObj.Object_name)).FirstOrDefault();
                        
                        if (type_control == null)
                        {
                            Assembly _dll = Assembly.LoadFrom(Globals.getAssembly(bObj.Assembly));
                            type_control = _dll.GetType(bObj.Object_name);
                        }

                        if (type_control == null)
                        {
                            errors.add(String.Format("Error creating object instance '{0}' from type '{1}'.", bObj.GUID, bObj.Assembly));
                        }
                        else
                        {
                            Object typed_control = (Object)Activator.CreateInstance(type_control);                            

                            // Configure control
                            JToken cfg = Configure(bObj, ref typed_control);
                            
                            // add control to list                        
                            objects.Add(bObj.GUID, new ObjStruct(bObj.GUID, typed_control, cfg, bObj.Events, bObj.Methods));
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                errors.add(String.Format("Error loading assembly and type '{0}': {1}", bObj.Assembly, exc.Message));
            }
        }

        /// <summary>
        /// Remove a object from objects struct's list
        /// </summary>
        /// <param name="control_guid"></param>
        public void RemoveObject(String object_guid)
        {
            if (objects.Keys.Contains(object_guid))
            {
                // clear vars
                objects[object_guid].vars.Clear();

                // remove from controls struct
                objects.Remove(object_guid);
            }
        }

        /// <summary>
        /// Generic method call (parameters in object json definition)
        /// </summary>
        /// <param name="object_guid"></param>
        /// <param name="Method_name"></param>
        /// <returns></returns>
        public object MethodCall(String object_guid, String Method_name)
        {            
            return MethodCall(object_guid, Method_name, null);
        }

        /// <summary>
        /// Generic method call with optional parameter object list
        /// </summary>
        /// <param name="object_guid"></param>
        /// <param name="Method_name"></param>
        /// <returns></returns>
        public object MethodCall(String object_guid, String Method_name, object[] overwrited_parameters)
        {
            // get object struct
            ObjStruct oStr = getObjStruct(object_guid);

            // get user method
            JObject jMethod = oStr.methods[Method_name] as JObject;
            if (jMethod != null)
            {
                object obj = oStr.obj;
                JObject jParams = jMethod["Parameters"] as JObject;

                // search method Name from jMethod "Name" 
                foreach (MethodInfo method in obj.GetType().GetMethods().Where(c => c.Name == jMethod["Name"].ToString()).ToList())
                {
                    // In case of overloaded methods, check method param number. )
                    bool method_conflict = false;
                    if (method.GetParameters().Count() == jParams.Properties().Count())
                    {
                        object[] parameters = null;
                        int i = 1;

                        foreach (ParameterInfo pi in method.GetParameters())
                        {
                            // Check method's param name to avoid erroneous call 
                            if (jParams[pi.Name] != null)
                            {
                                // set parameters if the overwritten are null
                                if (overwrited_parameters == null)
                                {
                                    Array.Resize(ref parameters, i);
                                    parameters[i - 1] = jParams[pi.Name].ToObject(pi.ParameterType);
                                    i++;
                                }
                            }
                            else
                            {
                                method_conflict = true;
                            }
                        }
                        // Invoke method if there isn't conflicts. Send overwrited parameters if there are not null
                        if (!method_conflict) return method.Invoke(obj, (overwrited_parameters != null)? overwrited_parameters: parameters);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Set object property
        /// </summary>
        /// <param name="object_guid"></param>
        /// <param name="property_name"></param>
        /// <param name="property_value"></param>
        public void ConfigureObjectProperty(String object_guid, String property_name, JToken property_value)
        {
            ObjStruct oStr = getObjStruct(object_guid);

            PropertyInfo pi = oStr.obj.GetType().GetProperty(property_name);
            if (pi != null)
            {
                try
                {
                    pi.SetValue(oStr.obj, property_value.ToObject(pi.PropertyType));
                }
                catch (Exception exc)
                {
                    errors.add(String.Format("Error setting property '{0}': {1}", property_name, exc.Message));
                }
            }
            else
            {
                errors.add(String.Format("Error property '{0}' not found.", property_name));
            }
        }

        /// <summary>
        /// Configure a object with the configuration node path
        /// </summary>
        /// <param name="object_guid"></param>
        /// <param name="config_path"></param>
        public void ConfigureObject(String object_guid, params String[] config_path)
        {
            ObjStruct oStr = getObjStruct(object_guid);

            JToken jConfig = oStr.config;
            foreach (String cfg_node in config_path)
            {
                jConfig = jConfig[cfg_node];
            }

            set_properties(ref oStr.obj, jConfig);
        }

        #endregion

        #region Objects struct access

        /// <summary>
        /// get object from this view 
        /// </summary>
        /// <param name="object_guid"></param>
        /// <returns></returns>
        public object getObject(String object_guid)
        {
            try
            {
                return (objects.Keys.Contains(object_guid)) ? objects[object_guid].obj : null;
            }
            catch
            {
                debug.add(String.Format("Object not found: {0}, in view: {1}", object_guid, View_name));
            }
            return null;
        }

        /// <summary>
        /// get object from this view 
        /// </summary>
        /// <param name="object_guid"></param>
        /// <returns></returns>
        public ObjStruct getObjStruct(String object_guid)
        {
            try
            {
                return (objects.Keys.Contains(object_guid)) ? objects[object_guid] : null;
            }
            catch
            {
                debug.add(String.Format("Object struct not found: {0}, in view: {1}", object_guid, View_name));
            }
            return null;
        }
        #endregion

        #region Manage object struct vars

        /// <summary>
        /// Get variables from control struct
        /// </summary>
        /// <param name="object_guid"></param>
        /// <returns></returns>
        public Dictionary<String, object> ObjVars(String object_guid)
        {
            return (objects.Keys.Contains(object_guid)) ? objects[object_guid].vars : null;
        }

        /// <summary>
        /// Add a variable to control struct
        /// </summary>
        /// <param name="object_guid"></param>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        public bool addObjVar(String object_guid, String variable, object value)
        {
            if (objects.Keys.Contains(object_guid))
            {
                objects[object_guid].vars.Add(variable, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a variable from control struct
        /// </summary>
        /// <param name="object_guid"></param>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        public bool removeObjVar(String object_guid, String variable)
        {
            if (objects.Keys.Contains(object_guid))
            {
                objects[object_guid].vars.Remove(variable);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clear all variables in control struct
        /// </summary>
        /// <param name="object_guid"></param>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        public bool clearObjVars(String object_guid)
        {
            if (objects.Keys.Contains(object_guid))
            {
                objects[object_guid].vars.Clear();
                return true;
            }
            return false;
        }
        #endregion

        #endregion

    }
}
#endregion