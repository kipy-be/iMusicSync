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
using System.Threading.Tasks;
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
            LoadTitlesCommand = new RelayCommand(LoadTitles);
        }

        public ICommand LoadPlayListCommand { get; set; }
        public ICommand LoadTitlesCommand { get; set; }

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

        private async Task SyncContent(List<PlaylistItem> playlist)
        {
            TitleSyncResult result;
            int skipCount = 0;
            int errorCount = 0;
            int syncCount = 0;

            AddLogLine($"Synchronisation du contenu...");

            foreach (var title in playlist.Where(t => t.HasData))
            {
                result = await _playlistService.SyncTitleToLocation(title, SelectedDevice.Path);

                if (!result.IsSuccess)
                {
                    errorCount++;
                    AddLogLine($@"Erreur lors du traitement du titre ""{title.Label()}"" : {result.Message}.");
                    continue;
                }

                if (result.IsSkipped)
                {
                    skipCount++;
                    continue;
                }

                syncCount++;
                AddLogLine($@"Titre ""{title.Label()}"" synchronisé.");
            }

            AddLogLine();
            AddLogLine($"Terminé. {syncCount} titre{(syncCount > 1 ? "s" : "")} synchronisé{(syncCount > 1 ? "s" : "")}, {skipCount} déjà synchronisé{(skipCount > 1 ? "s" : "")}, {errorCount} erreur{(errorCount > 1 ? "s" : "")}.");

            UpdateDeviceSize();
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
                            AddLogLine(@$"/!\ Le titre ""{title.Label()}"" n'a aucun contenu associé, il sera ignoré.");
                        }
                        AddLogLine();
                    }

                    if (playList.Count == 0)
                    {
                        AddLogLine("Rien à traiter.");
                        return;
                    }

                    await SyncContent(playList);
                }
            }
            catch
            {}
            finally
            {
                IsBusy = false;
            }
        }

        private async void LoadTitles()
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
                    var playList = _playlistService.ToPlaylist(dialog.FileNames);
                    AddLogLine($"Chargé {playList.Count} titres.");
                    AddLogLine();

                    if (playList.Count == 0)
                    {
                        AddLogLine("Rien à traiter.");
                        return;
                    }

                    await SyncContent(playList);
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
