using Common;
using kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file_structure
{
    public class CustomGrammarCache
    {
        private static Dictionary<string, Grammar> dicFileName2Grammar = new Dictionary<string, Grammar>();
        public static async Task<Grammar> LoadCustomGrammarWithCache(string fileName)
        {
            Grammar grammar;
            if (dicFileName2Grammar.TryGetValue(fileName, out grammar))
            {
                return grammar;            
            }
            grammar = new Grammar();
            try
            {
                string file_content = await AppData.appData.LoadCustomGrammar(fileName);
                grammar.readFromString(file_content);                
            }
            catch(Exception)
            {
                
            }
            dicFileName2Grammar[fileName] = grammar;
            return grammar;
        }

        public static void DiscardCache(string fileName)
        {
            dicFileName2Grammar.Remove(fileName);
        }
    }
}
