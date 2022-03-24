using DGP.Genshin.DataModel.Launching;
using DGP.Genshin.Service.Abstraction.Launching;
using DGP.Genshin.Service.Abstraction.Setting;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.VisualBasic.Devices;
using Snap.Core.DependencyInjection;
using Snap.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Genshin.Launcher.Plus.SE.Plugin
{
    [ViewModel(InjectAs.Transient)]
    public class ConvertDialogViewModel : ObservableObject
    {
        //转换文件列表
        private string[] globalfiles = new string[]
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

        private string[] cnfiles = new string[]
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

        private const string AccountsFileName = "accounts.json";
        private const string LauncherSection = "launcher";
        private const string GameName = "game_start_name";
        private const string GeneralSection = "General";
        private const string Channel = "channel";
        private const string CPS = "cps";
        private const string SubChannel = "sub_channel";
        private const string GameInstallPath = "game_install_path";
        private const string ConfigFileName = "config.ini";
        private const string LauncherExecutable = "launcher.exe";

        public ConvertDialogViewModel(ConvertDialog dialog)
        {
            Dialog = dialog;
            ReadHtmlHelper readHtmlHelper = new();
            string? launcherPath = Setting2.LauncherPath;
            TryLoadIniData(launcherPath);
            pkgVer = readHtmlHelper.GetFromHtml("pkg");
            if (GameFolder != null && GameFolder != string.Empty)
            {
                GameSwitchLog += $"已成功识别到游戏路径：{GameFolder}\r\n当前Pkg最新版本号为：{pkgVer}\r\n";
                ButtonEnabled = true;
            }
            else
            {
                GameSwitchLog += $"未识别到游戏路径，请先前往左边的[启动游戏]中设置好路径再使用本插件\r\n";
                ButtonEnabled = false;
            }
            TimeStatus = "状态：无状态";
            CloseButtonVisibility = "Hidden";
            CloseDialogCommand = new RelayCommand(CloseDialog);
            GameFileConvert();
        }
        private ConvertDialog _Dialog;
        public ConvertDialog Dialog
        {
            get=> _Dialog;
            set => SetProperty(ref _Dialog, value);
        }

        public ICommand CloseDialogCommand { get; set; }
        private void CloseDialog()
        {
            DGP.Genshin.App.Current.Dispatcher.Invoke(() => Dialog.Hide());
        }

        private bool UnZip(string pkgName)
        {
            try
            {
                ZipFile.ExtractToDirectory($"{Environment.CurrentDirectory}/{pkgName}File.pkg", Environment.CurrentDirectory, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string pkgVer { get; set; }

        private bool _ButtonEnabled;
        public bool ButtonEnabled
        {
            get => _ButtonEnabled;
            set => SetProperty(ref _ButtonEnabled, value);
        }

        private string _CloseButtonVisibility;
        public string CloseButtonVisibility
        {
            get => _CloseButtonVisibility;
            set => SetProperty(ref _CloseButtonVisibility, value);
        }
        //转换时的日志列表
        private string _GameSwitchLog;
        public string GameSwitchLog
        {
            get => _GameSwitchLog;
            set => SetProperty(ref _GameSwitchLog, value);
        }

        //转换时的控件状态
        private string _PageUiStatus = "true";
        public string PageUiStatus
        {
            get => _PageUiStatus;
            set => SetProperty(ref _PageUiStatus, value);
        }

        //转换状态
        private string _TimeStatus;
        public string TimeStatus
        {
            get => _TimeStatus;
            set => SetProperty(ref _TimeStatus, value);
        }

        private string GameFolder { get; set; }

        public void GameFileConvert()
        {
            try
            {
                string currentPath = Environment.CurrentDirectory;
                string port = JudgeGamePort();
                string newport = port == "Cn" ? "Global" : "Cn";
                string[]? oldfiles = port switch
                {
                    "Cn" => cnfiles,
                    "Global" => globalfiles,
                    "unknown" => null,
                    _ => throw new NotImplementedException()
                };
                string[]? newfiles = port switch
                {
                    "Cn" => globalfiles,
                    "Global" => cnfiles,
                    "unknown" => null,
                    _ => throw new NotImplementedException()
                };
                if (oldfiles != null && newfiles != null)
                {
                    Task.Run(async () =>
                    {
                        if (!CheckFileIntegrity(GameFolder, oldfiles, 1, ".bak"))
                        {
                            if (Directory.Exists($"{currentPath}/{newport}File"))
                            {
                                if (JudgePkgVer($"{newport}File"))
                                {
                                    if (CheckFileIntegrity($"{currentPath}/{newport}File", newfiles, 0))
                                    {
                                        await ReplaceGameClientFile(oldfiles, newfiles, newport);
                                    }
                                }
                                else
                                {
                                    DirectoryInfo di = new($"{currentPath}/{newport}File");
                                    di.Delete(true);
                                }
                            }
                            else if (File.Exists($"{currentPath}/{newport}File.pkg"))
                            {
                                TimeStatus = "状态：解压PKG资源文件中";
                                if (UnZip(newport))
                                {
                                    if (JudgePkgVer($"{newport}File"))
                                    {
                                        await ReplaceGameClientFile(oldfiles, newfiles, newport);
                                    }
                                    else
                                    {
                                        DirectoryInfo di = new($"{currentPath}/{newport}File");
                                        di.Delete(true);
                                        TimeStatus = "状态：PKG资源文件有新版本";
                                        CloseButtonVisibility = "Visible";
                                    }
                                }
                                else
                                {
                                    CloseButtonVisibility = "Visible";
                                    TimeStatus = "状态：PKG解压失败，请检查PKG是否正常";
                                    GameSwitchLog += $"资源[{newport}File.pkg]解压失败，请检查Pkg文件是否正常\r\n";
                                }
                            }
                            else
                            {
                                CloseButtonVisibility = "Visible";
                                TimeStatus = "状态：请检查PKG文件是否存在";
                                GameSwitchLog += $"没有找到资源[{newport}File.pkg]，请检查Pkg文件是否存在于SG本体目录下\r\n";
                            }
                        }
                        else
                        {
                            await RestoreGameClientFile(oldfiles, newfiles, port);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                if (GameFolder == null || GameFolder == "")
                {
                    GameSwitchLog += $"请先前往左边的[启动游戏]中设置好路径再进行转换\r\n";
                    CloseButtonVisibility = "Visible";
                }
                else
                {
                    GameSwitchLog += $"发生异常：\r\n{ex}\r\n请将该异常反馈给插件作者";
                    CloseButtonVisibility = "Visible";
                }
            }
        }
        //转换国际服及转换国服核心逻辑-判断客户端
        private string JudgeGamePort()
        {
            if (File.Exists(Path.Combine(GameFolder, "YuanShen.exe")))
            {
                return "Cn";
            }
            else if (File.Exists(Path.Combine(GameFolder, "GenshinImpact.exe")))
            {
                return "Global";
            }
            else
            {
                return "unknown";
            }
        }

        //转换国际服及转换国服核心逻辑-判断PKG文件版本
        private bool JudgePkgVer(string GamePort)
        {
            if (!File.Exists($"{Environment.CurrentDirectory}/{GamePort}/{pkgVer}"))
            {
                MessageBox.Show($"pkg文件存在新版本：{pkgVer}");
                ProcessStartInfo info = new()
                {
                    FileName = "https://pan.baidu.com/s/1-5zQoVfE7ImdXrn8OInKqg",
                    UseShellExecute = true,
                };
                Process.Start(info);
                return false;
            }
            else
            {
                return true;
            }
        }

        //遍历判断文件是否存在
        private bool CheckFileIntegrity(string dirpath, string[] filepath, int len, string postfix = "")
        {
            bool notError = true;
            for (int i = 0; i < filepath.Length - len; i++)
            {
                if (File.Exists(Path.Combine(dirpath, filepath[i] + postfix)) == false)
                {
                    GameSwitchLog += $"{filepath[i]} {postfix} 文件不存在，将尝试下一步操作\r\n若无反应请重新下载资源文件！\r\n";
                    notError = false;
                    break;
                }
                GameSwitchLog += $"{filepath[i]} {postfix} 存在\r\n";
            }
            return notError;
        }

        //替换客户端文件
        private async Task ReplaceGameClientFile(string[] originalfile, string[] newfile, string port)
        {
            Computer redir = new();
            TimeStatus = "状态：备份原始客户端中";
            for (int a = 0; a < originalfile.Length; a++)
            {
                string newFileName = Path.GetFileNameWithoutExtension(Path.Combine(GameFolder, originalfile[a])) + Path.GetExtension(Path.Combine(GameFolder, originalfile[a]));
                if (File.Exists(Path.Combine(GameFolder, originalfile[a])))
                {
                    try
                    {
                        redir.FileSystem.RenameFile(Path.Combine(GameFolder, originalfile[a]), newFileName + ".bak");
                        GameSwitchLog += $"{newFileName} 备份成功\r\n";
                    }
                    catch (Exception ex)
                    {
                        GameSwitchLog += $"{newFileName} 备份失败：原因：";
                        GameSwitchLog += $"{ex.Message}\r\n\r\n";
                    }
                }
                else
                {
                    GameSwitchLog += $"{newFileName} 文件不存在，备份失败，跳过\r\n";
                }
            }
            TimeStatus = "状态：正在替换新文件到客户端";
            string originalGameDataFolder = port == "Global" ? "YuanShen_Data" : "GenshinImpact_Data";
            string newGameDataFolder = port == "Global" ? "GenshinImpact_Data" : "YuanShen_Data";
            redir.FileSystem.RenameDirectory(Path.Combine(GameFolder, originalGameDataFolder), newGameDataFolder);
            for (int i = 0; i < newfile.Length; i++)
            {
                File.Copy(Path.Combine(@$"{port}File", newfile[i]), Path.Combine(GameFolder, newfile[i]), true);
                GameSwitchLog += $"{newfile[i]} 替换成功\r\n";
            };
            GameSwitchLog += "转换完成，您可以启动游戏了";
            TimeStatus = "状态：无状态";
            DGP.Genshin.App.Current.Dispatcher.Invoke(()=> Dialog.Hide());
        }

        //还原客户端文件
        private async Task RestoreGameClientFile(string[] nowfile, string[] originalfile, string port)
        {
            Computer redir = new();
            TimeStatus = "状态：清理多余文件中";
            for (int i = 0; i < nowfile.Length; i++)
            {
                if (File.Exists(Path.Combine(GameFolder, nowfile[i])))
                {
                    File.Delete(Path.Combine(GameFolder, nowfile[i]));
                    GameSwitchLog += $"{nowfile[i]} 清理完毕\r\n";
                }
                else
                {
                    GameSwitchLog += $"{nowfile[i]} 文件不存在，已跳过\r\n";
                }
            }
            TimeStatus = "状态：正在还原原始客户端文件";
            string nowGameDataFolder = port == "Global" ? "GenshinImpact_Data" : "YuanShen_Data";
            string originalGameDataFolder = port == "Global" ? "YuanShen_Data" : "GenshinImpact_Data";
            redir.FileSystem.RenameDirectory(Path.Combine(GameFolder, nowGameDataFolder), originalGameDataFolder);
            int whole = 0, success = 0;
            for (int a = 0; a < originalfile.Length; a++)
            {
                string newFileName = Path.GetFileNameWithoutExtension(originalfile[a]) + Path.GetExtension(originalfile[a]);
                if (File.Exists(Path.Combine(GameFolder, originalfile[a] + ".bak")))
                {
                    redir.FileSystem.RenameFile(Path.Combine(GameFolder, originalfile[a] + ".bak"), newFileName);
                    GameSwitchLog += $"{originalfile[a]} 还原成功\r\n";
                    success++;
                }
                else
                {
                    GameSwitchLog += $"{originalfile[a]} 跳过还原\r\n";
                    whole++;
                }
            }
            string newport = port == "Cn" ? "Global" : "Cn";
            TimeStatus = "状态：无状态";
            GameSwitchLog += $"还原完毕 , 还原成功 : {success} 个文件 ,还原失败 : {whole} 个文件";
            GameSwitchLog += "转换完成，您可以启动游戏了";
            DGP.Genshin.App.Current.Dispatcher.Invoke(() => Dialog.Hide());

        }

        private IniData? launcherConfig;
        private IniData? gameConfig;
        public IniData LauncherConfig
        {
            get => launcherConfig ?? throw new Exception("启动器路径不能为 null");
        }
        public IniData GameConfig
        {
            get => gameConfig ?? throw new Exception("启动器路径不能为 null");
        }
        public WorkWatcher GameWatcher { get => throw new NotImplementedException(); }

        [MemberNotNullWhen(true, nameof(gameConfig)), MemberNotNullWhen(true, nameof(launcherConfig))]
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
                    gameConfig = GetIniData(Path.Combine(unescapedGameFolder, ConfigFileName));
                }
                catch (ParsingException) { return false; }
                catch (ArgumentNullException) { return false; }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                    MessageBox.Show(ex + "");
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
    }
}
