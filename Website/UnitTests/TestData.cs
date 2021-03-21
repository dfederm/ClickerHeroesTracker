// <copyright file="TestData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests
{
    using System.IO;

    internal static class TestData
    {
        public static string ReadAllText(string fileName) => File.ReadAllText(Path.Combine("TestData", fileName));
    }
}
