using DGP.Genshin.Control.Infrastructure.Observable;
using DGP.Genshin.Core.ImplementationSwitching;
using DGP.Genshin.DataModel.Launching;
using DGP.Genshin.FPSUnlocking;
using DGP.Genshin.Helper;
using DGP.Genshin.Service.Abstraction.Launching;
using DGP.Genshin.Service.Abstraction.Setting;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using Microsoft.AppCenter.Crashes;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using ModernWpf.Controls;
using Snap.Core.Logging;
using Snap.Data.Json;
using Snap.Data.Primitive;
using Snap.Data.Utility;
using Snap.Exception;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Genshin.Launcher.Plus.SE.Plugin
{
    [SwitchableImplementation(typeof(ILaunchService), "Genshin.Launcher.Plus", "原神启动器Plus")]
    public class LaunchService2 : ILaunchService
    {
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

        /// <summary>
        /// 定义了对注册表的操作
        /// </summary>
        private class GenshinRegistry
        {
            private const string GenshinKey = @"HKEY_CURRENT_USER\Software\miHoYo\原神";
            private const string SdkKey = "MIHOYOSDK_ADL_PROD_CN_h3123967166";

            public static bool Set(GenshinAccount? account)
            {
                if (account?.MihoyoSDK is not null)
                {
                    Registry.SetValue(GenshinKey, SdkKey, Encoding.UTF8.GetBytes(account.MihoyoSDK));
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 在注册表中获取账号信息
            /// 若不提供命名，则返回的账号仅用于比较，不应存入列表中
            /// </summary>
            /// <param name="accountNamer"></param>
            /// <returns></returns>
            public static GenshinAccount? Get()
            {
                object? sdk = Registry.GetValue(GenshinKey, SdkKey, string.Empty);

                if (sdk is null)
                {
                    return null;
                }

                string sdkString = Encoding.UTF8.GetString((byte[])sdk);

                return new GenshinAccount { MihoyoSDK = sdkString };
            }
        }

        private IniData? launcherConfig;
        private IniData? gameConfig;

        public IniData LauncherConfig
        {
            get => launcherConfig ?? throw new ArgumentNullException("启动器路径不能为 null");
        }

        public IniData GameConfig
        {
            get => gameConfig ?? throw new ArgumentNullException("启动器路径不能为 null");
        }

        public WorkWatcher GameWatcher { get; } = new();

        public LaunchService2()
        {
            PathContext.CreateFileOrIgnore(AccountsFileName);
            string? launcherPath = Setting2.LauncherPath;
            TryLoadIniData(launcherPath);
        }

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
                    gameConfig = GetIniData(Path.Combine(unescapedGameFolder, ConfigFileName));
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

        /// <summary>
        /// 读取 ini 文件
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        private IniData GetIniData(string file)
        {
            FileIniDataParser parser = new();
            parser.Parser.Configuration.AssigmentSpacer = string.Empty;
            return parser.ReadFile(file);
        }


        private Unlocker? unlocker;

        public async Task LaunchAsync(LaunchOption option, Action<Exception> failAction)
        {


            string? launcherPath = Setting2.LauncherPath.Get();
            if (launcherPath is not null)
            {
                string unescapedGameFolder = GetUnescapedGameFolderFromLauncherConfig();
                string gamePath;
                if (File.Exists(Path.Combine(unescapedGameFolder, "GenshinImpact.exe")))
                {
                    gamePath = Path.Combine(unescapedGameFolder, "GenshinImpact.exe");
                }
                else
                {
                    gamePath = Path.Combine(unescapedGameFolder, LauncherConfig[LauncherSection][GameName]);
                }

                try
                {
                    if (GameWatcher.IsWorking)
                    {
                        throw new SnapGenshinInternalException("游戏已经启动");
                    }
                    //https://docs.unity.cn/cn/current/Manual/CommandLineArguments.html
                    string commandLine = new CommandLineBuilder()
                        .AppendIf("-popupwindow", option.IsBorderless)
                        .Append("-screen-fullscreen", option.IsFullScreen ? 1 : 0)
                        .Append("-screen-width", option.ScreenWidth)
                        .Append("-screen-height", option.ScreenHeight)
                        .Build();

                    Process? game = new()
                    {
                        StartInfo = new()
                        {
                            FileName = gamePath,
                            Verb = "runas",
                            WorkingDirectory = Path.GetDirectoryName(gamePath),
                            UseShellExecute = true,
                            Arguments = commandLine
                        }
                    };

                    using (GameWatcher.Watch())
                    {
                        if (option.UnlockFPS)
                        {
                            unlocker = new(game, option.TargetFPS);
                            UnlockResult result = await unlocker.StartProcessAndUnlockAsync();
                            this.Log(result);
                        }
                        else
                        {
                            if (game.Start())
                            {
                                await game.WaitForExitAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    failAction.Invoke(ex);
                }
            }
        }

        /// <summary>
        /// 动态更改FPS，仅在使用解锁帧率后有效
        /// </summary>
        /// <param name="targetFPS"></param>
        public void SetTargetFPSDynamically(int targetFPS)
        {
            if (unlocker is not null)
            {
                unlocker.TargetFPS = targetFPS;
            }
        }

        public void OpenOfficialLauncher(Action<Exception>? failAction)
        {
            string? launcherPath = Setting2.LauncherPath.Get();
            try
            {
                ProcessStartInfo info = new()
                {
                    FileName = launcherPath,
                    Verb = "runas",
                    UseShellExecute = true,
                };
                Process? p = Process.Start(info);
            }
            catch (Exception ex)
            {
                failAction?.Invoke(ex);
            }
        }

        /// <summary>
        /// 还原转义后的原游戏目录
        /// 目录符号应为/
        /// 因为配置中的游戏目录若包含中文会转义为 \xaaaa 形态
        /// </summary>
        /// <returns></returns>
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

        public string? SelectLaunchDirectoryIfIncorrect(string? launcherPath)
        {
            if (!File.Exists(launcherPath) || Path.GetFileName(launcherPath) != LauncherExecutable)
            {
                OpenFileDialog openFileDialog = new()
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Filter = "启动器|launcher.exe",
                    Title = "选择启动器文件",
                    CheckPathExists = true,
                    FileName = LauncherExecutable
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string fileName = openFileDialog.FileName;
                    if (Path.GetFileName(fileName) == LauncherExecutable)
                    {
                        launcherPath = openFileDialog.FileName;
                        Setting2.LauncherPath.Set(launcherPath);
                    }
                }
            }
            return launcherPath;
        }

        public void SaveLaunchScheme(LaunchScheme? scheme)
        {
            if (scheme is null)
            {
                return;
            }

            string unescapedGameFolder = GetUnescapedGameFolderFromLauncherConfig();
            string configFilePath = Path.Combine(unescapedGameFolder, ConfigFileName);
            if (GameWatcher.IsWorking)
            {
                throw new SnapGenshinInternalException("游戏已经启动");
            }
            switch (scheme.CPS)
            {
                case "mihoyo":
                    if (!File.Exists(Path.Combine(unescapedGameFolder, "GenshinImpact.exe")))
                    {
                        //await new ContentDialog() { Title = "test", PrimaryButtonText = "Ok" }.ShowAsync();
                        DGP.Genshin.App.Current.Dispatcher.Invoke(async () => await new ConvertDialog().ShowAsync()).Forget();
                        if (File.Exists(Path.Combine(unescapedGameFolder, "GenshinImpact_Data/Plugins/PCGameSDK.dll")))
                        {
                            File.Delete(Path.Combine(unescapedGameFolder, "GenshinImpact_Data/Plugins/PCGameSDK.dll"));
                        }
                    }
                    break;
                case "bilibili":
                    if (!File.Exists(Path.Combine(unescapedGameFolder, "YuanShen_Data/Plugins/PCGameSDK.dll")))
                    {
                        new ContentDialog() { Title = "哔哩哔哩SDK不存在", PrimaryButtonText = "确定" }.ShowAsync().Forget();
                    }
                    break;
                case "pcadbdpz":
                    if (!File.Exists(Path.Combine(unescapedGameFolder, "YuanShen.exe")))
                    {
                        //await new ContentDialog() { Title = "test", PrimaryButtonText = "Ok" }.ShowAsync();
                        DGP.Genshin.App.Current.Dispatcher.Invoke(async () => await new ConvertDialog().ShowAsync()).Forget();
                        if (File.Exists(Path.Combine(unescapedGameFolder, "YuanShen_Data/Plugins/PCGameSDK.dll")))
                        {
                            File.Delete(Path.Combine(unescapedGameFolder, "YuanShen_Data/Plugins/PCGameSDK.dll"));
                        }
                    }
                    break;
            }
            GameConfig[GeneralSection][Channel] = scheme.Channel;
            GameConfig[GeneralSection][CPS] = scheme.CPS;
            GameConfig[GeneralSection][SubChannel] = scheme.SubChannel;
            //new UTF8Encoding(false) compat with https://github.com/DawnFz/GenShin-LauncherDIY
            new FileIniDataParser().WriteFile(configFilePath, GameConfig, new UTF8Encoding(false));
            //
        }

        public ObservableCollection<GenshinAccount> LoadAllAccount()
        {
            //fix load file failure while launched by updater in admin
            return Json.FromFileOrNew<ObservableCollection<GenshinAccount>>(PathContext.Locate(AccountsFileName));
        }
        public void SaveAllAccounts(IEnumerable<GenshinAccount> accounts)
        {
            //trim account with same id
            Json.ToFile(PathContext.Locate(AccountsFileName), accounts.DistinctBy(account => account.MihoyoSDK));
        }
        public GenshinAccount? GetFromRegistry()
        {
            return GenshinRegistry.Get();
        }
        public bool SetToRegistry(GenshinAccount? account)
        {
            return GenshinRegistry.Set(account);
        }
    }
}
