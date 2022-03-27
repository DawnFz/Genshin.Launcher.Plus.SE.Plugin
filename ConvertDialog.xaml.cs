using ModernWpf.Controls;

namespace Genshin.Launcher.Plus.SE.Plugin
{
    /// <summary>
    /// ConvertDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConvertDialog : ContentDialog
    {
        public ConvertDialog()
        {
            this.DataContext = new ConvertDialogViewModel(this);
            this.InitializeComponent();
        }
        public bool IsCloseAllowed { get; set; }

        private void DialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (this.IsCloseAllowed == false)
            {
                args.Cancel = true;
            }
        }
    }
}
