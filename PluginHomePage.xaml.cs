using Snap.Core.DependencyInjection;
using SystemPage = System.Windows.Controls.Page;

namespace Genshin.Launcher.Plus.SE.Plugin
{
    [View(InjectAs.Transient)]
    public partial class PluginHomePage : SystemPage
    {
        public PluginHomePage(PluginHomeViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
