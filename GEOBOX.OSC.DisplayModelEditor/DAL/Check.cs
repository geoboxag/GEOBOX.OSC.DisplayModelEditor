using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GEOBOX.OSC.DisplayModelEditor.Enums;
using GEOBOX.OSC.DisplayModelEditor.Properties;

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

        /// <summary>
        /// Remove a TaskKey and decrements the <see cref="CountFaults"/>
        /// </summary>
        /// <param name="taskKey"></param>
        /// <param name="decrementCount">true for decrementing <see cref="Count"/></param>
        internal void RemoveTaskKey(string taskKey, bool decrementCount)
        {
            var foundTasksKeys = TaskKeys.FindAll(k => k.Contains(taskKey));

            foreach (var foundTasksKey in foundTasksKeys)
            {
                if (decrementCount)
                {
                    Count = Count - 1;
                }

                TaskKeys.Remove(foundTasksKey);
                CountFaults = CountFaults - 1;
            }

            if (!TaskKeys.Any())
            {
                IsOk = true;
            }

        }

        internal bool IsDecrementCountNecessary(string taskKey)
        {
            if (taskKey.Contains(TaskType.RemoveUnusedGroup.ToString()))
            {
                return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
