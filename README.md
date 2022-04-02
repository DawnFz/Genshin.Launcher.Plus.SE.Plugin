# Snap Genshin 插件-客户端转换

### 说明：

1. 本插件由 [原神启动器Plus](https://github.com/DawnFz/Genshin.Launcher.Plus) 提供开发支持
1. 本插件基于[Snap Genshin](https://github.com/DGP-Studio/Snap.Genshin)的插件模板进行开发
2. Clone本项目后必须把本项目放入[Snap Genshin](https://github.com/DGP-Studio/Snap.Genshin)项目中才可编译
3. 感谢[Lightczx(DismissedLight)](https://github.com/Lightczx)开发的[Snap Genshin](https://github.com/DGP-Studio/Snap.Genshin)

### 使用方法：

前往 [Releases页面](https://github.com/DawnFz/Genshin.Launcher.Plus.SE.Plugin/releases) 下载编译好并发布的DLL文件，放入SG目录下的Plugins文件夹中，重新启动SG检查插件是否加载，本插件仅提供LaunchService启动服务，在SG左侧没有界面入口，您可以在插件管理页检查是否成功加载。插件加载后您需要进入 [设置] --> [应用服务实现] 中将启动游戏服务切换到 [原神启动器Plus] 即可使用。切换完成后您即可在SG左侧启动游戏栏目中选择服务器切换，国服、国际服切换需要Pkg文件支持

PKG转换资源下载地址及访问密码: etxd
https://pan.baidu.com/s/1-5zQoVfE7ImdXrn8OInKqg

### 预览图：

![截图](https://s2.loli.net/2022/03/25/YpTbyWoq1i79hvE.jpg)

![截图](https://s2.loli.net/2022/03/25/o5q8J2ZzC3hBSL4.jpg)



### 版本迭代：

1.0.0.0 - 测试发布

1.0.3.1 - 修改了大部分逻辑，支持新版SG

1.0.4.0 - 更新了2.6的Pkg文件支持，请务必更新，旧版本不支持新的pkg文件列表

1.0.4.5 - 支持新版本SG，加入了插件版本检查的功能
