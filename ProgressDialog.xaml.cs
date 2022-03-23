using DGP.Genshin.Control.Infrastructure.Observable;
using ModernWpf.Controls;

namespace Genshin.Launcher.Plus.SE.Plugin
{
    /// <summary>
    /// ProgressDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressDialog : ObservableContentDialog
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        public bool IsCloseAllowed { get; set; }

        private void DialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (IsCloseAllowed == false)
            {
                args.Cancel = true;
            }
        }
    }
}
