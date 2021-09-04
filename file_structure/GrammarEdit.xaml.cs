using Common;
using kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
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
    public sealed partial class GrammarEdit : Page
    {
        private GrammarRecord record = null;
        public AppData appData => AppData.appData;
        private ElementEditUserControl elementEditUserControl = null;
        private Dictionary<ELEMENT_TYPE, ElementEditUserControl> cachedControl = new Dictionary<ELEMENT_TYPE, ElementEditUserControl>();
        

        public GrammarEdit()
        {
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.InitializeComponent();            
        }



        public bool btn_enabled_delete
        {
            get { return (bool)GetValue(btn_enabled_deleteProperty); }
            set { SetValue(btn_enabled_deleteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for btn_enabled_delete.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty btn_enabled_deleteProperty =
            DependencyProperty.Register("btn_enabled_delete", typeof(bool), typeof(GrammarEdit), new PropertyMetadata(false));



        public bool btn_enabled_move_up
        {
            get { return (bool)GetValue(btn_enabled_move_upProperty); }
            set { SetValue(btn_enabled_move_upProperty, value); }
        }

        // Using a DependencyProperty as the backing store for btn_enabled_move_up.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty btn_enabled_move_upProperty =
            DependencyProperty.Register("btn_enabled_move_up", typeof(bool), typeof(GrammarEdit), new PropertyMetadata(false));



        public bool btn_enabled_move_down
        {
            get { return (bool)GetValue(btn_enabled_move_downProperty); }
            set { SetValue(btn_enabled_move_downProperty, value); }
        }

        // Using a DependencyProperty as the backing store for btn_enabled_move_down.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty btn_enabled_move_downProperty =
            DependencyProperty.Register("btn_enabled_move_down", typeof(bool), typeof(GrammarEdit), new PropertyMetadata(false));



        public bool btn_enabeld_set_start
        {
            get { return (bool)GetValue(btn_enabeld_set_startProperty); }
            set { SetValue(btn_enabeld_set_startProperty, value); }
        }

        // Using a DependencyProperty as the backing store for btn_enabeld_set_start.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty btn_enabeld_set_startProperty =
            DependencyProperty.Register("btn_enabeld_set_start", typeof(bool), typeof(GrammarEdit), new PropertyMetadata(false));


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);     
            if (e.NavigationMode != NavigationMode.New)
            {
                return;
            }
            AppData.appData.editingGrammarElements.Clear();
            record = e.Parameter as GrammarRecord;
            String fileName = record.fileName;
            AppData.appData.editingGrammar = await CustomGrammarCache.LoadCustomGrammarWithCache(record.fileName);            
            if (AppData.appData.editingGrammar.rootStructures(true).Count() == 0)
            {
                AppData.appData.editingGrammar.AddStructure(AppData.appData.editingGrammar.CreateDefaultElement(ELEMENT_TYPE.ELEMENT_STRUCTURE) as ElementStructure);
            }

            AppData.appData.editingGrammar.MakesureStartStructure();
            
            foreach (ElementBase elementBase in AppData.appData.editingGrammar.rootStructures(true))
            {
                ElementBaseNode node = new ElementBaseNode();
                node.element = elementBase;
                node.expand = false;
                AppData.appData.editingGrammarElements.Add(node);
            }
            if (AppData.appData.editingGrammarElements.Count() == 1)
            {
                Expand(AppData.appData.editingGrammarElements[0]);
            }
            data_grid_tree.SelectedItem = AppData.appData.editingGrammarElements.FirstOrDefault();
        }

         
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.FromResult(0);
        }

        public void SaveCurrent()
        {
            ElementEditUserControl lastUsed = Grid_Element_Edit_Container.Children.FirstOrDefault() as ElementEditUserControl;            
            lastUsed?.Save();
        }

        private void data_grid_element_base_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ElementBaseNode node = data_grid_tree.SelectedItem as ElementBaseNode;
            EnableButtons(node);
            if (node == null)
            {
                return;
            }
            ElementEditUserControl lastUsed = Grid_Element_Edit_Container.Children.FirstOrDefault() as ElementEditUserControl;
            lastUsed?.Save();            
            elementEditUserControl = CreateElementEditUserControl(node.element.GetElementType());                        
            Grid_Element_Edit_Container.Children.Clear();
            Grid_Element_Edit_Container.Children.Add(elementEditUserControl); 
            elementEditUserControl.LoadElement(node.element);
        }

        private void EnableButtons(ElementBaseNode node)
        {
            if (node == null)
            {
                btn_enabled_delete = false;
                btn_enabled_move_up = false;
                btn_enabled_move_down = false;
                btn_enabeld_set_start = false;
            }
            else
            {
                btn_enabled_delete = (node.element.IdWithPrefix() != node.element.grammar.strStartStructure);
                btn_enabeld_set_start = (node.element.parentStructure == null);
                if (node.element.parentStructure == null)
                {
                    var rootStructures = node.element.grammar.rootStructures(true).ToList();
                    int index = rootStructures.IndexOf(node.element as ElementStructure);
                    btn_enabled_move_up = (index > 0);
                    btn_enabled_move_down = (index >= 0) && (index < rootStructures.Count() - 1);
                }
                else
                {
                    var subElements = node.element.parentStructure.elements(true).ToList();
                    int index = subElements.IndexOf(node.element);
                    btn_enabled_move_up = (index > 0);
                    btn_enabled_move_down = (index >= 0) && (index < subElements.Count() - 1);
                }
            }
        }

        private ElementEditUserControl CreateElementEditUserControl(ELEMENT_TYPE elementType)
        {
            ElementEditUserControl userControl = null;
            if (cachedControl.TryGetValue(elementType, out userControl))
            {
                return userControl;
            }
            Dictionary<ELEMENT_TYPE, Type> dicElementType2Type = new Dictionary<ELEMENT_TYPE, Type>()
            {
                {ELEMENT_TYPE.ELEMENT_BINARY,           typeof(ElementEditBinary) },
                {ELEMENT_TYPE.ELEMENT_OFFSET,           typeof(ElementEditOffset) },
                {ELEMENT_TYPE.ELEMENT_CUSTOM,           typeof(ElementEditCustom) },
                {ELEMENT_TYPE.ELEMENT_GRAMMAR_REF,      typeof(ElementEditGrammarRef) },
                {ELEMENT_TYPE.ELEMENT_NUMBER,           typeof(ElementEditNumber) },
                {ELEMENT_TYPE.ELEMENT_SCRIPT,           typeof(ElementEditScript) },
                {ELEMENT_TYPE.ELEMENT_STRING,           typeof(ElementEditString) },
                {ELEMENT_TYPE.ELEMENT_STRUCTURE,        typeof(ElementEditStructure) },
                {ELEMENT_TYPE.ELEMENT_STRUCTURE_REF,    typeof(ElementEditStructureRef) },
            };

            Type type = null;
            if (dicElementType2Type.TryGetValue(elementType, out type) == false)
            {
                type = typeof(ElementEditNotSupported);
            }            
            userControl = System.Activator.CreateInstance(type) as ElementEditUserControl;
            userControl.eventExtendChanged += UserControl_eventExtendChanged;
            userControl.eventNameChanged += UserControl_eventNameChanged;
            userControl.eventEnabledChanged += UserControl_eventEnabledChanged;
            cachedControl[elementType] = userControl;
            return userControl;
        }

        private void UserControl_eventEnabledChanged(object sender, ElementBase e)
        {
            ElementBaseNode node = data_grid_tree.SelectedItem as ElementBaseNode;
            if (node == null)
            {
                return;
            }
            if (node.element != e)
            {
                return;
            }
            node.NotifyTextColorBrushChanged();
        }

        private void UserControl_eventNameChanged(object sender, ElementBase e)
        {
            ElementBaseNode node = data_grid_tree.SelectedItem as ElementBaseNode;
            if (node == null)
            {
                return;
            }
            if (node.element != e)
            {
                return;
            }

            node.name = e.name;
        }

        private void UserControl_eventExtendChanged(object sender, ElementBase e)
        {
            ElementBaseNode node = data_grid_tree.SelectedItem as ElementBaseNode;
            if (node == null)
            {
                return;
            }
            if (node.element != e)
            {
                return;
            }
            node.RefreshType();
            
            ElementStructure structure = node.element as ElementStructure;
            structure.RebuildSubElementDerivedFromConnection();
            if (node.expand)
            {
                Collapse(node);
                Expand(node);
            }
        }

        private void Expand(ElementBaseNode node)
        {
            Debug.WriteLine("Expand begin");
            int selected_index = AppData.appData.editingGrammarElements.IndexOf(node);
            int insert_from_index = selected_index + 1;
            ElementStructure elementStructure = node.element as ElementStructure;
            node.children.Clear();
            foreach (ElementBase elementBase in elementStructure.elements(true))
            {
                ElementBaseNode insert_node = new ElementBaseNode()
                {
                    element = elementBase,
                    parent = node,
                };
                node.children.Add(insert_node);
                insert_node.expand = false;
                AppData.appData.editingGrammarElements.Insert(insert_from_index, insert_node);
                insert_from_index++;
            }
            node.expand = true;
            Debug.WriteLine("Expand end");
        }

        private void Collapse(ElementBaseNode node)
        {
            Debug.WriteLine("Collapse begin");
            int selected_index = AppData.appData.editingGrammarElements.IndexOf(node);
            int collapse_from_index = selected_index + 1;

            int collapse_count = count_level_deeper_than(node.level, collapse_from_index);
            if (collapse_count == 0)
            {
                return;
            }

            for (int collapse_index = collapse_from_index + collapse_count - 1; collapse_index >= collapse_from_index; collapse_index--)
            {
                AppData.appData.editingGrammarElements[collapse_index].expand = false;
                AppData.appData.editingGrammarElements.RemoveAt(collapse_index);
            }
            node.expand = false;
            Debug.WriteLine("Collapse end");
        }

        private void Button_Click_Toggle_Expand(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
            {
                return;
            }

            ElementBaseNode node = btn.DataContext as ElementBaseNode;
            if (node == null)
            {
                return;
            }
            data_grid_tree.SelectedItem = node;
            if (node.expand)
            {
                Collapse(node);
            }
            else
            {
                Expand(node);
            }
        }



        private async void Button_Contact_Author_Click(object sender, RoutedEventArgs e)
        {
            string address = appData.editingGrammar.GetAttribute(Grammar.attribute_email);
            if (String.IsNullOrEmpty(address))
            {
                await ShowMessage("The author did not fill in the email address");
                return;
            }
            await SendEmail(address, "", "", null);
        }

        private async void Button_Share_Click(object sender, RoutedEventArgs e)
        {
            string grammar_content_str = appData.editingGrammar.SaveToString();            
            StorageFolder localFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile attachmentFile = await localFolder.CreateFileAsync("Share.grammar", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(attachmentFile, grammar_content_str);
            var stream = RandomAccessStreamReference.CreateFromFile(attachmentFile);
            var attachment = new EmailAttachment(
                         attachmentFile.Name,
                         stream);
            
            await SendEmail("support@bami-tech.net", $"Share grammar: {appData.editingGrammar.GetAttribute(Grammar.attribute_name)}", "", attachment);
        }

        private async Task SendEmail(string targetAddress, string subject, string body, EmailAttachment attachment)
        {
            var emailMessage = new EmailMessage();            
            emailMessage.Subject = subject;
            emailMessage.To.Add(new EmailRecipient(targetAddress));
            emailMessage.Body = body;
            if (attachment != null)
            {
                emailMessage.Attachments.Add(attachment);
            }
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private async void Button_Grammar_Inf_Click(object sender, RoutedEventArgs e)
        {
            GrammarInfContentDialog dialog = new GrammarInfContentDialog()
            {
                grammarName = appData.editingGrammar.GetAttribute(Grammar.attribute_name),
                author = appData.editingGrammar.GetAttribute(Grammar.attribute_author),
                email = appData.editingGrammar.GetAttribute(Grammar.attribute_email),
                description = appData.editingGrammar.description,
                extension = appData.editingGrammar.GetAttribute(Grammar.attribute_fileextension),

            };
            if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
            {
                appData.editingGrammar.SetAttribute(Grammar.attribute_name, dialog.grammarName);
                appData.editingGrammar.SetAttribute(Grammar.attribute_author, dialog.author);
                appData.editingGrammar.SetAttribute(Grammar.attribute_email, dialog.email);
                appData.editingGrammar.description = dialog.description;
                appData.editingGrammar.SetAttribute(Grammar.attribute_fileextension, dialog.extension);

                record.extName = dialog.extension;
                record.grammarName = dialog.grammarName;
                appData.SaveCustomGrammarList();
            }
        }

        private async void Button_Del_Selected_Click(object sender, RoutedEventArgs e)
        {
            ElementBaseNode elementBaseNode = data_grid_tree.SelectedItem as ElementBaseNode;
            if (elementBaseNode == null)
            {
                return;
            }
            await DeleteSelected(elementBaseNode);            
        }

        private void Button_Add_Structure_Click(object sender, RoutedEventArgs e)
        {
            ElementStructure structure = appData.editingGrammar.CreateDefaultElement(ELEMENT_TYPE.ELEMENT_STRUCTURE) as ElementStructure;
            appData.editingGrammar.AddStructure(structure);
            appData.editingGrammarElements.Add(new ElementBaseNode() { element = structure, expand = false, });
        }

        private async void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            string final_string_in_memory = appData.editingGrammar.SaveToString();
            string final_string_in_file = await AppData.appData.LoadCustomGrammar(record.fileName);
            if (final_string_in_file != final_string_in_memory)
            {
                Debug.WriteLine("final_string_in_file\n" + final_string_in_file);
                Debug.WriteLine("final_string_in_memory\n" + final_string_in_memory);

                var dialog = new MessageDialog("Grammar is not saved.");
                dialog.Commands.Add(new UICommand("Cancel", cmd => { }, commandId: 0));
                dialog.Commands.Add(new UICommand("Discard", cmd => { }, commandId: 1));
                dialog.Commands.Add(new UICommand("Save", cmd => { }, commandId: 2));

                dialog.DefaultCommandIndex = 2;
                int dialogResult = (int)(await dialog.ShowAsync()).Id;
                if (dialogResult == 0)
                {
                    return;
                }
                else if (dialogResult == 1)
                {
                    CustomGrammarCache.DiscardCache(record.fileName);
                }
                else if (dialogResult == 2)
                {
                    CustomGrammarCache.DiscardCache(record.fileName);
                    await AppData.appData.SaveCustomGrammar(record.fileName, final_string_in_memory);
                }
            }            
            this.Frame.GoBack();
        }

        private MenuFlyout BuildContextMenu_Structure(ElementBaseNode elementBaseNode, List<ElementBaseNode> offsetsAndStructureRef)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            {
                MenuFlyoutSubItem menuSubItem = new MenuFlyoutSubItem();                
                menuSubItem.Icon = new SymbolIcon() { Symbol = Symbol.Add };
                menuSubItem.Text = "Append";

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Binary";
                    menuItem.Tag = ELEMENT_TYPE.ELEMENT_BINARY;
                    menuItem.Click += Context_menu_add_sub_element;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Number";
                    menuItem.Tag = ELEMENT_TYPE.ELEMENT_NUMBER;
                    menuItem.Click += Context_menu_add_sub_element;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "String";
                    menuItem.Tag = ELEMENT_TYPE.ELEMENT_STRING;
                    menuItem.Click += Context_menu_add_sub_element;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Script";
                    menuItem.Tag = ELEMENT_TYPE.ELEMENT_SCRIPT;
                    menuItem.Click += Context_menu_add_sub_element;
                    menuSubItem.Items.Add(menuItem);
                }

                

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Structure";
                    menuItem.Tag = ELEMENT_TYPE.ELEMENT_STRUCTURE;
                    menuItem.Click += Context_menu_add_sub_element;
                    menuSubItem.Items.Add(menuItem);
                }
               
                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Grammar Reference";
                    menuItem.Tag = ELEMENT_TYPE.ELEMENT_GRAMMAR_REF;
                    menuItem.Click += Context_menu_add_sub_element;
                    menuSubItem.Items.Add(menuItem);
                }

                // custom
                {
                    IReadOnlyList<EmbedScript> customScripts = appData.editingGrammar.GetScript(Grammar.script_type_datatype);
                    if (customScripts.Count > 0)
                    {
                        MenuFlyoutSubItem menuSubItemCustom = new MenuFlyoutSubItem();
                        menuSubItemCustom.Text = "Custom";                        

                        foreach (EmbedScript customScript in customScripts)
                        {
                            MenuFlyoutItem menuItem = new MenuFlyoutItem();
                            menuItem.Text = customScript.name;
                            menuItem.Tag = customScript;
                            menuItem.Click += Context_menu_add_sub_element_custom;
                            menuSubItemCustom.Items.Add(menuItem);
                        }
                        menuSubItem.Items.Add(menuSubItemCustom);
                    }
                }

                {
                    MenuFlyoutSubItem menuSubItemOffset = new MenuFlyoutSubItem();
                    //menuSubItem.Icon = new SymbolIcon() { Symbol = Symbol.Add };
                    menuSubItemOffset.Text = "Offset";
                    menuSubItemOffset.IsEnabled = offsetsAndStructureRef.Count > 0;

                    foreach (ElementBaseNode node in offsetsAndStructureRef)
                    {
                        MenuFlyoutItem menuItem = new MenuFlyoutItem();
                        menuItem.Text = node.name;
                        menuItem.Tag = node;
                        menuItem.Click += Context_menu_add_sub_element_offset;
                        menuSubItemOffset.Items.Add(menuItem);
                    }
                    menuSubItem.Items.Add(menuSubItemOffset);

                    MenuFlyoutSubItem menuSubItemStructureRef = new MenuFlyoutSubItem();
                    //menuSubItem.Icon = new SymbolIcon() { Symbol = Symbol.Add };
                    menuSubItemStructureRef.Text = "Structure Reference";
                    menuSubItemStructureRef.IsEnabled = offsetsAndStructureRef.Count > 0;
                    foreach (ElementBaseNode node in offsetsAndStructureRef)
                    {
                        MenuFlyoutItem menuItem = new MenuFlyoutItem();
                        menuItem.Text = node.name;
                        menuItem.Tag = node;
                        menuItem.Click += Context_menu_add_sub_element_structure_reference;
                        menuSubItemStructureRef.Items.Add(menuItem);
                    }
                    menuSubItem.Items.Add(menuSubItemStructureRef);
                }
                menuFlyout.Items.Add(menuSubItem);
            }
            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.Text = "Delete";                
                menuItem.Click += Context_menu_add_delete_element;
                menuItem.Tag = elementBaseNode;
                menuItem.IsEnabled = (elementBaseNode.element.IdWithPrefix() != elementBaseNode.element.grammar.strStartStructure);
                menuFlyout.Items.Add(menuItem);
            }
            return menuFlyout;
        }

        private int count_level_deeper_than(int level, int from_index)
        {
            int count = 0;
            for (int current_index = from_index; current_index < AppData.appData.editingGrammarElements.Count; current_index++)
            {
                if (AppData.appData.editingGrammarElements[current_index].level > level)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            return count;
        }



        private void Context_menu_add_sub_element_custom(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            EmbedScript customScript = menuItem.Tag as EmbedScript;
            ElementBase elementBase = AppData.appData.editingGrammar.CreateDefaultElement(ELEMENT_TYPE.ELEMENT_CUSTOM);
            elementBase.SetAttribute(ElementKey.script, $"id:{customScript.id}");
            add_new_element(elementBase);
        }

        private void Context_menu_add_sub_element_offset(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            ElementBaseNode node = menuItem.Tag as ElementBaseNode;
            ElementBase elementBase = AppData.appData.editingGrammar.CreateDefaultElement(ELEMENT_TYPE.ELEMENT_OFFSET);
            elementBase.SetAttribute(ElementKey.references, node.element.IdWithPrefix());
            add_new_element(elementBase);
        }

        private void Context_menu_add_sub_element_structure_reference(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            ElementBaseNode node = menuItem.Tag as ElementBaseNode;
            ElementBase elementBase = AppData.appData.editingGrammar.CreateDefaultElement(ELEMENT_TYPE.ELEMENT_STRUCTURE_REF);
            elementBase.SetAttribute(ElementKey.structure, node.element.IdWithPrefix());
            add_new_element(elementBase);
        }

        private void Context_menu_add_sub_element(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }            
            ELEMENT_TYPE type = (ELEMENT_TYPE)menuItem.Tag;

            ElementBase createdElementBase = AppData.appData.editingGrammar.CreateDefaultElement(type);
            add_new_element(createdElementBase);
        }

        private void add_new_element(ElementBase createdElementBase)
        {
            (ElementBaseNode elementBaseNode, int index) = GetSelectedElementStructureNode();
            if (elementBaseNode == null)
            {
                return;
            }
            ElementBaseNode newNode = new ElementBaseNode()
            {
                parent = elementBaseNode,
                element = createdElementBase,
            };

            ElementStructure parentElementStructure = elementBaseNode.element as ElementStructure;
            createdElementBase.parentStructure = parentElementStructure;

            int parentElementBaseNodeIndex = appData.editingGrammarElements.IndexOf(elementBaseNode);

            // No child elements selected
            if (index == -1)
            {
                elementBaseNode.children.Add(newNode);
                parentElementStructure.AddElement(createdElementBase);
                if (elementBaseNode.expand)
                {
                    int skip_count = count_level_deeper_than(elementBaseNode.level, parentElementBaseNodeIndex + 1);
                    appData.editingGrammarElements.Insert(parentElementBaseNodeIndex + 1 + skip_count, newNode);
                }
                else
                {
                    int insert_count = 0;
                    foreach (ElementBaseNode node in elementBaseNode.children)
                    {
                        appData.editingGrammarElements.Insert(parentElementBaseNodeIndex + insert_count + 1, node);
                        insert_count++;
                    }
                    elementBaseNode.expand = true;
                }
            }
            else
            {
                elementBaseNode.children.Insert(index + 1, newNode);
                parentElementStructure.InsertElement(index + 1, createdElementBase);
                appData.editingGrammarElements.Insert((parentElementBaseNodeIndex + 1) + (index + 1), newNode);
            }

            elementBaseNode.RefreshHasChilderen();
        }

        private async void Context_menu_add_delete_element(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
            if (menuItem == null)
            {
                return;
            }
            ElementBaseNode node = menuItem.Tag as ElementBaseNode;
            await DeleteSelected(node);
        }

        public static async Task ShowMessage(string message)
        {
            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand("OK", cmd => { }, commandId: 0));

            dialog.DefaultCommandIndex = 0;
            await dialog.ShowAsync();
        }
        private async Task DeleteSelected(ElementBaseNode elementBaseNode)
        {
            if (elementBaseNode == null)
            {
                return;
            }
            if (elementBaseNode.element.IdWithPrefix() == elementBaseNode.element.grammar.strStartStructure)
            {
                await ShowMessage("The start structure cannot be deleted.");
                return;
            }

            // data
            if (elementBaseNode.parent != null)
            {
                elementBaseNode.element.parentStructure.RemoveElement(elementBaseNode.element);
                elementBaseNode.parent.children.Remove(elementBaseNode);
            }
            else
            {
                appData.editingGrammar.RemoveStructure(elementBaseNode.element as ElementStructure);
            }

            // UI
            int selected_index = appData.editingGrammarElements.IndexOf(elementBaseNode);
            int count_to_be_collapsed = count_level_deeper_than(elementBaseNode.level, selected_index + 1) + 1;
            for (int collapse_index = selected_index + count_to_be_collapsed - 1; collapse_index >= selected_index; collapse_index--)
            {
                AppData.appData.editingGrammarElements[collapse_index].expand = false;
                AppData.appData.editingGrammarElements.RemoveAt(collapse_index);
            }

            elementBaseNode.parent?.RefreshHasChilderen();
        }

        private (ElementBaseNode, int) GetSelectedElementStructureNode()
        {
            ElementBaseNode elementBaseNode = data_grid_tree.SelectedItem as ElementBaseNode;
            if (elementBaseNode == null)
            {
                return (null, -1);
            }
            if (elementBaseNode.element is ElementStructure)
            {
                return (elementBaseNode, -1);                
            }
            return (elementBaseNode.parent, elementBaseNode.parent.children.IndexOf(elementBaseNode));
        }

        private List<DependencyObject> GetVisualChildren(DependencyObject dependencyObject)
        {            
            int count = VisualTreeHelper.GetChildrenCount(dependencyObject);
            List<DependencyObject> objects = new List<DependencyObject>();
            for (int i = 0; i < count; i ++)
            {
                objects.Add(VisualTreeHelper.GetChild(dependencyObject, i));
            }
            return objects;
        }

        private void data_grid_tree_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {

            List<DependencyObject> children = GetVisualChildren(GetVisualChildren(GetVisualChildren(sender)[0])[0]);
            Panel rowsContainer = children.Where(item => item.GetType().Name == "DataGridRowsPresenter").FirstOrDefault() as Panel;
            List<Control> dataGridRows = GetVisualChildren(rowsContainer).Where(item => item.GetType().Name == "DataGridRow").Select(item => item as Control).ToList();

            //rowsContainer.ActualOffset.Y
            Point pt = new Point(0, 0);
            if (args.TryGetPosition(rowsContainer, out pt) == false)
            {
                return;
            }

            List<Control> hitedControls = dataGridRows.Where(item => item.ActualOffset.Y <= pt.Y && pt.Y <= item.ActualOffset.Y + item.ActualHeight).ToList();
            if (hitedControls.Count == 0)
            {
                return;
            }                        
            
            ElementBaseNode elementBaseNode = (hitedControls.Last().DataContext as ElementBaseNode);
            data_grid_tree.SelectedItem = elementBaseNode;
            List<ElementBaseNode> root_nodes = appData.editingGrammarElements.Where(item => item.parent == null).ToList();
            root_nodes.Remove(elementBaseNode);

            MenuFlyout menuFlyout = BuildContextMenu_Structure(elementBaseNode, root_nodes);
            menuFlyout.ShowAt(rowsContainer, pt);
        }

        private async void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            elementEditUserControl?.Save();

            String str = appData.editingGrammar.SaveToString();            
            await appData.SaveCustomGrammar(record.fileName, str);
        }

        private void Button_MoveUp_Selected_Click(object sender, RoutedEventArgs e)
        {
            ElementBaseNode elementBaseNode = data_grid_tree.SelectedItem as ElementBaseNode;
            MoveUp(elementBaseNode);
            EnableButtons(elementBaseNode);
        }

        private void MoveUp(ElementBaseNode elementBaseNode)
        {
            if (elementBaseNode == null)
            {
                return;
            }

            
            if (elementBaseNode.element.parentStructure == null) // root node
            {
                ElementStructure move_up_structure = elementBaseNode.element as ElementStructure;
                List<ElementStructure> rootstructure = elementBaseNode.element.grammar.rootStructures(true).ToList();
                int index = rootstructure.IndexOf(move_up_structure);
                if ((index == 0) || (index < 0))
                {
                    return;
                }

                ElementStructure upper_structure = rootstructure[index - 1];
                ElementBaseNode upper_node = appData.editingGrammarElements.First(item => item.element == upper_structure);
                bool old_upper_expand = upper_node.expand;
                bool old_current_expand = elementBaseNode.expand;
                if (old_upper_expand)
                {
                    Collapse(upper_node);
                }
                if (old_current_expand)
                {
                    Collapse(elementBaseNode);
                }
                int move_to_ui_index = appData.editingGrammarElements.IndexOf(upper_node);
                elementBaseNode.element.grammar.ExchangeRootStructureAtIndex(index - 1, index);

                appData.editingGrammarElements.RemoveAt(move_to_ui_index + 1);
                appData.editingGrammarElements.Insert(move_to_ui_index, elementBaseNode);

                if (old_upper_expand)
                {
                    Expand(upper_node);
                }
                if (old_current_expand)
                {
                    Expand(elementBaseNode);
                }

                data_grid_tree.SelectedItem = elementBaseNode;
            }
            else
            {
                ElementBase move_up_element = elementBaseNode.element;
                List<ElementBase> subElements = elementBaseNode.element.parentStructure.elements(true).ToList();
                int index = subElements.IndexOf(move_up_element);
                if ((index == 0) || (index < 0))
                {
                    return;
                }

                ElementBase upper_element = subElements[index - 1];
                ElementBaseNode upper_node = appData.editingGrammarElements.First(item => item.element == upper_element);
                bool old_upper_expand = upper_node.expand;
                bool old_current_expand = elementBaseNode.expand;
                if (old_upper_expand)
                {
                    Collapse(upper_node);
                }
                if (old_current_expand)
                {
                    Collapse(elementBaseNode);
                }
                int move_to_ui_index = appData.editingGrammarElements.IndexOf(upper_node);
                elementBaseNode.element.parentStructure.ExchangeElementAtIndex(index - 1, index);

                appData.editingGrammarElements.RemoveAt(move_to_ui_index + 1);
                appData.editingGrammarElements.Insert(move_to_ui_index, elementBaseNode);

                if (old_upper_expand)
                {
                    Expand(upper_node);
                }
                if (old_current_expand)
                {
                    Expand(elementBaseNode);
                }

                data_grid_tree.SelectedItem = elementBaseNode;
            }
        }

        private void MoveDown(ElementBaseNode elementBaseNode)
        {
            if (elementBaseNode == null)
            {
                return;
            }


            if (elementBaseNode.element.parentStructure == null) // root node
            {
                ElementStructure move_down_structure = elementBaseNode.element as ElementStructure;
                List<ElementStructure> rootstructure = elementBaseNode.element.grammar.rootStructures(true).ToList();
                int index = rootstructure.IndexOf(move_down_structure);
                if ((index == rootstructure.Count - 1) || (index < 0))
                {
                    return;
                }

                ElementStructure down_structure = rootstructure[index + 1];
                ElementBaseNode down_node = appData.editingGrammarElements.First(item => item.element == down_structure);
                bool old_down_expand = down_node.expand;
                bool old_current_expand = elementBaseNode.expand;
                if (old_down_expand)
                {
                    Collapse(down_node);
                }
                if (old_current_expand)
                {
                    Collapse(elementBaseNode);
                }
                int move_to_ui_index = appData.editingGrammarElements.IndexOf(down_node);
                elementBaseNode.element.grammar.ExchangeRootStructureAtIndex(index + 1, index);

                appData.editingGrammarElements.RemoveAt(move_to_ui_index);
                appData.editingGrammarElements.Insert(move_to_ui_index - 1, down_node);

                if (old_down_expand)
                {
                    Expand(down_node);
                }
                if (old_current_expand)
                {
                    Expand(elementBaseNode);
                }

                data_grid_tree.SelectedItem = elementBaseNode;
            }
            else
            {
                ElementBase move_down_element = elementBaseNode.element;
                List<ElementBase> subElements = elementBaseNode.element.parentStructure.elements(true).ToList();
                int index = subElements.IndexOf(move_down_element);
                if ((index == subElements.Count - 1) || (index < 0))
                {
                    return;
                }

                ElementBase down_element = subElements[index + 1];
                ElementBaseNode down_node = appData.editingGrammarElements.First(item => item.element == down_element);
                bool old_down_expand = down_node.expand;
                bool old_current_expand = elementBaseNode.expand;
                if (old_down_expand)
                {
                    Collapse(down_node);
                }
                if (old_current_expand)
                {
                    Collapse(elementBaseNode);
                }
                int move_to_ui_index = appData.editingGrammarElements.IndexOf(down_node);
                elementBaseNode.element.parentStructure.ExchangeElementAtIndex(index + 1, index);

                appData.editingGrammarElements.RemoveAt(move_to_ui_index);
                appData.editingGrammarElements.Insert(move_to_ui_index - 1, down_node);

                if (old_down_expand)
                {
                    Expand(down_node);
                }
                if (old_current_expand)
                {
                    Expand(elementBaseNode);
                }

                data_grid_tree.SelectedItem = elementBaseNode;
            }
            EnableButtons(elementBaseNode);
        }

        private void Button_MoveDown_Selected_Click(object sender, RoutedEventArgs e)
        {
            ElementBaseNode elementBaseNode = data_grid_tree.SelectedItem as ElementBaseNode;
            MoveDown(elementBaseNode);
        }

        private void Button_MoveDown_SetStart_Click(object sender, RoutedEventArgs e)
        {
            ElementBaseNode elementBaseNode = data_grid_tree.SelectedItem as ElementBaseNode;
            if (elementBaseNode == null)
            {
                return;
            }
            if (elementBaseNode.element.parentStructure != null)
            {
                return;
            }

            elementBaseNode.element.grammar.SetStartStructure(elementBaseNode.element as ElementStructure);
            foreach(var elementNodes in appData.editingGrammarElements)
            {
                if (elementNodes.element.parentStructure == null)
                {
                    elementNodes.NotifyFontWeightChanged();
                }
            }
        }

        private void Button_Grammar_Script_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GrammarEditScript), null, new SuppressNavigationTransitionInfo());
        }

        private void data_grid_tree_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ElementBaseNode node = data_grid_tree.SelectedItem as ElementBaseNode;
            if (node == null)
            {
                return;
            }
            if (node.element is not ElementStructure)
            {
                return;
            }
            if (node.hasChildren == false)
            {
                return;
            }

            if (node.expand)
            {
                Collapse(node);
            }
            else
            {
                Expand(node);
            }
        }
    }
}
