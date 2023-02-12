using Dalamud.Configuration;
using Dalamud.Game.Gui.Toast;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace MusicPlayer
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public string MusicFolderPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        public Vector2 WindowPosition { get; set; } = new Vector2(30, 30);
        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        [NonSerialized]
        public List<string>? Songs;
        
        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            LoadSongs();
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }

        public void LoadSongs()
        {
            Songs = Directory.EnumerateFiles(MusicFolderPath, "*.mp3").ToList<string>();
            PluginLog.Debug("Loaded: " + Songs.Count);
        }

    }
}
