﻿using System;
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
        private InstallationTask _selectedInstallationTask;

        public ICommand AddTaskCommand { get; }

        public IEnumerable<InstallationTask> InstallationTasks => _remoteInstaller.InstallationTasks;

        public InstallationTask SelectedInstallationTask
        {
            get => _selectedInstallationTask;
            set => Set(nameof(SelectedInstallationTask), ref _selectedInstallationTask, value);
        }

        public ICommand AddBigTaskCommand { get; }

        public ICommand StopTaskCommand { get; }

        public MainViewModel()
        {
            AddTaskCommand = new RelayCommand(AddTask);
            AddBigTaskCommand = new RelayCommand(AddBigTask);
            StopTaskCommand = new RelayCommand(StopTask);

            _remoteInstaller = new RemoteInstaller(SynchronizationContext.Current, new FakeRemoteInstallator());
        }

        private readonly Random _random = new Random();

        private int _counter = 0;

        private void AddTask()
        {
            SelectedInstallationTask = _remoteInstaller.CreateTask("Installation "  +(_counter++), "Computer " + _random.Next(10));
        }

        private void AddBigTask()
        {
            var computers = Enumerable.Range(0, 100).Select(x => "Computer " + x);
            SelectedInstallationTask = _remoteInstaller.CreateTask("Installation " + (_counter++), computers);
        }

        private void StopTask()
        {
            _remoteInstaller.StopTask(SelectedInstallationTask);
        }
    }
}
