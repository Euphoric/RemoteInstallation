namespace RemoteInstallation
{
    public class ComputerInstallationTask
    {
        public string Installation { get; }
        public string Computer { get; }
        public InstalationTaskStatus Status { get; set; }

        public ComputerInstallationTask(string installation, string computer)
        {
            Installation = installation;
            Computer = computer;
            Status = InstalationTaskStatus.Standby;
        }
    }
}