// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Threading.Tasks;
using DSharpPlus.Analyzers.Core;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = DSharpPlus.Test.Analyzers.Verifiers.CSharpCodeFixVerifier<DSharpPlus.Analyzers.Core.OptionalAddJsonIgnoreAnalyzer, DSharpPlus.Analyzers.Core.OptionalAddJsonIgnoreCodeFixProvider>;

namespace DSharpPlus.Test.Analyzers.Core
{
    [TestClass]
    public class DSP0001UnitTest
    {
        private const string TestSource = @"using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace System.Text.Json.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class JsonIgnoreAttribute : JsonAttribute
    {
        public JsonIgnoreCondition Condition { get; set; } = JsonIgnoreCondition.Always;
        public JsonIgnoreAttribute() { }
    }

    public enum JsonIgnoreCondition
    {
        Never = 0,
        Always = 1,
        WhenWritingDefault = 2,
        WhenWritingNull = 3
    }
}

namespace DSharpPlus.Core.Entities
{
    public struct Optional<T> { }
}

namespace ConsoleApplication1
{
    class Test
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> {|#0:Prop|} { get; set; }
        public int {|#1:Number|} { get; set; }
    }
}";

        private static readonly string TestSourceWithoutJsonIgnoreAttribute = string.Join('\n', TestSource.Split('\n').RemoveAt(36)); // Removes line 37 from the code (JsonIgnoreAttribute), which should throw a warning.
        private static readonly string TestSourceWithoutJsonIgnoreAttributeOrUsing = string.Join('\n', TestSource.Split('\n').RemoveAt(7).RemoveAt(35)); // Removes line 8 from the code (using Json namespace), which should be added back in the code fix.

        [TestMethod]
        // Verifies that the analyzer isn't always throwing a warning.
        public async Task VerifyNoCodeAsync() => await VerifyCS.VerifyAnalyzerAsync(@"");

        [TestMethod]
        // Verifies that the analyzer sees the expected JsonIgnoreAttribute.
        public async Task VerifyNoWarningAsync() => await VerifyCS.VerifyAnalyzerAsync(TestSource);

        [TestMethod]
        // Verifies that the analyzer sees a missing JsonIgnoreAttribute
        public async Task VerifyWarningAsync() => await VerifyCS.VerifyAnalyzerAsync(TestSourceWithoutJsonIgnoreAttribute, VerifyCS.Diagnostic(OptionalAddJsonIgnoreAnalyzer.Id).WithLocation(0).WithArguments("Prop"));

        [TestMethod]
        // Verifies that the `JsonIgnoreAttribute` gets added.
        // Verifies that the `using System.Text.Json.Serialization;` doesn't get duplicated.
        public async Task VerifyCodeFixAsync()
        {
            DiagnosticResult expected = VerifyCS.Diagnostic(OptionalAddJsonIgnoreAnalyzer.Id).WithLocation(0).WithArguments("Prop");
            await VerifyCS.VerifyCodeFixAsync(TestSourceWithoutJsonIgnoreAttribute, expected, TestSource);
        }

        [TestMethod]
        // Verifies that the code fix adds the `using System.Text.Json.Serialization;` back in.
        public async Task VerifyCodeFixWithoutUsingAsync()
        {
            DiagnosticResult expected = VerifyCS.Diagnostic(OptionalAddJsonIgnoreAnalyzer.Id).WithLocation(0).WithArguments("Prop");
            await VerifyCS.VerifyCodeFixAsync(TestSourceWithoutJsonIgnoreAttributeOrUsing, expected, TestSource);
        }
    }
}
