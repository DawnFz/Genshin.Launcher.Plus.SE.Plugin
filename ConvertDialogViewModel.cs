using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DGP.Genshin.Service.Abstraction.Setting;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using Microsoft;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Uwp.Notifications;
using Snap.Core.DependencyInjection;
using Snap.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Genshin.Launcher.Plus.SE.Plugin
{
    [ViewModel(InjectAs.Transient)]
    public class ConvertDialogViewModel : ObservableObject
    {
        private readonly string[] globalfiles = new string[]
        { "GenshinImpact_Data/app.info",
          "GenshinImpact_Data/globalgamemanagers",
          "GenshinImpact_Data/globalgamemanagers.assets",
          "GenshinImpact_Data/globalgamemanagers.assets.resS",
          "GenshinImpact_Data/upload_crash.exe",
          "GenshinImpact_Data/Managed/Metadata/global-metadata.dat" ,
          "GenshinImpact_Data/Native/Data/Metadata/global-metadata.dat",
          "GenshinImpact_Data/Native/UserAssembly.dll",
          "GenshinImpact_Data/Native/UserAssembly.exp",
          "GenshinImpact_Data/Native/UserAssembly.lib",
          //2.6新加部分
          "GenshinImpact_Data/Plugins/ZFProxyWeb.dll",
          "GenshinImpact_Data/Plugins/ZFEmbedWeb.dll",
          "GenshinImpact_Data/Plugins/zf_cef.dll",
          "GenshinImpact_Data/Plugins/XInputInterface64.dll",
          "GenshinImpact_Data/Plugins/widevinecdmadapter.dll",
          "GenshinImpact_Data/Plugins/sqlite3.dll",
          "GenshinImpact_Data/Plugins/Rewired_DirectInput.dll",
          "GenshinImpact_Data/Plugins/metakeeper.dll",
          "GenshinImpact_Data/Plugins/libUbiCustomEvent.dll",
          "GenshinImpact_Data/Plugins/libGLESv2.dll",
          "GenshinImpact_Data/Plugins/libEGL.dll",
          "GenshinImpact_Data/Plugins/InControlNative.dll",
          "GenshinImpact_Data/Plugins/chrome_elf.dll",
          //
          "GenshinImpact_Data/Plugins/cri_mana_vpx.dll",
          "GenshinImpact_Data/Plugins/cri_vip_unity_pc.dll",
          "GenshinImpact_Data/Plugins/cri_ware_unity.dll",
          "GenshinImpact_Data/Plugins/d3dcompiler_43.dll",
          "GenshinImpact_Data/Plugins/d3dcompiler_47.dll",
          "GenshinImpact_Data/Plugins/hdiffz.dll",
          "GenshinImpact_Data/Plugins/hpatchz.dll",
          "GenshinImpact_Data/Plugins/mihoyonet.dll",
          "GenshinImpact_Data/Plugins/Mmoron.dll",
          "GenshinImpact_Data/Plugins/MTBenchmark_Windows.dll",
          "GenshinImpact_Data/Plugins/NamedPipeClient.dll",
          "GenshinImpact_Data/Plugins/UnityNativeChromaSDK.dll",
          "GenshinImpact_Data/Plugins/UnityNativeChromaSDK3.dll",
          "GenshinImpact_Data/Plugins/xlua.dll",
          "GenshinImpact_Data/StreamingAssets/20527480.blk",
          //2.6新加部分
          "mhyprot3.Sys",
          "mhyprot2.Sys",
          //
          "Audio_Chinese_pkg_version",
          "pkg_version",
          "UnityPlayer.dll",
          "GenshinImpact.exe"
        };

        private readonly string[] cnfiles = new string[]
        { "YuanShen_Data/app.info",
          "YuanShen_Data/globalgamemanagers",
          "YuanShen_Data/globalgamemanagers.assets",
          "YuanShen_Data/globalgamemanagers.assets.resS",
          "YuanShen_Data/upload_crash.exe",
          "YuanShen_Data/Managed/Metadata/global-metadata.dat" ,
          "YuanShen_Data/Native/Data/Metadata/global-metadata.dat",
          "YuanShen_Data/Native/UserAssembly.dll",
          "YuanShen_Data/Native/UserAssembly.exp",
          "YuanShen_Data/Native/UserAssembly.lib",
          //2.6新加部分
          "YuanShen_Data/Plugins/ZFProxyWeb.dll",
          "YuanShen_Data/Plugins/ZFEmbedWeb.dll",
          "YuanShen_Data/Plugins/zf_cef.dll",
          "YuanShen_Data/Plugins/XInputInterface64.dll",
          "YuanShen_Data/Plugins/widevinecdmadapter.dll",
          "YuanShen_Data/Plugins/sqlite3.dll",
          "YuanShen_Data/Plugins/Rewired_DirectInput.dll",
          "YuanShen_Data/Plugins/metakeeper.dll",
          "YuanShen_Data/Plugins/libUbiCustomEvent.dll",
          "YuanShen_Data/Plugins/libGLESv2.dll",
          "YuanShen_Data/Plugins/libEGL.dll",
          "YuanShen_Data/Plugins/InControlNative.dll",
          "YuanShen_Data/Plugins/chrome_elf.dll",
          //
          "YuanShen_Data/Plugins/cri_mana_vpx.dll",
          "YuanShen_Data/Plugins/cri_vip_unity_pc.dll",
          "YuanShen_Data/Plugins/cri_ware_unity.dll",
          "YuanShen_Data/Plugins/d3dcompiler_43.dll",
          "YuanShen_Data/Plugins/d3dcompiler_47.dll",
          "YuanShen_Data/Plugins/hdiffz.dll",
          "YuanShen_Data/Plugins/hpatchz.dll",
          "YuanShen_Data/Plugins/mihoyonet.dll",
          "YuanShen_Data/Plugins/Mmoron.dll",
          "YuanShen_Data/Plugins/MTBenchmark_Windows.dll",
          "YuanShen_Data/Plugins/NamedPipeClient.dll",
          "YuanShen_Data/Plugins/UnityNativeChromaSDK.dll",
          "YuanShen_Data/Plugins/UnityNativeChromaSDK3.dll",
          "YuanShen_Data/Plugins/xlua.dll",
          "YuanShen_Data/StreamingAssets/20527480.blk",
          //2.6新加部分
          "mhyprot3.Sys",
          "mhyprot2.Sys",
          //
          "Audio_Chinese_pkg_version",
          "pkg_version",
          "UnityPlayer.dll",
          "YuanShen.exe"
        };

        private const string LauncherSection = "launcher";
        private const string GameInstallPath = "game_install_path";
        private const string ConfigFileName = "config.ini";

        private const string CnFolderName = "Cn";
        private const string GlobalFolderName = "Global";
        private const string UnknownFolderName = "unknown";

        private const string YuanShenDataFolderName = "YuanShen_Data";
        private const string GenshinImpactDataFolderName = "GenshinImpact_Data";

        #region Observable
        private bool buttonEnabled;
        private bool isCloseButtonEnabled = false;
        private string switchLog = "请稍候";
        private string stateIndicator = "状态：无状态";

        public bool ButtonEnabled
        {
            get => this.buttonEnabled;
            set => this.SetProperty(ref this.buttonEnabled, value);
        }
        public bool IsCloseButtonEnabled
        {
            get => this.isCloseButtonEnabled;
            set => this.SetProperty(ref this.isCloseButtonEnabled, value);
        }
        /// <summary>
        /// 转换时的日志列表
        /// </summary>
        public string SwitchLog
        {
            get => this.switchLog;
            set => this.SetProperty(ref this.switchLog, value);
        }
        /// <summary>
        /// 转换状态
        /// </summary>
        public string StateIndicator
        {
            get => this.stateIndicator;
            set => this.SetProperty(ref this.stateIndicator, value);
        }

        public ICommand OpenUICommand { get; }
        #endregion

        private string PackageVersion { get; set; } = string.Empty;
        private string GameFolder { get; set; } = string.Empty;

        private readonly ConvertDialog dialog;
        public ConvertDialogViewModel(ConvertDialog dialog)
        {
            this.dialog = dialog;
            string? launcherPath = Setting2.LauncherPath;
            this.TryLoadIniData(launcherPath);
            this.OpenUICommand = new AsyncRelayCommand(this.OpenUIAsync);
        }

        /// <summary>
        /// 对话框加载后执行
        /// </summary>
        /// <returns></returns>
        public async Task OpenUIAsync()
        {
            this.PackageVersion = await HtmlHelper.GetInfoFromHtmlAsync("pkg");

            if (!string.IsNullOrEmpty(this.GameFolder))
            {
                this.SwitchLog += $"已成功识别到游戏路径：{this.GameFolder}\r\n当前Pkg最新版本号为：{this.PackageVersion}\r\n";
                this.ButtonEnabled = true;
            }
            else
            {
                this.SwitchLog += $"未识别到游戏路径，请先前往左边的[启动游戏]中设置好路径再使用本插件\r\n";
                this.ButtonEnabled = false;
            }

            await this.ConvertGameFileAsync();
        }

        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="archiveName"></param>
        /// <returns></returns>
        private bool Decompress(string archiveName)
        {
            try
            {
                ZipFile.ExtractToDirectory($"{Environment.CurrentDirectory}/{archiveName}File.pkg", Environment.CurrentDirectory, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 转换游戏文件
        /// </summary>
        public async Task ConvertGameFileAsync()
        {
            try
            {
                string currentPath = Environment.CurrentDirectory;
                string port = this.GetCurrentSchemeName();
                string newport = port == CnFolderName ? GlobalFolderName : CnFolderName;

                string[]? oldfiles = port switch
                {
                    CnFolderName => cnfiles,
                    GlobalFolderName => globalfiles,
                    UnknownFolderName => null,
                    _ => throw Assumes.NotReachable()
                };

                string[]? newfiles = port switch
                {
                    CnFolderName => globalfiles,
                    GlobalFolderName => cnfiles,
                    UnknownFolderName => null,
                    _ => throw Assumes.NotReachable()
                };

                if (oldfiles != null && newfiles != null)
                {
                    await Task.Run(() =>
                    {
                        if (!this.CheckFileIntegrity(this.GameFolder, oldfiles, 1, ".bak"))
                        {
                            if (Directory.Exists($"{currentPath}/{newport}File"))
                            {
                                if (this.CheckPackageVersion($"{newport}File"))
                                {
                                    if (this.CheckFileIntegrity($"{currentPath}/{newport}File", newfiles, 0))
                                    {
                                        this.ReplaceGameFiles(oldfiles, newfiles, newport);
                                    }
                                }
                                else
                                {
                                    Directory.Delete($"{currentPath}/{newport}File", true);
                                }
                            }
                            else if (File.Exists($"{currentPath}/{newport}File.pkg"))
                            {
                                this.StateIndicator = "状态：解压PKG资源文件中";
                                if (this.Decompress(newport))
                                {
                                    if (this.CheckPackageVersion($"{newport}File"))
                                    {
                                        this.ReplaceGameFiles(oldfiles, newfiles, newport);
                                    }
                                    else
                                    {
                                        Directory.Delete($"{currentPath}/{newport}File", true);
                                        this.StateIndicator = "状态：PKG资源文件有新版本";
                                    }
                                }
                                else
                                {
                                    this.StateIndicator = "状态：PKG解压失败，请检查PKG是否正常";
                                    this.SwitchLog += $"资源[{newport}File.pkg]解压失败，请检查Pkg文件是否正常\r\n";
                                }
                            }
                            else
                            {
                                this.StateIndicator = "状态：请检查PKG文件是否存在";
                                this.SwitchLog += $"没有找到资源[{newport}File.pkg]，请检查Pkg文件是否存在于SG本体目录下\r\n";
                            }
                        }
                        else
                        {
                            this.RestoreGameFiles(oldfiles, newfiles, port);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(this.GameFolder))
                {
                    this.SwitchLog += $"请先前往[启动游戏]中选择正确的路径后再进行转换\r\n";
                }
                else
                {
                    this.SwitchLog += $"发生异常：\r\n{ex}\r\n请将该异常反馈给插件作者";
                }
            }
            finally
            {
                this.IsCloseButtonEnabled = true;
                this.dialog.IsCloseAllowed = true;
            }
        }

        /// <summary>
        /// 转换国际服及转换国服核心逻辑-判断客户端
        /// </summary>
        /// <returns></returns>
        private string GetCurrentSchemeName()
        {
            if (File.Exists(Path.Combine(this.GameFolder, "YuanShen.exe")))
            {
                return CnFolderName;
            }
            else if (File.Exists(Path.Combine(this.GameFolder, "GenshinImpact.exe")))
            {
                return GlobalFolderName;
            }
            else
            {
                return UnknownFolderName;
            }
        }

        /// <summary>
        /// 转换国际服及转换国服核心逻辑-判断PKG文件版本
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        private bool CheckPackageVersion(string scheme)
        {
            if (!File.Exists($"{Environment.CurrentDirectory}/{scheme}/{this.PackageVersion}"))
            {
                DGP.Genshin.App.Current.Dispatcher.Invoke(() =>
                new ToastContentBuilder()
                .AddText($"pkg文件存在新版本：{this.PackageVersion}")
                .AddText("已为您打开下载地址")
                .Show());

                Browser.Open("https://pan.baidu.com/s/1-5zQoVfE7ImdXrn8OInKqg");
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 遍历判断文件是否存在
        /// </summary>
        /// <param name="dirpath"></param>
        /// <param name="filepath"></param>
        /// <param name="length"></param>
        /// <param name="surfix"></param>
        /// <returns></returns>
        private bool CheckFileIntegrity(string dirpath, string[] filepath, int length, string surfix = "")
        {
            bool succeed = true;
            for (int i = 0; i < filepath.Length - length; i++)
            {
                if (File.Exists(Path.Combine(dirpath, filepath[i] + surfix)) == false)
                {
                    this.SwitchLog += $"{filepath[i]} {surfix} 文件不存在，将尝试下一步操作\r\n若无反应请重新下载资源文件！\r\n";
                    succeed = false;
                    break;
                }
                this.SwitchLog += $"{filepath[i]} {surfix} 存在\r\n";
            }
            return succeed;
        }

        /// <summary>
        /// 替换客户端文件
        /// </summary>
        /// <param name="originalfile"></param>
        /// <param name="newfile"></param>
        /// <param name="scheme"></param>
        private void ReplaceGameFiles(string[] originalfile, string[] newfile, string scheme)
        {
            this.StateIndicator = "状态：备份原始客户端中";
            for (int a = 0; a < originalfile.Length; a++)
            {
                string newFileName = Path.Combine(this.GameFolder, originalfile[a]);

                if (File.Exists(Path.Combine(this.GameFolder, originalfile[a])))
                {
                    try
                    {
                        File.Move(newFileName, newFileName + ".bak");
                        this.SwitchLog += $"{newFileName} 备份成功\r\n";
                    }
                    catch (Exception ex)
                    {
                        this.SwitchLog += $"{newFileName} 备份失败：{ex.Message}\r\n";
                    }
                }
                else
                {
                    this.SwitchLog += $"{newFileName} 文件不存在，备份失败，跳过\r\n";
                }
            }

            this.StateIndicator = "状态：正在替换新文件到客户端";
            string originalGameDataFolder = scheme == GlobalFolderName ? YuanShenDataFolderName : GenshinImpactDataFolderName;
            string newGameDataFolder = scheme == GlobalFolderName ? GenshinImpactDataFolderName : YuanShenDataFolderName;

            Directory.Move(Path.Combine(this.GameFolder, originalGameDataFolder), Path.Combine(this.GameFolder, newGameDataFolder));
            for (int i = 0; i < newfile.Length; i++)
            {
                File.Copy(Path.Combine(@$"{scheme}File", newfile[i]), Path.Combine(this.GameFolder, newfile[i]), true);
                this.SwitchLog += $"{newfile[i]} 替换成功\r\n";
            };

            this.SwitchLog += "转换完成，您可以启动游戏了";
            this.StateIndicator = "状态：无状态";
        }

        /// <summary>
        /// 还原客户端文件
        /// </summary>
        /// <param name="newfile"></param>
        /// <param name="originalfile"></param>
        /// <param name="scheme"></param>
        private void RestoreGameFiles(string[] newfile, string[] originalfile, string scheme)
        {
            //Computer redir = new();
            this.StateIndicator = "状态：清理多余文件中";
            for (int i = 0; i < newfile.Length; i++)
            {
                if (File.Exists(Path.Combine(this.GameFolder, newfile[i])))
                {
                    File.Delete(Path.Combine(this.GameFolder, newfile[i]));
                    this.SwitchLog += $"{newfile[i]} 清理完毕\r\n";
                }
                else
                {
                    this.SwitchLog += $"{newfile[i]} 文件不存在，已跳过\r\n";
                }
            }
            this.StateIndicator = "状态：正在还原原始客户端文件";

            string nowGameDataFolder = scheme == GlobalFolderName ? GenshinImpactDataFolderName : YuanShenDataFolderName;
            string originalGameDataFolder = scheme == GlobalFolderName ? YuanShenDataFolderName : GenshinImpactDataFolderName;

            Directory.Move(Path.Combine(this.GameFolder, nowGameDataFolder), Path.Combine(this.GameFolder, originalGameDataFolder));
            int total = 0, success = 0;
            for (int a = 0; a < originalfile.Length; a++)
            {
                string newFileName = Path.Combine(this.GameFolder, originalfile[a]);
                if (File.Exists(Path.Combine(this.GameFolder, originalfile[a] + ".bak")))
                {
                    Directory.Move(newFileName + ".bak", newFileName);
                    this.SwitchLog += $"{originalfile[a]} 还原成功\r\n";
                    success++;
                }
                else
                {
                    this.SwitchLog += $"{originalfile[a]} 跳过还原\r\n";
                    total++;
                }
            }

            this.StateIndicator = "状态：无状态";
            this.SwitchLog += $"还原完毕 , 还原成功 : {success} 个文件 ,还原失败 : {total} 个文件\r\n";
            this.SwitchLog += "转换完成，您可以启动游戏了";
        }

        #region Copy from service
        private IniData? launcherConfig;
        public IniData LauncherConfig
        {
            get => Requires.NotNull(this.launcherConfig!, nameof(this.launcherConfig));
        }

        [MemberNotNullWhen(true, nameof(launcherConfig))]
        public bool TryLoadIniData(string? launcherPath)
        {
            if (launcherPath != null)
            {
                try
                {
                    string configPath = Path.Combine(Path.GetDirectoryName(launcherPath)!, ConfigFileName);
                    this.launcherConfig = this.GetIniData(configPath);
                    string unescapedGameFolder = this.GetUnescapedGameFolderFromLauncherConfig();
                    this.GameFolder = unescapedGameFolder;
                }
                catch (ParsingException) { return false; }
                catch (ArgumentNullException) { return false; }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private IniData GetIniData(string file)
        {
            FileIniDataParser parser = new();
            parser.Parser.Configuration.AssigmentSpacer = string.Empty;
            return parser.ReadFile(file);
        }

        /// <summary>
        /// 还原转义后的原游戏目录
        /// 目录符号应为/
        /// 因为配置中的游戏目录若包含中文会转义为 \xaaaa 形态
        /// </summary>
        private string GetUnescapedGameFolderFromLauncherConfig()
        {
            string gameInstallPath = this.LauncherConfig[LauncherSection][GameInstallPath];
            string? hex4Result = Regex.Replace(gameInstallPath, @"\\x([0-9a-f]{4})", @"\u$1");
            if (!hex4Result.Contains(@"\u"))//不包含中文
            {
                //fix path with \
                hex4Result = hex4Result.Replace(@"\", @"\\");
            }
            return Regex.Unescape(hex4Result);
        }
        #endregion
    }
}
