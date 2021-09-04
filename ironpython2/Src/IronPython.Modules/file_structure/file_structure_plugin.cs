// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using AnyPrefix.Microsoft.Scripting;
using AnyPrefix.Microsoft.Scripting.Runtime;
using AnyPrefix.Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using System.Numerics;

[assembly: PythonModule("file_structure_plugin", typeof(IronPython.Modules.file_structure_plugin))]
namespace IronPython.Modules 
{
    public partial class file_structure_plugin
    {   
        public interface IFileStructurePluginHelper
        {
            object CreateValue();
            object CreateElement(string element_type, string name, bool autosetDefaults);

            void logMessage(String module, int messageID, string severity, String message);
        }
        public static IFileStructurePluginHelper helper;

        public static readonly string ENDIAN_UNDEFINED = "ENDIAN_UNDEFINED";
        public static readonly string ENDIAN_UNKNOWN = "ENDIAN_UNKNOWN";
        public static readonly string ENDIAN_BIG = "ENDIAN_BIG";
        public static readonly string ENDIAN_LITTLE = "ENDIAN_LITTLE";
        public static readonly string ENDIAN_DYNAMIC = "ENDIAN_DYNAMIC";


        public static readonly string LENGTH_UNSPECIFIED = "LENGTH_UNSPECIFIED";
        public static readonly string LENGTH_BYTE = "LENGTH_BYTE";
        public static readonly string LENGTH_BIT = "LENGTH_BIT";

        public static readonly string ELEMENT_NONE = "ELEMENT_NONE";
        public static readonly string ELEMENT_BINARY = "ELEMENT_BINARY";
        public static readonly string ELEMENT_CUSTOM = "ELEMENT_CUSTOM";
        public static readonly string ELEMENT_GRAMMAR_REF = "ELEMENT_GRAMMAR_REF";
        public static readonly string ELEMENT_NUMBER = "ELEMENT_NUMBER";
        public static readonly string ELEMENT_STRING = "ELEMENT_STRING";
        public static readonly string ELEMENT_OFFSET = "ELEMENT_OFFSET";
        public static readonly string ELEMENT_SCRIPT = "ELEMENT_SCRIPT";
        public static readonly string ELEMENT_STRUCTURE = "ELEMENT_STRUCTURE";
        public static readonly string ELEMENT_STRUCTURE_REF = "ELEMENT_STRUCTURE_REF";

        public static readonly string SEVERITY_UNKNOWN = "SEVERITY_UNKNOWN";
        public static readonly string SEVERITY_FATAL = "SEVERITY_FATAL";
        public static readonly string SEVERITY_ERROR = "SEVERITY_ERROR";
        public static readonly string SEVERITY_WARNING = "SEVERITY_WARNING";
        public static readonly string SEVERITY_INFO = "SEVERITY_INFO";
        public static readonly string SEVERITY_VERBOSE = "SEVERITY_VERBOSE";
        public static readonly string SEVERITY_DEBUG = "SEVERITY_DEBUG";

        public static readonly string RESULT_STRUCTURE_START_TYPE = "RESULT_STRUCTURE_START_TYPE";
        public static readonly string RESULT_STRUCTURE_END_TYPE = "RESULT_STRUCTURE_END_TYPE";
        public static readonly string RESULT_STRUCTURE_ELEMENT_TYPE = "RESULT_STRUCTURE_ELEMENT_TYPE";
        public static readonly string RESULT_MASK_TYPE = "RESULT_MASK_TYPE";

        public static readonly string VALUE_TYPE_BINARY = "VALUE_TYPE_BINARY";
        public static readonly string VALUE_TYPE_BOOLEAN = "VALUE_TYPE_BOOLEAN";
        public static readonly string VALUE_TYPE_NUMBER_UNSIGNED = "VALUE_TYPE_NUMBER_UNSIGNED";
        public static readonly string VALUE_TYPE_NUMBER_SIGNED = "VALUE_TYPE_NUMBER_SIGNED";
        public static readonly string VALUE_TYPE_NUMBER_FLOAT = "VALUE_TYPE_NUMBER_FLOAT";
        public static readonly string VALUE_TYPE_STRING = "VALUE_TYPE_STRING";

        public static readonly string NUMBER_INTEGER = "NUMBER_INTEGER";
        public static readonly string NUMBER_FLOAT = "NUMBER_FLOAT";

        public static readonly string NUMBER_DISPLAY_DECIMAL = "NUMBER_DISPLAY_DECIMAL";
        public static readonly string NUMBER_DISPLAY_EXPONENT = "NUMBER_DISPLAY_EXPONENT";
        public static readonly string NUMBER_DISPLAY_HEX = "NUMBER_DISPLAY_HEX";
        public static readonly string NUMBER_DISPLAY_OCTAL = "NUMBER_DISPLAY_OCTAL";
        public static readonly string NUMBER_DISPLAY_BINARY = "NUMBER_DISPLAY_BINARY";

        public static readonly string STRING_LENGTH_FIXED = "STRING_LENGTH_FIXED";
        public static readonly string STRING_LENGTH_ZERO_TERMINATED = "STRING_LENGTH_ZERO_TERMINATED";
        public static readonly string STRING_LENGTH_PASCAL = "STRING_LENGTH_PASCAL";
        public static readonly string STRING_LENGTH_DELIMITER_TERMINATED = "STRING_LENGTH_DELIMITER_TERMINATED";

                
        public static object Value(CodeContext/*!*/ context)
        {
            return helper.CreateValue();
        }


        public static object Element(CodeContext/*!*/ context, string element_type, string name, bool autosetDefaults)
        {
            return helper.CreateElement(element_type, name, autosetDefaults);
        }

        public static void logMessage(String module, int messageID, string severity, String message)
        {
            helper.logMessage(module, messageID, severity, message);
        }

    }
}
