using iMusicSync.Services;
using IMusicSync.Common;
using IMusicSync.Data;
using IMusicSync.Services;
using IMusicSyncException.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace IMusicSync.States
{
    public class MainState : Notifiable
    {
        public readonly PlaylistService _playlistService = new();
        public readonly SyncService _syncService = new();

        private DeviceInfo _selectedDevice;
        private bool _isBusy = false;
        private int _progress = 0;

        public ObservableCollection<DeviceInfo> Devices { get; private set; } = new();
        public bool IsDeviceSelected => SelectedDevice != null;
        public bool IsLoadEnabled => IsDeviceSelected && !IsBusy;
        public List<string> LogsItems { get; private set; } = new();
        public string Logs => string.Join(Environment.NewLine, LogsItems);

        public MainState()
        {
            LoadUsbDevices();
            LoadPlayListCommand = new RelayCommand(LoadPlayList);
            LoadTitlesCommand = new RelayCommand(LoadTitles);

            _syncService.OnSyncProgress += OnlaylistSyncProgress;
            _syncService.OnSyncError += OnPlaylistSyncError;
        }

        public ICommand LoadPlayListCommand { get; set; }
        public ICommand LoadTitlesCommand { get; set; }

        public DeviceInfo SelectedDevice
        {
            get => _selectedDevice;
            set => SetValue(ref _selectedDevice, value, nameof(SelectedDevice), nameof(IsDeviceSelected));
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetValue(ref _isBusy, value, nameof(IsBusy), nameof(IsLoadEnabled));
        }

        public int Progress
        {
            get => _progress;
            set => SetValue(ref _progress, value, nameof(Progress));
        }

        public void SetProgress(int num, int count) => Progress = (int)Math.Round((double)num / count * 100, 0);

        public void ClearLogs()
        {
            LogsItems.Clear();
            NotifyChange(nameof(Logs));
        }

        public void AddLogLine(string line = "")
        {
            LogsItems.Add(line);
            NotifyChange(nameof(Logs));
        }

        private void LoadUsbDevices()
        {
            var list = DriveInfo
                .GetDrives()
                .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                .Select(d => new DeviceInfo
                {
                    Name = d.VolumeLabel,
                    AvailableSize = d.TotalFreeSpace,
                    Size = d.TotalSize,
                    Path = d.Name
                })
                .ToList();

            Devices = new ObservableCollection<DeviceInfo>(list);

            if (list.Any())
            {
                SelectedDevice = list[0];
                AddLogLine($"{list.Count} device{(list.Count > 1 ? "s" : "")} trouvé{(list.Count > 1 ? "s" : "")}.");
            }
            else
            {
                AddLogLine("Aucun device trouvé.");
            }
        }

        private void UpdateDeviceSize()
        {
            if (SelectedDevice == null)
            {
                return;
            }

            var di = DriveInfo
                .GetDrives()
                .Where(d => d.DriveType == DriveType.Removable && d.IsReady && d.Name == SelectedDevice.Path)
                .FirstOrDefault();

            if (di != null)
            {
                SelectedDevice.AvailableSize = di.TotalFreeSpace;
            }
        }

        private void OnlaylistSyncProgress(object sender, SyncProgressEventArgs e)
        {
            var result = e.Result;

            if (!result.IsSuccess)
            {
                AddLogLine($@"Erreur lors du traitement du titre ""{e.PlaylistItem.Label()}"" : {result.Message}.");
            }
            else
            {
                AddLogLine($@"Titre ""{e.PlaylistItem.Label()}"" synchronisé.");
            }

            SetProgress(e.Num, e.TotalCount);

            if (e.Num >= e.TotalCount)
            {
                AddLogLine();
                AddLogLine($"Terminé. {e.SyncCount} titre{(e.SyncCount > 1 ? "s" : "")} synchronisé{(e.SyncCount > 1 ? "s" : "")}, {e.SkipCount} déjà synchronisé{(e.SkipCount > 1 ? "s" : "")}, {e.ErrorCount} erreur{(e.ErrorCount > 1 ? "s" : "")}.");

                UpdateDeviceSize();
            }
        }

        private void OnPlaylistSyncError(object sender, SyncErrorEventArgs e)
        {
            AddLogLine($@"Erreur fatale: ""{e.Message}""");
            UpdateDeviceSize();
        }

        public void LoadPlayList()
        {
            try
            {
                IsBusy = true;

                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.DefaultExt = ".txt";
                dialog.Filter = "Playlist Files (*.txt)|*.txt";

                if (dialog.ShowDialog() == true)
                {
                    ClearLogs();
                    AddLogLine(@$"Chargement de la playlist ""{Path.GetFileName(dialog.FileName)}""...");
                    var playlist = _playlistService.ReadPlaylistFile(dialog.FileName);
                    AddLogLine($"Chargé {playlist.Count} lignes.");
                    AddLogLine();

                    if (playlist.Any(t => !t.HasData))
                    {
                        foreach (var title in playlist.Where(t => !t.HasData))
                        {
                            AddLogLine(@$"/!\ Le titre ""{title.Label()}"" n'a aucun contenu associé, il sera ignoré.");
                        }
                        AddLogLine();
                    }

                    if (playlist.Count == 0)
                    {
                        AddLogLine("Rien à traiter.");
                        return;
                    }

                    AddLogLine($"Synchronisation du contenu...");
                    _syncService.SyncTitles(playlist, SelectedDevice.Path);
                }
            }
            catch
            {}
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadTitles()
        {
            try
            {
                IsBusy = true;

                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Filter = "Music Files|*.mp3;*.m4a;*.m4p;*.ogg";
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == true)
                {
                    ClearLogs();
                    AddLogLine(@$"Chargement des titres...");
                    var playlist = _playlistService.ToPlaylist(dialog.FileNames);
                    AddLogLine($"Chargé {playlist.Count} titres.");
                    AddLogLine();

                    if (playlist.Count == 0)
                    {
                        AddLogLine("Rien à traiter.");
                        return;
                    }

                    AddLogLine($"Synchronisation du contenu...");
                    _syncService.SyncTitles(playlist, SelectedDevice.Path);
                }
            }
            catch
            { }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
