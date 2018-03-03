using System.ComponentModel;
using System.Runtime.CompilerServices;
using RemoteInstallation.Annotations;

namespace RemoteInstallation
{
    public class ComputerInstallationTask : INotifyPropertyChanged
    {
        private InstalationTaskStatus _status;
        public string Installation { get; }
        public string Computer { get; }

        public InstalationTaskStatus Status
        {
            get => _status;
            set { _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public ComputerInstallationTask(string installation, string computer)
        {
            Installation = installation;
            Computer = computer;
            Status = InstalationTaskStatus.Standby;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}