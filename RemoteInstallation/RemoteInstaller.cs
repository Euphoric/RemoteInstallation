using System;

namespace RemoteInstallation
{
    public class RemoteInstaller
    {
        private readonly IRemoteComputerInstallator _installator;
        private InstallationTask _installationTask;

        public RemoteInstaller(IRemoteComputerInstallator installator)
        {
            _installator = installator;
        }

        public InstallationTask CreateTask(string installation, string computer)
        {
            _installationTask = new InstallationTask(installation, computer);

            return _installationTask;
        }

        public void Iterate()
        {
            _installator.InstallOnComputer(_installationTask.Installation, _installationTask.Computer);
            _installationTask.Status = InstalationTaskStatus.Installing;
        }
    }
}
