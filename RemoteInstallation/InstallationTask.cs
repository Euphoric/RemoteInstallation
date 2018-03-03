using System.Collections.Generic;
using System.Linq;

namespace RemoteInstallation
{
    public class InstallationTask
    {
        public List<ComputerInstallationTask> ComputerInstallations { get; }
        public InstalationTaskStatus Status { get; set; }
        public string Installation { get; }

        public InstallationTask(string installation, IEnumerable<ComputerInstallationTask> installationTasks)
        {
            Installation = installation;
            ComputerInstallations = installationTasks.ToList();
        }
    }
}