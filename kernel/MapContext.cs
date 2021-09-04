using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{
    public class MapContext
    {
        public static Stack<MapContext> mapContexts = new Stack<MapContext>();

        public ScriptInstance scriptInstance;
        public Results results;        
        public Stack<(ElementStructure elementStructure, Result result, ByteView byteView)> parseStructureStack;
        public Stack<(ElementBase elementBase, Result result, ByteView byteView)> parseElementStack;
        public Grammar gramma;
        public StructureMapper sturctureMapper;
        public ByteView totalByteView;
        public ILog log;

        public HashSet<string> stringEncodingNotFound;

    }
}
