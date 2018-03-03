using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace RemoteInstallation
{
    public class RemoteComputerInstallator : IRemoteComputerInstallator
    {
        public class ActiveInstallation
        {
            public ActiveInstallation(string installation, string computer, Action<InstallationFinishedStatus> finishedCallback)
            {
                Installation = installation;
                Computer = computer;
                FinishedCallback = finishedCallback;
            }

            public string Installation { get; }
            public string Computer { get; }
            public Action<InstallationFinishedStatus> FinishedCallback { get; }
        }

        public List<ActiveInstallation> ActiveInstallations { get;  } = new List<ActiveInstallation>();

        public void InstallOnComputer(string installation, string computer, Action<InstallationFinishedStatus> finishedCallback)
        {
            ActiveInstallations.Add(new ActiveInstallation(installation, computer, finishedCallback));
        }

        public void FinishInstallation(string installation, string computer, InstallationFinishedStatus finishedStatus)
        {
            var installationToFinish = ActiveInstallations.Single(x => x.Installation == installation && x.Computer == computer);
            ActiveInstallations.Remove(installationToFinish);
            installationToFinish.FinishedCallback(finishedStatus);
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

            var computerInstallation = task.InstallationTasks.Single();
            Assert.AreEqual("WorkX", computerInstallation.Installation);
            Assert.AreEqual("ComputerX", computerInstallation.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, computerInstallation.Status);
        }

        [Test]
        public void Creates_multiple_installation_tasks()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task1 = ri.CreateTask("WorkX", "ComputerX");
            var task2 = ri.CreateTask("WorkY", "ComputerY");

            var computerInstallation = task2.InstallationTasks.Single();
            Assert.AreEqual("WorkY", computerInstallation.Installation);
            Assert.AreEqual("ComputerY", computerInstallation.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, computerInstallation.Status);
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

            var computerInstallation = task.InstallationTasks.Single();
            Assert.AreEqual(InstalationTaskStatus.Installing, computerInstallation.Status);
        }

        [Test]
        public void Installation_tasks_starts_installation_on_computers()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task1 = ri.CreateTask("WorkX", "ComputerX");
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

            var computerInstallation = task.InstallationTasks.Single();
            Assert.AreEqual(InstalationTaskStatus.Installing, computerInstallation.Status);
        }

        [Test]
        public void Finished_installation_should_be_reflected_in_task()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual(0, installator.ActiveInstallations.Count);

            ri.Iterate();

            installator.FinishInstallation("WorkX", "ComputerX", InstallationFinishedStatus.Success);

            ri.Iterate();

            var computerInstallation = task.InstallationTasks.Single();
            Assert.AreEqual(InstalationTaskStatus.Success, computerInstallation.Status);
        }

        [Test]
        public void Should_not_query_for_finished_installations_when_done()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual(0, installator.ActiveInstallations.Count);

            ri.Iterate();

            installator.FinishInstallation("WorkX", "ComputerX", InstallationFinishedStatus.Success);

            ri.Iterate();
            ri.Iterate();
        }

        [Test]
        public void Installation_can_fail()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual(0, installator.ActiveInstallations.Count);

            ri.Iterate();

            installator.FinishInstallation("WorkX", "ComputerX", InstallationFinishedStatus.Failed);

            ri.Iterate();

            var computerInstallation = task.InstallationTasks.Single();
            Assert.AreEqual(InstalationTaskStatus.Failed, computerInstallation.Status);
        }

        [Test]
        public void Single_task_can_install_on_multiple_computers()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var tasks = ri.CreateTask("WorkX", new []{ "ComputerX", "ComputerY"});

            ri.Iterate();

            Assert.AreEqual(2, installator.ActiveInstallations.Count);

            var activeInstallation1 = installator.ActiveInstallations[0];
            Assert.AreEqual("WorkX", activeInstallation1.Installation);
            Assert.AreEqual("ComputerX", activeInstallation1.Computer);

            var activeInstallation2 = installator.ActiveInstallations[1];
            Assert.AreEqual("WorkX", activeInstallation2.Installation);
            Assert.AreEqual("ComputerY", activeInstallation2.Computer);
        }

        [Test]
        public void Multiple_computers_task_status()
        {
            RemoteComputerInstallator installator = new RemoteComputerInstallator();
            RemoteInstaller ri = new RemoteInstaller(installator);
            var tasks = ri.CreateTask("WorkX", new[] { "ComputerX", "ComputerY" });

            Assert.AreEqual(2, tasks.InstallationTasks.Count);

            var installation1 = tasks.InstallationTasks[0];
            Assert.AreEqual("WorkX", installation1.Installation);
            Assert.AreEqual("ComputerX", installation1.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, installation1.Status);

            var installation2 = tasks.InstallationTasks[1];
            Assert.AreEqual("WorkX", installation2.Installation);
            Assert.AreEqual("ComputerY", installation2.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, installation2.Status);
        }
    }
}
