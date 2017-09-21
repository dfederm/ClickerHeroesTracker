// <copyright file="AssertionGrantOptionsTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services.Authentication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Website.Services.Authentication;
    using Xunit;

    public sealed class AssertionGrantOptionsTests
    {
        [Fact]
        public void EmptyByDefault()
        {
            var options = new AssertionGrantOptions();

            Assert.Equal(0, options.AssertionGrantTypeMap.Count);
        }

        [Fact]
        public void AddAssertionGrantType_NullGrantTypeThrows()
        {
            var options = new AssertionGrantOptions();

            Assert.Throws<ArgumentNullException>(() => options.AddAssertionGrantType<MockAssertionGrantHandler1>(null));
            Assert.Equal(0, options.AssertionGrantTypeMap.Count);
        }

        [Fact]
        public void AddAssertionGrantType_AddsToCollection()
        {
            const string GrantType = "SomeGrantType";
            var options = new AssertionGrantOptions();
            options.AddAssertionGrantType<MockAssertionGrantHandler1>(GrantType);

            Assert.Equal(1, options.AssertionGrantTypeMap.Count);
            Assert.Equal(typeof(MockAssertionGrantHandler1), options.AssertionGrantTypeMap[GrantType]);
        }

        [Fact]
        public void AddAssertionGrantType_Multiple()
        {
            const string GrantType1 = "SomeGrantType1";
            const string GrantType2 = "SomeGrantType2";
            const string GrantType3 = "SomeGrantType3";
            var options = new AssertionGrantOptions();
            options.AddAssertionGrantType<MockAssertionGrantHandler1>(GrantType1);
            options.AddAssertionGrantType<MockAssertionGrantHandler2>(GrantType2);
            options.AddAssertionGrantType<MockAssertionGrantHandler3>(GrantType3);

            Assert.Equal(3, options.AssertionGrantTypeMap.Count);
            Assert.Equal(typeof(MockAssertionGrantHandler1), options.AssertionGrantTypeMap[GrantType1]);
            Assert.Equal(typeof(MockAssertionGrantHandler2), options.AssertionGrantTypeMap[GrantType2]);
            Assert.Equal(typeof(MockAssertionGrantHandler3), options.AssertionGrantTypeMap[GrantType3]);
        }

        [Fact]
        public void AddAssertionGrantType_DuplicatesThrow()
        {
            const string GrantType = "SomeGrantType";
            var options = new AssertionGrantOptions();
            options.AddAssertionGrantType<MockAssertionGrantHandler1>(GrantType);

            Assert.Throws<InvalidOperationException>(() => options.AddAssertionGrantType<MockAssertionGrantHandler2>(GrantType));
            Assert.Throws<InvalidOperationException>(() => options.AddAssertionGrantType<MockAssertionGrantHandler3>(GrantType));

            Assert.Equal(1, options.AssertionGrantTypeMap.Count);
            Assert.Equal(typeof(MockAssertionGrantHandler1), options.AssertionGrantTypeMap[GrantType]);
        }

        [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "We just need a type for a type param")]
        private sealed class MockAssertionGrantHandler1 : IAssertionGrantHandler
        {
            public string Name => throw new NotImplementedException();

            public Task<AssertionGrantResult> ValidateAsync(string assertion)
            {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "We just need a type for a type param")]
        private sealed class MockAssertionGrantHandler2 : IAssertionGrantHandler
        {
            public string Name => throw new NotImplementedException();

            public Task<AssertionGrantResult> ValidateAsync(string assertion)
            {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "We just need a type for a type param")]
        private sealed class MockAssertionGrantHandler3 : IAssertionGrantHandler
        {
            public string Name => throw new NotImplementedException();

            public Task<AssertionGrantResult> ValidateAsync(string assertion)
            {
                throw new NotImplementedException();
            }
        }
    }
}
