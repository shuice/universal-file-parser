using Common;
using kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace file_structure
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GrammarList : Page
    {
        public GrammarList()
        {
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.InitializeComponent();            
        }

        public AppData appData => AppData.appData;

        public bool btn_enabled_edit
        {
            get { return (bool)GetValue(btn_enabled_editProperty); }
            set { SetValue(btn_enabled_editProperty, value); }
        }

        // Using a DependencyProperty as the backing store for btn_enabled_edit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty btn_enabled_editProperty =
            DependencyProperty.Register("btn_enabled_edit", typeof(bool), typeof(GrammarList), new PropertyMetadata(false));



        public bool btn_enabled_delete
        {
            get { return (bool)GetValue(btn_enabled_deleteProperty); }
            set { SetValue(btn_enabled_deleteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for btn_enabled_delete.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty btn_enabled_deleteProperty =
            DependencyProperty.Register("btn_enabled_delete", typeof(bool), typeof(GrammarList), new PropertyMetadata(false));




        public bool btn_enabled_duplicate
        {
            get { return (bool)GetValue(btn_enabled_duplicateProperty); }
            set { SetValue(btn_enabled_duplicateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for btn_enabled_duplicate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty btn_enabled_duplicateProperty =
            DependencyProperty.Register("btn_enabled_duplicate", typeof(bool), typeof(GrammarList), new PropertyMetadata(false));



        public bool btn_enabled_export
        {
            get { return (bool)GetValue(btn_enabled_exportProperty); }
            set { SetValue(btn_enabled_exportProperty, value); }
        }

        // Using a DependencyProperty as the backing store for btn_enabled_export.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty btn_enabled_exportProperty =
            DependencyProperty.Register("btn_enabled_export", typeof(bool), typeof(GrammarList), new PropertyMetadata(false));







        private string NextAutoFileNameWithPrefix(string prefix = "unnamed")
        {
            HashSet<string> existFileNams = new HashSet<string>(appData.allSortedGrammarRecords().Select(item => Path.GetFileNameWithoutExtension(item.fileName).ToLower()));
            if (existFileNams.Contains(prefix.ToLower()) == false)
            {
                return prefix;
            }
            int index = 1;
            while (true)
            {
                string tryFileName = $"{prefix} {index}";
                if (existFileNams.Contains(tryFileName.ToLower()) == false)
                {
                    return tryFileName;
                }
                index++;
            }            
        }




        private string FileNameVerify(string fileName)
        {
            HashSet<string> existFileNams = new HashSet<string>(appData.allSortedGrammarRecords().Select(item => Path.GetFileNameWithoutExtension(item.fileName).ToLower()));
            if (existFileNams.Contains(fileName.ToLower()))
            {
                return "File name already exists";
            }
            HashSet<char> invalidPathChars = new HashSet<char>(Path.GetInvalidFileNameChars());
            char[] fileNameChars = fileName.ToCharArray();
            IEnumerable<char> intersects = invalidPathChars.Intersect(fileNameChars);
            if (intersects.Count() > 0)
            {
                string wrong_char = string.Format("{0:c}", intersects.First());
                // "{0:c},\t{1:X4}", someChar, (int)someChar
                return $"Illegal character: {wrong_char}";
            }
            return "";            
        }

        private async void Button_New_Click(object sender, RoutedEventArgs e)
        {
            NewGrammarContentDialog dialog = new NewGrammarContentDialog(NextAutoFileNameWithPrefix(), FileNameVerify);
            if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
            {
                GrammarRecord record = new GrammarRecord()
                {
                    fileName = dialog.fileName + ".grammar",    
                    isBuildin = false,
                };
                AppData.appData.customGrammarList.Add(record);
                AppData.appData.SaveCustomGrammarList();
                data_grid_custom.SelectedItem = record;
                await SaveNewGrammarWithName(record.fileName);
            }
        }

        private void Button_Edit_Click(object sender, RoutedEventArgs e)
        {
            GrammarRecord selectedRecord = data_grid_custom.SelectedItem as GrammarRecord;
            if (selectedRecord != null)
            {
                this.Frame.Navigate(typeof(GrammarEdit), selectedRecord, new SuppressNavigationTransitionInfo());                
            }
        }

        private async void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            GrammarRecord selectedRecord = data_grid_custom.SelectedItem as GrammarRecord;
            if (selectedRecord != null)
            {
                appData.customGrammarList.Remove(selectedRecord);
                appData.SaveCustomGrammarList();
                await appData.RemoveCustomGrammar(selectedRecord.fileName);
            }
        }

        private async Task SaveNewGrammarWithName(String fileName)
        {
            Grammar newGrammar = Grammar.CreateDefaultGrammar();            
            string grammarContent = newGrammar.SaveToString();
            await appData.SaveCustomGrammar(fileName, grammarContent);
        }

        private async void Button_Duplicate_Click(object sender, RoutedEventArgs e)
        {
            GrammarRecord selectedRecord = data_grid_custom.SelectedItem as GrammarRecord;
            if (selectedRecord != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(selectedRecord.fileName).Trim();
                int last_index = fileName.LastIndexOf(" ");
                if (last_index != -1)
                {
                    string maybenumber = fileName.Substring(last_index + 1);
                    int number = 0;
                    if (Int32.TryParse(maybenumber, out number))
                    {
                        fileName = fileName.Substring(0, last_index);
                    }
                }                
                NewGrammarContentDialog dialog = new NewGrammarContentDialog(NextAutoFileNameWithPrefix(fileName),
                                                FileNameVerify);
                if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
                {
                    GrammarRecord record = new GrammarRecord()
                    {
                        fileName = dialog.fileName + ".grammar",
                        isBuildin = false,
                    };
                    AppData.appData.customGrammarList.Add(record);
                    AppData.appData.SaveCustomGrammarList();
                    data_grid_custom.SelectedItem = record;
                    await appData.CopyCustomGrammar(selectedRecord.fileName, record.fileName);                    
                }
            }
        }

        private MenuFlyout BuildContextMenu_Import()
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            {
                MenuFlyoutSubItem menuSubItem = new MenuFlyoutSubItem();
                menuSubItem.Text = "From Buildin";
                menuFlyout.Items.Add(menuSubItem);

                List<GrammarRecord> buildInList = appData.buildinGrammarList.ToList();
                buildInList.Sort((a, b) => a.fileName.CompareTo(b.fileName));
                foreach (GrammarRecord record in buildInList)
                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = record.FullName();
                    menuItem.Tag = record;
                    menuItem.Click += Context_menu_Import_Buildin;
                    menuSubItem.Items.Add(menuItem);
                }
                
            }

               
            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.OpenFile };
                menuItem.Text = "From File";
                menuItem.Click += Context_menu_Import_File;                                
                menuFlyout.Items.Add(menuItem);
            }
            return menuFlyout;
        }

        private async void Context_menu_Import_Buildin(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            GrammarRecord buildinRecord = menuItem.Tag as GrammarRecord;
            if (buildinRecord == null)
            {
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(buildinRecord.fileName);
            if (FileNameVerify(fileName).Length != 0)
            {
                NewGrammarContentDialog dialog = new NewGrammarContentDialog(NextAutoFileNameWithPrefix(fileName),
                                            FileNameVerify);
                if (await dialog.ShowAsync() != ContentDialogResult.Secondary)
                {
                    return;
                }
                fileName = dialog.fileName;
            }

            GrammarRecord record = new GrammarRecord()
            {
                fileName = fileName + ".grammar",
                grammarName = buildinRecord.grammarName,
                extName = buildinRecord.extName,
                isBuildin = false,
            };

            AppData.appData.customGrammarList.Add(record);
            AppData.appData.SaveCustomGrammarList();
            data_grid_custom.SelectedItem = record;
            Grammar grammar = new Grammar();
            grammar.readFromString(await appData.LoadBuildinGrammar(buildinRecord.fileName));
            await appData.SaveCustomGrammar(record.fileName, grammar.SaveToString());
        }

        private async void Context_menu_Import_File(object sender, RoutedEventArgs e)
        {
            try
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".grammar");

                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file.Path);
                    if (FileNameVerify(fileName).Length != 0)
                    {
                        NewGrammarContentDialog dialog = new NewGrammarContentDialog(NextAutoFileNameWithPrefix(fileName),
                                                    FileNameVerify);
                        if (await dialog.ShowAsync() != ContentDialogResult.Secondary)
                        {
                            return;
                        }
                        fileName = dialog.fileName;
                    }
                    string file_content = await FileIO.ReadTextAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    Grammar grammar = new Grammar();
                    grammar.readFromString(file_content);

                    GrammarRecord record = new GrammarRecord()
                    {
                        fileName = fileName + ".grammar",
                        grammarName = grammar.GetAttribute(Grammar.attribute_name),
                        extName = grammar.GetAttribute(Grammar.attribute_fileextension),
                        isBuildin = false,
                    };

                    AppData.appData.customGrammarList.Add(record);
                    AppData.appData.SaveCustomGrammarList();
                    data_grid_custom.SelectedItem = record;                    
                    await appData.SaveCustomGrammar(record.fileName, grammar.SaveToString());
                }
            }
            catch (Exception ex)
            {
                await appData.ShowMessage($"Failed to import grammar with exception.\n{ex.Message}");
            }
        }

        private void Button_Import_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout menuFlyout = BuildContextMenu_Import();
            menuFlyout.ShowAt(sender as DependencyObject, new FlyoutShowOptions() { Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft });
        }

        private async void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            GrammarRecord selectedRecord = data_grid_custom.SelectedItem as GrammarRecord;
            if (selectedRecord == null)
            {
                return;
            }

            try
            {
                string file_content = await appData.LoadCustomGrammar(selectedRecord.fileName);
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Grammar File", new List<string>() { ".grammar" });
                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = selectedRecord.fileName;

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    await FileIO.WriteTextAsync(file, file_content, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        await appData.ShowMessage("Failed to export grammar.");
                    }
                }
            }
            catch(Exception ex)
            {
                await appData.ShowMessage($"Failed to export grammar with exception.\n{ex.Message}");
            }
        }

        private void data_grid_custom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool can_edit = (data_grid_custom.SelectedItem != null);
            btn_enabled_delete = can_edit;
            btn_enabled_edit = can_edit;
            btn_enabled_duplicate = can_edit;
            btn_enabled_export = can_edit;
        }

        private void data_grid_custom_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            GrammarRecord selectedRecord = data_grid_custom.SelectedItem as GrammarRecord;
            if (selectedRecord != null)
            {
                this.Frame.Navigate(typeof(GrammarEdit), selectedRecord, new SuppressNavigationTransitionInfo());
            }
        }
    }
}
