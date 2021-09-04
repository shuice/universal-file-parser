using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Windows.Storage;

namespace Common
{
    public class AppSetting
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private static ApplicationDataContainer default_value = ApplicationData.Current.LocalSettings.CreateContainer("_app_setting_default_", ApplicationDataCreateDisposition.Always);

        static AppSetting()
        {
            AppSettingDefault.SetDefaults();            
        }

        public static void SetDefault<T>(AppSettingKey key, string key_uni_name, T v)
        {
            string strKey = JoinKey(key, key_uni_name);
            default_value.Values[strKey] = v;
        }

        private static void SetDefault<T>(AppSettingKey key, T v)
        {
            SetDefault(key, null, v);
        }

        public static void SetDefaultUInt64(AppSettingKey key, UInt64 v)
        {
            SetDefault<UInt64>(key, v);
        }

        public static void SetDefaultInt64(AppSettingKey key, Int64 v)
        {
            SetDefault<Int64>(key, v);
        }

        public static void SetDefaultDouble(AppSettingKey key, double v)
        {
            SetDefault<double>(key, v);
        }

        public static void SetDefaultString(AppSettingKey key, string v)
        {
            SetDefault<string>(key, v);
        }

        public static void SetDefaultBoolean(AppSettingKey key, Boolean v)
        {
            SetDefault<Boolean>(key, v);
        }

        public static void SetDefaultEnum<T>(AppSettingKey key, T v)
        {
            SetDefault<string>(key, v.ToString());
        }

        private static T GetNumber<T>(AppSettingKey key, T failback_value) where T : struct
        {
            T t = GetNumber<T>(key, null, failback_value);
            return t;
        }

        public static T GetNumber<T>(AppSettingKey key, string key_uni_name, T failback_value) where T : struct
        {
            T t = failback_value;
            string strKey = JoinKey(key, key_uni_name);
            object v;
            bool failed = true;
            if (localSettings.Values.TryGetValue(strKey, out v))
            {
                if (v.GetType() == typeof(T))
                {
                    failed = false;
                    t = (T)v;
                }
            }
            if (failed && default_value.Values.TryGetValue(strKey.ToString(), out v))
            {
                if (v.GetType() == typeof(T))
                {
                    t = (T)v;
                }
            }
            return t;
        }

        private static T GetObject<T>(AppSettingKey key, T failback_value) where T : class
        {
            T t = GetObject<T>(key, null, failback_value);
            return t;
        }

        public static T GetObject<T>(AppSettingKey key, string key_uni_name, T failback_value) where T : class
        {
            T t = failback_value;
            string strKey = JoinKey(key, key_uni_name);
            object v;
            bool failed = true;
            if (localSettings.Values.TryGetValue(strKey, out v))
            {
                if (v != null && v.GetType() == typeof(T))
                {
                    failed = false;
                    t = v as T;
                }
            }
            if (failed && default_value.Values.TryGetValue(strKey.ToString(), out v))
            {
                if (v != null && v.GetType() == typeof(T))
                {
                    t = v as T;
                }
            }
            return t;
        }

        public static UInt64 GetUInt64(AppSettingKey key, UInt64 failback_value = default(UInt64))
        {
            return GetNumber<UInt64>(key, failback_value);
        }

        public static Int64 GetInt64(AppSettingKey key, Int64 failback_value = default(Int64))
        {
            return GetNumber<Int64>(key, failback_value);
        }

        public static Double GetDouble(AppSettingKey key, Double failback_value = default(Double))
        {
            return GetNumber<Double>(key, failback_value);
        }

        public static String GetString(AppSettingKey key, String failback_value = "")
        {
            return GetObject<String>(key, failback_value);
        }

        public static Boolean GetBoolean(AppSettingKey key, Boolean failback_value = default(Boolean))
        {
            return GetNumber<Boolean>(key, failback_value);
        }

        public static T GetEnum<T>(AppSettingKey key, T failback_value) where T : struct
        {
            string s = GetString(key);
            T t = s.ToEnum<T>(failback_value);
            return t;
        }

        public static void Set<T>(AppSettingKey key, string key_uni_name, T v)
        {
            string strKey = JoinKey(key, key_uni_name);
            localSettings.Values[strKey] = v;
        }

        private static void Set<T>(AppSettingKey key, T v)
        {
            Set<T>(key, null, v);
        }

        public static void SetUInt64(AppSettingKey key, UInt64 v)
        {
            Set<UInt64>(key, v);
        }

        public static void SetInt64(AppSettingKey key, Int64 v)
        {
            Set<Int64>(key, v);
        }

        public static void SetDouble(AppSettingKey key, Double v)
        {
            Set<Double>(key, v);
        }

        public static void SetString(AppSettingKey key, String v)
        {
            Set<String>(key, v);
        }

        public static void SetBoolean(AppSettingKey key, Boolean v)
        {
            Set<Boolean>(key, v);
        }

        public static void SetEnum<T>(AppSettingKey key, T t) where T : struct
        {
            Set<string>(key, t.ToString());
        }

        private static string JoinKey(AppSettingKey key, string key_uni_name)
        {
            string strKey = key.ToString();
            if (String.IsNullOrEmpty(key_uni_name) == false)
            {
                strKey = strKey + "_@_" + key_uni_name;
            }
            return strKey;
        }
    }


    public static class Extension
    {
        // string -> enum
        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            T result;
            return Enum.TryParse<T>(value, true, out result) ? result : defaultValue;
        }
    }
}
