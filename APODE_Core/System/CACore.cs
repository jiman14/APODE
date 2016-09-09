using System.Windows.Forms;
using TControls;
using TLogic;


namespace APODE_Core
{
    public class ACORE
    {
        #region constructor

        CSystem Sistema;
        CViewsManager ViewsManager;
        CRunner Runner;
        CAConsole console = null;        
        
        /// <summary>
        /// APODE_Core Load application. 
        /// </summary>
        public ACORE()
        {
            Initialize_APODE_Core(false);            
            if (!ErrorsLoadingViews) Application.Run(MainForm);
        }
        /// <summary>
        /// Main app entry point
        /// </summary>
        public ACORE(bool console_mode)
        {
            Initialize_APODE_Core(console_mode);
        }
        #endregion

        #region public APODE_Core properties
        public bool is_console_active
        {
            get { return console.isActive; }
        }
        #endregion

        #region Manage APODE_Core

        /// <summary>
        /// Initialize core system
        /// </summary>
        /// <param name="console_mode"></param>
        private void Initialize_APODE_Core(bool console_mode)
        { 
            // Init UI
            Sistema = new CSystem();
            Sistema.Console_mode = console_mode;

            Sistema.Cron.runProgram += Vm_runProgram;

            // Init views manager & load UI
            ViewsManager = new CViewsManager(Sistema);

            if (!console_mode)
            {
                ViewsManager.runProgram += Vm_runProgram;
            }
            else
            {
                console = new CAConsole(Sistema);
                console.runProgram += Vm_runProgram;
                console.start();
            }

            // Init program runner
            Runner = new CRunner(Sistema, ViewsManager);
        }

        /// <summary>
        /// Return errors
        /// </summary>
        private bool ErrorsLoadingViews
        {
            get { return ViewsManager.hasErrors(); }
        }

        /// <summary>
        /// Return Main form for Run
        /// </summary>
        private Form MainForm
        {
            get { return ViewsManager.getMainForm(); }
        }
        #endregion

        #region Run programas from events

        /// <summary>
        /// Launch program from event
        /// </summary>
        /// <param name="event_desc"></param>
        private void Vm_runProgram(CEventDesc event_desc)
        {
            if (event_desc.Program != "")
            {
                Runner.LaunchProgram(event_desc);
            }

            // Run programs in queue
            while (Runner.Programs_in_queue)
            {
                Runner.LaunchProgramInQueue();
            }

            Sistema.reset_UI_errors();
        }
        #endregion
    }
}
