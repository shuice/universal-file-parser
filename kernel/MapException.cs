using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace kernel
{
    public class MapException : Exception
    {
        public static MapContext mapContext;
        public enum ExceptionReason
        {
            not_enough_data,
            jump_address_out_of_bounds,
        }

        public ExceptionReason reason { get; private set; }
        public MapException(ExceptionReason reason, string message)
            : base(message)
        {
            this.reason = reason;
        }


        public static string CreateJumpToBitsOutOfBoundsString(ByteView byteView, long wanted, string while_doing)
        {            
            string position = ByteView.format_bit_index_dec_hex_ui(byteView.index_of_bits);
            string exist_length = ByteView.format_bit_index_dec_hex_ui(byteView.count_of_bits);
            string strWanted = ByteView.format_bit_index_dec_hex_ui(wanted);
            string Message = $"Exception: Cannot jump to position {strWanted} within the specified range (position: {position}, length: {exist_length}) while {while_doing}";
            return Message;
        }

        public static string CreateBitsNotEnoughString(ByteView byteView, long wanted, string while_doing)
        {
            string position = ByteView.format_bit_index_dec_hex_ui(byteView.index_of_bits);
            string exist_length = ByteView.format_bit_index_dec_hex_ui(byteView.count_of_bits);
            string strWanted = ByteView.format_bit_index_dec_hex_ui(wanted);
            string Message = $"Exception: Cannot get the desired length {strWanted} within the specified range (position: {position}, length: {exist_length}) while {while_doing}";
            return Message;
        }
    }
}
