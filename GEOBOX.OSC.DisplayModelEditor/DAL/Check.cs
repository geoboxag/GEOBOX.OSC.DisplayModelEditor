using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GEOBOX.OSC.DisplayModelEditor.DAL
{
    public class Check : INotifyPropertyChanged
    {
        private bool isOk = false;
        public bool IsOk 
        { 
            get 
            {
                return isOk;
            }
            set 
            { 
                isOk = value;
                OnPropertyChanged(nameof(IsOk));
            }
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private int count;
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                OnPropertyChanged(nameof(Count));
            }
        }

        private int countFaults;
        public int CountFaults
        {
            get
            {
                return countFaults;
            }
            set
            {
                countFaults = value;
                OnPropertyChanged(nameof(CountFaults));
            }
        }

        public List<string> TaskKeys { get; private set; } = new List<string>();

        internal void AddTaskKey(string taskKey)
        {
            TaskKeys.Add(taskKey);
        }

        internal void RemoveTaskKey(string taskKey)
        {
            if(TaskKeys.Remove(taskKey))
            {
                CountFaults = CountFaults - 1;
            }

            if (!TaskKeys.Any())
            {
                IsOk = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
