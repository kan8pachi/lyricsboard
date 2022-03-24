using IPA;
using IPA.Config;
using IPA.Config.Stores;
using LyricsBoard.Core.System;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace LyricsBoard
{
    [Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
    public class Plugin
    {
        //public const string HarmonyId = "com.github.kan8pachi.LyricsBoard";
        //internal static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(HarmonyId);

        [Init]
        public Plugin(IPALogger logger, Config conf, Zenjector zenjector)
        {
            var pluginConfig = conf.Generated<Configuration.PluginConfig>();
            zenjector.UseLogger(logger);
            zenjector.Install(Location.App, container => {
                container.Bind<IFileSystem>().To<SilentFileSystem>().AsCached();
                container.Bind<Core.LyricsBoardContext>().AsSingle().WithArguments(pluginConfig);
            });
            zenjector.Install(Location.Menu, container =>
            {
                container.BindInterfacesTo<View.MenuViewController>().AsSingle();
            });
            zenjector.Install(Location.Player, container =>
            {
                container.BindInterfacesTo<View.BoardBehaviour>().AsCached();
            });
        }
    }
}
