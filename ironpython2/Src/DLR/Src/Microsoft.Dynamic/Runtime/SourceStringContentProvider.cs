// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

using AnyPrefix.Microsoft.Scripting.Utils;

namespace AnyPrefix.Microsoft.Scripting {

    [Serializable]
    internal sealed class SourceStringContentProvider : TextContentProvider {
        private readonly string _code;

        internal SourceStringContentProvider(string code) {
            ContractUtils.RequiresNotNull(code, nameof(code));

            _code = code;
        }

        public override SourceCodeReader GetReader() {
            return new SourceCodeReader(new StringReader(_code), null);
        }
    }
}
