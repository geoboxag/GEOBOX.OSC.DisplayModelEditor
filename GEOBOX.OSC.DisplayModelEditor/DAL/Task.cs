using GEOBOX.OSC.DisplayModelEditor.Enums;
using System.ComponentModel;

namespace GEOBOX.OSC.DisplayModelEditor.DAL
{
    public class Task : INotifyPropertyChanged
    {
        public string Text { get; set; }
        public string FileName { get; set; }
        public TaskType Tag { get; set; }
        private bool isActive;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
        public bool isEnabled = true;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public int imageIndex;
        public int ImageIndex
        {
            get
            {
                return imageIndex;
            }
            set
            {
                imageIndex = value;
                OnPropertyChanged(nameof(ImageIndex));
            }
        }

        private bool isFixed;
        public bool IsFixed
        {
            get
            {
                return isFixed;
            }
            set
            {
                isFixed = value;
                OnPropertyChanged(nameof(IsFixed));
            }
        }

        public Task(string filename, string text, TaskType type, TaskImage status)
        {
            this.Text = text;
            this.Tag = type;
            this.ImageIndex = GetImageIndex(status);
            this.FileName = filename;
        }

        public string TaskKey => $"{this.Tag}_{this.FileName}";

        private int GetImageIndex(TaskImage ti)
        {
            switch (ti)
            {
                case TaskImage.Done:
                    return 1;

                case TaskImage.Warning:
                    return 2;

                case TaskImage.ToDo:
                default: return 0;
            }
        }

        internal void SetImage(TaskImage image)
        {
            this.ImageIndex = GetImageIndex(image);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
