using DGP.Genshin.Core.Plugins;
using System;

[assembly: SnapGenshinPlugin]

namespace Genshin.Launcher.Plus.SE.Plugin
{
    /// <summary>
    /// 插件实例实现
    /// </summary>
    public class GLPSEPlugin : IPlugin
    {
        public string Name => "GLP.SE-客户端转换";
        public string Description => "本插件用于快速切换国服与国际服文件，免去下载两个游戏客户端在电脑上的时间与空间，需要下载Pkg转换资源文件支持";
        public string Author => "原神启动器Plus";
        public Version Version => new("1.0.2.1");
        public bool IsEnabled
        {
            get;
        }
    }
}