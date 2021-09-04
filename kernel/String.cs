using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel
{

    /*
          The String object contains a string ;)
     */
    public class StringEncodingWrapper
    {
        static StringEncodingWrapper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static readonly IReadOnlyDictionary<string, Int32> dicEncodingName = new Dictionary<string, Int32>()
            {
                {"IBM EBCDIC (US-Canada)", 37},
                {"OEM United States", 437},
                {"IBM EBCDIC (International)", 500},
                {"Arabic (ASMO 708)", 708},
                {"Arabic (DOS)", 720},
                {"Greek (DOS)", 737},
                {"Baltic (DOS)", 775},
                {"Western European (DOS)", 850},
                {"Central European (DOS)", 852},
                {"OEM Cyrillic", 855},
                {"Turkish (DOS)", 857},
                {"OEM Multilingual Latin I", 858},
                {"Portuguese (DOS)", 860},
                {"Icelandic (DOS)", 861},
                {"Hebrew (DOS)", 862},
                {"French Canadian (DOS)", 863},
                {"Arabic (864)", 864},
                {"Nordic (DOS)", 865},
                {"Cyrillic (DOS)", 866},
                {"Greek, Modern (DOS)", 869},
                {"IBM EBCDIC (Multilingual Latin-2)", 870},
                {"Thai (Windows)", 874},
                {"IBM EBCDIC (Greek Modern)", 875},
                {"Japanese (Shift-JIS)", 932},
                {"Chinese Simplified (GB2312)", 936},
                {"Korean", 949},
                {"Chinese Traditional (Big5)", 950},
                {"IBM EBCDIC (Turkish Latin-5)", 1026},
                //{"IBM Latin-1", 1047},
                {"IBM EBCDIC (US-Canada-Euro)", 1140},
                {"IBM EBCDIC (Germany-Euro)", 1141},
                {"IBM EBCDIC (Denmark-Norway-Euro)", 1142},
                {"IBM EBCDIC (Finland-Sweden-Euro)", 1143},
                {"IBM EBCDIC (Italy-Euro)", 1144},
                {"IBM EBCDIC (Spain-Euro)", 1145},
                {"IBM EBCDIC (UK-Euro)", 1146},
                {"IBM EBCDIC (France-Euro)", 1147},
                {"IBM EBCDIC (International-Euro)", 1148},
                {"IBM EBCDIC (Icelandic-Euro)", 1149},
                {"Central European (Windows)", 1250},
                {"Cyrillic (Windows)", 1251},
                {"Western European (Windows)", 1252},
                {"Greek (Windows)", 1253},
                {"Turkish (Windows)", 1254},
                {"Hebrew (Windows)", 1255},
                {"Arabic (Windows)", 1256},
                {"Baltic (Windows)", 1257},
                {"Vietnamese (Windows)", 1258},
                {"Korean (Johab)", 1361},
                {"Western European (Mac)", 10000},
                {"Japanese (Mac)", 10001},
                {"Chinese Traditional (Mac)", 10002},
                {"Korean (Mac)", 10003},
                {"Arabic (Mac)", 10004},
                {"Hebrew (Mac)", 10005},
                {"Greek (Mac)", 10006},
                {"Cyrillic (Mac)", 10007},
                {"Chinese Simplified (Mac)", 10008},
                {"Romanian (Mac)", 10010},
                {"Ukrainian (Mac)", 10017},
                {"Thai (Mac)", 10021},
                {"Central European (Mac)", 10029},
                {"Icelandic (Mac)", 10079},
                {"Turkish (Mac)", 10081},
                {"Croatian (Mac)", 10082},
                {"Chinese Traditional (CNS)", 20000},
                {"TCA Taiwan", 20001},
                {"Chinese Traditional (Eten)", 20002},
                {"IBM5550 Taiwan", 20003},
                {"TeleText Taiwan", 20004},
                {"Wang Taiwan", 20005},
                {"Western European (IA5)", 20105},
                {"German (IA5)", 20106},
                {"Swedish (IA5)", 20107},
                {"Norwegian (IA5)", 20108},
                {"T.61", 20261},
                {"ISO-6937", 20269},
                {"IBM EBCDIC (Germany)", 20273},
                {"IBM EBCDIC (Denmark-Norway)", 20277},
                {"IBM EBCDIC (Finland-Sweden)", 20278},
                {"IBM EBCDIC (Italy)", 20280},
                {"IBM EBCDIC (Spain)", 20284},
                {"IBM EBCDIC (UK)", 20285},
                {"IBM EBCDIC (Japanese katakana)", 20290},
                {"IBM EBCDIC (France)", 20297},
                {"IBM EBCDIC (Arabic)", 20420},
                {"IBM EBCDIC (Greek)", 20423},
                {"IBM EBCDIC (Hebrew)", 20424},
                {"IBM EBCDIC (Korean Extended)", 20833},
                {"IBM EBCDIC (Thai)", 20838},
                {"Cyrillic (KOI8-R)", 20866},
                {"IBM EBCDIC (Icelandic)", 20871},
                {"IBM EBCDIC (Cyrillic Russian)", 20880},
                {"IBM EBCDIC (Turkish)", 20905},
                {"IBM Latin-1", 20924},
                {"Japanese (JIS 0208-1990 and 0212-1990)", 20932},
                {"Chinese Simplified (GB2312-80)", 20936},
                {"Korean Wansung", 20949},
                {"IBM EBCDIC (Cyrillic Serbian-Bulgarian)", 21025},
                {"Cyrillic (KOI8-U)", 21866},
                {"Western European (ISO)", 28591},
                {"Central European (ISO)", 28592},
                {"Latin 3 (ISO)", 28593},
                {"Baltic (ISO)", 28594},
                {"Cyrillic (ISO)", 28595},
                {"Arabic (ISO)", 28596},
                {"Greek (ISO)", 28597},
                {"Hebrew (ISO-Visual)", 28598},
                {"Turkish (ISO)", 28599},
                {"Estonian (ISO)", 28603},
                {"Latin 9 (ISO)", 28605},
                {"Europa", 29001},
                {"Hebrew (ISO-Logical)", 38598},
                {"Japanese (JIS)", 50220},
                {"Japanese (JIS-Allow 1 byte Kana)", 50221},
                {"Japanese (JIS-Allow 1 byte Kana - SO/SI)", 50222},
                {"Korean (ISO)", 50225},
                {"Chinese Simplified (ISO-2022)", 50227},
                {"Japanese (EUC)", 51932},
                {"Chinese Simplified (EUC)", 51936},
                {"Korean (EUC)", 51949},
                {"Chinese Simplified (HZ)", 52936},
                {"Chinese Simplified (GB18030)", 54936},
                {"ISCII Devanagari", 57002},
                {"ISCII Bengali", 57003},
                {"ISCII Tamil", 57004},
                {"ISCII Telugu", 57005},
                {"ISCII Assamese", 57006},
                {"ISCII Oriya", 57007},
                {"ISCII Kannada", 57008},
                {"ISCII Malayalam", 57009},
                {"ISCII Gujarati", 57010},
                {"ISCII Punjabi", 57011},

                {"Shift-JIS", 932},
                {"GB2312", 936},
                {"windows-1252", 1252},
                {"IBM850", 850},
                {"ISO_8859-1:1987", 28591},
                {"ISO_8859-3:1988", 28593},
                {"ANSI_X3.4-1968", 20127},
                {"macintosh", 10000},
                {"UTF-16", 1200},
                {"UTF-16LE", 1200},
                {"UTF-8", 65001},
                {"UTF-7", 65000},
                {"UTF-16BE", 1201},
                {"UTF-32", 12000},
                {"UTF-32BE", 12001},
                {"ASCII", 20127},
            };
                
        /*

         */
        public String getEncoding()
        {
            throw new NotImplementedException();
        }

        public static Encoding GetEncodingByName(string name, MapContext mapContext, Func<string> funcGenerateErrorString)
        {
            
            Int32 codePage;
            if (dicEncodingName.TryGetValue(name, out codePage))
            {
                Encoding encoding = Encoding.GetEncoding(codePage);
                if (encoding != null)
                {
                    return encoding;
                }
            }
            if (mapContext.stringEncodingNotFound.Contains(name) == false)
            {
                mapContext.stringEncodingNotFound.Add(name);
                mapContext.log.addLog(EnumLogLevel.warning, funcGenerateErrorString());
            }            
            return Encoding.UTF8;
        }

    }
}
