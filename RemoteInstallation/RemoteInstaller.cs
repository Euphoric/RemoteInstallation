using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace RemoteInstallation
{
    public class RemoteInstaller
    {
        private readonly SynchronizationContext _context;
        private readonly IRemoteComputerInstallator _installator;
        private readonly ObservableCollection<InstallationTask> _installationTasks = new ObservableCollection<InstallationTask>();

        public int ConcurrentInstallationLimit
        {
            get => _concurrentInstallationLimit;
            set { _concurrentInstallationLimit = value; UpdateStatus(); }
        }

        private int _currentInstallationsCount;
        private int _concurrentInstallationLimit = 4;

        public RemoteInstaller(SynchronizationContext context, IRemoteComputerInstallator installator)
        {
            _context = context;
            _installator = installator;
        }

        public IEnumerable<InstallationTask> InstallationTasks => _installationTasks;

        public InstallationTask CreateTask(string installation, IEnumerable<string> computers)
        {
            var installationTasks = computers.Select(pc => new ComputerInstallationTask(installation, pc)).ToList();
            var installationTask = new InstallationTask(installation, installationTasks);

            _installationTasks.Add(installationTask);

            UpdateStatus();

            return installationTask;
        }

        public InstallationTask CreateTask(string installation, string computer)
        {
            return CreateTask(installation, new[] { computer });
        }

        private void UpdateStatus()
        {
            foreach (var installationTask in _installationTasks)
            {
                var standbyInstallations = installationTask.ComputerInstallations.Where(x => x.Status == InstalationTaskStatus.Standby);
                foreach (var computerInstallation in standbyInstallations)
                {
                    if (_currentInstallationsCount >= ConcurrentInstallationLimit)
                    {
                        break;
                    }

                    StartInstallation(installationTask, computerInstallation);

                    computerInstallation.Status = InstalationTaskStatus.Installing;
                }

                UpdateTaskStatus(installationTask);
            }
        }

        private void StartInstallation(InstallationTask installationTask, ComputerInstallationTask computerInstallation)
        {
            Action<InstallationFinishedStatus> finishedCallback = status => _context.Post(obj => FinishedTask(installationTask, computerInstallation, status), null);
            _installator.InstallOnComputer(computerInstallation.Installation, computerInstallation.Computer, finishedCallback);

            _currentInstallationsCount++;
        }

        private void FinishedTask(
            InstallationTask installationTask,
            ComputerInstallationTask computerInstallationTask,
            InstallationFinishedStatus finishedStatus)
        {
            computerInstallationTask.Status = finishedStatus == InstallationFinishedStatus.Failed ? InstalationTaskStatus.Failed : InstalationTaskStatus.Success;

            _currentInstallationsCount--;

            UpdateTaskStatus(installationTask);

            UpdateStatus();
        }

        private static void UpdateTaskStatus(InstallationTask installationTask)
        {
            var isStandby = installationTask.ComputerInstallations.All(x => x.Status == InstalationTaskStatus.Standby);

            var isInstalling = installationTask.ComputerInstallations.Any(x => x.Status == InstalationTaskStatus.Installing || x.Status == InstalationTaskStatus.Standby);

            if (isStandby)
            {
                installationTask.Status = InstalationTaskStatus.Standby;
            }
            else if (isInstalling)
            {
                installationTask.Status = InstalationTaskStatus.Installing;
            }
            else
            {
                if (installationTask.ComputerInstallations.All(x => x.Status == InstalationTaskStatus.Success))
                {
                    installationTask.Status = InstalationTaskStatus.Success;
                }
                else if (installationTask.ComputerInstallations.All(x => x.Status == InstalationTaskStatus.Failed))
                {
                    installationTask.Status = InstalationTaskStatus.Failed;
                }
                else
                {
                    installationTask.Status = InstalationTaskStatus.PartialSuccess;
                }
            }
        }
    }
}
