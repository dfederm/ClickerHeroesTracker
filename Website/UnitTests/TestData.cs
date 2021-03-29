// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.IO;

namespace UnitTests
{
    internal static class TestData
    {
        public static string ReadAllText(string fileName) => File.ReadAllText(Path.Combine("TestData", fileName));
    }
}
