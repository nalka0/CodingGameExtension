﻿
using CodingBrowser;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Interop = Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using EnvDTE80;
using EnvDTE;
using CodingGameExtension.Tools;
using System.IO;
using System.Text;

namespace CodingGameExtension.Command
{
    /// <summary>
    /// Command handler
    /// 
    /// https://docs.microsoft.com/en-us/visualstudio/extensibility/saving-data-in-project-files?view=vs-2022
    /// 
    /// </summary>
    internal sealed class CommandPush
    {

        public const int CommandId = 0x0102;

        public static readonly Guid CommandSet = new Guid("24fae27f-5144-4741-b6fb-f2f8121376e1");

        private readonly AsyncPackage package;


        private CommandPush(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static CommandPush Instance
        {
            get;
            private set;
        }

        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in Command1's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CommandPush(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            var vs = new VsManager();

            vs.SaveAllDocument();

            var sb = new StringBuilder();
            sb.AppendLine("using System;\r\nusing System.Linq;\r\nusing System.IO;\r\nusing System.Text;\r\nusing System.Collections;\r\nusing System.Collections.Generic;");
         

            foreach (FileInfo file in vs.getProjetFiles().Select(f => new FileInfo(f)))
            {
                using (var reader = file.OpenText())
                {
                    string s;
                    var isCopyStart = false;
                    while ((s = reader.ReadLine()) != null)
                    {
                        if (isCopyStart)
                            sb.AppendLine(s);
                        if (s.TrimStart(' ').TrimStart('\t').StartsWith("namespace"))
                            isCopyStart = true;
                    }
                }
            }

            var b = Browser.Start();

            b.SendCode(sb.ToString());
         //   b.LaunchTest();
        }
    }

}
