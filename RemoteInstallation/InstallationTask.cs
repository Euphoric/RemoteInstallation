using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RemoteInstallation
{
    public class InstallationTask : INotifyPropertyChanged
    {
        private InstalationTaskStatus _status;
        public List<ComputerInstallationTask> ComputerInstallations { get; }

        public InstalationTaskStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public string Installation { get; }

        public InstallationTask(string installation, IEnumerable<ComputerInstallationTask> installationTasks)
        {
            Installation = installation;
            ComputerInstallations = installationTasks.ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}