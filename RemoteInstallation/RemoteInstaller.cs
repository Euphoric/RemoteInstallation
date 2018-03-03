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

        public IEnumerable<InstallationTask> CreateTask(string installation, IEnumerable<string> computers)
        {
            var installationTask = computers.Select(pc => new InstallationTask(installation, pc)).ToList();

            _installationTasks.AddRange(installationTask);

            return installationTask;
        }

        public InstallationTask CreateTask(string installation, string computer)
        {
            return CreateTask(installation, new[] {computer}).Single();
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
