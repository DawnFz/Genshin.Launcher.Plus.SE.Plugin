using DGP.Genshin.Core.Plugins;
using System;
using System.Windows.Input;

[assembly: SnapGenshinPlugin]

namespace Genshin.Launcher.Plus.SE.Plugin
{
    /// <summary>
    /// 插件实例实现
    /// </summary>
    public class GLPSEPlugin : IPlugin, IPlugin2
    {
        #region IPlugin
        public string Name
        {
            get => "原P.SE-客户端转换";
        }

        public string Description
        {
            get => "本插件用于快速切换国服与国际服文件，免去下载两个游戏客户端在电脑上的时间与空间，需要下载Pkg转换资源文件支持";
        }

        public string Author
        {
            get => "原神启动器Plus";
        }

        public Version Version
        {
            get => new("1.1.0.0");
        }

        public bool IsEnabled
        {
            get;
        }

        #endregion

        #region IPlugin2
        public string? DetailLink
        {
            get => "https://github.com/DawnFz/Genshin.Launcher.Plus.SE.Plugin";
        }

        public bool IsSettingSupported
        {
            get => false;
        }

        public ICommand SettingCommand
        {
            get => null!;
        }
        #endregion
    }
}