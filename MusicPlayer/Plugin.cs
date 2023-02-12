using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using MusicPlayer.Windows;

namespace MusicPlayer
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "MusicPlayer";
        private const string CommandName = "/musicplayer"; //Command to use

        private DalamudPluginInterface pluginInterface { get; init; }
        private CommandManager commandManager { get; init; }
        public Configuration PluginConfiguration { get; init; } //Plugin configuration
        public WindowSystem WindowSystem = new WindowSystem("MusicPlayer");

        private ConfigWindow ConfigWindow { get; init; }
        private PlayerWindow PlayerWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.pluginInterface = pluginInterface;
            this.commandManager = commandManager;

            this.PluginConfiguration = this.pluginInterface.GetPluginConfig() as Configuration ?? new Configuration(); //Get previously saved configuration or build a new one if none exists.
            this.PluginConfiguration.Initialize(this.pluginInterface);

            
            PlayerWindow = new PlayerWindow(this);
            PlayerWindow.IsOpen= true;
            ConfigWindow = new ConfigWindow(this, PlayerWindow);

            //Add our windows to the system
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(PlayerWindow);

            this.commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Configure the music player."
            });

            this.pluginInterface.UiBuilder.Draw += DrawUI;
            this.pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            PlayerWindow.Dispose();

            this.commandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
