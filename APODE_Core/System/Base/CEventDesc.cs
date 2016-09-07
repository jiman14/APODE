using System;
using System.Windows.Forms;


namespace APODE_Core
{
    public class CEventDesc
    {
        public String View_Guid = "";
        public String View_Name = "";
        public String Control_Name = "";
        public String Event_Name = "";
        public String Program = "";

        // Control fired and args
        public object fireCtrl;
        public object[] args;

        public CEventDesc(String _view_guid, String _view_name, String _control_name, String _event_name, String _program_name)
        {
            View_Guid = _view_guid; View_Name = _view_name; Control_Name = _control_name; Event_Name = _event_name; Program = _program_name;
        }

        public void addArgs(object[] _args)
        {
            args = _args;
        }
    }
}
