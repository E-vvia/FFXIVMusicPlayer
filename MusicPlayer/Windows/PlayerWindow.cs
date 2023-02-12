using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using ImGuiScene;
using NAudio.Wave;

namespace MusicPlayer.Windows;

public class PlayerWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private WaveOutEvent? outputDevice = null;
    private AudioFileReader? currentSong = null;
    private int songIndex = 0;
    private float volume = 0.5f;

    public Vector2 CurrentWindowPosition { get; set; }
    public bool CanBeMoved = false;

    public PlayerWindow(Plugin plugin) : base(
        "Music Player", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground )
    {
        CurrentWindowPosition = plugin.PluginConfiguration.WindowPosition;
        this.plugin = plugin;
        this.IsOpen = true;
        this.RespectCloseHotkey= false;
    }

    public void Dispose()
    {

        DisposeWaveOut();

        DisposeSong();

        PluginLog.Log("PlayerWindow disposed");
        GC.SuppressFinalize(this);
    }

    public void EnableMovable()
    {
        CanBeMoved = true;
        this.Flags &= ~ImGuiWindowFlags.NoMove;
        this.Flags &= ~ImGuiWindowFlags.NoBackground;
    }

    public void DisableMovable()
    {
        CanBeMoved = false;
        this.Flags |= ImGuiWindowFlags.NoMove;
        this.Flags |= ImGuiWindowFlags.NoBackground;
    }

    public void RestoreDefaults()
    {
        CurrentWindowPosition = this.plugin.PluginConfiguration.WindowPosition;
        ImGui.SetNextWindowPos(CurrentWindowPosition);
    }

    public override void Draw()
    {
        string songName = string.Empty;


        if (currentSong != null)
        {
            songName = Path.GetFileNameWithoutExtension(currentSong.FileName);
        }
        else
        {
            songName = "No song selected";
        }


        ImGui.Text(songName);
        if (currentSong != null)
        {
            ImGui.SameLine();

            ImGui.Text(currentSong?.CurrentTime.ToString(@"mm\:ss"));
        }

        if (ImGui.ArrowButton("Prev", ImGuiDir.Left))
        {
            PreviousSong();
        }

        ImGui.SameLine();
        if (outputDevice == null|| outputDevice?.PlaybackState == PlaybackState.Paused || outputDevice?.PlaybackState == PlaybackState.Stopped)
        {
            if (ImGui.Button("Play"))
            {
                if (currentSong == null || outputDevice==null)
                {
                    PlaySong();
                }
                else
                {
                    outputDevice?.Play();
                }
            }
        }
        else
        {
            if (ImGui.Button("Pause"))
            {
                outputDevice?.Pause();
            }
        }

        ImGui.SameLine();

        if (ImGui.ArrowButton("Next", ImGuiDir.Right))
        {
            NextSong();
        }

        ImGui.SliderFloat("Volume", ref volume, 0, 1, "%.1f");

        if (currentSong != null)
            currentSong.Volume = volume;
        CurrentWindowPosition = ImGui.GetWindowPos();
    }

    private void PreviousSong()
    {
        songIndex = songIndex - 1 < 0 ? 0 : songIndex;
        PlaySong();
    }

    private void PlaySong()
    {
        if (plugin.PluginConfiguration.Songs == null)
            return;

        if (outputDevice != null)
            outputDevice.PlaybackStopped -= PlayBackStopped;

        DisposeWaveOut();
        DisposeSong();

        currentSong = new AudioFileReader(fileName: plugin.PluginConfiguration.Songs[songIndex]); 

        outputDevice = new WaveOutEvent();

        outputDevice?.Init(currentSong);
        outputDevice?.Play();

        if (outputDevice != null)
            outputDevice.PlaybackStopped += PlayBackStopped;

    }

    private void PlayBackStopped(object? sender, StoppedEventArgs e)
    {
        NextSong();
    }

    private void NextSong()
    {
        if (plugin.PluginConfiguration.Songs != null && currentSong!= null)
            songIndex = songIndex + 1 >= plugin.PluginConfiguration.Songs.Count ? 0 : songIndex + 1;
        PluginLog.Log("Next song: " + songIndex);
        PlaySong();
    }

    private void DisposeWaveOut()
    {
        if (outputDevice != null)
            outputDevice.PlaybackStopped -= PlayBackStopped;

        outputDevice?.Stop();
        outputDevice?.Dispose();
        outputDevice = null;
    }

    public void DisposeSong()
    {
        currentSong?.Dispose();
        currentSong = null;
    }
}

