using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public class ByteViewReader

    {
        private ByteView byteView;


        public ByteViewReader(ByteView byteView)
        {
            this.byteView = byteView;
        }

        public void AssumeBytesAligned(long how_many_bytes)
        {
            Debug.Assert(byteView.index_of_bits % ByteView.BITS_PER_BYTE == 0);
            long index_of_bytes = byteView.index_of_bits / ByteView.BITS_PER_BYTE;
            if (how_many_bytes != 0)
            {
                Debug.Assert(index_of_bytes % how_many_bytes == 0);
            }
        }
                

        private byte CutByte(long bit_start_index, long bit_used_len, bool padding_zero_to_left, bool is_first_byte)
        {
            Debug.Assert(bit_used_len <= ByteView.BITS_PER_BYTE);
            long byte_start_index = bit_start_index / ByteView.BITS_PER_BYTE;
            long bits_skiped_in_first_byte = bit_start_index % ByteView.BITS_PER_BYTE;

            byte first_byte = byteView.bytes[byte_start_index];
            if (bits_skiped_in_first_byte == 0 && bit_used_len == ByteView.BITS_PER_BYTE)
            {
                // Most likely scenario
                return byteView.bytes[byte_start_index];
            }
            else
            {
                // ((1 << 7) - 1)  0x0111 1111;
                // ((1 << 6) - 1)  0x0011 1111;
                // ((1 << 5) - 1)  0x0001 1111;
                // ((1 << 4) - 1)  0x0000 1111;
                // ((1 << 3) - 1)  0x0000 0111;
                // ((1 << 2) - 1)  0x0000 0011;
                // ((1 << 1) - 1)  0x0000 0001;
                byte bit_used_len_value = 0;
                byte bits_skiped_in_first_byte_mask = (byte)((1 << (ByteView.BITS_PER_BYTE - (byte)bits_skiped_in_first_byte)) - 1);
                first_byte &= bits_skiped_in_first_byte_mask;
                long first_byte_remain_bits = (ByteView.BITS_PER_BYTE - bits_skiped_in_first_byte);
                if (first_byte_remain_bits > bit_used_len)
                {

                    bit_used_len_value = (byte)(first_byte >> (byte)(first_byte_remain_bits - bit_used_len));                    
                }
                else if (first_byte_remain_bits == bit_used_len)
                {
                    bit_used_len_value = first_byte;
                }
                else
                {
                    byte second_byte = byteView.bytes[byte_start_index + 1];

                    second_byte >>= (byte)(ByteView.BITS_PER_BYTE - (bit_used_len - first_byte_remain_bits));
                    first_byte <<= (byte)(bit_used_len - first_byte_remain_bits);
                    bit_used_len_value = (byte)(first_byte + second_byte);
                }

                if ((bit_used_len < ByteView.BITS_PER_BYTE) && (padding_zero_to_left == false) && (is_first_byte == false))
                {
                    bit_used_len_value <<= (byte)(ByteView.BITS_PER_BYTE - bit_used_len);
                }
                return bit_used_len_value;
            }
        }

        /*
         * padding_zero_to_left
         * 
         * true:  Fill 0 on the left to align the right，BIG Endian
         * false: Fill 0 on the right to align the left，Little Endian
         */
        public IEnumerable<byte> GetEnumerator(bool padding_zero_to_left)
        {
            long fragment_bits_count = (byteView.count_of_bits % ByteView.BITS_PER_BYTE);            
            long total_used_bits = 0;
            long count_of_bytes = (byteView.count_of_bits + ByteView.BITS_PER_BYTE - 1) / ByteView.BITS_PER_BYTE;

            for (long byte_index = 0; byte_index < count_of_bytes; byte_index++)
            {
                long bit_start_index = byteView.index_of_bits + total_used_bits;
                long bit_used_len = ((byte_index == 0) && padding_zero_to_left) 
                                    ? ((fragment_bits_count == 0) ? 8 : fragment_bits_count)
                                    : ByteView.BITS_PER_BYTE;
                
                if (bit_start_index + bit_used_len >= byteView.index_of_bits + byteView.count_of_bits)
                {
                    bit_used_len = byteView.index_of_bits + byteView.count_of_bits - bit_start_index;
                }
                Byte cuted = CutByte(bit_start_index, bit_used_len, padding_zero_to_left, byte_index == 0);
                total_used_bits += bit_used_len;
                yield return cuted;
            }
        }
    }


}
