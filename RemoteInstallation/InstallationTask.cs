namespace RemoteInstallation
{
    public class InstallationTask
    {
        public string Installation { get; }
        public string Computer { get; }
        public InstalationTaskStatus Status { get; set; }

        public InstallationTask(string installation, string computer)
        {
            Installation = installation;
            Computer = computer;
            Status = InstalationTaskStatus.Standby;
        }
    }
}