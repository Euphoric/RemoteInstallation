using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RemoteInstallation
{
    public class RemoteComputerInstallator : IRemoteComputerInstallator
    {
        public string Installation { get; private set; }
        public string Computer { get; private set; }

        public void InstallOnComputer(string installation, string computer)
        {
            Installation = installation;
            Computer = computer;
        }
    }

    public class RemoteInstallerSpec
    {
        [Test]
        public void Creates_installation_task()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual("WorkX", task.Installation);
            Assert.AreEqual("ComputerX", task.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, task.Status);
        }

        [Test]
        public void Installation_task_starts_installation_on_computer()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual(null, installator.Installation);
            Assert.AreEqual(null, installator.Computer);

            ri.Iterate();

            Assert.AreEqual("WorkX", installator.Installation);
            Assert.AreEqual("ComputerX", installator.Computer);

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);
        }
    }
}
