using System;
using System.ComponentModel;
using System.Diagnostics;
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

        string version;
        public string Version
        {
            get { return version; }
            set
            {
                if (version == value) return;

                version = value;
                OnPropertyChanged();
            }
        }

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

        public RelayCommand CheckForUpdateCommand { get; set; }

        async void CheckForUpdate()
        {
            using (var updater = new UpdateManager(UpdatePath, "TestApp", FrameworkVersion.Net45))
            {
                UpdateInfo = await updater.CheckForUpdate();
            }
        }

        bool CanCheckForUpdate()
        {
            return !String.IsNullOrWhiteSpace(UpdatePath);
        }

        public RelayCommand DownloadReleasesCommand { get; set; }

        async void DownloadReleases()
        {
            using (var updater = new UpdateManager(UpdatePath, "TestApp", FrameworkVersion.Net45))
            {
               await updater.DownloadReleases(UpdateInfo.ReleasesToApply);
               DownloadedUpdateInfo = UpdateInfo;
            }
        }

        bool CanDownloadReleases()
        {
            return UpdateInfo != null;
        }

        public RelayCommand ApplyReleasesCommand { get; set; }

        async void ApplyReleases()
        {
            using (var updater = new UpdateManager(UpdatePath, "TestApp", FrameworkVersion.Net45))
            {
                await updater.ApplyReleases(DownloadedUpdateInfo);
            }
        }

        bool CanApplyReleases()
        {
            return DownloadedUpdateInfo != null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}