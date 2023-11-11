// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Threading.Tasks;
using Website.Services.Authentication;
using Xunit;

namespace UnitTests.Services.Authentication
{
    public static class AssertionGrantOptionsTests
    {
        [Fact]
        public static void EmptyByDefault()
        {
            AssertionGrantOptions options = new();

            Assert.Empty(options.AssertionGrantTypeMap);
        }

        [Fact]
        public static void AddAssertionGrantType_NullGrantTypeThrows()
        {
            AssertionGrantOptions options = new();

            Assert.Throws<ArgumentNullException>(() => options.AddAssertionGrantType<MockAssertionGrantHandler1>(null));
            Assert.Empty(options.AssertionGrantTypeMap);
        }

        [Fact]
        public static void AddAssertionGrantType_AddsToCollection()
        {
            const string GrantType = "SomeGrantType";
            AssertionGrantOptions options = new();
            options.AddAssertionGrantType<MockAssertionGrantHandler1>(GrantType);

            Assert.Single(options.AssertionGrantTypeMap);
            Assert.Equal(typeof(MockAssertionGrantHandler1), options.AssertionGrantTypeMap[GrantType]);
        }

        [Fact]
        public static void AddAssertionGrantType_Multiple()
        {
            const string GrantType1 = "SomeGrantType1";
            const string GrantType2 = "SomeGrantType2";
            const string GrantType3 = "SomeGrantType3";
            AssertionGrantOptions options = new();
            options.AddAssertionGrantType<MockAssertionGrantHandler1>(GrantType1);
            options.AddAssertionGrantType<MockAssertionGrantHandler2>(GrantType2);
            options.AddAssertionGrantType<MockAssertionGrantHandler3>(GrantType3);

            Assert.Equal(3, options.AssertionGrantTypeMap.Count);
            Assert.Equal(typeof(MockAssertionGrantHandler1), options.AssertionGrantTypeMap[GrantType1]);
            Assert.Equal(typeof(MockAssertionGrantHandler2), options.AssertionGrantTypeMap[GrantType2]);
            Assert.Equal(typeof(MockAssertionGrantHandler3), options.AssertionGrantTypeMap[GrantType3]);
        }

        [Fact]
        public static void AddAssertionGrantType_DuplicatesThrow()
        {
            const string GrantType = "SomeGrantType";
            AssertionGrantOptions options = new();
            options.AddAssertionGrantType<MockAssertionGrantHandler1>(GrantType);

            Assert.Throws<InvalidOperationException>(() => options.AddAssertionGrantType<MockAssertionGrantHandler2>(GrantType));
            Assert.Throws<InvalidOperationException>(() => options.AddAssertionGrantType<MockAssertionGrantHandler3>(GrantType));

            Assert.Single(options.AssertionGrantTypeMap);
            Assert.Equal(typeof(MockAssertionGrantHandler1), options.AssertionGrantTypeMap[GrantType]);
        }

        private sealed class MockAssertionGrantHandler1 : IAssertionGrantHandler
        {
            public string Name => throw new NotImplementedException();

            public Task<AssertionGrantResult> ValidateAsync(string assertion)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class MockAssertionGrantHandler2 : IAssertionGrantHandler
        {
            public string Name => throw new NotImplementedException();

            public Task<AssertionGrantResult> ValidateAsync(string assertion)
            {
                throw new NotImplementedException();
            }
        }

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
