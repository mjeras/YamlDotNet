//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;
using Scalar = YamlDotNet.Representation.Scalar;
using Stream = YamlDotNet.Representation.Stream;

namespace YamlDotNet.Test.Representation
{
    public class SchemaTests
    {
        private readonly ITestOutputHelper output;

        public SchemaTests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public void ParseWithoutSchemaProducesNonSpecificTags()
        {
            AssertParseWithSchemaProducesCorrectTags(
                new NullSchema(),
                @"
                    - { actual: plain, expected: !? 'plain' }
                    - { actual: 'single quoted', expected: ! 'single quoted' }
                    - { actual: ""double quoted"", expected: ! 'double quoted' }

                    - { actual: { a: b }, expected: !? { a: b } }
                    - { actual: [ a, b ], expected: !? [ a, b ] }
                "
            );
        }

        [Fact]
        public void ParseWithFailsafeSchemaProducesCorrectTags()
        {
            AssertParseWithSchemaProducesCorrectTags(
                FailsafeSchema.Strict,
                @"
                    - { actual: null, expected: !? 'null' }
                    - { actual: Null, expected: !? 'Null' }
                    - { actual: NULL, expected: !? 'NULL' }
                    - { actual: ~, expected: !? '~' }
                    - { actual: , expected: !? '' }

                    - { actual: true, expected: !? 'true' }
                    - { actual: True, expected: !? 'True' }
                    - { actual: TRUE, expected: !? 'TRUE' }
                    - { actual: false, expected: !? 'false' }
                    - { actual: False, expected: !? 'False' }
                    - { actual: FALSE, expected: !? 'FALSE' }

                    - { actual: 0, expected: !? '0' }
                    - { actual: 13, expected: !? '13' }
                    - { actual: -6, expected: !? '-6' }
                    - { actual: 0o10, expected: !? '8' }
                    - { actual: 0x3A, expected: !? '58' }

                    - { actual: 0., expected: !? '0.' }
                    - { actual: -0.0, expected: !? '-0.0' }
                    - { actual: .5, expected: !? '.5' }
                    - { actual: +12e03, expected: !? '+12e03' }
                    - { actual: -2E+05, expected: !? '-2E+05' }
                    - { actual: .inf, expected: !? '.inf' }
                    - { actual: -.Inf, expected: !? '-.Inf' }
                    - { actual: +.INF, expected: !? '+.inf' }
                    - { actual: .nan, expected: !? '.nan' }

                    - { actual: 'non-plain', expected: !!str 'non-plain' }

                    - { actual: { a: b }, expected: !!map { a: b } }
                    - { actual: ! { a: b }, expected: !!map { a: b } }
                    - { actual: [ a, b ], expected: !!seq [ a, b ] }
                    - { actual: ! [ a, b ], expected: !!seq [ a, b ] }
                "
            );
        }

        [Fact]
        public void ParseWithJsonSchemaProducesCorrectTags()
        {
            AssertParseWithSchemaProducesCorrectTags(
                JsonSchema.Strict,
                @"
                    - { 'actual': null, 'expected': !!null }
                    - { 'actual': ! null, 'expected': !!str 'null' }

                    - { 'actual': true, 'expected': !!bool true }
                    - { 'actual': false, 'expected': !!bool false }

                    - { 'actual': 0, 'expected': !!int 0 }
                    - { 'actual': 13, 'expected': !!int 13 }
                    - { 'actual': -6, 'expected': !!int -6 }

                    - { 'actual': 0., 'expected': !!float 0. }
                    - { 'actual': -0.0, 'expected': !!float -0.0 }
                    - { 'actual': 0.5, 'expected': !!float 0.5 }
                    - { 'actual': 12e03, 'expected': !!float 12e03 }
                    - { 'actual': -2E+05, 'expected': !!float -2E+05 }

                    - { 'actual': { 'a': 'b' }, 'expected': !!map { 'a': 'b' } }
                    - { 'actual': ! { 'a': 'b' }, 'expected': !!map { 'a': 'b' } }
                    - { 'actual': [ 'a', 'b' ], 'expected': !!seq [ 'a', 'b' ] }
                    - { 'actual': ! [ 'a', 'b' ], 'expected': !!seq [ 'a', 'b' ] }
                "
            );
        }

        [Fact]
        public void ParseWithCoreSchemaProducesCorrectTags()
        {
            AssertParseWithSchemaProducesCorrectTags(
                CoreSchema.Instance,
                @"
                    - { actual: null, expected: !!null }
                    - { actual: Null, expected: !!null }
                    - { actual: NULL, expected: !!null }
                    - { actual: ~, expected: !!null }
                    - { actual: , expected: !!null }

                    - { actual: true, expected: !!bool true }
                    - { actual: True, expected: !!bool true }
                    - { actual: TRUE, expected: !!bool true }
                    - { actual: false, expected: !!bool false }
                    - { actual: False, expected: !!bool false }
                    - { actual: FALSE, expected: !!bool false }

                    - { actual: 0, expected: !!int 0 }
                    - { actual: 13, expected: !!int 13 }
                    - { actual: -6, expected: !!int -6 }
                    - { actual: 0o10, expected: !!int 8 }
                    - { actual: 0x3A, expected: !!int 58 }

                    - { actual: 0., expected: !!float 0 }
                    - { actual: -0.0, expected: !!float 0 }
                    - { actual: .5, expected: !!float 0.5 }
                    - { actual: +12e03, expected: !!float 12000 }
                    - { actual: -2E+05, expected: !!float -200000 }
                    - { actual: .inf, expected: !!float .inf }
                    - { actual: -.Inf, expected: !!float -.Inf }
                    - { actual: +.INF, expected: !!float +.inf }
                    - { actual: .nan, expected: !!float .nan }
                "
            );
        }

        private void AssertParseWithSchemaProducesCorrectTags(ISchema schema, string yaml)
        {
            var document = Stream.Load(Yaml.ParserForText(yaml), _ => schema).Single();

            foreach (Mapping testCase in (Sequence)document.Content)
            {
                var expected = testCase["expected"];
                var actual = testCase["actual"];

                output.WriteLine("actual: {0}\nexpected: {1}\n\n", actual, expected);

                // Since we can't specify the '?' tag, we'll use '!?' and translate here
                var expectedTag = expected.Tag;
                if (expectedTag.Value == "!?")
                {
                    expectedTag = TagName.Empty;
                }

                Assert.Equal(expectedTag, actual.Tag);
            }
        }

        private sealed class NullSchema : ISchema
        {
            public bool ResolveNonSpecificTag(YamlDotNet.Core.Events.Scalar node, IEnumerable<INodePathSegment> path, [NotNullWhen(true)] out INodeMapper? resolvedTag)
            {
                resolvedTag = null;
                return false;
            }

            public bool ResolveNonSpecificTag(MappingStart node, IEnumerable<INodePathSegment> path, [NotNullWhen(true)] out INodeMapper? resolvedTag)
            {
                resolvedTag = null;
                return false;
            }

            public bool ResolveNonSpecificTag(SequenceStart node, IEnumerable<INodePathSegment> path, [NotNullWhen(true)] out INodeMapper? resolvedTag)
            {
                resolvedTag = null;
                return false;
            }

            public bool IsTagImplicit(Scalar node, IEnumerable<INodePathSegment> path, out ScalarStyle style)
            {
                style = default;
                return false;
            }

            public bool IsTagImplicit(Mapping node, IEnumerable<INodePathSegment> path, out MappingStyle style)
            {
                style = default;
                return false;
            }

            public bool IsTagImplicit(Sequence node, IEnumerable<INodePathSegment> path, out SequenceStyle style)
            {
                style = default;
                return false;
            }

            public bool ResolveMapper(TagName tag, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                mapper = null;
                return false;
            }

            public INodeMapper ResolveChildMapper(object? native, IEnumerable<INodePathSegment> path)
            {
                return null; // TODO
            }
        }
    }
}
