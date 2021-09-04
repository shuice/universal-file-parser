using Common;
using NetTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 

namespace NetTools
{
    public sealed partial class ContentDialogFreeLimit : ContentDialog
    {
        public bool purchase_clicked { get; private set; } = false;
        public int seconds_left = 5;

        public Windows.UI.Xaml.DispatcherTimer auto_dismiss_timer;    // Refresh regularly how many files and directories have been collected
        public ContentDialogFreeLimit(int seconds_left)
        {
            this.InitializeComponent();
            this.seconds_left = seconds_left;
            text_block_seconds.Text = seconds_left.ToString();
            StartUpdateCollectedCountTimer();

            SecondaryButtonText = "Purchase    " + AppSetting.GetString(AppSettingKey.app_price);
        }

        public AppData appData
        {
            get
            {
                return AppData.appData;
            }
        }

        

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            purchase_clicked = true;
            StopUpdateCollectedCountTimer();
            this.Hide();
        }

        public void StartUpdateCollectedCountTimer()
        {
            StopUpdateCollectedCountTimer();
            auto_dismiss_timer = new DispatcherTimer();
            auto_dismiss_timer.Tick += OnTimerUpdateCollectedCount;
            auto_dismiss_timer.Interval = TimeSpan.FromSeconds(1);
            auto_dismiss_timer.Start();
        }

        public void StopUpdateCollectedCountTimer()
        {
            if (auto_dismiss_timer != null)
            {
                auto_dismiss_timer.Stop();
                auto_dismiss_timer = null;
            }
        }


        private void OnTimerUpdateCollectedCount(object sender, object e)
        {
            seconds_left -= 1;
            if (seconds_left <= 0)
            {
                StopUpdateCollectedCountTimer();
                this.Hide();
            }
            else
            {
                text_block_seconds.Text = seconds_left.ToString();
            }
        }
    }
}
