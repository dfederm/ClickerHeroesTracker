// <copyright file="MockDatabaseHelper.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Moq;

    internal static class MockDatabaseHelper
    {
        public static Mock<IDataReader> CreateMockDataReader(
            IDictionary<string, object> expectedData)
        {
            var mockDataReader = new Mock<IDataReader>(MockBehavior.Strict);
            mockDataReader.Setup(_ => _.Dispose()).Verifiable();

            if (expectedData != null)
            {
                mockDataReader.Setup(_ => _.Read()).Returns(true).Verifiable();
                foreach (var pair in expectedData)
                {
                    mockDataReader.Setup(_ => _[pair.Key]).Returns(pair.Value ?? DBNull.Value).Verifiable();
                }
            }
            else
            {
                mockDataReader.Setup(_ => _.Read()).Returns(false).Verifiable();
            }

            return mockDataReader;
        }

        public static Mock<IDatabaseCommand> CreateMockDatabaseCommand(
            IDictionary<string, object> expectedParameters,
            IDataReader dataReader)
        {
            var mockDatabaseCommand = new Mock<IDatabaseCommand>(MockBehavior.Strict);
            mockDatabaseCommand.SetupSet(_ => _.CommandText = It.IsAny<string>()).Verifiable();
            mockDatabaseCommand.SetupSet(_ => _.Parameters = It.Is<IDictionary<string, object>>(parameters => DictionaryEquals(parameters, expectedParameters))).Verifiable();
            mockDatabaseCommand.Setup(_ => _.ExecuteReader()).Returns(dataReader).Verifiable();
            mockDatabaseCommand.Setup(_ => _.Dispose()).Verifiable();
            return mockDatabaseCommand;
        }

        private static bool DictionaryEquals(IDictionary<string, object> dict1, IDictionary<string, object> dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            foreach (var pair in dict1)
            {
                object value;
                if (dict2.TryGetValue(pair.Key, out value))
                {
                    if (!value.Equals(pair.Value))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
