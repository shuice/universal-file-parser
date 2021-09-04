using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage.FileProperties;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HexEditor
{
    public struct FillColor
    {
        public Color color;
        public bool backgroundExtendRight;
        public bool backgroundExtendBotton;
        public bool backgroundExtendRightBottom;
    }
    public struct StrokeColor
    {
        public Color? left;
        public Color? top;
        public Color? right;
        public Color? bottom;
        public bool leftExtendBotton;
        public bool rightExtendUp;
    }
    public class DrawColor
    {
        public Color header_text;
        public Color header_text_hilighted;
        public Color offset_text;
        public Color offset_text_hilighted;
        public Color hex_ascii_text;
        public Color hex_ascii_text_odd;
        public Color hex_ascii_text_point_selected;
        public Color hex_ascii_text_point_selected_background;
        public Color hex_ascii_text_program_selected;
        public Color hex_ascii_text_program_selected_background;

        public Color hex_ascii_text_finded_selected_background;
        public Color hex_ascii_text_finded_selected_background_light;


        public DrawColor(Func<string, Color> get_theme_color_func)
        {
            header_text = get_theme_color_func("SystemBaseMediumColor");
            header_text_hilighted = get_theme_color_func("SystemBaseHighColor");
            offset_text = get_theme_color_func("SystemBaseMediumColor");
            offset_text_hilighted = get_theme_color_func("SystemBaseHighColor");
            hex_ascii_text = get_theme_color_func("SystemBaseHighColor");
            hex_ascii_text_odd = get_theme_color_func("SystemAccentColor"); ;
            hex_ascii_text_point_selected = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            hex_ascii_text_point_selected_background = get_theme_color_func("SystemAccentColor");
            hex_ascii_text_program_selected = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            hex_ascii_text_program_selected_background = get_theme_color_func("SystemAccentColorLight2");


            hex_ascii_text_finded_selected_background = Color.FromArgb(0xFF, 0xFF, 0xA5, 0x00);
            hex_ascii_text_finded_selected_background_light = Color.FromArgb(0xFF, 0xF5, 0xbf, 0x5a);
            
            Debug.WriteLine($"lowColor:{header_text}, highColor:{hex_ascii_text}, altHigh:{hex_ascii_text_point_selected} ");
        }
    }

    public interface IHexViewControlConfig
    {
        int GetInsertLength();
        void SetInsertLength(int length);

        string GetFillByte();
        void SetFillByte(string b);

        Color GetThemeResourceColor(String themeColorName);

        (FillColor fillColor, StrokeColor strokeColor) GetNormalColorForIndex(Int64 index);
    }


    public class HexViewControlConfig : IHexViewControlConfig
    {
        private string _fill_byte = "00";
        private int _insert_length = 4;
        public string GetFillByte()
        {
            return _fill_byte;
        }

        public int GetInsertLength()
        {
            return _insert_length;
        }

        public (FillColor fillColor, StrokeColor strokeColor) GetNormalColorForIndex(long index)
        {
            throw new NotImplementedException();
        }

        public Color GetThemeResourceColor(string themeColorName)
        {
            Random random = new Random();
            return Color.FromArgb(0xFF, (byte)(random.Next() % 256), (byte)(random.Next() % 256), (byte)(random.Next() % 256));
        }

        public void SetFillByte(string b)
        {
            _fill_byte = b;
        }

        public void SetInsertLength(int length)
        {
            _insert_length = length;
        }
    }

    public sealed partial class HexViewControl : UserControl
    {

        private DrawColor drawColor;

        static char[] hexLettersBig =
            new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        public event EventHandler<object> eventPoinPressed;
        public event EventHandler<object> eventPositionChanged;
        public event EventHandler<object> eventSelectionChanged;
        public event EventHandler<object> eventBytesLengthChanged;
        public event EventHandler<object> eventBytesContentChanged;

        public event EventHandler<object> eventMenuJump;
        public event EventHandler<object> eventMenuFind;


        static int width_offset = 100;
        static int width_distance_offset_and_hex = 10;
        static int width_hex_00 = 30;
        static int width_distance_hex_and_preview_char = 20;
        static int width_preview_char = 10;
        static int height_header = 40;
        static int height_line = 30;
        static int distance_four_bytes = 0;
        static int ascii_offset_begin = width_offset + width_distance_offset_and_hex + 16 * width_hex_00 + width_distance_hex_and_preview_char + distance_four_bytes * 3;
        static Color transparent_color = Color.FromArgb(0, 0, 0, 0);



        public bool is_readonly
        {
            get { return (bool)GetValue(is_readonlyProperty); }
            set { SetValue(is_readonlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for is_readonly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty is_readonlyProperty =
            DependencyProperty.Register("is_readonly", typeof(bool), typeof(HexViewControl), new PropertyMetadata(false));






        public bool is_hide_editable_menu
        {
            get { return (bool)GetValue(is_hide_editable_menuProperty); }
            set { SetValue(is_hide_editable_menuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for is_hide_editable_menu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty is_hide_editable_menuProperty =
            DependencyProperty.Register("is_hide_editable_menu", typeof(bool), typeof(HexViewControl), new PropertyMetadata(false));



        private bool tracking = false;
        private bool tracking_in_hex = true;
        private double tracking_from_index = -0.5f;
        private double tracking_to_index = -0.5f;

        private (int from, int to, bool finded) _selected_index = (-1, -1, false);


   
        public IHexViewControlConfig config
        {
            get { return (IHexViewControlConfig)GetValue(configProperty); }
            set { SetValue(configProperty, value); }
        }

        // Using a DependencyProperty as the backing store for config.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty configProperty =
            DependencyProperty.Register("config", typeof(IHexViewControlConfig), typeof(HexViewControl), new PropertyMetadata(null));



        public void set_selected_index(int from, int to, bool finded)
        {
            _selected_index.from = from;
            _selected_index.to = to;
            _selected_index.finded = finded;
            eventSelectionChanged?.Invoke(this, null);
        }
        private int selected_from_index
        {
            get => _selected_index.from;
        }
        private int selected_to_index
        {
            get => _selected_index.to;
        }

        private bool finded_selection
        {
            get => _selected_index.finded;
        }

        private int _point_pressed_index = -1;
        public int point_pressed_index
        {
            get
            {
                return _point_pressed_index;
            }
            private set
            {
                if (_point_pressed_index != value)
                {
                    _point_pressed_index = value;
                    eventPositionChanged?.Invoke(this, null);
                }
            }
        }

        private bool first_four_bits = true;

        

        /// <summary>
        /// Gets or sets the bytes to be shown
        /// </summary>
        public byte[] Bytes
        {
            get { return (byte[])GetValue(BytesProperty); }
            set
            {
                SetValue(BytesProperty, value);
                InvalidateHexView();
                eventBytesLengthChanged?.Invoke(this, null);
            }
        }


        private void tracking_index_to_selected_index()
        {
            double track_min = Math.Min(tracking_from_index, tracking_to_index);
            double track_max = Math.Max(tracking_from_index, tracking_to_index);

            if ((Math.Abs(track_min - track_max) < double.Epsilon) && (track_min - (int)track_min) > double.Epsilon)
            {
                return;
            }
            if (track_max < 0 || track_min > Bytes.Length)
            {
                return;
            }
            int from = (int)(track_min + 0.5f);
            int to = (int)track_max;
            this.set_selected_index(from, to, false);   
        }


        /// <summary>
        /// Stores the blocksize of a single letter
        /// </summary>
        private Vector2 letterSize;


        public static readonly DependencyProperty BytesProperty =
            DependencyProperty.Register("Bytes", typeof(byte[]), typeof(HexViewControl), new PropertyMetadata(new byte[] { }));



        public HexViewControl()
        {
            InitializeComponent();
            if (config == null)
            {
                config = new HexViewControlConfig();
            }
        }

        public void InvalidateHexView()
        {
            this.mainContainer.Invalidate();
        }

        /// <summary>
        /// Updates the scrollbar properties, reflecting the latest screensizes and content sizes. 
        /// Drawing has to be called before
        /// </summary>
        private void UpdateScrollbarProperties()
        {
            var bytes = Bytes;
            if (bytes == null || bytes.Length == 0)
            {
                this.scrollBar.Visibility = Visibility.Collapsed;
                return;
            }
            int row_count_in_screen = (int)((mainContainer.ActualHeight - (double)height_header) / (double)height_line);
            int total_row = (Bytes.Length + 15) / 16;

            if (total_row <= row_count_in_screen)
            {
                this.scrollBar.Visibility = Visibility.Collapsed;
                return;
            }
            this.scrollBar.Visibility = Visibility.Visible;

            

            
            this.scrollBar.Minimum = 0;
            this.scrollBar.Maximum = total_row - (row_count_in_screen - 1);
            this.scrollBar.LargeChange = 16;
            this.scrollBar.SmallChange = 1;
            this.scrollBar.ViewportSize = row_count_in_screen;
        }


        private void mainContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.InvalidateHexView();
        }

        private void scrollBar_Scroll(object sender, ScrollEventArgs e)
        {            
            this.InvalidateHexView();
        }


        private void DrawStroke(CanvasDrawingSession ds, StrokeColor strokeColor, double left, double top, double width, double height)
        {
            double line_width = 1.0;
            double right = left + width - line_width ;
            double bottom = top + height - line_width ;
            CanvasStrokeStyle style = new CanvasStrokeStyle()
            {
                StartCap = CanvasCapStyle.Square,
                EndCap = CanvasCapStyle.Square,
            };
            if (strokeColor.left.HasValue)
            {
                ds.DrawLine((float)left, (float)top, (float)left, (float)bottom + (strokeColor.leftExtendBotton ? 1 : 0), strokeColor.left.Value, (float)line_width, style);
            }
            if (strokeColor.top.HasValue)
            {
                ds.DrawLine((float)left, (float)top, (float)right, (float)top, strokeColor.top.Value, (float)line_width, style);
            }
            if (strokeColor.right.HasValue)
            {
                ds.DrawLine((float)right, (float)top - (strokeColor.rightExtendUp ? 1 : 0), (float)right, (float)bottom, strokeColor.right.Value, (float)line_width, style);
            }
            if (strokeColor.bottom.HasValue)
            {
                ds.DrawLine((float)left, (float)bottom, (float)right, (float)bottom, strokeColor.bottom.Value, (float)line_width, style);
            }
        }

        private Color RandomColor()
        {
            Random random = new Random();
            return Color.FromArgb(0xFF, (Byte)random.Next(0, 255), (Byte)random.Next(0, 255), (Byte)random.Next(0, 255));
        }

        private void DrawTextAtCenter(CanvasDrawingSession ds, 
            string s, 
            Color textColor,             
            Color? backgroundColor, 
            bool backgroundExtendRight, 
            bool backgroundExtendBotton, 
            bool backgroundExtendRightBottom,
            CanvasTextFormat format, Vector2 letter_size, double left, double top, double width, double height)
        {
            if (backgroundColor.HasValue)
            {
                ds.FillRectangle(new Rect(left,
                    top, 
                    width - 2,
                    height -2
                    ),
                    backgroundColor.Value);

                if (backgroundExtendRight)
                {
                    ds.FillRectangle(new Rect(left + width - 2,
                                        top,
                                        2,
                                        height - 2
                                        ),
                                        backgroundColor.Value);
                }
                if (backgroundExtendBotton)
                {
                    ds.FillRectangle(new Rect(left,
                                        top + height - 2,
                                        width - 2,
                                        2
                                        ),
                                        backgroundColor.Value);
                }
                if (backgroundExtendRightBottom)
                {
                    ds.FillRectangle(new Rect(left + width - 2,
                                        top + height - 2,
                                        2,
                                        2
                                        ),
                                        backgroundColor.Value);
                }
            }
            double left_margin = (width - letterSize.X * s.Length) / 2;
            double top_margin = (height - letterSize.Y) / 2;
            ds.DrawText(s, new Vector2((float)(left + left_margin), (float)(top + top_margin)), textColor, format);
        }
     
        /// <summary>
        /// Called, when drawing is requestes
        /// </summary>
        /// <param name="sender">Sender calling this method</param>
        /// <param name="args">The arguments for drawing</param>
        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (config == null)
            {
                config = new HexViewControlConfig();
            }
            if (drawColor == null)
            {
                drawColor = new DrawColor(config.GetThemeResourceColor);
            }
            var ds = args.DrawingSession;
            ds.Antialiasing = CanvasAntialiasing.Aliased;
            var format = new CanvasTextFormat { FontSize = 16, WordWrapping = CanvasWordWrapping.NoWrap, FontFamily = "Consolas" };
            var format_bold = new CanvasTextFormat { FontSize = 16, WordWrapping = CanvasWordWrapping.NoWrap, FontFamily = "Consolas", FontWeight = new FontWeight() { Weight = 600 } };
            var textLayout = new CanvasTextLayout(ds, "W", format, 0.0f, 0.0f);

            this.letterSize =
                new Vector2(
                    Convert.ToSingle(textLayout.LayoutBounds.Width),
                    Convert.ToSingle(textLayout.LayoutBounds.Height));



            bool isFinded = finded_selection;


            Action<IEnumerable<byte>, double, bool, string, bool, Func<int, bool>, bool, Int64> draw_line_text_action =
                (IEnumerable<byte> bytes, 
                double top_offset, 
                bool is_header,
                string left_offset_desc, 
                bool draw_ascii, 
                Func<int, bool> isHilighted,
                bool select_point_is_hex_content, 
                Int64 firstByteIndex) =>
            {
                // left offset
                if (is_header == false)
                {
                    DrawTextAtCenter(ds,
                        left_offset_desc,
                        isHilighted(-1) ? drawColor.offset_text_hilighted : drawColor.offset_text,
                        null,
                        false,
                        false,
                        false,
                        isHilighted(-1) ? format_bold : format,
                        this.letterSize,
                        0,
                        top_offset,
                        width_offset,
                        height_line
                        );
                }

                // hex
                int byte_index = 0;
                foreach (byte b in bytes)
                {
                    double distance_four_bytes_total = byte_index / 4 * distance_four_bytes;
                    double offset_x = width_offset + width_distance_offset_and_hex + byte_index * width_hex_00 + distance_four_bytes_total;
                    bool is_hilighted = isHilighted(byte_index);
                    if (is_header)
                    {
                        DrawTextAtCenter(ds,
                            format_byte(b),
                            is_hilighted ? drawColor.header_text_hilighted : drawColor.header_text,
                            null,
                            false,
                            false,
                            false,
                            is_hilighted ? format_bold : format,
                            this.letterSize,
                            offset_x,
                            top_offset,
                            width_hex_00,
                            height_line);
                    }
                    else
                    {
                        (FillColor fillColor, StrokeColor normalStrokeColor) = config.GetNormalColorForIndex(byte_index + firstByteIndex);
                        

                        DrawTextAtCenter(ds,
                            format_byte(b),
                            is_hilighted ? (select_point_is_hex_content ? drawColor.hex_ascii_text_point_selected : drawColor.hex_ascii_text_program_selected) : ((byte_index % 2 == 0) ? drawColor.hex_ascii_text : drawColor.hex_ascii_text_odd),
                            is_hilighted 
                                ?  (isFinded 
                                        ? drawColor.hex_ascii_text_finded_selected_background 
                                        : (select_point_is_hex_content 
                                                ? drawColor.hex_ascii_text_point_selected_background 
                                                : drawColor.hex_ascii_text_program_selected_background)) 
                                : fillColor.color,
                            fillColor.backgroundExtendRight,
                            fillColor.backgroundExtendBotton,
                            fillColor.backgroundExtendRightBottom,
                            format,
                            this.letterSize,
                            offset_x,
                            top_offset,
                            width_hex_00,
                            height_line);

                        DrawStroke(ds,
                            normalStrokeColor,
                            offset_x,
                            top_offset,
                            width_hex_00,
                            height_line);
                    }
                    byte_index++;
                }

                // ascii                
                byte_index = 0;

                foreach (byte b in bytes)
                {
                    bool is_hilighted = isHilighted(byte_index);
                    if (is_header)
                    {
                        DrawTextAtCenter(ds,
                            b.ToString("X"),
                            is_hilighted ? drawColor.header_text_hilighted : drawColor.header_text,
                            null,
                            false,
                            false,
                            false,
                            is_hilighted ? format_bold : format,
                            this.letterSize,
                            ascii_offset_begin + byte_index * width_preview_char,
                            top_offset,
                            width_preview_char,
                            height_line);
                    }
                    else
                    {
                        DrawTextAtCenter(ds,
                            ConvertToString(b),
                            is_hilighted ? (select_point_is_hex_content ? drawColor.hex_ascii_text_program_selected : drawColor.hex_ascii_text_point_selected) : drawColor.hex_ascii_text,
                            is_hilighted 
                                ? (isFinded 
                                        ? drawColor.hex_ascii_text_finded_selected_background_light
                                        : (select_point_is_hex_content 
                                                ? drawColor.hex_ascii_text_program_selected_background 
                                                : drawColor.hex_ascii_text_point_selected_background))
                                : transparent_color,
                            true,
                            true,
                            true,
                            format,
                            this.letterSize,
                            ascii_offset_begin + byte_index * width_preview_char,
                            top_offset,
                            width_preview_char,
                            height_line);
                    }
                    byte_index++;
                }
            };

            
            // draw header
            draw_line_text_action(new List<Byte> {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F },
                0,
                true,
                null,
                false,
                (int byte_index) =>
                {
                    if (point_pressed_index == -1)
                    {
                        return false;
                    }
                    return (point_pressed_index & 0xF) == byte_index;
                },
                false,
                0
                );

            // draw each line
            int skip_count = (int)((int)scrollBar.Value * 16);
            int max_row_count = (int)((mainContainer.ActualHeight - (double)height_header) / (double)height_line);
            byte[] bytes_line = new byte[16];
            max_row_count = Math.Max(0, max_row_count);
            foreach (int row_index in Enumerable.Range(0, max_row_count))
            {
                IEnumerable<byte> current_line_bytes = new List<byte>();
                int take_count = Math.Min(Bytes.Length - skip_count, 16);
                if (take_count > 0)
                {
                    Array.Copy(Bytes, skip_count, bytes_line, 0, take_count);
                    current_line_bytes = bytes_line.Take(take_count);
                }
                
                draw_line_text_action(current_line_bytes,
                    height_header + row_index * height_line,
                    false,
                    "0x" + skip_count.ToString("X8"),
                    true,
                    (int byte_index) =>
                    {
                        if (byte_index == -1)
                        {
                            return (point_pressed_index >= skip_count) && (point_pressed_index < skip_count + 16);
                        }
                        return (skip_count + byte_index >= selected_from_index) && (skip_count + byte_index <= selected_to_index);
                    },
                    tracking_in_hex,
                    skip_count
                    );
                if (current_line_bytes.Count() < 16)
                {
                    //break;
                }
                skip_count += 16;
            }

            this.UpdateScrollbarProperties();
        }

        private Rect HitRectHex()
        {
            int row_count = (int)((mainContainer.ActualHeight - (double)height_header) / (double)height_line);

            double left_top_x = width_offset + width_distance_offset_and_hex;
            double left_top_y = height_header;
            double right_bottom_x = left_top_x + (16 * width_hex_00 + distance_four_bytes * 3);
            double right_bottom_y = left_top_y + row_count * height_line;
            return new Rect(left_top_x, left_top_y, right_bottom_x - left_top_x, right_bottom_y - left_top_y);

        }

        private Rect HitRectAscii()
        {
            int row_count = (int)((mainContainer.ActualHeight - (double)height_header) / (double)height_line);

            double left_top_x = ascii_offset_begin;
            double left_top_y = height_header;
            double right_bottom_x = left_top_x + (16 * width_preview_char);
            double right_bottom_y = left_top_y + row_count * height_line;
            return new Rect(left_top_x, left_top_y, right_bottom_x - left_top_x, right_bottom_y - left_top_y);
        }

        public void SetMarkPosition(int pos, int len, bool finded)
        {
            set_selected_index(pos, pos + len - 1, finded);
            (int from, int to) = VisiableIndex();
            if (selected_from_index >= from && selected_to_index <= to)
            {
                // in range, do not need to scroll
            }
            else
            {
                // to better to v-align to center
                int topRowIndex = selected_from_index / 16;
                int bottomRowIndex = (selected_to_index + 15) / 16;
                int selected_row_count = bottomRowIndex - topRowIndex + 1;
                int row_count_in_screen = (int)((mainContainer.ActualHeight - (double)height_header) / (double)height_line);
                if (selected_row_count < row_count_in_screen)
                {
                    // v-align to center
                    scrollBar.Value = topRowIndex - ((row_count_in_screen - selected_row_count) / 2);
                }
                else
                {
                    // scroll to top rowindex
                    scrollBar.Value = topRowIndex;
                }                
            }
            
            this.InvalidateHexView();
        }

        // -0.5, max+0.5
        private double Point2VirtualIndex(double x, double y, bool hexArea)
        {
            int row_count = (int)((mainContainer.ActualHeight - (double)height_header) / (double)height_line);

            Rect rect = hexArea ? HitRectHex() : HitRectAscii();
            double left_top_x = (double)rect.X;
            double left_top_y = (double)rect.Y;
            double right_bottom_x = (double)rect.Right;
            double right_bottom_y = (double)rect.Bottom;

            if (y < left_top_y)
            {
                return -0.5f;
            }
            else if (left_top_y <= y && y <= right_bottom_y)
            {
                int row_index = (int)((y - left_top_y) / height_line);
                if (x < left_top_x)
                {
                    return row_index * 16 - 0.5f;
                }
                else if (left_top_x <=x && x <= right_bottom_x)
                {
                    if (hexArea)
                    {
                        double[] width_grow =
                        {
                            width_hex_00,
                            width_hex_00 * 2,
                            width_hex_00 * 3,
                            width_hex_00 * 4,

                            width_hex_00 * 4 + distance_four_bytes,

                            width_hex_00 * 5 + distance_four_bytes,
                            width_hex_00 * 6 + distance_four_bytes,
                            width_hex_00 * 7 + distance_four_bytes,
                            width_hex_00 * 8 + distance_four_bytes,

                            width_hex_00 * 8 + distance_four_bytes * 2,

                            width_hex_00 * 9 + distance_four_bytes * 2,
                            width_hex_00 * 10 + distance_four_bytes * 2,
                            width_hex_00 * 11 + distance_four_bytes * 2,
                            width_hex_00 * 12 + distance_four_bytes * 2,

                            width_hex_00 * 12 + distance_four_bytes * 3,

                            width_hex_00 * 13 + distance_four_bytes * 3,
                            width_hex_00 * 14 + distance_four_bytes * 3,
                            width_hex_00 * 15 + distance_four_bytes * 3,
                            width_hex_00 * 16 + distance_four_bytes * 3,
                        };
                        double[] index_grow =
                        {
                            0f, 1f, 2f, 3f,
                            3.5f, 
                            4f, 5f, 6f, 7f, 
                            7.5f,
                            8f, 9f, 10f, 11f,
                            11.5f,
                            12f, 13f, 14f, 15f
                        };
                        double distance = x - left_top_x;
                        double dis_index = 0;
                        for (int i = 0; i < width_grow.Length; i ++)
                        {
                            if (distance < width_grow[i])
                            {
                                dis_index = index_grow[i];
                                break;
                            }
                        }
                        return row_index * 16 + dis_index;
                    }
                    else
                    {
                        return row_index * 16 + (int)((x - left_top_x) / width_preview_char);
                    }
                }
                else
                {
                    return (row_index + 1) * 16 - 0.5f;
                }
            }
            else
            {
                return 16 * row_count - 0.5f;
            }
        }

        private double Point2RealIndex(double x, double y, bool hexArea)
        {
            double virtualIndex = Point2VirtualIndex(x, y, hexArea);
            (int visiableFrom, int visiableTo) = VisiableIndex();
            if (visiableFrom == -1)
            {
                // nothing visiable
                return (double)Bytes.Count() - 0.5f;
            }
            else
            {
                double index = virtualIndex + visiableFrom;
                if (index > visiableTo)
                {
                    return visiableTo + 0.5f;
                }
                return index;
            }
        }

        private (int, int) VisiableIndex()
        {
            int row_count_in_screen = (int)((mainContainer.ActualHeight - (double)height_header) / (double)height_line);
            int begin = (int)scrollBar.Value * 16;
            int end = Math.Min(begin + row_count_in_screen * 16 - 1, Bytes.Count() - 1);
            return (begin, end);
        }



        private string format_byte(byte currentByte)
        {
            var big = (currentByte & 0xF0) >> 4;
            var small = (currentByte & 0x0F);

            var bigLetter = hexLettersBig[big];
            var smallLetter = hexLettersBig[small];
            var text = $"{bigLetter}{smallLetter}";
            return text;
        }


        private static Encoding ISO8859 = System.Text.Encoding.GetEncoding("iso-8859-1");

        /// <summary>
        /// Converts the given byte to an ASCII character
        /// </summary>
        /// <param name="value">Value to be converted</param>
        /// <returns>Converted character</returns>
        private static string ConvertToString(byte value)
        {
           
            if (value >= 0 && value <= 31)
            {
                return ".";
            }
            if (value == 0x7F)
            {
                return ".";
            }
            if (value >= 0x80 && value <= 0x9F)
            {
                return ".";
            }
            char[] c = ISO8859.GetChars(new byte[] { value });
            string s = new string(c);
            if (s.Length == 0)
            {
                s = ".";
            }
            else if (s.Length > 1)
            {
                s = s.Substring(0, 1);
            }
            return s;            
        }

        private void mainContainer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (tracking)
            {
                var point = e.GetCurrentPoint(this.mainContainer);
                tracking_to_index = Point2RealIndex((double)point.Position.X, (double)point.Position.Y, tracking_in_hex);
                tracking_index_to_selected_index();
                this.InvalidateHexView();
            }
        }

        private void mainContainer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            PointerPoint ptrPt = e.GetCurrentPoint(this);
            if (ptrPt.PointerDevice.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse)
            {                
                mainContainer.ReleasePointerCapture(e.Pointer);
            }


            tracking = false;
        }

        private void mainContainer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            eventPoinPressed?.Invoke(this, null);
            this.first_four_bits = true;
            PointerPoint point = e.GetCurrentPoint(this.mainContainer);

            bool shift_pressed = e.KeyModifiers == Windows.System.VirtualKeyModifiers.Shift;
            bool left_button = point.Properties.IsLeftButtonPressed;
            bool right_button = point.Properties.IsRightButtonPressed;

            if (left_button)
            {
                tracking = true;
                if (tracking_from_index != -1 && shift_pressed)
                {
                    tracking_to_index = Point2RealIndex((double)point.Position.X, (double)point.Position.Y, tracking_in_hex);
                    tracking_index_to_selected_index();
                }
                else
                {
                    tracking_in_hex = point.Position.X <= HitRectHex().Right;

                    tracking_from_index = Point2RealIndex((double)point.Position.X, (double)point.Position.Y, tracking_in_hex);
                    tracking_to_index = tracking_from_index;
                    tracking_index_to_selected_index();

                    point_pressed_index = selected_from_index;

                    if (Math.Abs(tracking_from_index - (int)tracking_from_index) < double.Epsilon)
                    {
                        point_pressed_index = (int)tracking_from_index;
                    }
                    else
                    {
                        point_pressed_index = -1;
                    }
                }

                this.mainContainer.CapturePointer(e.Pointer);
            }
            else if (right_button)
            {
                return;
                // select all
                // copy
                // fill with 00
                // delete

                bool menu_in_selection = false;


                bool hex_area = PointInRect(point.Position, HitRectHex());
                bool ascii_area = PointInRect(point.Position, HitRectAscii());
                if (hex_area || ascii_area)
                {
                    double right_tracking_index = Point2RealIndex((double)point.Position.X, (double)point.Position.Y, hex_area);

                    if (right_tracking_index >= selected_from_index && right_tracking_index <= selected_to_index)
                    {
                        menu_in_selection = true;
                    }
                    else if (right_tracking_index >= 0 && right_tracking_index < Bytes.Count() && (Math.Abs(right_tracking_index - (int)right_tracking_index) < double.Epsilon))
                    {
                        this.set_selected_index((int)right_tracking_index, (int)right_tracking_index, false);
                        point_pressed_index = (int)right_tracking_index;
                        menu_in_selection = true;
                    }
                }


                MenuFlyout menuFlyout = BuildMenuFlyout(menu_in_selection);

                this.InvalidateHexView();
                menuFlyout.ShowAt(mainContainer, point.Position);
            }

            this.InvalidateHexView();

        }

        private MenuFlyout BuildMenuFlyout(bool menu_in_selection)
        {
            MenuFlyout menuFlyout = new MenuFlyout();


            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.IsEnabled = menu_in_selection;
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Copy };
                menuItem.Text = "Copy Hex";
                menuItem.Click += Context_menu_copy_hex;
                menuFlyout.Items.Add(menuItem);
            }

            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.IsEnabled = menu_in_selection;
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.FontColor };
                menuItem.Text = "Copy ASCII";
                menuItem.Click += Context_menu_copy_ascii;
                menuFlyout.Items.Add(menuItem);
            }

            {
                MenuFlyoutSubItem menuSubItem = new MenuFlyoutSubItem();
                menuSubItem.IsEnabled = menu_in_selection;
                // menuSubItem.Icon = new SymbolIcon() { Symbol = Symbol.Copy };
                menuSubItem.Text = "Copy Code";

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "C";
                    menuItem.Click += Context_menu_copy_code_c;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "C#";
                    menuItem.Click += Context_menu_copy_code_c_sharp;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Java";
                    menuItem.Click += Context_menu_copy_code_java;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Javascript";
                    menuItem.Click += Context_menu_copy_code_javascript;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Python";
                    menuItem.Click += Context_menu_copy_code_python;
                    menuSubItem.Items.Add(menuItem);
                }
                menuFlyout.Items.Add(menuSubItem);
            }

            if (is_hide_editable_menu == false)
            {

                menuFlyout.Items.Add(new MenuFlyoutSeparator());
                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Find };
                    menuItem.IsEnabled = true;
                    menuItem.Text = "Find";
                    menuItem.Click += Context_menu_find;
                    menuFlyout.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Go };
                    menuItem.IsEnabled = true;
                    menuItem.Text = "Jump ...";
                    menuItem.Click += Context_menu_jump;
                    menuFlyout.Items.Add(menuItem);
                }

                menuFlyout.Items.Add(new MenuFlyoutSeparator());
                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    //menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Previous };
                    menuItem.IsEnabled = (is_readonly == false);
                    menuItem.Text = "Add bytes to the head ...";
                    menuItem.Click += Context_menu_add_head;
                    menuFlyout.Items.Add(menuItem);
                }
                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    //menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Next };
                    menuItem.IsEnabled = (is_readonly == false);
                    menuItem.Text = "Add bytes to the tail ...";
                    menuItem.Click += Context_menu_add_tail;
                    menuFlyout.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.IsEnabled = menu_in_selection && (is_readonly == false);
                    menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Previous };
                    menuItem.Text = "Insert bytes before selection ...";
                    menuItem.Click += Context_menu_insert_before;
                    menuFlyout.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.IsEnabled = menu_in_selection && (is_readonly == false);
                    menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Next };
                    menuItem.Text = "Insert bytes after selection ...";
                    menuItem.Click += Context_menu_insert_after;
                    menuFlyout.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.IsEnabled = menu_in_selection && (is_readonly == false);
                    menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Edit };
                    menuItem.Text = "Fill selection with byte ...";
                    menuItem.Click += Context_menu_fill_with_00;
                    menuFlyout.Items.Add(menuItem);
                }

                menuFlyout.Items.Add(new MenuFlyoutSeparator());

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.IsEnabled = menu_in_selection && (is_readonly == false);
                    menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Delete };
                    menuItem.Text = "Delete selection";
                    menuItem.Click += Context_menu_delete;
                    menuFlyout.Items.Add(menuItem);
                }

                menuFlyout.Items.Add(new MenuFlyoutSeparator());
            }


            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.SelectAll };
                menuItem.Text = "Select all";
                menuItem.Click += Context_menu_select_all;
                menuFlyout.Items.Add(menuItem);
            }
            return menuFlyout;
        }

        internal void ScrollPosToTop(long offset)
        {
            scrollBar.Value = offset / 16;
            InvalidateHexView();
        }

        private async Task<(T, bool)> InputText<T>(string title, string default_value, Func<string, (string, T)> is_good_input)
        {
            StackPanel panel = new StackPanel();
            TextBox textBox = new TextBox();
            textBox.Margin = new Thickness(0, 0, 0, 10);
            textBox.Text = default_value;
            textBox.SelectAll();
            panel.Children.Add(textBox);

            TextBlock errorText = new TextBlock();
            errorText.TextAlignment = TextAlignment.Right;
            errorText.FontSize = 10;
            errorText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
            panel.Children.Add(errorText);

            ContentDialog content_dialog = new ContentDialog()
            {
                Title = title,
                Content = panel,
                PrimaryButtonText = "Cancel",
                SecondaryButtonText = "OK",
            };

            T t = default(T);
            bool canceled = true;
            content_dialog.DefaultButton = ContentDialogButton.Secondary;
            content_dialog.SecondaryButtonClick += (_s, _e) =>
            {
                (string error_msg, T r) = is_good_input(textBox.Text);
                if (String.IsNullOrEmpty(error_msg) == false)
                {
                    errorText.Text = error_msg;
                    _e.Cancel = true;
                    return;
                }
                canceled = false;
                t = r;                
            };
            await content_dialog.ShowAsync();
            return (t, canceled);
        }


        private async Task insert_new_bytes(int first_part_count)
        {
            if (is_readonly)
            {
                return;
            }
            int default_value = 4;
            if (config != null)
            {
                default_value = config.GetInsertLength();
            }
            (int length, bool canceled) = await InputText("How many bytes to insert:",
                default_value.ToString(),
                s =>
            {
                int i = 0;
                int.TryParse(s, out i);
                return (i <= 0 ? "Invalided number." : "", i);
            });

            if (canceled)
            {
                return;
            }
            if (config != null)
            {
                config.SetInsertLength(length);
            }
            insert_new_bytes(first_part_count, length);
        }

        private void Context_menu_jump(object sender, RoutedEventArgs e)
        {
            eventMenuJump?.Invoke(this, null);
        }
        private void Context_menu_find(object sender, RoutedEventArgs e)
        {
            eventMenuFind?.Invoke(this, null);
        }

        private async void Context_menu_add_head(object sender, RoutedEventArgs e)
        {
            await insert_new_bytes(0);
        }

        private async void Context_menu_add_tail(object sender, RoutedEventArgs e)
        {
            await insert_new_bytes(Bytes.Length);
        }

        private async void Context_menu_insert_before(object sender, RoutedEventArgs e)
        {
            await insert_new_bytes(selected_from_index);            
        }
        private async void Context_menu_insert_after(object sender, RoutedEventArgs e)
        {
            await insert_new_bytes(selected_to_index + 1);
        }

        private void insert_new_bytes(int first_part_count, int new_bytes_count)
        {
            int first_part_len = first_part_count;            
            int third_part_len = Bytes.Length - first_part_len;

            byte[] new_bytes = new byte[first_part_len + new_bytes_count + third_part_len];




            Array.Copy(Bytes, 0, new_bytes, 0, first_part_len);
            Array.Copy(Bytes, first_part_len, new_bytes, first_part_len + new_bytes_count, third_part_len);

            tracking_from_index = point_pressed_index = first_part_count;
            tracking_to_index = first_part_count + new_bytes_count - 1;

            this.set_selected_index(point_pressed_index, (int)tracking_to_index, false);
            this.Bytes = new_bytes;

            eventBytesContentChanged?.Invoke(this, null);
        }

     
        private async System.Threading.Tasks.Task<int> copy_large_check_continue_length()
        {
            if (selected_from_index <= selected_to_index && selected_from_index >= 0 && selected_to_index < Bytes.Length)
            {
                if (selected_to_index - selected_from_index + 1 > 50 * 1024)
                {
                    MessageDialog message_dialog = new MessageDialog("Selection length is too large, only first 50KB will be copyed.", "Too large");
                    message_dialog.Commands.Add(new UICommand("Continue", cmd => { }, "Continue"));
                    message_dialog.Commands.Add(new UICommand("Cancel", cmd => { }));
                    message_dialog.DefaultCommandIndex = 0;
                    message_dialog.CancelCommandIndex = 1;
                    IUICommand result = await message_dialog.ShowAsync();
                    if (result.Id as string == "Cancel")
                    {
                        return 0;
                    }
                }
                return Math.Min(selected_to_index - selected_from_index + 1, 50 * 1024);
            }
            else
            {
                return 0;
            }
        }

        private async void Context_menu_copy_hex(object sender, RoutedEventArgs e)
        {
            int length = await copy_large_check_continue_length();
            if (length <= 0)
            {
                return;
            }
            string content = String.Join(" ", CutByte(selected_from_index, length).Select(b => format_byte(b)));
           
            DataPackage dp = new DataPackage();
            dp.SetText(content);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        private async void Context_menu_copy_ascii(object sender, RoutedEventArgs e)
        {
            int length = await copy_large_check_continue_length();
            if (length <= 0)
            {
                return;
            }
            string content = String.Join("", CutByte(selected_from_index, length).Select(b => ConvertToString(b)));

            DataPackage dp = new DataPackage();
            dp.SetText(content);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        private async void Context_menu_copy_code_c(object sender, RoutedEventArgs e)
        {
            int length = await copy_large_check_continue_length();
            if (length <= 0)
            {
                return;
            }
            IEnumerable<string> cCodeLines = SpliteSourceByPageSize(CutByte(selected_from_index, length).Select(b => "0x" + format_byte(b)), 16).Select(lst => String.Join(", ", lst));
            string content = String.Join(",\n    ", cCodeLines);             
            string final = "unsigned char some_data[" + length.ToString() + "] = \n{\n    " + content + "\n};";
            DataPackage dp = new DataPackage();
            dp.SetText(final);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        private byte[] CutByte(int skip_count, int take_count)
        {
            byte[] r = new byte[take_count];
            Array.Copy(Bytes, skip_count, r, 0, take_count);
            return r;
        }

        private async void Context_menu_copy_code_c_sharp(object sender, RoutedEventArgs e)
        {
            int length = await copy_large_check_continue_length();
            if (length <= 0)
            {
                return;
            }
            IEnumerable<string> cCodeLines = SpliteSourceByPageSize(CutByte(selected_from_index, length).Select(b => "0x" + format_byte(b)), 16).Select(lst => String.Join(", ", lst));
            string content = String.Join(",\n    ", cCodeLines);
            string final = "byte[] some_data = new byte[" + length.ToString() + "] \n{\n    " + content + "\n};";
            DataPackage dp = new DataPackage();
            dp.SetText(final);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        private async void Context_menu_copy_code_java(object sender, RoutedEventArgs e)
        {
            int length = await copy_large_check_continue_length();
            if (length <= 0)
            {
                return;
            }
            IEnumerable<string> cCodeLines = SpliteSourceByPageSize(CutByte(selected_from_index, length).Select(b => "0x" + format_byte(b)), 16).Select(lst => String.Join(", ", lst));
            string content = String.Join(",\n    ", cCodeLines);
            string final = "byte[] some_data = \n{\n    " + content + "\n};";
            DataPackage dp = new DataPackage();
            dp.SetText(final);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        private async void Context_menu_copy_code_javascript(object sender, RoutedEventArgs e)
        {
            int length = await copy_large_check_continue_length();
            if (length <= 0)
            {
                return;
            }
            IEnumerable<string> cCodeLines = SpliteSourceByPageSize(CutByte(selected_from_index, length).Select(b => "0x" + format_byte(b)), 16).Select(lst => String.Join(", ", lst));
            string content = String.Join(",\n    ", cCodeLines);
            string final = "var some_data = new Array(\n    " + content + "\n);";
            DataPackage dp = new DataPackage();
            dp.SetText(final);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        private async void Context_menu_copy_code_python(object sender, RoutedEventArgs e)
        {
            int length = await copy_large_check_continue_length();
            if (length <= 0)
            {
                return;
            }
            IEnumerable<string> cCodeLines = SpliteSourceByPageSize(CutByte(selected_from_index, length).Select(b => "0x" + format_byte(b)), 16).Select(lst => String.Join(", ", lst));
            string content = String.Join(",\n    ", cCodeLines);
            string final = "some_data = \\\n{\n    " + content + "\n};";
            DataPackage dp = new DataPackage();
            dp.SetText(final);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        public bool has_selection()
        {
            bool r = (selected_from_index <= selected_to_index && selected_from_index >= 0 && selected_to_index < Bytes.Length);
            return r;
        }

        private async Task FillWithInputByte()
        {
            if (is_readonly)
            {
                return;
            }
            string defalut_value = "00";
            if (config != null)
            {
                defalut_value = config.GetFillByte();
            }
            (byte b, bool canceled) = await InputText("Which byte(00-FF) do you want to fill?",
                defalut_value,
                s =>
            {
                string ss = s.ToLower();
                if (ss.StartsWith("0x"))
                {
                    ss = ss.Substring(2);
                }
                byte i = 0;
                bool parse_result = byte.TryParse(ss, NumberStyles.HexNumber, null, out i);
                return (parse_result == false ? "Invalided byte." : "", i);
            });
            if (canceled)
            {
                return;
            }
            if (config != null)
            {
                config.SetFillByte(b.ToString("X"));
            }
            foreach (int i in Enumerable.Range(selected_from_index, selected_to_index - selected_from_index + 1))
            {
                Bytes[i] = b;
            }            
            this.InvalidateHexView();

            eventBytesContentChanged?.Invoke(this, null);
        }

        private async void Context_menu_fill_with_00(object sender, RoutedEventArgs e)
        {
            if (has_selection())
            {
                await FillWithInputByte();
            }
        }


        private void Delete_selection()
        {
            if (is_readonly)
            {
                return;
            }
            if (has_selection())
            {
                int first_part_len = selected_from_index;
                int deleted_part_len = selected_to_index - selected_from_index + 1;
                int third_part_len = Bytes.Length - first_part_len - deleted_part_len;

                byte[] new_bytes = new byte[first_part_len + third_part_len];



                Array.Copy(Bytes, 0, new_bytes, 0, first_part_len);
                Array.Copy(Bytes, first_part_len + deleted_part_len, new_bytes, first_part_len, third_part_len);


                point_pressed_index = -1;
                this.set_selected_index(-1, -1, false);
                
                tracking_from_index = tracking_to_index = -1;

                this.Bytes = new_bytes;

                eventBytesContentChanged?.Invoke(this, null);
            }
        }

        private void Context_menu_delete(object sender, RoutedEventArgs e)
        {
            Delete_selection();
        }

        

        private void Context_menu_select_all(object sender, RoutedEventArgs e)
        {
            if (Bytes.Count() > 0)
            {
                this.set_selected_index(0, Bytes.Count() - 1, false);
                
                this.InvalidateHexView();
            }
        }
        private bool PointInRect(Windows.Foundation.Point pt, Rect rect)
        {
            if (pt.X >= rect.Left && pt.X <= rect.Right && pt.Y >= rect.Top && pt.Y <= rect.Bottom)
            {
                return true;
            }
            return false;
        }

        private void mainContainer_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void mainContainer_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void mainContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void mainContainer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            
        }

        public void OnEventThemeChanged(object sender, object arg)
        {
            drawColor = null;
            this.InvalidateHexView();
        }

        public void On_ThemeChanged()
        {
            drawColor = null;
            this.InvalidateHexView();
        }

        private void mainContainer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (scrollBar.Visibility == Visibility.Collapsed)
            {
                return;
            }
            PointerPoint pt = e.GetCurrentPoint(sender as UIElement);
            int delta = pt.Properties.MouseWheelDelta;

            if (pt.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                scrollBar.Value += Math.Sign(delta);
            }
            else
            {
                scrollBar.Value -= Math.Sign(delta);
            }
            InvalidateHexView();
        }

        private static List<List<T>> SpliteSourceByPageSize<T>(IEnumerable<T> source, int pageSize)
        {
            List<List<T>> pages = new List<List<T>>();
            int taken_count = 0;
            while (taken_count < source.Count())
            {
                List<T> page = source.Skip(taken_count).Take(pageSize).ToList();
                taken_count += page.Count();
                pages.Add(page);
            }
            return pages;
        }

        private int VirtualKey_to_0_F(Windows.System.VirtualKey key)
        {
            if (key == Windows.System.VirtualKey.Number0 || key == Windows.System.VirtualKey.NumberPad0)
            {
                return 0;
            }
            else if (key == Windows.System.VirtualKey.Number1 || key == Windows.System.VirtualKey.NumberPad1)
            {
                return 1;
            }
            else if (key == Windows.System.VirtualKey.Number2 || key == Windows.System.VirtualKey.NumberPad2)
            {
                return 2;
            }
            else if (key == Windows.System.VirtualKey.Number3 || key == Windows.System.VirtualKey.NumberPad3)
            {
                return 3;
            }
            else if (key == Windows.System.VirtualKey.Number4 || key == Windows.System.VirtualKey.NumberPad4)
            {
                return 4;
            }
            else if (key == Windows.System.VirtualKey.Number5 || key == Windows.System.VirtualKey.NumberPad5)
            {
                return 5;
            }
            else if (key == Windows.System.VirtualKey.Number6 || key == Windows.System.VirtualKey.NumberPad6)
            {
                return 6;

            }
            else if (key == Windows.System.VirtualKey.Number7 || key == Windows.System.VirtualKey.NumberPad7)
            {
                return 7;
            }
            else if (key == Windows.System.VirtualKey.Number8 || key == Windows.System.VirtualKey.NumberPad8)
            {
                return 8;
            }
            else if (key == Windows.System.VirtualKey.Number9 || key == Windows.System.VirtualKey.NumberPad9)
            {
                return 9;
            }
            else if (key == Windows.System.VirtualKey.A)
            {
                return 0xA;
            }
            else if (key == Windows.System.VirtualKey.B)
            {
                return 0xB;
            }
            else if (key == Windows.System.VirtualKey.C)
            {
                return 0xC;
            }
            else if (key == Windows.System.VirtualKey.D)
            {
                return 0xD;
            }
            else if (key == Windows.System.VirtualKey.E)
            {
                return 0xE;
            }
            else if (key == Windows.System.VirtualKey.F)
            {
                return 0xF;
            }
            return -1;
        }

        public void On_SelectAll()
        {
            if (Bytes.Count() > 0)
            {
                this.set_selected_index(0, Bytes.Count() - 1, false);
                this.InvalidateHexView();
            }
        }

        public void On_Delete()
        {
            Delete_selection();
        }
        public async Task On_Fill()
        {
            if (has_selection())
            {
                await FillWithInputByte();
            }
        }

        public MenuFlyout On_Create_Insert_Menu()
        {
            MenuFlyout menuFlyout = new MenuFlyout();
            bool exist_selection = has_selection();


            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                //menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Previous };
                menuItem.IsEnabled = (is_readonly == false);
                menuItem.Text = "Add bytes to the head ...";
                menuItem.Click += Context_menu_add_head;
                menuFlyout.Items.Add(menuItem);
            }
            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                //menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Next };
                menuItem.IsEnabled = (is_readonly == false);
                menuItem.Text = "Add bytes to the tail ...";
                menuItem.Click += Context_menu_add_tail;
                menuFlyout.Items.Add(menuItem);
            }

            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.IsEnabled = exist_selection && (is_readonly == false);
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Previous };
                menuItem.Text = "Insert bytes before selection ...";
                menuItem.Click += Context_menu_insert_before;
                menuFlyout.Items.Add(menuItem);
            }

            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.IsEnabled = exist_selection && (is_readonly == false);
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Next };
                menuItem.Text = "Insert bytes after selection ...";
                menuItem.Click += Context_menu_insert_after;
                menuFlyout.Items.Add(menuItem);
            }

            



            return menuFlyout;
        }
        public MenuFlyout On_Create_Copy_Menu()
        {            
            MenuFlyout menuFlyout = new MenuFlyout();
            bool exist_selection = has_selection();

            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.IsEnabled = exist_selection;
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.Copy };
                menuItem.Text = "Copy Hex";
                menuItem.Click += Context_menu_copy_hex;
                menuFlyout.Items.Add(menuItem);
            }

            {
                MenuFlyoutItem menuItem = new MenuFlyoutItem();
                menuItem.IsEnabled = exist_selection;
                menuItem.Icon = new SymbolIcon() { Symbol = Symbol.FontColor };
                menuItem.Text = "Copy ASCII";
                menuItem.Click += Context_menu_copy_ascii;
                menuFlyout.Items.Add(menuItem);
            }

            {
                MenuFlyoutSubItem menuSubItem = new MenuFlyoutSubItem();
                menuSubItem.IsEnabled = exist_selection;
                // menuSubItem.Icon = new SymbolIcon() { Symbol = Symbol.Copy };
                menuSubItem.Text = "Copy Code";

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "C";
                    menuItem.Click += Context_menu_copy_code_c;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "C#";
                    menuItem.Click += Context_menu_copy_code_c_sharp;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Java";
                    menuItem.Click += Context_menu_copy_code_java;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Javascript";
                    menuItem.Click += Context_menu_copy_code_javascript;
                    menuSubItem.Items.Add(menuItem);
                }

                {
                    MenuFlyoutItem menuItem = new MenuFlyoutItem();
                    menuItem.Text = "Python";
                    menuItem.Click += Context_menu_copy_code_python;
                    menuSubItem.Items.Add(menuItem);
                }
                menuFlyout.Items.Add(menuSubItem);
            }

            return menuFlyout;
        }

        public string SelectionRangeText()
        {
            if (has_selection())
            {
                return String.Format("0x{0:X8} - 0x{1:X8}", selected_from_index, selected_to_index);
            }
            return "---------- - ----------";
        }

        public string PositionText()
        {
            if (point_pressed_index >= 0 && point_pressed_index < Bytes.Length)
            {
                return String.Format("0x{0:X8}", point_pressed_index);
            }
            return "----------";
        }

        public string LengthText()
        {
            return String.Format("0x{0:X8}", Bytes.Length);            
        }
        public void On_PreviewKeyDown(KeyRoutedEventArgs e)
        {
            int n = 0;
            var ctrl_down = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control);
            if (ctrl_down == Windows.UI.Core.CoreVirtualKeyStates.Down && e.Key == Windows.System.VirtualKey.A)
            {
                // select all
                if (Bytes.Count() > 0)
                {
                    this.set_selected_index(0, Bytes.Count() - 1, false);
                    this.InvalidateHexView();
                }
            }            
            else if (point_pressed_index == selected_from_index && point_pressed_index == selected_to_index && point_pressed_index >= 0)
            {
                bool need_check_cursor_visiable = false;
                if (e.Key == Windows.System.VirtualKey.Up)
                {
                    if (point_pressed_index >= 16)
                    {
                        point_pressed_index -= 16;
                        this.set_selected_index(point_pressed_index, point_pressed_index, false);                        
                        tracking_from_index = tracking_to_index = selected_from_index;
                        need_check_cursor_visiable = true;
                    }
                }
                else if (e.Key == Windows.System.VirtualKey.Down)
                {
                    if (point_pressed_index < Bytes.Length - 1)
                    {
                        point_pressed_index += 16;
                        if (point_pressed_index > Bytes.Length - 1)
                        {
                            point_pressed_index = Bytes.Length - 1;
                        }
                        this.set_selected_index(point_pressed_index, point_pressed_index, false);                        
                        tracking_from_index = tracking_to_index = selected_from_index;
                        need_check_cursor_visiable = true;
                    }
                }
                else if (e.Key == Windows.System.VirtualKey.Left)
                {
                    if (point_pressed_index > 0)
                    {
                        point_pressed_index -= 1;
                        this.set_selected_index(point_pressed_index, point_pressed_index, false);                        
                        tracking_from_index = tracking_to_index = selected_from_index;
                        need_check_cursor_visiable = true;
                    }
                }
                else if (e.Key == Windows.System.VirtualKey.Right)
                {
                    if (point_pressed_index < Bytes.Length - 1)
                    {
                        point_pressed_index += 1;
                        this.set_selected_index(point_pressed_index, point_pressed_index, false);                        
                        tracking_from_index = tracking_to_index = selected_from_index;
                        need_check_cursor_visiable = true;
                    }
                    
                }                
                else if ((n = VirtualKey_to_0_F(e.Key)) != -1)
                {
                    if (is_readonly)
                    {
                        return;
                    }
                    Bytes[point_pressed_index] = apply_0_f_to_byte(Bytes[point_pressed_index], n, this.first_four_bits);
                    if (this.first_four_bits)
                    {
                        this.first_four_bits = false;
                    }
                    else
                    {
                        this.first_four_bits = true;
                        if (point_pressed_index < Bytes.Length - 1)
                        {
                            point_pressed_index += 1;
                            this.set_selected_index(point_pressed_index, point_pressed_index, false);                            
                            tracking_from_index = tracking_to_index = selected_from_index;
                            need_check_cursor_visiable = true;
                        }
                    }

                    eventBytesContentChanged?.Invoke(this, null);
                }

                if (need_check_cursor_visiable)
                {
                    this.first_four_bits = true;
                    // selected_from_index visiable?....
                    (int begin, int end) = VisiableIndex();
                    if (point_pressed_index < begin)
                    {
                        scrollBar.Value -=1;
                    }
                    else if (point_pressed_index > end)
                    {
                        scrollBar.Value += 1;
                    }
                }
                this.InvalidateHexView();
            }
        }

        private byte apply_0_f_to_byte(byte b, int input, bool first_four_bits)
        {
            if (first_four_bits)
            {
                return (byte)((b & 0x0F) + ((input & 0xF) << 4));
            }
            else
            {
                return (byte)((b & 0xF0) + (input & 0xF));
            }
        }
    }

    

    public class ByteSelectionEventArgs : EventArgs
    {
        public int indexByte
        {
            get;
            private set;
        }

        public ByteSelectionEventArgs(int indexByte)
        {
            this.indexByte = indexByte;
        }
    }
}
