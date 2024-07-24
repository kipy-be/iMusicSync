using iMusicSync.Data;
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

        public ObservableCollection<DeviceInfo> Devices { get; private set; } = new();

        private DeviceInfo _selectedDevice;
        public DeviceInfo SelectedDevice
        {
            get => _selectedDevice;
            set => SetValue(ref _selectedDevice, value, nameof(SelectedDevice), nameof(IsDeviceSelected));
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetValue(ref _isBusy, value, nameof(IsBusy), nameof(IsLoadEnabled));
        }
        public bool IsDeviceSelected => SelectedDevice != null;
        public bool IsLoadEnabled => IsDeviceSelected && !IsBusy;

        public List<string> LogsItems { get; private set; } = new();
        public string Logs => string.Join(Environment.NewLine, LogsItems);

        public MainState()
        {
            LoadUsbDevices();
            LoadPlayListCommand = new RelayCommand(LoadPlayList);
        }

        public ICommand LoadPlayListCommand { get; set; }

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
                    AvailableSize = d.AvailableFreeSpace,
                    Size = d.TotalSize,
                    Path = d.Name
                })
                .ToList();

            Devices = new ObservableCollection<DeviceInfo>(list);

            if (list.Any())
            {
                SelectedDevice = list[0];
                AddLogLine($"{list.Count} device{(list.Count > 1 ? "s" : "")} trouvé{(list.Count > 1 ? "s" : "")}");
            }
            else
            {
                AddLogLine("Aucun device trouvé");
            }
        }

        public async void LoadPlayList()
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
                    var playList = _playlistService.ReadPlaylistFile(dialog.FileName);
                    AddLogLine($"Chargé {playList.Count} lignes.");
                    AddLogLine();

                    if (playList.Any(t => !t.HasData))
                    {
                        foreach (var title in playList.Where(t => !t.HasData))
                        {
                            AddLogLine(@$"/!\ Le titre ""{title.Artist} - {title.Title}"" n'a aucun contenu associé, il sera ignoré.");
                        }
                        AddLogLine();
                    }

                    if (playList.Count == 0)
                    {
                        AddLogLine("Rien à traiter.");
                        return;
                    }

                    AddLogLine($"Synchronisation du contenu...");

                    TitleSyncResult result;
                    int skipCount = 0;
                    int errorCount = 0;
                    int syncCount = 0;
                    foreach (var title in playList.Where(t => t.HasData))
                    {
                        result = await _playlistService.SyncTitleToLocation(title, SelectedDevice.Path);

                        if (!result.IsSuccess)
                        {
                            errorCount++;
                            AddLogLine($@"Erreur lors du traitement du titre ""{title.Artist} - {title.Title}"" : {result.Message}");
                            continue;
                        }

                        if (result.IsSkipped)
                        {
                            skipCount++;
                            continue;
                        }

                        syncCount++;
                        AddLogLine($@"Titre ""{title.Artist} - {title.Title}"" synchronisé");
                    }

                    AddLogLine();
                    AddLogLine($"Terminé. {syncCount} titre{(syncCount > 1 ? "s" : "")} synchronisé{(syncCount > 1 ? "s" : "")}, {skipCount} déjà synchronisé{(skipCount > 1 ? "s" : "")}, {errorCount} erreur{(errorCount > 1 ? "s" : "")})");
                }
            }
            catch
            {}
            finally
            {
                IsBusy = false;
            }
        }
    }
}
