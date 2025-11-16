using System;
using System.Drawing;
using System.Windows.Forms;
using ConEmu.WinForms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ConsoleTabControl : UserControl
    {
        public event EventHandler CloseRequested;

        public ConEmuControl ConEmuControl => conEmuControl;

        public ConsoleTabControl()
        {
            InitializeComponent();
        }
    }
}