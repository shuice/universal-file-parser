using Common;
using NetTools;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace NetTools
{
    public enum FreeLimitChoice
    {
        Cancel,
        Continue_Full,
        Continue_Limit,
        To_Purchase,
    }

    public class FreeLimit
    {
        public static async Task<FreeLimitChoice> LimitToPurchase(Func<bool> reach_limit, string message, bool can_continue)
        {
#if DEBUG
            return FreeLimitChoice.Continue_Full;
#endif
            if (AppSetting.GetBoolean(AppSettingKey.app_addon_has_online) == false || AppSetting.GetBoolean(AppSettingKey.app_purchased))
            {
                return FreeLimitChoice.Continue_Full;
            }

            if (reach_limit() == false)
            {
                return FreeLimitChoice.Continue_Full;
            }

            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand("Cancel", cmd => { }, commandId: 0));
            if (can_continue)
            {
                dialog.Commands.Add(new UICommand("Continue", cmd => { }, commandId: 1));
            }
            dialog.Commands.Add(new UICommand("Purchase    " + AppSetting.GetString(AppSettingKey.app_price), cmd => { }, commandId: 2));

            dialog.DefaultCommandIndex = 2;
            IUICommand choose = await dialog.ShowAsync();
            int chooseId = (int)choose.Id;
            if (chooseId == 0)
            {
                return FreeLimitChoice.Cancel;
            }
            else if (chooseId == 1)
            {
                return FreeLimitChoice.Continue_Limit;
            }
            return FreeLimitChoice.To_Purchase;
        }

        public static async Task<bool> LimitToPurchase()
        {
#if DEBUG
            return await Task.FromResult(false);            
#else
            if (AppSetting.GetBoolean(AppSettingKey.app_addon_has_online) == false || AppSetting.GetBoolean(AppSettingKey.app_purchased))
            {
                return false;
            }

            int seconds = Limit_reached_seconds();
            if (seconds == 0)
            {
                return false;
            }
            ContentDialogFreeLimit dialog = new ContentDialogFreeLimit(seconds);
            await dialog.ShowAsync();
            return dialog.purchase_clicked;
#endif
        }

        public static int Limit_reached_seconds()
        {            
            string count_suffix = AppData.appData.appID.ToString();
            
            Int64 used_count = AppSetting.GetNumber(AppSettingKey.common_used_count, count_suffix, 0L);            
            used_count++;
            AppSetting.Set(AppSettingKey.common_used_count, count_suffix, used_count);
            if (used_count <= 5)
            {
                return 0;
            }
            if (used_count < 10)
            {
                return 5;
            }
            return 30;
        }

        public static async Task<bool> common_check_license(string message, bool can_continue, Func<bool> reach_limit, Action limit_func)
        {
            if (can_continue)
            {
                Debug.Assert(limit_func != null);
            }

            FreeLimitChoice choice = await FreeLimit.LimitToPurchase(reach_limit, message, can_continue);
            if (choice == FreeLimitChoice.Cancel)
            {
                return false;
            }
            else if (choice == FreeLimitChoice.To_Purchase)
            {
                await StoreHelper.PurchaseFullLicense(StoreHelper.PurchaseLicenseType.app);
                return false;
            }
            else if (choice == FreeLimitChoice.Continue_Limit)
            {
                limit_func();
            }
            return true;
        }
    }
}
