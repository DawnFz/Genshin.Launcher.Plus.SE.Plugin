using DGP.Genshin.Core.Plugins;
using System;

[assembly: SnapGenshinPlugin]

namespace Genshin.Launcher.Plus.SE.Plugin
{
    /// <summary>
    /// 插件实例实现
    /// </summary>
    [ImportPage(typeof(PluginHomePage), "客户端转换", "\uE1CA")]
    public class SamplePlugin : IPlugin
    {
        public string Name
        {
            get => "GLP.SE-客户端转换";
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
            get => new("1.0.0.0");
        }

        public bool IsEnabled
        {
            get;
        }
    }
}