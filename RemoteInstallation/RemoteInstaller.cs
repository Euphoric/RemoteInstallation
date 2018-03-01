using System;
using System.Collections.Generic;

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
            foreach (var installationTask in _installationTasks)
            {
                _installator.InstallOnComputer(installationTask.Installation, installationTask.Computer);
                installationTask.Status = InstalationTaskStatus.Installing;
            }
        }
    }
}
