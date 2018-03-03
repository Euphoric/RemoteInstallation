using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    public class FakeSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state)
        {
            base.Send(d, state);
        }
    }

    public class RemoteInstallerSpec
    {
        private RemoteComputerInstallator _installator;
        private RemoteInstaller ri;

        [SetUp]
        public void Setup()
        {
            var synchContext = new FakeSynchronizationContext();
            _installator = new RemoteComputerInstallator();
            ri = new RemoteInstaller(synchContext, _installator);
        }

        [Test]
        public void Creates_installation_task()
        {
            ri.EnableInstallation = false;

            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual("WorkX", task.Installation);

            Assert.AreEqual(InstalationTaskStatus.Standby, task.Status);

            var computerInstallation = task.ComputerInstallations.Single();
            Assert.AreEqual("WorkX", computerInstallation.Installation);
            Assert.AreEqual("ComputerX", computerInstallation.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, computerInstallation.Status);
        }

        [Test]
        public void Creates_multiple_installation_tasks()
        {
            ri.EnableInstallation = false;

            var task1 = ri.CreateTask("WorkX", "ComputerX");
            var task2 = ri.CreateTask("WorkY", "ComputerY");

            Assert.AreEqual(InstalationTaskStatus.Standby, task2.Status);

            var computerInstallation = task2.ComputerInstallations.Single();
            Assert.AreEqual("WorkY", computerInstallation.Installation);
            Assert.AreEqual("ComputerY", computerInstallation.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, computerInstallation.Status);
        }

        [Test]
        public void Installation_task_starts_installation_on_computer()
        {
            ri.EnableInstallation = false;

            var task = ri.CreateTask("WorkX", "ComputerX");

            Assert.AreEqual(0, _installator.ActiveInstallations.Count);

            ri.EnableInstallation = true;

            ri.Iterate();

            var activeInstallation = _installator.ActiveInstallations.Single();

            Assert.AreEqual("WorkX", activeInstallation.Installation);
            Assert.AreEqual("ComputerX", activeInstallation.Computer);

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);

            var computerInstallation = task.ComputerInstallations.Single();
            Assert.AreEqual(InstalationTaskStatus.Installing, computerInstallation.Status);
        }

        [Test]
        public void Installation_tasks_starts_installation_on_computers()
        {
            ri.EnableInstallation = false;

            var task1 = ri.CreateTask("WorkX", "ComputerX");
            var task2 = ri.CreateTask("WorkY", "ComputerY");

            Assert.AreEqual(0, _installator.ActiveInstallations.Count);

            ri.EnableInstallation = true;

            ri.Iterate();

            Assert.AreEqual(2, _installator.ActiveInstallations.Count);

            var activeInstallation1 = _installator.ActiveInstallations[0];
            Assert.AreEqual("WorkX", activeInstallation1.Installation);
            Assert.AreEqual("ComputerX", activeInstallation1.Computer);

            var activeInstallation2 = _installator.ActiveInstallations[1];
            Assert.AreEqual("WorkY", activeInstallation2.Installation);
            Assert.AreEqual("ComputerY", activeInstallation2.Computer);
        }

        [Test]
        public void Iterating_again_should_not_result_in_multiple_installations()
        {
            var task = ri.CreateTask("WorkX", "ComputerX");

            ri.Iterate();
            ri.Iterate();

            Assert.AreEqual(1, _installator.ActiveInstallations.Count);

            var computerInstallation = task.ComputerInstallations.Single();
            Assert.AreEqual(InstalationTaskStatus.Installing, computerInstallation.Status);
        }

        [Test]
        public void Finished_installation_should_be_reflected_in_task()
        {
            var task = ri.CreateTask("WorkX", "ComputerX");

            ri.Iterate();

            _installator.FinishInstallation("WorkX", "ComputerX", InstallationFinishedStatus.Success);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.Success, task.Status);

            var computerInstallation = task.ComputerInstallations.Single();
            Assert.AreEqual(InstalationTaskStatus.Success, computerInstallation.Status);
        }

        [Test]
        public void Installation_can_fail()
        {
            var task = ri.CreateTask("WorkX", "ComputerX");

            ri.Iterate();

            _installator.FinishInstallation("WorkX", "ComputerX", InstallationFinishedStatus.Failed);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.Failed, task.Status);

            var computerInstallation = task.ComputerInstallations.Single();
            Assert.AreEqual(InstalationTaskStatus.Failed, computerInstallation.Status);
        }

        [Test]
        public void Single_task_can_install_on_multiple_computers()
        {
            var tasks = ri.CreateTask("WorkX", new []{ "ComputerX", "ComputerY"});

            ri.Iterate();

            Assert.AreEqual(2, _installator.ActiveInstallations.Count);

            var activeInstallation1 = _installator.ActiveInstallations[0];
            Assert.AreEqual("WorkX", activeInstallation1.Installation);
            Assert.AreEqual("ComputerX", activeInstallation1.Computer);

            var activeInstallation2 = _installator.ActiveInstallations[1];
            Assert.AreEqual("WorkX", activeInstallation2.Installation);
            Assert.AreEqual("ComputerY", activeInstallation2.Computer);
        }

        [Test]
        public void Multiple_computers_instalations_status()
        {
            ri.EnableInstallation = false;

            var tasks = ri.CreateTask("WorkX", new[] { "ComputerX", "ComputerY" });

            Assert.AreEqual(2, tasks.ComputerInstallations.Count);

            var installation1 = tasks.ComputerInstallations[0];
            Assert.AreEqual("WorkX", installation1.Installation);
            Assert.AreEqual("ComputerX", installation1.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, installation1.Status);

            var installation2 = tasks.ComputerInstallations[1];
            Assert.AreEqual("WorkX", installation2.Installation);
            Assert.AreEqual("ComputerY", installation2.Computer);
            Assert.AreEqual(InstalationTaskStatus.Standby, installation2.Status);
        }

        [Test]
        public void Multiple_computers_task_status_complete_sucess()
        {
            var task = ri.CreateTask("WorkX", new[] { "ComputerX", "ComputerY" });

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);

            ri.Iterate();

            _installator.FinishInstallation("WorkX", "ComputerX", InstallationFinishedStatus.Success);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);

            _installator.FinishInstallation("WorkX", "ComputerY", InstallationFinishedStatus.Success);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.Success, task.Status);
        }

        [Test]
        public void Multiple_computers_task_status_complete_failure()
        {
            var task = ri.CreateTask("WorkX", new[] { "ComputerX", "ComputerY" });

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);

            ri.Iterate();

            _installator.FinishInstallation("WorkX", "ComputerX", InstallationFinishedStatus.Failed);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);

            _installator.FinishInstallation("WorkX", "ComputerY", InstallationFinishedStatus.Failed);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.Failed, task.Status);
        }

        [Test]
        public void Multiple_computers_task_status_partial_success_1()
        {
            var task = ri.CreateTask("WorkX", new[] { "ComputerX", "ComputerY" });

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);

            ri.Iterate();

            _installator.FinishInstallation("WorkX", "ComputerX", InstallationFinishedStatus.Success);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);

            _installator.FinishInstallation("WorkX", "ComputerY", InstallationFinishedStatus.Failed);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.PartialSuccess, task.Status);
        }

        [Test]
        public void Multiple_computers_task_status_partial_success_2()
        {
            var task = ri.CreateTask("WorkX", new[] { "ComputerX", "ComputerY" });

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);

            ri.Iterate();

            _installator.FinishInstallation("WorkX", "ComputerX", InstallationFinishedStatus.Failed);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.Installing, task.Status);

            _installator.FinishInstallation("WorkX", "ComputerY", InstallationFinishedStatus.Success);

            ri.Iterate();

            Assert.AreEqual(InstalationTaskStatus.PartialSuccess, task.Status);
        }
    }
}
