using Common;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace file_structure
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {        
            this.InitializeComponent();
            update_license_text();
            about_text_app_version.Text = Get_App_Version();
        }

        private void update_license_text()
        {
            // about_text_app_license.Text = AppSetting.GetBoolean(AppSettingKey.app_purchased) ? "Full license" : "Inactive license";
            // about_button_purchase.Visibility = AppSetting.GetBoolean(AppSettingKey.app_purchased) ? Visibility.Collapsed : Visibility.Visible;
            // about_text_block_price.Text = AppSetting.GetString(AppSettingKey.app_price);

            // if (AppSetting.GetBoolean(AppSettingKey.app_addon_has_online) == false)
            // {
                about_text_app_license.Visibility = Visibility.Collapsed;
                about_button_purchase.Visibility = Visibility.Collapsed;
            // }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void about_btn_send_mail_Click(object sender, RoutedEventArgs e)
        {
            var emailMessage = new EmailMessage();
            String subject = Package.Current.DisplayName + "-" + Get_App_Version();
            emailMessage.Subject = subject;
            emailMessage.To.Add(new EmailRecipient("support@bami-tech.net"));
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private String Get_App_Version()
        {
            string appVersion = string.Format("{0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);
            return appVersion;
        }

        private async void about_button_purchase_Click(object sender, RoutedEventArgs e)
        {
            await StoreHelper.PurchaseFullLicense(StoreHelper.PurchaseLicenseType.app);
            update_license_text();
        }

        private async void about_link_rate_us_Click(object sender, RoutedEventArgs e)
        {
            var pfn = Package.Current.Id.FamilyName;
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?PFN=" + pfn));
        }


        public AppData appData
        {
            get
            {
                return AppData.appData;
            }
        }
    }
}
