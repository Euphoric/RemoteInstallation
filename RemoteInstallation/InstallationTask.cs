using System.Collections.Generic;
using System.Linq;

namespace RemoteInstallation
{
    public class InstallationTask
    {
        public List<ComputerInstallationTask> InstallationTasks { get; }

        public InstallationTask(IEnumerable<ComputerInstallationTask> installationTasks)
        {
            InstallationTasks = installationTasks.ToList();
        }
    }
}