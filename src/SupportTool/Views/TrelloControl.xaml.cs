using SupportTool.Services;
using SupportTool.Utilities.Helper;
using SupportTool.ViewModel.Trello;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using MESSAGE_CONSTANT = SupportTool.Constant.Constant.MessageConstant;
using TRELLO_CONSTANT = SupportTool.Constant.Constant.TrelloConstant;

namespace SupportTool.Views
{
    public partial class TrelloControl : Window
    {
        private DispatcherTimer dispatcherTimer;
        public TrelloService trelloService;
        private CommonHelper _commonHelper;
        static public string ROOT_FOLDER_PATH;

        public TrelloControl()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
           
            trelloService = new TrelloService();
            _commonHelper = new CommonHelper();

            ROOT_FOLDER_PATH = _commonHelper.GetRootPath();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
        }

        #region Button Action

        private void btnAddTask_Click(object sender, RoutedEventArgs e)
        {
            AddTask(TRELLO_CONSTANT.SideType.Default);
        }

        private async void btnRefreshTrello_Click(object sender, RoutedEventArgs e)
        {
            EnableControl(false);

            ChangeMessage(MESSAGE_CONSTANT.MessageType.Notification, true, "Clonning data from trello board...");
            await Task.Delay(1000);
            trelloService.CloneData();
            ChangeMessage(MESSAGE_CONSTANT.MessageType.Notification, true, ">>>Clonned data successfully<<<");
            dispatcherTimer.Start();

            EnableControl(true);
        }

        private void btnRefreshForm_Click(object sender, RoutedEventArgs e)
        {
            RefreshForm(isClearAll : true);
        }

        #endregion

        #region Common function

        private async void AddTask(string taskSide)
        {
            EnableControl(false);

            string taskName = TaskName.Text;

            DateTime? duedateAt = dpkDuedateAt.SelectedDate;

            if (string.IsNullOrEmpty(taskName) || taskName.Trim() == "")
            {
                ChangeMessage(MESSAGE_CONSTANT.MessageType.Error, true, "Missing card info");
            }
            else
            {
                var card = new CardAddNewViewModel()
                {
                    Name = taskName,
                    Type = taskSide,
                    DuedateAt = duedateAt
                };

                ChangeMessage(MESSAGE_CONSTANT.MessageType.Notification, true, "Adding card to trello board...");

                //Call API
                await Task.Delay(1000);
                trelloService.AddTask(card);
                RefreshForm();

                ChangeMessage(MESSAGE_CONSTANT.MessageType.Notification, true, ">>>Added card successfully<<<");
            }
            EnableControl(true);
            //Start the timer
            dispatcherTimer.Start();
        }

        private void ChangeMessage(string type, bool isDisplay, string message)
        {
            switch (type)
            {
                case MESSAGE_CONSTANT.MessageType.Notification:
                    AlterMessage.Content = message;
                    AlterMessage.Visibility = !isDisplay || string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case MESSAGE_CONSTANT.MessageType.Error:
                    ErrorMessage.Content = message;
                    ErrorMessage.Visibility = !isDisplay || string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            AlterMessage.Visibility = Visibility.Collapsed;
            ErrorMessage.Visibility = Visibility.Collapsed;

            //Disable the timer
            dispatcherTimer.IsEnabled = false;
        }

        public void RefreshForm(bool isClearAll = false)
        {
            TaskName.Text = "";

            if (isClearAll)
            {
                dpkDuedateAt.SelectedDate = null;
            }
        }

        private void EnableControl(bool isDisplay)
        {
            btnAddTask.IsEnabled = isDisplay;
            btnRefreshTrello.IsEnabled = isDisplay;
            btnRefreshForm.IsEnabled = isDisplay;
        }

        #endregion

        #region Trigger

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        #endregion
    }
}
