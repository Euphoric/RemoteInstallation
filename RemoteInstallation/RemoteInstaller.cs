using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteInstallation
{
    public class RemoteInstaller
    {
        private readonly IRemoteComputerInstallator _installator;
        private readonly List<InstallationTask> _installationTasks = new List<InstallationTask>();

        public RemoteInstaller(IRemoteComputerInstallator installator)
        {
            _installator = installator;
        }

        public InstallationTask CreateTask(string installation, IEnumerable<string> computers)
        {
            var installationTasks = computers.Select(pc => new ComputerInstallationTask(installation, pc)).ToList();
            var installationTask = new InstallationTask(installationTasks);
            _installationTasks.Add(installationTask);

            return installationTask;
        }

        public InstallationTask CreateTask(string installation, string computer)
        {
            return CreateTask(installation, new[] {computer});
        }

        public void Iterate()
        {
            foreach (var installationTask in _installationTasks.Where(x=>x.Status == InstalationTaskStatus.Standby))
            {
                foreach (var computerInstallation in installationTask.ComputerInstallations.Where(x=>x.Status == InstalationTaskStatus.Standby))
                {
                    _installator.InstallOnComputer(computerInstallation.Installation, computerInstallation.Computer, status => FinishedTask(installationTask, computerInstallation, status));
                    computerInstallation.Status = InstalationTaskStatus.Installing;
                }

                installationTask.Status = InstalationTaskStatus.Installing;
            }
        }

        private void FinishedTask(
            InstallationTask installationTask,
            ComputerInstallationTask computerInstallationTask,
            InstallationFinishedStatus finishedStatus)
        {
            computerInstallationTask.Status = finishedStatus == InstallationFinishedStatus.Failed ? InstalationTaskStatus.Failed : InstalationTaskStatus.Success;

            UpdateTaskStatus(installationTask);
        }

        private static void UpdateTaskStatus(InstallationTask installationTask)
        {
            var isInstalling = installationTask.ComputerInstallations.Any(x => x.Status == InstalationTaskStatus.Installing);

            if (isInstalling)
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
