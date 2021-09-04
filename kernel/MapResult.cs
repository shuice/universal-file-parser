using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public enum MapError
    {
        Ok,
        must_match_failed,
        gramma_error,
        not_enough_data,        
        not_reach_repeat_count,
        grammra_ref_map_error,
        min_max_limit_error,               
    }

    public class MapErrorItem
    {
        public MapError mapError { get; private set; } = MapError.Ok;
        public string Message { get; private set; } = "";          
        
        public MapErrorItem(MapError mapError, string Message)
        {
            this.mapError = mapError;
            this.Message = Message;
        }
    }

    [DebuggerDisplay("usedbits:{used_bits}, breaked:{Breaked()}")]
    public class MapResult
    {
        public static MapResult NotImp() { throw new NotImplementedException(); }
        public static MapResult CreateWithLength(Int64 used_bits)
        {
            return new MapResult()
            {
                used_bits = used_bits,
            };
        }

        public static MapResult CreateWithError(MapError error, String Message)
        {
            Debug.WriteLine(Message);
            MapResult mapResult = new MapResult();
            mapResult.mapErrors = new List<MapErrorItem>() { new MapErrorItem(error, Message)};
            mapResult.used_bits = 0;
            return mapResult;
        }

        public List<MapErrorItem> mapErrors = new List<MapErrorItem>() { };
        public Int64 used_bits = 0;

        public void AppendResult(MapResult other)
        {               
            used_bits += other.used_bits;
            if (used_bits > 200 * 1024 * 1024)
            {
                Debugger.Break();
            }
            mapErrors.AddRange(other.mapErrors);
        }

        public static string ErrorMessageStacks(List<MapErrorItem> items)
        {
            StringBuilder sb = new StringBuilder("The stack of the parsing error:\n");
            for (int index = 0; index < items.Count; index ++)
            {
                sb.AppendLine($"{index + 1}>     {items[index].Message}");
            }
            return sb.ToString();
        }

        public static bool Breaked(List<MapErrorItem> listMapError, out List<MapErrorItem> mapErrorItem)
        {
            mapErrorItem = null;
            if (listMapError.Count == 0)
            {
                return false;
            }
            mapErrorItem = listMapError.Where(item => IsBreakedMapError(item.mapError)).ToList();
            return (mapErrorItem.Count > 0);
        }

        public bool Breaked(out List<MapErrorItem> mapErrorItem)
        {
            return Breaked(mapErrors, out mapErrorItem);
        }

        public bool Breaked()
        {
            List<MapErrorItem> mapErrorItem;
            return Breaked(mapErrors, out mapErrorItem);
        }

        private static bool IsBreakedMapError(MapError mapError)
        {
            if ((mapError == MapError.must_match_failed) 
                || (mapError == MapError.gramma_error) 
                || (mapError == MapError.not_enough_data)
                || (mapError == MapError.not_reach_repeat_count)
                || (mapError == MapError.grammra_ref_map_error)
                || (mapError == MapError.min_max_limit_error))
            {
                return true;
            }
            return false;
        }
    }
}
