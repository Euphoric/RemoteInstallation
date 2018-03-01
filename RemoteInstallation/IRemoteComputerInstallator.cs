namespace RemoteInstallation
{
    public interface IRemoteComputerInstallator
    {
        void InstallOnComputer(string installation, string computer);
        bool GetFinishStatus(string installation, string computer);
    }
}