using DGP.Genshin.Control.Infrastructure.Observable;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Genshin.Launcher.Plus.SE.Plugin
{
    /// <summary>
    /// ConvertDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConvertDialog : ObservableContentDialog
    {
        public ConvertDialog()
        {
            DataContext = new ConvertDialogViewModel(this);
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
