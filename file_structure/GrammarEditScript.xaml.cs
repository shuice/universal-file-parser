using Common;
using kernel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    /// 

    public class TreeViewNodeDataItem : DependencyObject
    {
        public bool isFolder;

        public string scriptType;
        public EmbedScript embedScript;

        public ObservableCollection<TreeViewNodeDataItem> childrenItems = new ObservableCollection<TreeViewNodeDataItem>();

        public void UpdateText()
        {
            if (isFolder)
            {
                this.text = $"{Grammar.script_type_2_ui()[scriptType]} ({childrenItems.Count})";
            }
            else
            {
                this.text = embedScript.name;
            }
        }


        public string text
        {
            get { return (string)GetValue(textProperty); }
            set { SetValue(textProperty, value); }
        }
        // Using a DependencyProperty as the backing store for text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty textProperty =
            DependencyProperty.Register("text", typeof(string), typeof(TreeViewNodeDataItem), new PropertyMetadata(""));

    }

    class ExplorerItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var dataItem = (TreeViewNodeDataItem)item;
            return dataItem.isFolder ? FolderTemplate : FileTemplate;
        }
    }


    public sealed partial class GrammarEditScript : Page
    {
        private Dictionary<string, TreeViewNodeDataItem> dicName2RootItems = new Dictionary<string, TreeViewNodeDataItem>();

        public ObservableCollection<TreeViewNodeDataItem> treeViewDataSource = new ObservableCollection<TreeViewNodeDataItem>();

        public bool script_selected
        {
            get { return (bool)GetValue(script_selectedProperty); }
            set { SetValue(script_selectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for script_selected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty script_selectedProperty =
            DependencyProperty.Register("script_selected", typeof(bool), typeof(GrammarEditScript), new PropertyMetadata(false));




        public TreeViewNodeDataItem nodeData
        {
            get { return (TreeViewNodeDataItem)GetValue(nodeDataProperty); }
            set { SetValue(nodeDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for embed_scriptProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty nodeDataProperty =
            DependencyProperty.Register("nodeData", typeof(TreeViewNodeDataItem), typeof(TextBox), new PropertyMetadata(null));



        public void SaveCurrent()
        {
            TreeViewNodeDataItem embedScrptInTextBox = TextBox_Source.GetValue(nodeDataProperty) as TreeViewNodeDataItem;
            if (embedScrptInTextBox != null)
            {
                embedScrptInTextBox.embedScript.source = TextBox_Source.Text;
            }
        }




        public GrammarEditScript()
        {
            this.InitializeComponent();

        }

        public AppData appData => AppData.appData;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);            
            foreach (string script_type in new List<string> {
                                                //Grammar.script_type_generic,
                                                //Grammar.script_type_file,
                                                //Grammar.script_type_grammar,
                                                //Grammar.script_type_process_results,
                                                Grammar.script_type_datatype,
                                                //Grammar.script_type_selection
                                                })
            {
                List<EmbedScript> embedScripts = appData.editingGrammar.GetScript(script_type).ToList();
                TreeViewNodeDataItem rootItem = new TreeViewNodeDataItem() { isFolder = true, scriptType = script_type };     
                
                foreach (EmbedScript embedScript in embedScripts)
                {
                    var subItem = new TreeViewNodeDataItem() { isFolder = false, embedScript = embedScript };   
                    subItem.UpdateText();
                    rootItem.childrenItems.Add(subItem);
                }
                rootItem.UpdateText();
                dicName2RootItems[script_type] = rootItem;
                treeViewDataSource.Add(rootItem);                
            }
            
        }



        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrent();
            this.Frame.GoBack();
        }

        private void TreeView_EmbedScript_ItemInvoked(Microsoft.UI.Xaml.Controls.TreeView sender, Microsoft.UI.Xaml.Controls.TreeViewItemInvokedEventArgs args)
        {
            SelectionChanged(args.InvokedItem as TreeViewNodeDataItem);
        }

        private async void Button_ScriptInfo_Click(object sender, RoutedEventArgs e)
        {
            TreeViewNodeDataItem selectedTreeViewNode = TreeView_EmbedScript.SelectedItem as TreeViewNodeDataItem;
            if (selectedTreeViewNode == null)
            {
                return;
            }
            await EditEmbedScriptByTreeViewNode(selectedTreeViewNode);
        }

        

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            TreeViewNodeDataItem selectedTreeViewNode = TreeView_EmbedScript.SelectedItem as TreeViewNodeDataItem;
            if (selectedTreeViewNode == null)
            {
                return;
            }
            DeleteEmbedScriptByTreeViewNode(selectedTreeViewNode);
        }

        private async void Button_New_Click(object sender, RoutedEventArgs e)
        {
            await NewScript(null);
        }

        private async Task<bool> EditScript(EmbedScript script, bool canChangeType)
        {
            ContentDialogNewScript dialog = new ContentDialogNewScript(canChangeType);
            dialog.name = script.name;
            dialog.language = script.language;
            dialog.type = script.type;
            dialog.description = script.description;
            if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
            {
                script.name = dialog.name;
                script.language = dialog.language;
                script.type = dialog.type;
                script.description = dialog.description;
                return true;
            }
            return false;
        }

        private MenuFlyout BuildContextMenu_Type(string script_type)
        {
            MenuFlyout menuFlyout = new MenuFlyout();
            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.OpenFile };
                menuItem.Text = "New Script";
                menuItem.Tag = script_type;
                menuItem.Click += Context_menu_New_Script;
                menuFlyout.Items.Add(menuItem);
            }
            return menuFlyout;
        }

        private MenuFlyout BuildContextMenu_Script(TreeViewNodeDataItem item)
        {
            MenuFlyout menuFlyout = new MenuFlyout();
            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Edit};
                menuItem.Text = "Script Info";
                menuItem.Tag = item;
                menuItem.Click += Context_menu_script_info;
                menuFlyout.Items.Add(menuItem);
            }
            menuFlyout.Items.Add(new MenuFlyoutSeparator());
            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Delete };
                menuItem.Text = "Delete";
                menuItem.Tag = item;
                menuItem.Click += Context_menu_delete;
                menuFlyout.Items.Add(menuItem);
            }
            return menuFlyout;
        }

        

        private async void Context_menu_New_Script(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            string script_type = menuItem.Tag as string;
            await NewScript(script_type);
        }

        private async Task NewScript(string script_type)
        {
            EmbedScript embedScript = appData.editingGrammar.CreateNewScript(script_type);
            if (await EditScript(embedScript, true) == false)
            {
                return;
            }
            TreeViewNodeDataItem rootItem;
            if (dicName2RootItems.TryGetValue(embedScript.type, out rootItem) == false)
            {
                return;
            }
            TreeViewNodeDataItem newItem = new TreeViewNodeDataItem() { isFolder = false, embedScript = embedScript };
            newItem.UpdateText();
            rootItem.childrenItems.Add(newItem);            
            TreeView_EmbedScript.SelectedItem = newItem;
            SelectionChanged(newItem);
            rootItem.UpdateText();            
            appData.editingGrammar.AddEmbedScript(embedScript);
        }

        private void SelectionChanged(TreeViewNodeDataItem newSelected)
        {
            TreeViewNodeDataItem embedScriptInTree = newSelected;
            if (embedScriptInTree == null)
            {
                return;
            }

            TreeViewNodeDataItem embedScrptInTextBox = TextBox_Source.GetValue(nodeDataProperty) as TreeViewNodeDataItem;
            if (embedScriptInTree != embedScrptInTextBox)
            {
                if (embedScrptInTextBox != null)
                {
                    embedScrptInTextBox.embedScript.source = TextBox_Source.Text;
                }
            }

            script_selected = !embedScriptInTree.isFolder;
            if (embedScriptInTree.isFolder)
            {
                // show xxxx 
                return;
            }

            TextBox_Source.Text = embedScriptInTree.embedScript.source;
            TextBox_Source.SetValue(nodeDataProperty, embedScriptInTree);
        }

        private void DeleteEmbedScriptByTreeViewNode(TreeViewNodeDataItem item)
        {
            DeleteByNode(item);
        }

        private async Task EditEmbedScriptByTreeViewNode(TreeViewNodeDataItem item)
        {
            if (item == null)
            {
                return;
            }
            EmbedScript embedScript = item.embedScript;
            if (embedScript == null)
            {
                return;
            }
            if (await EditScript(embedScript, false) == false)
            {
                return;
            }
            item.UpdateText();            
        }

        private async void Context_menu_script_info(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            TreeViewNodeDataItem item = menuItem.Tag as TreeViewNodeDataItem;
            await EditEmbedScriptByTreeViewNode(item);
        }

        private void Context_menu_delete(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            TreeViewNodeDataItem item = menuItem.Tag as TreeViewNodeDataItem;
            DeleteByNode(item);
        }

        private void DeleteByNode(TreeViewNodeDataItem selectedTreeViewNode)
        {
            if (selectedTreeViewNode == null)
            {
                return;
            }

            TreeViewNodeDataItem rootItem;
            if (dicName2RootItems.TryGetValue(selectedTreeViewNode.embedScript.type, out rootItem) == false)
            {
                return;
            }
            rootItem.childrenItems.Remove(selectedTreeViewNode);
            rootItem.UpdateText();
            appData.editingGrammar.RemoveEmbedScript(selectedTreeViewNode.embedScript);
        }


 
        private void TreeViewItem_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            Microsoft.UI.Xaml.Controls.TreeViewItem treeViewItem = (sender as Microsoft.UI.Xaml.Controls.TreeViewItem);
            if (treeViewItem == null)
            {
                return;
            }
            TreeViewNodeDataItem treeViewNodeDataItem = treeViewItem.DataContext as TreeViewNodeDataItem;
            if (treeViewNodeDataItem == null)
            {
                return;
            }
           

            Point pt;
            if (args.TryGetPosition(sender, out pt) == false)
            {
                return;
            }
            treeViewItem.IsSelected = true;
            SelectionChanged(treeViewNodeDataItem);
            MenuFlyout menuFlyOut = null;
            if (treeViewNodeDataItem.isFolder)
            {
                menuFlyOut = BuildContextMenu_Type(treeViewNodeDataItem.scriptType);
            }
            else
            {
                menuFlyOut = BuildContextMenu_Script(treeViewNodeDataItem);
            }
            if (menuFlyOut == null)
            {
                return;
            }
            menuFlyOut.ShowAt(sender, new FlyoutShowOptions() { Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft, Position = pt });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.UI.Xaml.Controls.TreeViewNode node = TreeView_EmbedScript.RootNodes.FirstOrDefault();
            if (node == null)
            {
                return;
            }
            Microsoft.UI.Xaml.Controls.TreeViewNode childNode = node.Children.FirstOrDefault();
            if (childNode == null)
            {
                return;
            }
            TreeView_EmbedScript.SelectedItem = childNode.Content;
            SelectionChanged(childNode.Content as TreeViewNodeDataItem);
        }
    }
}