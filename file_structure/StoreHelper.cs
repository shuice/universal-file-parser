using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Services.Store;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace Shared
{
    public class StoreHelper
    {
        public enum AppPayType
        {
            undefined,
            free_trial,
            free_plus,
        }

        private static StoreContext _storeContext;

        private static string addon_store_id_full_life = "";
        
        private static string addon_store_id_subscription_monthly = "";
        private static string addon_store_id_subscription_yearly = "";
        private static AppPayType appPayType = AppPayType.undefined;

        public static void ConfigStoreID(AppPayType appPayType,
                                string addon_store_id_full_life = "",
                                string addon_store_id_subscription_monthly = "",
                                string addon_store_id_subscription_yearly = "")
        {
            StoreHelper.appPayType = appPayType;
            StoreHelper.addon_store_id_full_life = addon_store_id_full_life;
            StoreHelper.addon_store_id_subscription_monthly = addon_store_id_subscription_monthly;
            StoreHelper.addon_store_id_subscription_yearly = addon_store_id_subscription_yearly;
        }


        public static bool is_any_addon_online()
        {
            bool all_addon_online = AppSetting.GetBoolean(AppSettingKey.app_addon_has_online);
            return all_addon_online;
        }

        public static bool is_user_purchased()
        {
            if (appPayType == AppPayType.free_trial)
            {
                return AppSetting.GetBoolean(AppSettingKey.app_purchased);
            }
            else
            {
                bool one_of_addon_purchased =
                    AppSetting.GetBoolean(AppSettingKey.addon_purchased_full_life)
                    || AppSetting.GetBoolean(AppSettingKey.addon_purchased_subscription_monthly)
                    || AppSetting.GetBoolean(AppSettingKey.addon_purchased_subscription_yearly);
                return one_of_addon_purchased;
            }
        }


        public static bool is_usage_limited()
        {
            if (appPayType == AppPayType.free_plus)
            {
                if (is_any_addon_online() == false)
                {
                    return false;
                }
            }
            if (is_user_purchased())
            {
                return false;
            }
            return true;
        }



        private static StoreContext storeContext
        {
            get
            {
                if (_storeContext == null)
                {
                    _storeContext = StoreContext.GetDefault();
                    _storeContext.OfflineLicensesChanged += OfflineLicensesChanged;
                }
                return _storeContext;
            }
        }


        private static void OfflineLicensesChanged(StoreContext sender, object args)
        {
            var task = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await GetLicenseState();
            });
        }

        private static string Price(StoreProduct sp)
        {
            return sp.Price.FormattedPrice + " (" + sp.Price.CurrencyCode + ")";
        }

        private static void AddonOnlineParser(IEnumerable<StoreProduct> products, string addon_store_id, AppSettingKey price_key)
        {
            if (String.IsNullOrEmpty(addon_store_id) == false)
            {
                StoreProduct storeProduct = products.FirstOrDefault(product => product.StoreId.StartsWith(addon_store_id));
                if (storeProduct != null)
                {
                    AppSetting.SetBoolean(AppSettingKey.app_addon_has_online, true);
                    AppSetting.SetString(price_key, Price(storeProduct));
                }
            }
        }

        public static async Task GetLicenseState()
        {
            Debug.Assert(appPayType != AppPayType.undefined);
            string[] filterList = new string[] { "Durable" };
            StoreProductQueryResult addOns = await storeContext.GetAssociatedStoreProductsAsync(filterList);
            IEnumerable<StoreProduct> products = addOns.Products.Values;

            AppSetting.SetBoolean(AppSettingKey.app_addon_has_online, false);
            AddonOnlineParser(products, addon_store_id_full_life, AppSettingKey.addon_price_full_life);
            AddonOnlineParser(products, addon_store_id_subscription_monthly, AppSettingKey.addon_price_subscription_monthly);
            AddonOnlineParser(products, addon_store_id_subscription_yearly, AppSettingKey.addon_price_subscription_yearly);
            await GetSubscriptionStatus();
        }

        public enum PurchaseLicenseType
        {
            app,
            addon_life_time,
            addon_subscription_monthly,
            addon_subscription_yearly,
        }

        private static string GetStoreIdByPurchaseLicenseType(PurchaseLicenseType licenseType)
        {
            if (licenseType == PurchaseLicenseType.addon_life_time)
            {
                return addon_store_id_full_life;
            }
            else if (licenseType == PurchaseLicenseType.addon_subscription_monthly)
            {
                return addon_store_id_subscription_monthly;
            }
            else if (licenseType == PurchaseLicenseType.addon_subscription_yearly)
            {
                return addon_store_id_subscription_yearly;
            }
            return "";
        }

        public static async Task PurchaseFullLicense(PurchaseLicenseType licenseType)
        {
            StorePurchaseResult result;
            if (appPayType == AppPayType.free_trial)
            {
                StoreProductResult productResult = await storeContext.GetStoreProductForCurrentAppAsync();
                if (productResult.ExtendedError != null)
                {
                    await ShowMessage(productResult.ExtendedError.ToString());
                    return;
                }
                result = await productResult.Product.RequestPurchaseAsync();
            }
            else
            {
                result = await storeContext.RequestPurchaseAsync(GetStoreIdByPurchaseLicenseType(licenseType));
            }

            if (result.ExtendedError != null)
            {
                await ShowMessage(result.ExtendedError.ToString());
                return;
            }

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                    await ShowMessage($"You already bought this AddOn.");
                    break;

                case StorePurchaseStatus.Succeeded:
                    await ShowMessage($"Thanks for your purchase.");
                    break;

                case StorePurchaseStatus.NotPurchased:
                    // await ShowMessage("Product was not purchased, it may have been canceled.");
                    break;

                case StorePurchaseStatus.NetworkError:
                    await ShowMessage("Product was not purchased due to a network error.");
                    break;

                case StorePurchaseStatus.ServerError:
                    await ShowMessage("Product was not purchased due to a server error.");
                    break;

                default:
                    await ShowMessage("Product was not purchased due to an unknown error.");
                    break;
            }
            await GetLicenseState();
        }

        public static async Task ShowMessage(string message)
        {
            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand("OK", cmd => { }, commandId: 0));

            dialog.DefaultCommandIndex = 0;
            await dialog.ShowAsync();
        }

        public static async Task RateApp(bool jump_to_store_app, Action in_app_rate_success)
        {
            if (jump_to_store_app)
            {
                var pfn = Package.Current.Id.FamilyName;
                await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?PFN=" + pfn));
            }
            else
            {
                await ShowInAppRateDialog(in_app_rate_success);
            }
        }

        private static async Task ShowInAppRateDialog(Action in_app_rate_success)
        {
            StoreRateAndReviewResult result = await storeContext.RequestRateAndReviewAppAsync();

            // Check status
            if (result.Status == StoreRateAndReviewStatus.Succeeded)
            {
                in_app_rate_success?.Invoke();
            }
        }


        // subscription or durable
        private static async Task GetSubscriptionStatus()
        {
            StoreAppLicense appLicense = await storeContext.GetAppLicenseAsync();

            AppSetting.SetBoolean(AppSettingKey.app_purchased, (appLicense.IsActive && (appLicense.IsTrial == false)));

            IReadOnlyDictionary<string, StoreLicense> addon_licenses = appLicense.AddOnLicenses;
            IEnumerable<StoreLicense> activeLicenseList = addon_licenses.Values.Where(license => license.IsActive);
            StoreLicense full_license = activeLicenseList.FirstOrDefault(license => license.SkuStoreId.StartsWith(StoreHelper.addon_store_id_full_life));
            StoreLicense monthly_license = activeLicenseList.FirstOrDefault(license => license.SkuStoreId.StartsWith(StoreHelper.addon_store_id_subscription_monthly));
            StoreLicense yearly_license = activeLicenseList.FirstOrDefault(license => license.SkuStoreId.StartsWith(StoreHelper.addon_store_id_subscription_yearly));

            if (full_license != null)
            {
                AppSetting.SetBoolean(AppSettingKey.app_addon_has_online, true);
                AppSetting.SetBoolean(AppSettingKey.addon_purchased_full_life, full_license.IsActive);
            }
            if (monthly_license != null)
            {
                AppSetting.SetBoolean(AppSettingKey.app_addon_has_online, true);
                AppSetting.SetBoolean(AppSettingKey.addon_purchased_subscription_monthly, full_license.IsActive);
                AppSetting.SetInt64(AppSettingKey.addon_end_time_subscription_monthly, full_license.ExpirationDate.ToUnixTimeSeconds());
            }
            if (yearly_license != null)
            {
                AppSetting.SetBoolean(AppSettingKey.app_addon_has_online, true);
                AppSetting.SetBoolean(AppSettingKey.addon_purchased_subscription_yearly, full_license.IsActive);
                AppSetting.SetInt64(AppSettingKey.addon_end_time_subscription_yearly, full_license.ExpirationDate.ToUnixTimeSeconds());
            }
        }

        internal static async Task<StoreRateAndReviewResult> RequestRateAndReviewAppAsync()
        {
            return await storeContext.RequestRateAndReviewAppAsync();
        }
    }


}

