using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace RemoteInstallation.Application
{
    public class FakeRemoteInstallator : IRemoteComputerInstallator
    {
        public void InstallOnComputer(string installation, string computer, Action<InstallationFinishedStatus> finishedCallback)
        {
            throw new NotImplementedException();
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

            _remoteInstaller = new RemoteInstaller(new FakeRemoteInstallator());
        }

        private void AddTask()
        {
            _remoteInstaller.CreateTask("TestA", "TestB");
        }
    }
}
