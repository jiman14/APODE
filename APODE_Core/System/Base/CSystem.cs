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
