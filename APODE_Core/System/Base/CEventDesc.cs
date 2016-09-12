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
#endregion