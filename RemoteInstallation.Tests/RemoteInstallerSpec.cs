using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RemoteInstallation
{
    public class RemoteComputerInstallator : IRemoteComputerInstallator
    {
        public class ActiveInstallation
        {
            public ActiveInstallation(string installation, string computer)
            {
                Installation = installation;
                Computer = computer;
            }

            public string Installation { get; }
            public string Computer { get; }
            public bool Finish { get; set; }
        }

        public List<ActiveInstallation> ActiveInstallations { get;  } = new List<ActiveInstallation>();

        public void InstallOnComputer(string installation, string computer)
        {
            ActiveInstallations.Add(new ActiveInstallation(installation, computer));
        }

        public bool GetFinishStatus(string installation, string computer)
        {
            var queriedInstallation = ActiveInstallations.Single(x => x.Installation == installation && x.Computer == computer);
            if (queriedInstallation.Finish)
                ActiveInstallations.Remove(queriedInstallation);
            return queriedInstallation.Finish;
        }

        public void FinishInstallation(string installation, string computer)
        {
            var installationToFinish = ActiveInstallations.Single(x => x.Installation == installation && x.Computer == computer);
            installationToFinish.Finish = true;
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
        public void Creates_multiple_installation_tasks()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task1 = ri.CreateTask("WorkX", "ComputerX");
            var task2 = ri.CreateTask("WorkY", "ComputerY");

            Assert.AreEqual("WorkY", task2.Installation);
            Assert.AreEqual("ComputerY", task2.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, task2.Status);
        }

        [Test]
        public void Installation_task_starts_installation_on_computer()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual(0, installator.ActiveInstallations.Count);

            ri.Iterate();

            var activeInstallation = installator.ActiveInstallations.Single();

            Assert.AreEqual("WorkX", activeInstallation.Installation);
            Assert.AreEqual("ComputerX", activeInstallation.Computer);

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);
        }

        [Test]
        public void Installation_tasks_starts_installation_on_computers()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");
            var task2 = ri.CreateTask("WorkY", "ComputerY");

            Assert.AreEqual(0, installator.ActiveInstallations.Count);

            ri.Iterate();

            Assert.AreEqual(2, installator.ActiveInstallations.Count);

            var activeInstallation1 = installator.ActiveInstallations[0];
            Assert.AreEqual("WorkX", activeInstallation1.Installation);
            Assert.AreEqual("ComputerX", activeInstallation1.Computer);

            var activeInstallation2 = installator.ActiveInstallations[1];
            Assert.AreEqual("WorkY", activeInstallation2.Installation);
            Assert.AreEqual("ComputerY", activeInstallation2.Computer);

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);
        }

        [Test]
        public void Iterating_again_should_not_result_in_multiple_installations()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual(0, installator.ActiveInstallations.Count);

            ri.Iterate();
            ri.Iterate();

            Assert.AreEqual(1, installator.ActiveInstallations.Count);

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);
        }

        [Test]
        public void Finished_installation_should_be_reflected_in_task()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual(0, installator.ActiveInstallations.Count);

            ri.Iterate();

            installator.FinishInstallation("WorkX", "ComputerX");

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.Success, task.Status);
        }

        [Test]
        public void Should_not_query_for_finished_installations_when_done()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual(0, installator.ActiveInstallations.Count);

            ri.Iterate();

            installator.FinishInstallation("WorkX", "ComputerX");

            ri.Iterate();
            ri.Iterate();
        }
    }
}
