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
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml.Controls;
using Shared;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace file_structure
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Dictionary<string, Color> dicHexColor2Color = new Dictionary<string, Color>();
        public static Color defaultColor = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        TabViewItem tabViewItemGrammaer = null;
        public MainPage()
        {
            this.DataContext = appData;
            this.InitializeComponent();
            
        }

        public AppData appData => AppData.appData;


        private void CreateTabItems()
        {
            {
                TabViewItem tabViewItem = new TabViewItem();
                tabViewItem.IsClosable = false;
                tabViewItem.Header = "Inspect";
                tabViewItem.IconSource = new Microsoft.UI.Xaml.Controls.FontIconSource { FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE094" };
                tabViewItem.Content = new ParserPage();                
                TabView_Main.TabItems.Add(tabViewItem);
            }
            {
                TabViewItem tabViewItem = new TabViewItem();
                tabViewItemGrammaer = tabViewItem;
                tabViewItem.IsClosable = false;
                tabViewItem.Header = "Grammars";
                tabViewItem.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Library };
                Frame frame = new Frame();
                frame.Navigate(typeof(GrammarList));
                tabViewItem.Content = frame;
                TabView_Main.TabItems.Add(tabViewItem);
                if (appData.appID == AppID.mp4)
                {
                    tabViewItem.Visibility = Visibility.Collapsed;
                }
            }

            {
                TabViewItem tabViewItem = new TabViewItem();
                tabViewItem.IsClosable = false;
                tabViewItem.IconSource = new Microsoft.UI.Xaml.Controls.FontIconSource { FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE77B" };                
                tabViewItem.Header = "About";
                tabViewItem.Content = new AboutPage();
                TabView_Main.TabItems.Add(tabViewItem);
            }
            TabView_Main.SelectedIndex = 0;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateTabItems();
            await AppData.appData.Load();
            await StoreHelper.GetLicenseState();
            
            //ApplicationView view = ApplicationView.GetForCurrentView();
            //view.SetPreferredMinSize(new Size(2000, 1000));
        }

        private void TabView_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (appData.appID == AppID.mp4)
            {
                return;
            }
            if (e.RemovedItems.Contains(tabViewItemGrammaer) == false)
            {
                return;
            }


            Frame frame = tabViewItemGrammaer.Content as Frame;
            if (frame == null)
            {
                return;
            }
            Page page = frame.Content as Page;
            GrammarEdit editPage = page as GrammarEdit;
            GrammarEditScript scriptEditPage = page as GrammarEditScript;

            editPage?.SaveCurrent();
            scriptEditPage?.SaveCurrent();
        }
    }
}
