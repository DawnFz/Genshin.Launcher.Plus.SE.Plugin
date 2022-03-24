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
            DataContext = new ConvertDialogViewModel(this);
            InitializeComponent();
        }
    }
}
