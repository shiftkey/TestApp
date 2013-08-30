using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows;
using ReactiveUI;
using ReactiveUI.Xaml;
using Shimmer.Client;

namespace TestApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();

            InitializeComponent();
        }
    }

    public class MainWindowViewModel : ReactiveObject
    {
        string _Version;
        public string Version
        {
            get { return _Version; }
            set { this.RaiseAndSetIfChanged(x => x.Version, value); }
        }

        string _UpdatePath;
        public string UpdatePath
        {
            get { return _UpdatePath; }
            set { this.RaiseAndSetIfChanged(x => x.UpdatePath, value); }
        }

        UpdateInfo _UpdateInfo;
        public UpdateInfo UpdateInfo
        {
            get { return _UpdateInfo; }
            set { this.RaiseAndSetIfChanged(x => x.UpdateInfo, value); }
        }

        UpdateInfo _DownloadedUpdateInfo;
        public UpdateInfo DownloadedUpdateInfo
        {
            get { return _DownloadedUpdateInfo; }
            set { this.RaiseAndSetIfChanged(x => x.DownloadedUpdateInfo, value); }
        }

        public ReactiveAsyncCommand CheckForUpdate { get; protected set; }
        public ReactiveAsyncCommand DownloadReleases { get; protected set; }
        public ReactiveAsyncCommand ApplyReleases { get; protected set; }

        public MainWindowViewModel()
        {
            var noneInFlight = new BehaviorSubject<bool>(false);
            var updateManager = default(UpdateManager);

            this.WhenAny(x => x.UpdatePath, x => x.Value)
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .Throttle(TimeSpan.FromMilliseconds(700), RxApp.DeferredScheduler)
                .Subscribe(x =>
                {
                    if (updateManager != null) updateManager.Dispose();
                    updateManager = new UpdateManager(UpdatePath, "TestApp", FrameworkVersion.Net40);
                });

            CheckForUpdate = new ReactiveAsyncCommand(noneInFlight);
            CheckForUpdate.RegisterAsyncObservable(_ => updateManager.CheckForUpdate())
                .Subscribe(x =>
                {
                    UpdateInfo = x;
                    DownloadedUpdateInfo = null;
                });

            DownloadReleases = new ReactiveAsyncCommand(noneInFlight.Where(_ => UpdateInfo != null));
            DownloadReleases.RegisterAsyncObservable(_ => updateManager.DownloadReleases(UpdateInfo.ReleasesToApply))
                .Subscribe(_ =>
                {
                    DownloadedUpdateInfo = UpdateInfo;
                });

            ApplyReleases = new ReactiveAsyncCommand(noneInFlight.Where(_ => DownloadedUpdateInfo != null));
            ApplyReleases.RegisterAsyncObservable(_ => updateManager.ApplyReleases(DownloadedUpdateInfo));

            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fvi.FileVersion;

            Observable.CombineLatest(
                CheckForUpdate.ItemsInflight.StartWith(0),
                DownloadReleases.ItemsInflight.StartWith(0),
                ApplyReleases.ItemsInflight.StartWith(0),
                this.WhenAny(x => x.UpdatePath, _ => 0),
                (a, b, c, _) => a + b + c
            ).Select(x => x == 0 && UpdatePath != null).Multicast(noneInFlight).Connect();
        }
    }


}
