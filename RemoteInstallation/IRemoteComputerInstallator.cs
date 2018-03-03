using System;

namespace RemoteInstallation
{
    public interface IRemoteComputerInstallator
    {
        void InstallOnComputer(string installation, string computer, Action<InstallationFinishedStatus> finishedCallback);
    }
}