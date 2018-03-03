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

        public InstallationTask CreateTask(string installation, string computer)
        {
            var installationTask = new InstallationTask(installation, computer);

            _installationTasks.Add(installationTask);

            return installationTask;
        }

        public void Iterate()
        {
            foreach (var installationTask in _installationTasks.Where(x=>x.Status == InstalationTaskStatus.Standby))
            {
                _installator.InstallOnComputer(installationTask.Installation, installationTask.Computer, status=>FinishedTask(installationTask, status));
                installationTask.Status = InstalationTaskStatus.Installing;
            }
        }

        private void FinishedTask(InstallationTask installationTask, InstallationFinishedStatus finishedStatus)
        {
            installationTask.Status = finishedStatus == InstallationFinishedStatus.Failed ? InstalationTaskStatus.Failed : InstalationTaskStatus.Success;
        }
    }
}
