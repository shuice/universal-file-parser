using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace file_structure
{
    public class GrammarRecord : DependencyObject
    {
        public bool isBuildin;

        // abc.grammar
        public string fileName
        {
            get { return (string)GetValue(fileNameProperty); }
            set { SetValue(fileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for fileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty fileNameProperty =
            DependencyProperty.Register("fileName", typeof(string), typeof(GrammarRecord), new PropertyMetadata(""));


        // abc file grammar
        public string grammarName
        {
            get { return (string)GetValue(grammarNameProperty); }
            set { SetValue(grammarNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for grammarName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty grammarNameProperty =
            DependencyProperty.Register("grammarName", typeof(string), typeof(GrammarRecord), new PropertyMetadata(""));


        


        // .mp3, .mp4
        public string extName
        {
            get { return (string)GetValue(extNameProperty); }
            set { SetValue(extNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for extName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty extNameProperty =
            DependencyProperty.Register("extName", typeof(string), typeof(GrammarRecord), new PropertyMetadata(""));

        public string FullName()
        {
            string v = fileName;
            if (String.IsNullOrEmpty(grammarName) == false)
            {
                v = $"{fileName} ({grammarName})";
            }
            return v;
        }
    }
}
