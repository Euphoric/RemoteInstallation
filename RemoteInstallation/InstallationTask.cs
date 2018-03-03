using System.Collections.Generic;
using System.Linq;

namespace RemoteInstallation
{
    public class InstallationTask
    {
        public List<ComputerInstallationTask> ComputerInstallations { get; }
        public InstalationTaskStatus Status { get; set; }

        public InstallationTask(IEnumerable<ComputerInstallationTask> installationTasks)
        {
            ComputerInstallations = installationTasks.ToList();
        }
    }
}