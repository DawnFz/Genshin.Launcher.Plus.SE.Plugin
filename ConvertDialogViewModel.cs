using DGP.Genshin.Service.Abstraction.Setting;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using Microsoft;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualBasic.Devices;
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
        private static readonly string[] globalfiles = new string[]
        {
            "GenshinImpact_Data/app.info",
            "GenshinImpact_Data/globalgamemanagers",
            "GenshinImpact_Data/globalgamemanagers.assets",
            "GenshinImpact_Data/globalgamemanagers.assets.resS",
            "GenshinImpact_Data/upload_crash.exe",
            "GenshinImpact_Data/Managed/Metadata/global-metadata.dat" ,
            "GenshinImpact_Data/Native/Data/Metadata/global-metadata.dat",
            "GenshinImpact_Data/Native/UserAssembly.dll",
            "GenshinImpact_Data/Native/UserAssembly.exp",
            "GenshinImpact_Data/Native/UserAssembly.lib",
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
            "Audio_Chinese_pkg_version",
            "pkg_version",
            "UnityPlayer.dll",
            "GenshinImpact.exe"
        };
        private static readonly string[] cnfiles = new string[]
        {
            "YuanShen_Data/app.info",
            "YuanShen_Data/globalgamemanagers",
            "YuanShen_Data/globalgamemanagers.assets",
            "YuanShen_Data/globalgamemanagers.assets.resS",
            "YuanShen_Data/upload_crash.exe",
            "YuanShen_Data/Managed/Metadata/global-metadata.dat" ,
            "YuanShen_Data/Native/Data/Metadata/global-metadata.dat",
            "YuanShen_Data/Native/UserAssembly.dll",
            "YuanShen_Data/Native/UserAssembly.exp",
            "YuanShen_Data/Native/UserAssembly.lib",
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
            get => buttonEnabled;
            set => SetProperty(ref buttonEnabled, value);
        }
        public bool IsCloseButtonEnabled
        {
            get => isCloseButtonEnabled;
            set => SetProperty(ref isCloseButtonEnabled, value);
        }
        /// <summary>
        /// 转换时的日志列表
        /// </summary>
        public string SwitchLog
        {
            get => switchLog;
            set => SetProperty(ref switchLog, value);
        }
        /// <summary>
        /// 转换状态
        /// </summary>
        public string StateIndicator
        {
            get => stateIndicator;
            set => SetProperty(ref stateIndicator, value);
        }

        public ICommand OpenUICommand { get; }
        #endregion

        private string PackageVersion { get; set; } = string.Empty;
        private string GameFolder { get; set; } = string.Empty;

        public ConvertDialogViewModel(ConvertDialog dialog)
        {
            string? launcherPath = Setting2.LauncherPath;
            TryLoadIniData(launcherPath);

            OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
        }

        /// <summary>
        /// 对话框加载后执行
        /// </summary>
        /// <returns></returns>
        public async Task OpenUIAsync()
        {
            PackageVersion = await HtmlHelper.GetInfoFromHtmlAsync("pkg");

            if (!string.IsNullOrEmpty(GameFolder))
            {
                SwitchLog += $"已成功识别到游戏路径：{GameFolder}\r\n当前Pkg最新版本号为：{PackageVersion}\r\n";
                ButtonEnabled = true;
            }
            else
            {
                SwitchLog += $"未识别到游戏路径，请先前往左边的[启动游戏]中设置好路径再使用本插件\r\n";
                ButtonEnabled = false;
            }

            await ConvertGameFileAsync();
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
                string port = GetCurrentSchemeName();
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
                        if (!CheckFileIntegrity(GameFolder, oldfiles, 1, ".bak"))
                        {
                            if (Directory.Exists($"{currentPath}/{newport}File"))
                            {
                                if (CheckPackageVersion($"{newport}File"))
                                {
                                    if (CheckFileIntegrity($"{currentPath}/{newport}File", newfiles, 0))
                                    {
                                        ReplaceGameFiles(oldfiles, newfiles, newport);
                                    }
                                }
                                else
                                {
                                    Directory.Delete($"{currentPath}/{newport}File", true);
                                }
                            }
                            else if (File.Exists($"{currentPath}/{newport}File.pkg"))
                            {
                                StateIndicator = "状态：解压PKG资源文件中";
                                if (Decompress(newport))
                                {
                                    if (CheckPackageVersion($"{newport}File"))
                                    {
                                        ReplaceGameFiles(oldfiles, newfiles, newport);
                                    }
                                    else
                                    {
                                        Directory.Delete($"{currentPath}/{newport}File", true);
                                        StateIndicator = "状态：PKG资源文件有新版本";
                                    }
                                }
                                else
                                {
                                    StateIndicator = "状态：PKG解压失败，请检查PKG是否正常";
                                    SwitchLog += $"资源[{newport}File.pkg]解压失败，请检查Pkg文件是否正常\r\n";
                                }
                            }
                            else
                            {
                                StateIndicator = "状态：请检查PKG文件是否存在";
                                SwitchLog += $"没有找到资源[{newport}File.pkg]，请检查Pkg文件是否存在于SG本体目录下\r\n";
                            }
                        }
                        else
                        {
                            RestoreGameFiles(oldfiles, newfiles, port);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(GameFolder))
                {
                    SwitchLog += $"请先前往[启动游戏]中选择正确的路径后再进行转换\r\n";
                }
                else
                {
                    SwitchLog += $"发生异常：\r\n{ex}\r\n请将该异常反馈给插件作者";
                }
            }
            finally
            {
                IsCloseButtonEnabled = true;
            }
        }

        /// <summary>
        /// 转换国际服及转换国服核心逻辑-判断客户端
        /// </summary>
        /// <returns></returns>
        private string GetCurrentSchemeName()
        {
            if (File.Exists(Path.Combine(GameFolder, "YuanShen.exe")))
            {
                return CnFolderName;
            }
            else if (File.Exists(Path.Combine(GameFolder, "GenshinImpact.exe")))
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
            if (!File.Exists($"{Environment.CurrentDirectory}/{scheme}/{PackageVersion}"))
            {
                DGP.Genshin.App.Current.Dispatcher.Invoke(() =>
                new ToastContentBuilder()
                .AddText($"pkg文件存在新版本：{PackageVersion}")
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
                    SwitchLog += $"{filepath[i]} {surfix} 文件不存在，将尝试下一步操作\r\n若无反应请重新下载资源文件！\r\n";
                    succeed = false;
                    break;
                }
                SwitchLog += $"{filepath[i]} {surfix} 存在\r\n";
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
            StateIndicator = "状态：备份原始客户端中";
            for (int a = 0; a < originalfile.Length; a++)
            {
                string newFileName = Path.GetFileName(Path.Combine(GameFolder, originalfile[a]));

                if (File.Exists(Path.Combine(GameFolder, originalfile[a])))
                {
                    try
                    {
                        File.Move(Path.Combine(GameFolder, originalfile[a]), newFileName + ".bak");
                        SwitchLog += $"{newFileName} 备份成功\r\n";
                    }
                    catch (Exception ex)
                    {
                        SwitchLog += $"{newFileName} 备份失败：{ex.Message}\r\n";
                    }
                }
                else
                {
                    SwitchLog += $"{newFileName} 文件不存在，备份失败，跳过\r\n";
                }
            }

            StateIndicator = "状态：正在替换新文件到客户端";
            string originalGameDataFolder = scheme == GlobalFolderName ? YuanShenDataFolderName : GenshinImpactDataFolderName;
            string newGameDataFolder = scheme == GlobalFolderName ? GenshinImpactDataFolderName : YuanShenDataFolderName;

            Directory.Move(Path.Combine(GameFolder, originalGameDataFolder), newGameDataFolder);
            for (int i = 0; i < newfile.Length; i++)
            {
                File.Copy(Path.Combine(@$"{scheme}File", newfile[i]), Path.Combine(GameFolder, newfile[i]), true);
                SwitchLog += $"{newfile[i]} 替换成功\r\n";
            };

            SwitchLog += "转换完成，您可以启动游戏了";
            StateIndicator = "状态：无状态";
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
            StateIndicator = "状态：清理多余文件中";
            for (int i = 0; i < newfile.Length; i++)
            {
                if (File.Exists(Path.Combine(GameFolder, newfile[i])))
                {
                    File.Delete(Path.Combine(GameFolder, newfile[i]));
                    SwitchLog += $"{newfile[i]} 清理完毕\r\n";
                }
                else
                {
                    SwitchLog += $"{newfile[i]} 文件不存在，已跳过\r\n";
                }
            }
            StateIndicator = "状态：正在还原原始客户端文件";

            string nowGameDataFolder = scheme == GlobalFolderName ? GenshinImpactDataFolderName : YuanShenDataFolderName;
            string originalGameDataFolder = scheme == GlobalFolderName ? YuanShenDataFolderName : GenshinImpactDataFolderName;

            Directory.Move(Path.Combine(GameFolder, nowGameDataFolder), originalGameDataFolder);
            int total = 0, success = 0;
            for (int a = 0; a < originalfile.Length; a++)
            {
                string newFileName = Path.GetFileNameWithoutExtension(originalfile[a]) + Path.GetExtension(originalfile[a]);
                if (File.Exists(Path.Combine(GameFolder, originalfile[a] + ".bak")))
                {
                    Directory.Move(Path.Combine(GameFolder, originalfile[a] + ".bak"), newFileName);
                    SwitchLog += $"{originalfile[a]} 还原成功\r\n";
                    success++;
                }
                else
                {
                    SwitchLog += $"{originalfile[a]} 跳过还原\r\n";
                    total++;
                }
            }

            StateIndicator = "状态：无状态";
            SwitchLog += $"还原完毕 , 还原成功 : {success} 个文件 ,还原失败 : {total} 个文件";
            SwitchLog += "转换完成，您可以启动游戏了";
        }

        #region Copy from service
        private IniData? launcherConfig;
        public IniData LauncherConfig
        {
            get => Requires.NotNull(launcherConfig!, nameof(launcherConfig));
        }

        [MemberNotNullWhen(true, nameof(launcherConfig))]
        public bool TryLoadIniData(string? launcherPath)
        {
            if (launcherPath != null)
            {
                try
                {
                    string configPath = Path.Combine(Path.GetDirectoryName(launcherPath)!, ConfigFileName);
                    launcherConfig = GetIniData(configPath);
                    string unescapedGameFolder = GetUnescapedGameFolderFromLauncherConfig();
                    GameFolder = unescapedGameFolder;
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
            string gameInstallPath = LauncherConfig[LauncherSection][GameInstallPath];
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
