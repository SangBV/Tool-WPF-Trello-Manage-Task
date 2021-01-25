using System.ComponentModel;

namespace SupportTool.ViewModel.App
{
    public class SplashScreenViewModel : INotifyPropertyChanged
    {
        private string _loaddingMessage { get; set; }

        public string LoadingMessage 
        {
            get { return _loaddingMessage;  }
            set
            {
                _loaddingMessage = value;
                NotifyPropertyChanged("LoadingMessage");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
