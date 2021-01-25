using SupportTool.Services;
using SupportTool.ViewModel.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SupportTool.Views
{
    public partial class SplashScreen : Window
    {
        SplashScreenViewModel splashScreenViewModel;

        public SplashScreen()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            ConfigDataBinding();

            Task.Factory.StartNew(() => InitializeTrelloData())
                .ContinueWith(task =>
                {
                    this.Hide();
                    TrelloControl mainWindown = new TrelloControl();
                    mainWindown.Show();
                }, System.Threading.CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void InitializeTrelloData()
        {
            var trelloService = new TrelloService();

            splashScreenViewModel.LoadingMessage = "Clonning trello data...";
            trelloService.CloneData();
        }

        public void ConfigDataBinding()
        {
            splashScreenViewModel = new SplashScreenViewModel()
            {
                LoadingMessage = "Start loading data..."
            };

            lblLoadingMessage.DataContext = splashScreenViewModel;
        }

        #region Trigger

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        #endregion
    }
}
