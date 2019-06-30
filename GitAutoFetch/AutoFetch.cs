using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace GitAutoFetch
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AutoFetch
    {
        public const int CommandId = 0x0100;
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("1963cd03-55e3-41de-b505-6965e7873f22");
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;
        private static Timer timerThread;
        private static Config Config;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFetch"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AutoFetch(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            CommandID menuCommandID = new CommandID(CommandSet, CommandId);
            MenuCommand menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AutoFetch Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, Config conf)
        {
            // Switch to the main thread - the call to AddCommand in AutoFetch's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AutoFetch(package, commandService);
            Config = conf;
            Instance.Execute(null, null);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (timerThread == null)
                    timerThread = new Timer(new TimerCallback(ExecCmd), null, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(Config.TimeValue()));
                else
                    timerThread.Change(TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(Config.TimeValue()));
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    ex.Message,
                    "VS Git AutoFetch -> Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                Dispose();
            }
        }

        private async void ExecCmd(object state)
        {
            try
            {
                DTE dte = await package.GetServiceAsync(typeof(DTE)).ConfigureAwait(false) as DTE;

                if (dte != null && dte.Solution.IsOpen)
                {
                    string solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
                    dte.ExecuteCommand("Team.Git.Fetch");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Dispose()
        {
            timerThread?.Dispose();
        }
    }
}
