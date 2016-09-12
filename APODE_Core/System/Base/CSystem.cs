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
using TLogic;

namespace APODE_Core
{
    public class CSystem
    {
        public CGlobals Globals;
        public CErrors errors;
        public CDebug debug;
        public TErrors ProgramErrors;
        public TDebug ProgramDebug;
        public bool Console_mode = false;
        public CCron Cron;

        /// <summary>
        /// Get global vars. And initialize basic error and debug objects
        /// </summary>
        public CSystem()
        {
            // Load globals vars
            Globals = new CGlobals();

            // errors & debug
            errors = new CErrors(Globals);
            debug = new CDebug(Globals);

            // Init Cron
            Cron = new CCron(Globals, errors, debug);
        }

        /// <summary>
        /// Initialize program & debug objects
        /// </summary>
        /// <param name="Program"></param>
        public void NewProgram(String Program)
        {
            ProgramErrors = new TErrors(Globals, Program);
            ProgramErrors.cleanProcess();
            ProgramDebug = new TDebug(Globals, Program);
            ProgramDebug.cleanProcess();
        }

        /// <summary>
        /// Reset UI Errors
        /// </summary>
        public void reset_UI_errors()
        {
            errors.reset_UI_errors();
        }

        /// <summary>
        /// Force active program break
        /// </summary>
        public void ForceExit()
        {
            ProgramErrors.forceExitProgram = true;
        }

    }
}
#endregion