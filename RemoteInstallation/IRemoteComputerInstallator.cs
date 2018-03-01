namespace RemoteInstallation
{
    public interface IRemoteComputerInstallator
    {
        void InstallOnComputer(string installation, string computer);
    }
}