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

        public ComputerInstallationTask CreateTask(string installation, string computer)
        {
            return CreateTask(installation, new[] {computer}).InstallationTasks.Single();
        }

        public void Iterate()
        {
            foreach (var installationTask in _installationTasks.SelectMany(x=>x.InstallationTasks).Where(x=>x.Status == InstalationTaskStatus.Standby))
            {
                _installator.InstallOnComputer(installationTask.Installation, installationTask.Computer, status=>FinishedTask(installationTask, status));
                installationTask.Status = InstalationTaskStatus.Installing;
            }
        }

        private void FinishedTask(ComputerInstallationTask computerInstallationTask, InstallationFinishedStatus finishedStatus)
        {
            computerInstallationTask.Status = finishedStatus == InstallationFinishedStatus.Failed ? InstalationTaskStatus.Failed : InstalationTaskStatus.Success;
        }
    }
}
