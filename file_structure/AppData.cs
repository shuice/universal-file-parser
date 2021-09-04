using Common;
using file_structure;
using kernel;
using Newtonsoft.Json;
using SimpleMVVM.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Services.Store;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Common
{

    public enum AppID
    {
        mp4,
        all,
    }

    public enum ConvertStatus
    {
        Pending,
        Converting,
        Completed,
        Failed,
        Canceled,
    }

    public class NameValuePair : AutoNotifyPropertyChanged
    {
        private string _name;
        private string _value;

        public String name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public String value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }

    public class ConvertItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private StorageFile _input_file;
        public StorageFile input_file
        {
            get => _input_file;
            set
            {
                _input_file = value;
                if (_input_file != null)
                {
                    this.file_name = Path.GetFileName(_input_file.Path);
                    this.path = Path.GetDirectoryName(_input_file.Path);
                }
                else
                {
                    this.file_name = "";
                    this.path = "";
                }
            }
        }
        private ConvertStatus _status = ConvertStatus.Pending;
        public ConvertStatus status
        {
            get => _status;

            set
            {
                _status = value;
                this.str_status = ConvertStatusToString(_status);
            }
        }
                

        private string ConvertStatusToString(ConvertStatus s)
        {
            return s.ToString();
        }

      

        private string _file_name = "";
        public string file_name
        {
            get => _file_name;
            set
            {
                _file_name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("file_name"));
            }
        }

        private string _path = "";
        public string path
        {
            get => _path;
            set
            {
                _path = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("path"));
            }
        }

        private string _str_status = "";
        public string str_status
        {
            get => _str_status;
            set
            {
                _str_status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("str_status"));
            }
        }

        private string _detail = "";
        public string detail
        {
            get => _detail;
            set
            {
                _detail = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("detail"));
            }
        }

    }

    

    public class AppData : AutoNotifyPropertyChanged
    {
        public static AppData appData = new AppData();

        public string last_goto_pos = "";
        public List<ResultNode> fullResultNodes = new List<ResultNode>();
        public BulkObservableCollection<ResultNode> resultNodes = new BulkObservableCollection<ResultNode>();
        public Dictionary<Result, ResultNode> dicResult2Node = new Dictionary<Result, ResultNode>();
        public ByteIndexRange2Result byteIndexRange2Result = null;
        public AppID appID;

        public ObservableCollection<GrammarRecord> customGrammarList = new ObservableCollection<GrammarRecord>();
        public IReadOnlyList<GrammarRecord> buildinGrammarList = new List<GrammarRecord>();

        public GrammarRecord currentParsingGrammarRecord = null;
        public byte[] toBeParsedFile = new byte[0];

        public static string output_folder_access_token = "output_folder_access_token";
        public ObservableCollection<ConvertItem> convert_items = new ObservableCollection<ConvertItem>();

        public Grammar editingGrammar { get; set; }
        public ObservableCollection<ElementBaseNode> editingGrammarElements = new ObservableCollection<ElementBaseNode>();
                
        public ObservableCollection<NameValuePair> editingElementFixedValues = new ObservableCollection<NameValuePair>();
        public ObservableCollection<NameValuePair> editingElementMasks = new ObservableCollection<NameValuePair>();


        private bool _isWorking = false;
        public bool isWorking
        {
            get => _isWorking;
            set
            {
                _isWorking = value;
                OnPropertyChanged();
            }
        }


        //OutputFormat _output_format = AppSetting.GetEnum<OutputFormat>(AppSettingKey.output_format, OutputFormat.PNG);
        private Byte[] _fileBytes = new byte[0];
        public Byte[] fileBytes
        {
            get => _fileBytes;
            set
            {
                _fileBytes = value;
                OnPropertyChanged();
            }
        }

        public List<GrammarRecord> allSortedGrammarRecords()
        {
            List<GrammarRecord> record = new List<GrammarRecord>();
            record.AddRange(appData.customGrammarList);
            record.AddRange(appData.buildinGrammarList);
            record.Sort((a, b) => a.fileName.CompareTo(b.fileName));
            return record;
        }

        public object grid_tree_view_selected_item;

        private bool _loaded = false;
        public async Task Load()
        {
            if (_loaded)
            {
                return;
            }
            _loaded = true;

            if (appID == AppID.mp4)
            {
                GrammarRecord mp4Grammar = new GrammarRecord();
                mp4Grammar.fileName = "qt.grammar";
                mp4Grammar.isBuildin = true;
                buildinGrammarList = new List<GrammarRecord>() { mp4Grammar };
                return;
            }
            else
            {

                List<GrammarRecord> lstCustom = JsonConvert.DeserializeObject<List<GrammarRecord>>(AppSetting.GetString(AppSettingKey.grammar_list_custom));
                if (lstCustom != null)
                {
                    List<GrammarRecord> lstCustomWithoutEmpty = lstCustom.Where(item => String.IsNullOrEmpty(item.fileName) == false).ToList();
                    lstCustomWithoutEmpty.Sort((a, b) => a.fileName.CompareTo(b.fileName));
                    lstCustomWithoutEmpty.ForEach(r => customGrammarList.Add(r));
                }

                Uri uri = new Uri($"{build_in_folder()}buildin_grammar.json");
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                string build_grammar_content = await FileIO.ReadTextAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                List<GrammarRecord> lstBuildin = JsonConvert.DeserializeObject<List<GrammarRecord>>(build_grammar_content);
                if (lstBuildin != null)
                {
                    buildinGrammarList = lstBuildin.Where(item => String.IsNullOrEmpty(item.fileName) == false).ToList();
                }
            }
        }

        private string build_in_folder()
        {
            return "ms-appx:///Assets/buildin_grammar/";
        }

        public void SaveCustomGrammarList()
        {
            string s = JsonConvert.SerializeObject(customGrammarList, Formatting.None);
            AppSetting.SetString(AppSettingKey.grammar_list_custom, s);
        }

        public async Task<StorageFolder> CustomGrammarFolder()
        {
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            StorageFolder grammar_folder = await folder.CreateFolderAsync("custom_grammar", CreationCollisionOption.OpenIfExists);
            return grammar_folder;
        }

        public async Task<string> LoadBuildinGrammar(string fileName)
        {
            string fileContent = "";
            do
            {
                Uri uri = new Uri($"{build_in_folder()}{fileName}");
                //folder
                try
                {
                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                    if (file == null)
                    {
                        break;
                    }
                    fileContent = await FileIO.ReadTextAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                }
                catch(Exception)
                {
                    break;
                }
            } while (false);

            return fileContent ?? "";
        }

        public async Task<string> LoadCustomGrammar(string fileName)
        {
            string fileContent = "";
            do
            {
                StorageFolder folder = await CustomGrammarFolder();
                if (folder == null)
                {
                    break;
                }
                //folder
                StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                if (file == null)
                {
                    break;
                }
                fileContent = await FileIO.ReadTextAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            } while (false);

            return fileContent ?? "";
        }

        public async Task SaveCustomGrammar(string fileName, string grammarContent)
        {
            try
            {
                StorageFolder folder = await CustomGrammarFolder();
                StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, grammarContent, Windows.Storage.Streams.UnicodeEncoding.Utf8);                
            }
            catch(Exception)
            {
                return;
            }
        }

        public async Task CopyCustomGrammar(string sourceFileName, string targetFileName)
        {
            try
            {
                StorageFolder folder = await CustomGrammarFolder();
                StorageFile file = await folder.CreateFileAsync(sourceFileName, CreationCollisionOption.OpenIfExists);
                await file.CopyAsync(folder, targetFileName, NameCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                return;
            }
        }

        public async Task RemoveCustomGrammar(string fileName)
        {
            try
            {
                StorageFolder folder = await CustomGrammarFolder();                
                StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                await file.DeleteAsync();
            }
            catch (Exception)
            {
                return;
            }
        }




        public async Task ShowMessage(string message)
        {
            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand("OK", cmd => { }, commandId: 0));

            dialog.DefaultCommandIndex = 0;
            await dialog.ShowAsync();
        }


    }


    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    public sealed class InvertBooleanConverter : BooleanConverter<bool>
    {
        public InvertBooleanConverter() :
            base(false, true)
        { }
    }

    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }

    public sealed class InvertBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public InvertBooleanToVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible)
        { }
    }

    public sealed class InvertBooleanToDoubleConverter : BooleanConverter<double>
    {
        public InvertBooleanToDoubleConverter() :
            base(0.0, 1.0)
        { }
    }

    public sealed class BooleanToDoubleConverter : BooleanConverter<double>
    {
        public BooleanToDoubleConverter() :
            base(1.0, 0.0)
        { }
    }
}
