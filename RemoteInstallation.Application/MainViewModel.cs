using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace RemoteInstallation.Application
{
    public class FakeRemoteInstallator : IRemoteComputerInstallator
    {
        readonly Random _random = new Random();

        public async void InstallOnComputer(string installation, string computer, Action<InstallationFinishedStatus> finishedCallback)
        {
            await Task.Delay((int)(_random.NextDouble()*1000+100));
            finishedCallback((InstallationFinishedStatus)_random.Next(2));
        }
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly RemoteInstaller _remoteInstaller;

        public ICommand AddTaskCommand { get; }

        public IEnumerable<InstallationTask> InstallationTasks => _remoteInstaller.InstallationTasks;

        public MainViewModel()
        {
            AddTaskCommand = new RelayCommand(AddTask);

            _remoteInstaller = new RemoteInstaller(SynchronizationContext.Current, new FakeRemoteInstallator());
        }

        private void AddTask()
        {
            _remoteInstaller.CreateTask("TestA", "TestB");
            _remoteInstaller.UpdateStatus();
        }
    }
}
