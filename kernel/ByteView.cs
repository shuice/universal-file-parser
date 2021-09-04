using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{    
    [DebuggerDisplay("{range_string}")]
    public class ByteView

    {
        private static readonly IEnumerable<byte> zeroCotentView = new List<byte>();

        public byte[] bytes { get; private set; }
        public long index_of_bits { get; private set; } = 0;
        public long count_of_bits { get; private set; } = 0;

        public long count_of_bytes => (long)((count_of_bits + BITS_PER_BYTE - 1) / BITS_PER_BYTE);

        public static readonly int BITS_PER_BYTE = 8;

        public string range_string => $"{ByteView.format_bit_index(index_of_bits)} - {ByteView.format_bit_index(count_of_bits)}"; 

        public ByteViewReader GetReader()
        {
            return new ByteViewReader(this);
        }


        public ByteView(byte[] bytes) : this(bytes, 0, bytes.Length * BITS_PER_BYTE)
        {

        }
        private ByteView(byte[] bytes, long index_of_bits, long count_of_bits)
        {
            index_of_bits = Math.Max(0, index_of_bits);
            count_of_bits = Math.Max(0, count_of_bits);
             
            this.bytes = bytes;
            this.index_of_bits = index_of_bits;
            this.count_of_bits = count_of_bits;
            long max_bits_in_bytes = bytes.Count() * BITS_PER_BYTE;
            if (this.index_of_bits > max_bits_in_bytes)
            {
                this.index_of_bits = max_bits_in_bytes;
                this.count_of_bits = 0;
            }
            else if (this.index_of_bits + this.count_of_bits > max_bits_in_bytes)
            {
                this.count_of_bits = max_bits_in_bytes - this.index_of_bits;
            }
        }

        public ByteView TakeBits(long count_of_bits)
        {
            return TakeBits(count_of_bits, ()=>("", true));
        }

        public ByteView TakeBits(long count_of_bits, Func<(string, bool)> funcWhileThrowCondition)
        {
            if (count_of_bits == Int64.MaxValue)
            {
                return this;
            }

            if (count_of_bits > this.count_of_bits)
            {
                (string while_doing, bool throw_exception) = funcWhileThrowCondition();
                String Message = MapException.CreateBitsNotEnoughString(this, count_of_bits, while_doing);
                if (throw_exception)
                {
                    throw new MapException(MapException.ExceptionReason.not_enough_data, Message);
                }
                MapContext.mapContexts.Peek().log.addLog(EnumLogLevel.warning, Message);
            }
            return new ByteView(bytes, index_of_bits, count_of_bits);
        }

        public ByteView SkipToBits(long index_to_of_bits, Func<(string, bool)> funcWhileThrowCondition)
        {
            if ((index_of_bits > index_to_of_bits) || (index_to_of_bits > index_of_bits + count_of_bits))
            {
                (string while_doing, bool throw_exception) = funcWhileThrowCondition();
                String Message = MapException.CreateJumpToBitsOutOfBoundsString(this, index_to_of_bits, while_doing);
                if (throw_exception)
                {
                    throw new MapException(MapException.ExceptionReason.jump_address_out_of_bounds, Message);
                }
                MapContext.mapContexts.Peek().log.addLog(EnumLogLevel.warning, Message);
            }
            return new ByteView(bytes, index_to_of_bits, index_of_bits + count_of_bits - index_to_of_bits);
        }

        public ByteView SkipBits(long count_of_bits)
        {
            return SkipBits(count_of_bits, ()=>("", true));

        }
        public ByteView SkipBits(long skip_count_of_bits, Func<(string, bool)> funcWhileThrowCondition)
        {            
            if (this.count_of_bits - skip_count_of_bits < 0)
            {
                (string while_doing, bool throw_exception) = funcWhileThrowCondition();
                String Message = MapException.CreateBitsNotEnoughString(this, skip_count_of_bits, while_doing);
                if (throw_exception)
                {
                    throw new MapException(MapException.ExceptionReason.not_enough_data, Message);
                }
                MapContext.mapContexts.Peek().log.addLog(EnumLogLevel.warning, Message);
            }
            return new ByteView(bytes, index_of_bits + skip_count_of_bits, this.count_of_bits - skip_count_of_bits);
        }

        public ByteView ExpandToLength(long expand_to_count_of_bits, Func<(string, bool)> funcWhileThrowCondition)
        {
            Int64 length_in_bits = ConvertBytesCount2BitsCount(bytes.Length);

            if (length_in_bits - index_of_bits < expand_to_count_of_bits)
            {
                (string while_doing, bool throw_exception) = funcWhileThrowCondition();
                String Message = MapException.CreateBitsNotEnoughString(this, expand_to_count_of_bits, while_doing);
                if (throw_exception)
                {
                    throw new MapException(MapException.ExceptionReason.not_enough_data, Message);
                }
                MapContext.mapContexts.Peek().log.addLog(EnumLogLevel.warning, Message);
            }
            return new ByteView(bytes, index_of_bits, expand_to_count_of_bits);
        }

        public static string padding_to_width(string s, int width)
        {
            string empty = "                          ";
            if (s.Length < width)
            {
                s = empty.Substring(0, width - s.Length) + s;
            }
            return s;
        }

        public static string format_bit_index_dec_hex_ui(long bits_index)
        {
            string str = $"{ByteView.format_bit_index_ui(bits_index, false)}({ByteView.format_bit_index_ui(bits_index, true)})";
            return str;
        }

        public static string format_bit_index_ui(long bits_index, bool hex)
        {
            long mod = bits_index % BITS_PER_BYTE;
            string strMod = ((mod == 0) ? "" : $":{mod}");
            long bytes = bits_index / BITS_PER_BYTE;
            if (hex)
            {
                return $"0x{bytes.ToString("X")}{strMod}";
            }
            else
            {
                return $"{bytes.ToString()}{strMod}";
            }
        }

        public static string format_bit_index(long bits_index)
        {
            long mod = bits_index % BITS_PER_BYTE;
            string strMod = ((mod == 0) ? "" : $".{mod}");
            long bytes = bits_index / BITS_PER_BYTE;

            return $"{padding_to_width("0x" + bytes.ToString("X"), 8)}({padding_to_width(bytes.ToString(), 8)}){strMod}";            
        }

        public long getLength()
        {
            Debug.Assert(index_of_bits % BITS_PER_BYTE == 0);
            Debug.Assert(count_of_bits % BITS_PER_BYTE == 0);
            return (count_of_bits + BITS_PER_BYTE - 1) / BITS_PER_BYTE;
        }

        public static long ConvertBytesCount2BitsCount(long bytes_count)
        {
            Int64 max_byte_count = Int64.MaxValue / ByteView.BITS_PER_BYTE;
            if (bytes_count >= max_byte_count)
            {
                return Int64.MaxValue;
            }
            return bytes_count * ByteView.BITS_PER_BYTE;
        }

        public static long ConvertBitsCount2BytesCount(long bits_count)
        {
            long a = (long)(bits_count / 8);
            long b = (((bits_count % 8) == 0) ? 0 : 1);
            return a + b;
        }

        public uint readUnsignedInt(long position_bytes, long length_bytes, string strEndianType)
        {
            uint r = readUnsignedIntBits(ConvertBytesCount2BitsCount(position_bytes), ConvertBytesCount2BitsCount(length_bytes), strEndianType);
            return r;
        }
                

        public uint readUnsignedIntBits(long position_bits, long length_bits, string strEndianType)
        {
            ByteView resultByteView = new ByteView(bytes).SkipToBits(position_bits, ()=>("reading integer", true)).TakeBits(length_bits, () => ("reading integer", true));
            ENDIAN_TYPE endianType = String2Enum.translateStringToEndianType(strEndianType);
            Value value = new Value();
            value.SetContent(VALUE_TYPE.VALUE_TYPE_NUMBER_UNSIGNED, resultByteView);
            value.endianType = endianType;
            uint r = (uint)value.getUnsignedNumber();
            return r;
        }
        public byte readByte(long position)
        {
            return bytes[position];
        }

    }
}
