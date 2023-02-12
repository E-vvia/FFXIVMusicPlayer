using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace MusicPlayer.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly FileDialogManager fileDialogManager = new FileDialogManager();
    private readonly PlayerWindow mainWindow;

    private bool dialogOpened = false;

    private string tempMusicFolderPath = string.Empty;

    public ConfigWindow(Plugin plugin, PlayerWindow mainWindow) : base(
        "Music Player configuration Window",
        ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(500, 150);
        this.SizeCondition = ImGuiCond.Appearing;

        this.mainWindow = mainWindow;
        this.configuration = plugin.PluginConfiguration;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public override void OnOpen()
    {
        tempMusicFolderPath = this.configuration.MusicFolderPath;
    }

    public override void OnClose()
    {
        
        this.mainWindow.RestoreDefaults();
        this.mainWindow.DisableMovable();
    }
    public override void Draw()
    {
        if (dialogOpened)
            fileDialogManager.Draw();

        ImGui.InputText("Music folder path", ref tempMusicFolderPath, 256);
        ImGui.SameLine();
        if (ImGui.Button("Search"))
        {
            dialogOpened = true;
            fileDialogManager.OpenFolderDialog("Search music folder",
              delegate (bool isOk, string result)
              {
                  if (isOk)
                  {
                      tempMusicFolderPath = result;
                  }
              },
              Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), true);
        }
        if (!mainWindow.CanBeMoved)
        {
            if (ImGui.Button("Edit player position"))
            {
                mainWindow.EnableMovable();
            }
        }
        else if (mainWindow.CanBeMoved)
        {
            if (ImGui.Button("Stop editing"))
            {
                mainWindow.DisableMovable();
            }
        }
        ImGui.Dummy(new Vector2(10.0f, 10.0f));
        ImGui.SameLine(ImGui.GetWindowWidth() - 100);
        if (ImGui.Button("Save and close"))
        {
            this.IsOpen = false;
            this.configuration.MusicFolderPath = tempMusicFolderPath;
            this.configuration.WindowPosition = this.mainWindow.CurrentWindowPosition;
            this.configuration.Save();
        }

    }
}
