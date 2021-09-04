using kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using System.Diagnostics;
using HexEditor;
using Windows.UI;
using System.Globalization;
using Common;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Documents;
using Windows.UI.Text;
using Windows.UI.Core;
using NetTools;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace file_structure
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ParserPage : Page, ILog, IHexViewControlConfig
    {        
        private Dictionary<string, Color> dicHexColor2Color = new Dictionary<string, Color>();
        public static Color defaultColor = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        private bool tree_item_selected_by_program = false;
        // private readonly string restrictMessage = "Users who have not purchased can not browse elements with a level greater than 3, purchase the application to remove the restriction.";
        List<String> customGrammarRecordFileName_LoadingGrammarRef = new List<String>();
        public ParserPage()
        {
            this.DataContext = appData;
            this.InitializeComponent();
        }

        public AppData appData => AppData.appData;


        public ParserPage thisPage => this;

        private async Task<Byte[]> readAssetFile(string fileName)
        {
            Uri uri = new Uri($"ms-appx:///Assets/{fileName}");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            return await readFile(file);
        }

        private async Task<Byte[]> readFile(StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            long length = stream.Length;
            byte[] bytes = new byte[length];
            await stream.ReadAsync(bytes, 0, (int)length);
            return bytes;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (appData.appID == AppID.mp4)
            {
                StackPanelUsingGrammar.Visibility = Visibility.Collapsed;
            }
            await appData.Load();
            if (appData.resultNodes.Contains(appData.grid_tree_view_selected_item))
            {
                data_grid_tree.SelectedItem = appData.grid_tree_view_selected_item;
            }
            DropDownButton_UsingGrammar.Content = (appData.currentParsingGrammarRecord == null) ? "<None>" : appData.currentParsingGrammarRecord.FullName();


        }


        private Grammar LoadGrammarRef(string fileName)
        {
            if (customGrammarRecordFileName_LoadingGrammarRef.Contains(fileName))
            {
                return CustomGrammarCache.LoadCustomGrammarWithCache(fileName).GetAwaiter().GetResult();
            }
            else
            {
                string file_content = appData.LoadBuildinGrammar(fileName).GetAwaiter().GetResult();
                Grammar grammar = new Grammar();
                grammar.readFromString(file_content);
                return grammar;
            }
        }

        

        public int GetInsertLength()
        {
            throw new NotImplementedException();
        }

        public void SetInsertLength(int length)
        {
            throw new NotImplementedException();
        }

        public string GetFillByte()
        {
            throw new NotImplementedException();
        }

        public void SetFillByte(string b)
        {
            throw new NotImplementedException();
        }

        public Color GetThemeResourceColor(string themeColorName)
        {
            object wantedNode = color_themeresource.FindName("color_" + themeColorName);
            TextBlock wantedChild = wantedNode as TextBlock;
            if (wantedChild == null)
            {
                Debug.Assert(false);
                return Color.FromArgb(0, 0, 0, 0);
            }
            SolidColorBrush solidColorBrush = wantedChild.Foreground as SolidColorBrush;
            if (solidColorBrush == null)
            {
                Debug.Assert(false);
                return Color.FromArgb(0, 0, 0, 0);
            }
            Color c = solidColorBrush.Color;
            return c;
        }

        public Color? ConvertColorStringToColor(string colorStringRRGGBB)
        {
            colorStringRRGGBB = colorStringRRGGBB.ToUpper();
            if (colorStringRRGGBB.Length == 6)
            {
                string r = colorStringRRGGBB.Substring(0, 2);
                string g = colorStringRRGGBB.Substring(2, 2);
                string b = colorStringRRGGBB.Substring(4, 2);
                Byte iR;
                Byte iG;
                Byte iB;
                if (Byte.TryParse(r, NumberStyles.HexNumber, null, out iR)
                    && Byte.TryParse(g, NumberStyles.HexNumber, null, out iG)
                    && Byte.TryParse(b, NumberStyles.HexNumber, null, out iB))
                {
                    Color color = Color.FromArgb(0xFF, iR, iG, iB);
                    dicHexColor2Color[colorStringRRGGBB] = color;
                    return color;
                }
            }
            return null;
        }

        public (FillColor fillColor, StrokeColor strokeColor) GetNormalColorForIndex(long index)
        {
            Color strokeColor;
            Color fillColor;
            StrokeColor normalStrokeColor = new StrokeColor();
            FillColor normalFillColor = new FillColor() { color = defaultColor };
            if ((appData.resultNodes.Count == 0) || (appData.byteIndexRange2Result == null))
            {
                return (normalFillColor, normalStrokeColor);
            }
            List<ByteIndexRange> ranges = appData.byteIndexRange2Result.FindRelatedByByteIndex(index);
            ByteIndexRange lastColorRange = ranges.LastOrDefault();

            string strFillColor = "";            
            string strStrokeColor = "";
            if (lastColorRange != null)
            {
                strFillColor = lastColorRange.result.fillColor;
                strStrokeColor = lastColorRange.result.strokeColor;
            }

            
            fillColor = ConvertColorStringToColor(strFillColor).GetValueOrDefault(defaultColor);
            strokeColor = ConvertColorStringToColor(strStrokeColor).GetValueOrDefault(defaultColor);            

            normalFillColor.color = fillColor;          


            if (lastColorRange != null)
            {
                long start_index_bytes = lastColorRange.start_index_bits / ByteView.BITS_PER_BYTE;
                long end_index_bytes = lastColorRange.end_index_bits / ByteView.BITS_PER_BYTE;
                normalFillColor.backgroundExtendRight = ((index + 1) % 16 != 0) && (index + 1 <= end_index_bytes);
                normalFillColor.backgroundExtendBotton = (index + 16 <= end_index_bytes);
                normalFillColor.backgroundExtendRightBottom = ((index + 1) % 16 != 0) && (index + 17 <= end_index_bytes);


                if ((index % 16 == 0) || (index == start_index_bytes))
                {
                    normalStrokeColor.left = strokeColor;
                    normalStrokeColor.leftExtendBotton = (index + 16 <= end_index_bytes);
                }

                if (((index + 1) % 16 == 0) || (index == end_index_bytes))
                {
                    normalStrokeColor.right = strokeColor;
                    normalStrokeColor.rightExtendUp = (index - 16 >= start_index_bytes);
                }

                if (index - 16 < start_index_bytes)
                {
                    normalStrokeColor.top = strokeColor;
                }

                if (index + 16 > end_index_bytes)
                {
                    normalStrokeColor.bottom = strokeColor;
                }
            }

            return (normalFillColor, normalStrokeColor);
        }


        private void HexView_eventPositionChanged(object sender, object e)
        {
            int point_pressed_index = HexView.point_pressed_index;
            if ((appData.resultNodes.Count == 0) || (appData.byteIndexRange2Result == null))
            {
                return;
            }

            ByteIndexRange byteIndexRange = appData.byteIndexRange2Result.FindRelatedByByteIndex(point_pressed_index).FirstOrDefault();
            if (byteIndexRange != null)
            {
                ResultNode targetResultNode;
                if (appData.dicResult2Node.TryGetValue(byteIndexRange.result, out targetResultNode) == false)
                {
                    return;
                }                
                //if (await FreeLimit.common_check_license(restrictMessage, false, () => targetResultNode.result.level > 4, null) == false)
                //{
                //    return;
                //}
                int index = appData.resultNodes.IndexOf(targetResultNode);
                if (index < 0)
                {
                    List<ResultNode> parentResultNodes = new List<ResultNode>();
                    ResultNode current = targetResultNode;
                    do
                    {
                        ResultNode parent = null;
                        if (appData.dicResult2Node.TryGetValue(current.result.parent, out parent) == false)
                        {
                            Debug.Assert(false);
                            return;
                        }
                        parentResultNodes.Add(parent);
                        if (appData.resultNodes.Contains(parent))
                        {
                            break;
                        }
                        current = parent;
                    } while (true);
                    Debug.Assert(parentResultNodes.Count > 0);                    

                    foreach (ResultNode parentResultNode in parentResultNodes.Reverse<ResultNode>())
                    {
                        Expand(parentResultNode);
                    }
                }

                tree_item_selected_by_program = true;
                data_grid_tree.SelectedItem = targetResultNode;
                tree_item_selected_by_program = false;

                data_grid_tree.ScrollIntoView(targetResultNode, null);
            }
        }

        

        private void Button_Click_Toggle_Expand(object sender, RoutedEventArgs e)
        {
            Button button = (sender as Button);
            ResultNode resultNode = button.DataContext as ResultNode;
            Debug.Assert(resultNode.hasChildren);
            object selectedItem = data_grid_tree.SelectedItem;
            if (resultNode.expand)
            {
                Collapse(resultNode);
            }
            else
            {
                //if (await FreeLimit.common_check_license(restrictMessage, false, () => resultNode.result.level > 3, null) == false)
                //{
                //    return;
                //}
                Expand(resultNode);
            }
        }

        public void Collapse(ResultNode resultNode)
        {            
            int collapse_from_index = appData.resultNodes.IndexOf(resultNode) + 1;
            int collapse_count = 0;
            for (int index = collapse_from_index; index < appData.resultNodes.Count; index++)
            {
                if (appData.resultNodes[index].result.level > resultNode.result.level)
                {
                    collapse_count++;
                }
                else
                {
                    break;
                }
            }
            Debug.Assert(collapse_count > 0);
            appData.resultNodes.BatchRemoveItems(collapse_from_index, collapse_count);
            resultNode.expand = false;
        }

        public void Expand(ResultNode resultNode)
        {
            int insert_to_index = appData.resultNodes.IndexOf(resultNode) + 1;
            Debug.Assert(insert_to_index > 0);
            // expand
            int expand_from_index = appData.fullResultNodes.IndexOf(resultNode) + 1;
            Debug.Assert(expand_from_index > 0);
            int expand_count = 0;
            for (int index = expand_from_index; index < appData.fullResultNodes.Count; index++)
            {
                if (appData.fullResultNodes[index].result.level > resultNode.result.level)
                {
                    expand_count++;
                }
                else
                {
                    break;
                }
            }
            Debug.Assert(expand_count > 0);
            List<ResultNode> to_be_added = appData.fullResultNodes.Skip(expand_from_index).Take(expand_count).Where(r => r.result.level == resultNode.result.level + 1).ToList();
            to_be_added.ForEach(r => r.expand = false);

            appData.resultNodes.BatchInsertItems(insert_to_index, to_be_added);

            resultNode.expand = true;
        }

        private void data_grid_tree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tree_item_selected_by_program)
            {
                return;
            }
            ResultNode resultNode = e.AddedItems.FirstOrDefault() as ResultNode;
            if (resultNode == null)
            {
                HexView.SetMarkPosition(-1, 0, false);
            }
            else
            {
                appData.grid_tree_view_selected_item = resultNode;
                Int64 index_of_bytes = ByteView.ConvertBitsCount2BytesCount(resultNode.result.value.index_of_bits + 1) - 1;
                Int64 end_of_bytes = ByteView.ConvertBitsCount2BytesCount(resultNode.result.value.index_of_bits + resultNode.result.value.count_of_bits) - 1;
                HexView.SetMarkPosition((int)index_of_bytes, (int)(end_of_bytes - index_of_bytes + 1), false);
            }
        }

        private void data_grid_tree_LoadingRow(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridRowEventArgs e)
        {
           // Debug.WriteLine("+   " + (e.Row.DataContext as ResultNode).name);
        }

        private void data_grid_tree_UnloadingRow(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridRowEventArgs e)
        {
           // Debug.WriteLine("-   " + (e.Row.DataContext as ResultNode).name);
        }

        private List<String> OpenFileFilter()
        {
            List<String> filters = new List<string>();
            if (appData.appID == AppID.mp4)
            {
                filters.Add(".mp4");
                filters.Add(".m4a");
                filters.Add(".mov");                
            }
            else if (appData.appID == AppID.all)
            {
                filters.Add("*");
            }
            return filters;
        }

        private async void button_open_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;  
            OpenFileFilter().ForEach(filter => picker.FileTypeFilter.Add(filter));
            StorageFile file = await picker.PickSingleFileAsync();    // Only one file can be selected
            if (file == null)
            {
                return;
            }

            parser_log.Inlines.Clear();
            parser_add_log($"Open file {file.Path}");

            appData.toBeParsedFile = await readFile(file);
            if (appData.appID == AppID.mp4)
            {
                appData.currentParsingGrammarRecord = appData.buildinGrammarList.Where(item => item.fileName == "qt.grammar").FirstOrDefault();
                Debug.Assert(appData.currentParsingGrammarRecord != null);
            }
            else
            {
                string ext = Path.GetExtension(file.Path).Trim('.');
                List<GrammarRecord> records = appData.allSortedGrammarRecords().Where(item => (item.extName??"").Split(",").Contains(ext)).ToList();
                if (records.Count > 0 && records.Contains(appData.currentParsingGrammarRecord) == false)
                {
                    GrammarRecord suggestRecord = records.First();
                    if (await ShowMessage_YES_NO($"A matching grammar has been found. Do you want to use it immediately?\n\t{suggestRecord.FullName()}"))
                    {
                        appData.currentParsingGrammarRecord = suggestRecord;
                    }
                }
            }
            DropDownButton_UsingGrammar.Content = (appData.currentParsingGrammarRecord == null) ? "<None>" : appData.currentParsingGrammarRecord.FullName();
            await ReParse();
        }


        private async void button_jump_Click(object sender, RoutedEventArgs e)
        {
            TextBox textBox = new TextBox();
            textBox.PlaceholderText = "FE30";
            textBox.Text = appData.last_goto_pos;
            textBox.SelectAll();
            ContentDialog content_dialog = new ContentDialog()
            {
                Title = "Enter the hexadecimal offset:",
                Content = textBox,
                PrimaryButtonText = "Goto",
                SecondaryButtonText = "Cancel",
            };

            String inputText = "";
            content_dialog.PrimaryButtonClick += (_s, _e) => inputText = textBox.Text;
            ContentDialogResult r = await content_dialog.ShowAsync();
            if (r == ContentDialogResult.Primary)
            {
                try
                {
                    Int64 offset = Convert.ToInt64(inputText, 16);
                    appData.last_goto_pos = inputText;
                    HexView.ScrollPosToTop(offset);
                }
                catch (Exception ex)
                {
                    await ShowMessage(ex.Message);
                }
            }

        }

        private async Task ShowMessage(string message)
        {
            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand("OK", cmd => { }, commandId: 0));

            dialog.DefaultCommandIndex = 0;
            await dialog.ShowAsync();
        }

        private async Task<bool> ShowMessage_YES_NO(string message)
        {
            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand("NO", cmd => { }, commandId: 0));
            dialog.Commands.Add(new UICommand("YES", cmd => { }, commandId: 1));

            dialog.DefaultCommandIndex = 1;
            IUICommand command = (await dialog.ShowAsync());
            int v = (int)command.Id;
            return v == 1;
        }


        private void parser_add_log(String log)
        {
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                parser_log.Inlines.Add(Common_create_inline_text(DateTime.Now.ToString() + "  " + log + "\n", false));
                parser_log_scroll.ChangeView(0, parser_log_scroll.ScrollableHeight, null);
            });
        }

        private Run Common_create_inline_text(string text, bool bold)
        {
            Run run = new Run();
            if (bold)
            {
                run.FontWeight = FontWeights.SemiBold;
            }
            run.Text = text;
            return run;
        }

        private Run Common_create_inline_text(string text, Color textColor)
        {
            Run run = new Run();
            run.Foreground = new SolidColorBrush(textColor);
            run.Text = text;
            return run;
        }

        public void addLog(EnumLogLevel level, string message)
        {
            parser_add_log($"[{level}]:{message}");
        }

        private MenuFlyout CreateCurrentUsingGrammarMenu()
        {
            GrammarRecord currentUsing = appData.currentParsingGrammarRecord;
            MenuFlyout menuFlyout = new MenuFlyout();
            // None
            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.Text = $"<None>";
                if (currentUsing == null)
                {
                    menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Accept };
                }
                menuItem.Click += Context_menu_Current_Grammar_Record_Selected;
                menuFlyout.Items.Add(menuItem);                
            }
            
            // custom
            {
                List<GrammarRecord> customList = appData.customGrammarList.ToList();
                customList.Sort((a, b) => a.fileName.CompareTo(b.fileName));
                foreach (GrammarRecord record in customList)
                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = record.FullName();
                    menuItem.Tag = record;
                    if (currentUsing == record)
                    {
                        menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Accept };
                    }
                    menuItem.Click += Context_menu_Current_Grammar_Record_Selected;
                    menuFlyout.Items.Add(menuItem);
                }
            }

            // build in
            {
                List<GrammarRecord> buildInList = appData.buildinGrammarList.ToList();

                MenuFlyoutSubItem menuSubItem = new MenuFlyoutSubItem();
                menuSubItem.Text = "Build In";
                if (buildInList.Contains(currentUsing))
                {
                    menuSubItem.Icon = new SymbolIcon() { Symbol = Symbol.Accept };
                }
                menuFlyout.Items.Add(menuSubItem);                
                

                buildInList.Sort((a, b) => a.fileName.CompareTo(b.fileName));
                foreach (GrammarRecord record in buildInList)
                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = record.FullName();
                    menuItem.Tag = record;
                    if (currentUsing == record)
                    {
                        menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Accept };
                    }
                    menuItem.Click += Context_menu_Current_Grammar_Record_Selected;
                    menuSubItem.Items.Add(menuItem);
                }
            }
            return menuFlyout;
        }

        private async void Context_menu_Current_Grammar_Record_Selected(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            GrammarRecord selectedRecord = menuItem.Tag as GrammarRecord;
            appData.currentParsingGrammarRecord = selectedRecord;
            DropDownButton_UsingGrammar.Content = (appData.currentParsingGrammarRecord == null) ? "<None>" : appData.currentParsingGrammarRecord.FullName();

            await ReParse();
        }

        private void DropDownButton_UsingGrammar_Click(object sender, RoutedEventArgs e)
        {
            var menu = CreateCurrentUsingGrammarMenu();
            menu.ShowAt(DropDownButton_UsingGrammar, new FlyoutShowOptions() { Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft });
        }

        private async Task ReParse()
        {
            // clear
            StructureMapper structureMapper = null;
            appData.fullResultNodes.Clear();
            appData.dicResult2Node.Clear();
            appData.resultNodes.Clear();
            appData.fileBytes = appData.toBeParsedFile;
            parser_log.Inlines.Clear();

            if (appData.fileBytes.Length == 0)
            {
                return;
            }

            if (appData.currentParsingGrammarRecord == null)
            {
                return;
            }

            Grammar grammar = new Grammar();
            string grammar_content = "";
            string fileName = appData.currentParsingGrammarRecord.fileName;
            if (appData.currentParsingGrammarRecord.isBuildin)
            {
                grammar_content = await appData.LoadBuildinGrammar(fileName);
                grammar.readFromString(grammar_content);
            }
            else
            {
                grammar = await CustomGrammarCache.LoadCustomGrammarWithCache(fileName);
            }
            grammar.loadGrammarRefDelegate = LoadGrammarRef;
            customGrammarRecordFileName_LoadingGrammarRef = appData.customGrammarList.Select(item => item.fileName).ToList();
            appData.isWorking = true;
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            await Task.Run(() =>
            {
                structureMapper = new StructureMapper(grammar, this, new ByteView(appData.toBeParsedFile));
                try
                {
                    structureMapper.process();
                }
                catch (Exception e)
                {
                    parser_add_log(e.Message);
                }


                List<Result> allResults = structureMapper.results.allResults();
                appData.fullResultNodes = new List<ResultNode>(allResults.Select(r => new ResultNode(r)));
                Dictionary<Result, ResultNode> dicResult2Node = new Dictionary<Result, ResultNode>();
                foreach (var resultNode in appData.fullResultNodes)
                {
                    Debug.Assert(dicResult2Node.ContainsKey(resultNode.result) == false);
                    dicResult2Node[resultNode.result] = resultNode;                    
                }
                appData.dicResult2Node = dicResult2Node;

                List<Result> canReserveFindResults = allResults.Where(r => r.CanReverseFind()).ToList();
                canReserveFindResults.Sort((a, b) => a.value.index_of_bits.CompareTo(b.value.index_of_bits));
                appData.byteIndexRange2Result = new ByteIndexRange2Result(canReserveFindResults);
            });

            appData.isWorking = false;
            foreach (ResultNode root_sub_result in appData.fullResultNodes.Where(r => r.result.parent == structureMapper.results.rootResult))
            {
                appData.resultNodes.Add(root_sub_result);
            }
            if (appData.resultNodes.Count == 1 && appData.resultNodes[0].result.results.Count > 0)
            {
                // expand it
                Expand(appData.resultNodes[0]);
            }
            HexView.InvalidateHexView();
        }

        private async void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            await ReParse();
        }
    }
}
