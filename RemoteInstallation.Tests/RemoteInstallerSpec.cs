using System;
using NUnit.Framework;

namespace RemoteInstallation.Tests
{
    public class RemoteInstallerSpec
    {
        [Test]
        public void Test()
        {
            RemoteInstaller ri = new RemoteInstaller();
            Assert.IsNotNull(ri);
        }
    }
}
