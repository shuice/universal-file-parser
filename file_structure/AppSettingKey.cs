using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum AppSettingKey
    {
        //
        app_purchased,        

        app_price,     
        app_addon_has_online,
        app_request_theme,
        addon_price_full_life,
        addon_price_subscription_monthly,
        addon_price_subscription_yearly,

        addon_purchased_full_life,
        addon_purchased_subscription_monthly,
        addon_purchased_subscription_yearly,

        addon_end_time_subscription_monthly,
        addon_end_time_subscription_yearly,

        common_used_count,

        grammar_list_custom,
        grammar_list_buildin,
    }

    public enum OutputFormat
    {
        JPEG,
        PNG
    }

    public enum OverwriteType
    {
        overwrite,
        create_new_one,
    }

}
