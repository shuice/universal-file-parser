using kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace file_structure
{
    public class ResultNode : AutoNotifyPropertyChanged
    {        
        public ResultNode(Result result)
        {
            this.result = result;
        }

        public Result result;

        private bool _expand = false;
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


        public double levelOffset => (result.level - 1) * 20;
        public string position
        {
            get
            {
                return ByteView.format_bit_index_ui(result.value.index_of_bits, true);
            }
        }
        public string offset
        {
            get
            {
                Result parent = result.parent;
                if (parent == null)
                {
                    return "0";
                }
                Int64 iOffset = result.value.index_of_bits - parent.value.index_of_bits;
                if (iOffset == 0)
                {
                    return "0";
                }
                else if (iOffset > 0)
                {
                    return "+" + ByteView.format_bit_index_ui(iOffset, false);
                }
                return "-" + ByteView.format_bit_index_ui(iOffset, false);                
            }
        }
        public string length
        {
            get
            {
                return ByteView.format_bit_index_ui(result.value.count_of_bits, false);
            }
        }
        
        public string index
        {
            get
            {
                return result.index_in_structure.ToString();
            }
        }
        public string name
        {
            get
            {
                return result.Name_UI();
            }
        }

        public string value
        {
            get
            {
                return result.Value_UI();
            }
        }

        public bool hasChildren
        {
            get
            {
                bool has = result.results.Count > 0;
                return has;
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
        

        public SolidColorBrush textColorBrush
        {
            get
            {
                if (result.isFragment)
                {
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0xa7, 0x32, 0x4a));
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                }
            }
        }

    }
}
