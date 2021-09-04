using kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml.Media;

namespace file_structure
{
    public class ElementBaseNode : AutoNotifyPropertyChanged
    {
        public ElementBaseNode parent = null;
        public List<ElementBaseNode> children = new List<ElementBaseNode>();
        public ElementBase element;
        private bool _expand = false;
        private static Dictionary<ELEMENT_TYPE, Color> type2Color = new Dictionary<ELEMENT_TYPE, Color>()
        {
            { ELEMENT_TYPE.ELEMENT_STRUCTURE,       Color.FromArgb(0xFF, 0x46, 0x82, 0xb4) },
            { ELEMENT_TYPE.ELEMENT_BINARY,          Color.FromArgb(0xFF, 0xF3, 0x00, 0x07) },
            { ELEMENT_TYPE.ELEMENT_GRAMMAR_REF,     Color.FromArgb(0xFF, 0xAD, 0xD8, 0xE6) },   
            { ELEMENT_TYPE.ELEMENT_NUMBER,          Color.FromArgb(0xFF, 0x00, 0xEF, 0x23) }, 
            { ELEMENT_TYPE.ELEMENT_STRING,          Color.FromArgb(0xFF, 0xFF, 0xDD, 0x2A) }, 
            { ELEMENT_TYPE.ELEMENT_OFFSET,          Color.FromArgb(0xFF, 0xFF, 0x3D, 0xE3) }, 
            { ELEMENT_TYPE.ELEMENT_STRUCTURE_REF,   Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF) },
            { ELEMENT_TYPE.ELEMENT_SCRIPT,          Color.FromArgb(0xFF, 0x85, 0x52, 0xA1) },
            { ELEMENT_TYPE.ELEMENT_CUSTOM,          Color.FromArgb(0xFF, 0xFF, 0x45, 0x00) },
        };
        public bool expand
        {
            get => _expand;
            set
            {
                _expand = value;
                buttonSymbol = _expand ? "\uE70d" : "\uE76c";
                OnPropertyChanged();               
            }
        }

        public SolidColorBrush textColorBrush
        {
            get
            {
                if (element.enabled)
                {
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0xD0, 0xD0, 0xD0));
                }
            }
            
        }

        public void NotifyTextColorBrushChanged()
        {
            OnPropertyChanged("textColorBrush");
        }

        public SolidColorBrush iconColorBrush
        {
            get
            {
                ELEMENT_TYPE type = element.GetElementType();
                Color c;
                if (type2Color.TryGetValue(type, out c))
                {                    
                    return new SolidColorBrush(c);
                }
                return new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            }
        }

        public FontWeight nameFontWeight
        {
            get => is_started_node() ? FontWeights.Bold : FontWeights.Normal;            
        }

        public void NotifyFontWeightChanged()
        {
            OnPropertyChanged("nameFontWeight");
        }

        private bool is_started_node()
        {
            if (element.parentStructure != null)
            {
                return false;
            }
            return element.grammar.strStartStructure == element.IdWithPrefix();
        }

        public int level
        {
            get
            {
                int level = 0;
                ElementStructure parent = element.parentStructure;
                while (parent != null)
                {
                    level++;
                    parent = parent.parentStructure;
                }
                return level;
            }
        }

        public double levelOffset
        {
            get
            {                
                return level * 20;
            }            
        }
        public string name
        {
            get
            {
                return element.name;
            }

            set
            {
                element.setName(value);
                OnPropertyChanged();
            }
        }


        public bool hasChildren
        {
            get
            {
                ElementStructure elementStructure = element as ElementStructure;
                if ((elementStructure != null) && elementStructure.elements(true).Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        private string _buttonSymbol = "\uE76c";
        public string buttonSymbol
        {
            get => _buttonSymbol;
            set
            {
                _buttonSymbol = value;
                OnPropertyChanged();
            }
        }

        public string type
        {
            get
            {
                if (element is ElementStructure)
                {
                    string extends = element.GetValue(ElementKey.extends);
                    if (extends.Length > 0)
                    {
                        ElementStructure extendFromStructure = element.grammar.GetStructureByIdWithPrefix(extends);
                        if (extendFromStructure != null)
                        {
                            return extendFromStructure.name;
                        }
                    }
                }
                return element.GetElementTypeUI();
            }
        }
        
        public void RefreshType()
        {
            OnPropertyChanged("type");
        }

        public void RefreshHasChilderen()
        {
            OnPropertyChanged("hasChildren");
        }
    }
}
