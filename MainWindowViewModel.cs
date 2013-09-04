using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Command;
using Shimmer.Client;
using TestApp.Annotations;

namespace TestApp
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fvi.FileVersion;

            CheckForUpdateCommand = new RelayCommand(CheckForUpdate, CanCheckForUpdate);
            DownloadReleasesCommand = new RelayCommand(DownloadReleases, CanDownloadReleases);
            ApplyReleasesCommand = new RelayCommand(ApplyReleases, CanApplyReleases);
        }

        public string Version { get; set; }

        string updatePath;
        public string UpdatePath
        {
            get { return updatePath; }
            set
            {
                if (updatePath == value) return;

                OnPropertyChanged();
                updatePath = value;

                UpdateInfo = null;
                DownloadedUpdateInfo = null;

                CheckForUpdateCommand.RaiseCanExecuteChanged();
                DownloadReleasesCommand.RaiseCanExecuteChanged();
                ApplyReleasesCommand.RaiseCanExecuteChanged();
            }
        }

        UpdateInfo updateInfo;
        public UpdateInfo UpdateInfo
        {
            get { return updateInfo; }
            set
            {
                if (updateInfo == value) return;

                updateInfo = value;
                OnPropertyChanged();

                DownloadedUpdateInfo = null;

                DownloadReleasesCommand.RaiseCanExecuteChanged();
                ApplyReleasesCommand.RaiseCanExecuteChanged();
            }
        }

        public string CheckUpdatesResult { get; set; }

        UpdateInfo downloadedUpdateInfo;
        public UpdateInfo DownloadedUpdateInfo
        {
            get { return downloadedUpdateInfo; }
            set
            {
                if (downloadedUpdateInfo == value) return;

                downloadedUpdateInfo = value;
                OnPropertyChanged();

                ApplyReleasesCommand.RaiseCanExecuteChanged();
            }
        }

        public string DownloadUpdatesResult { get; set; }

        public RelayCommand CheckForUpdateCommand { get; set; }

        async void CheckForUpdate()
        {
            CheckUpdatesResult = "";
            using (var updater = new UpdateManager(UpdatePath, "TestApp", FrameworkVersion.Net45))
            {
                UpdateInfo = await updater.CheckForUpdate();

                SetResult(UpdateInfo);
            }
        }

        void SetResult(UpdateInfo info)
        {
            if (info == null)
            {
                CheckUpdatesResult = "No updates found";
            }
            else if (!info.ReleasesToApply.Any())
            {
                CheckUpdatesResult = "You're up to date!";
            }
            else
            {
                var latest = info.ReleasesToApply.MaxBy(x => x.Version).First();
                CheckUpdatesResult = String.Format("You can update to {0}", latest.Version);
            }
        }

        bool CanCheckForUpdate()
        {
            return !String.IsNullOrWhiteSpace(UpdatePath);
        }

        public RelayCommand DownloadReleasesCommand { get; set; }

        async void DownloadReleases()
        {
            DownloadUpdatesResult = "";
            using (var updater = new UpdateManager(UpdatePath, "TestApp", FrameworkVersion.Net45))
            {
               await updater.DownloadReleases(UpdateInfo.ReleasesToApply);
               DownloadedUpdateInfo = UpdateInfo;
               DownloadUpdatesResult = String.Format("{0} updates downloaded", DownloadedUpdateInfo.ReleasesToApply.Count());
            }
        }

        bool CanDownloadReleases()
        {
            return UpdateInfo != null && UpdateInfo.ReleasesToApply.Any();
        }

        public RelayCommand ApplyReleasesCommand { get; set; }

        async void ApplyReleases()
        {
            ApplyUpdatesResult = "";
            using (var updater = new UpdateManager(UpdatePath, "TestApp", FrameworkVersion.Net45))
            {
                await updater.ApplyReleases(DownloadedUpdateInfo);
                ApplyUpdatesResult = String.Format("{0} updates applied!", DownloadedUpdateInfo.ReleasesToApply.Count());
            }
        }

        bool CanApplyReleases()
        {
            return DownloadedUpdateInfo != null;
        }

        public string ApplyUpdatesResult { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}