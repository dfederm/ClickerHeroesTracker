// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ClickerHeroesTrackerWebsite.Services.Database;
using Moq;

namespace ClickerHeroesTrackerWebsite.Tests.Mocks
{
    internal static class MockDatabaseHelper
    {
        public static Mock<IDataReader> CreateMockDataReader()
        {
            return CreateMockDataReader(Array.Empty<IDictionary<string, object>>());
        }

        public static Mock<IDataReader> CreateMockDataReader(IDictionary<string, object> dataSet)
        {
            return CreateMockDataReader(new[] { dataSet });
        }

        public static Mock<IDataReader> CreateMockDataReader(IList<IDictionary<string, object>> dataSets)
        {
            Mock<IDataReader> mockDataReader = new(MockBehavior.Strict);
            mockDataReader
                .Setup(_ => _.Dispose())
                .Verifiable();

            int i = -1;
            mockDataReader.Setup(_ => _.Read())
                .Returns(() => ++i < dataSets.Count)
                .Verifiable();

            if (dataSets != null && dataSets.Count > 0)
            {
                mockDataReader
                    .Setup(_ => _[It.IsAny<string>()])
                    .Returns((string key) => dataSets[i].TryGetValue(key, out object value) ? value : DBNull.Value)
                    .Verifiable();
            }

            return mockDataReader;
        }

        public static Mock<IDatabaseCommand> CreateMockDatabaseCommand(
            IDictionary<string, object> expectedParameters,
            IDataReader dataReader)
        {
            Mock<IDatabaseCommand> mockDatabaseCommand = CreateMockDatabaseCommandWithoutExecution(expectedParameters);
            mockDatabaseCommand.Setup(_ => _.ExecuteReaderAsync()).Returns(Task.FromResult(dataReader));
            return mockDatabaseCommand;
        }

        public static Mock<IDatabaseCommand> CreateMockDatabaseCommand(
            IDictionary<string, object> expectedParameters,
            object scalar)
        {
            Mock<IDatabaseCommand> mockDatabaseCommand = CreateMockDatabaseCommandWithoutExecution(expectedParameters);
            mockDatabaseCommand.Setup(_ => _.ExecuteScalarAsync()).Returns(Task.FromResult(scalar));
            return mockDatabaseCommand;
        }

        public static Mock<IDatabaseCommand> CreateMockDatabaseCommand(
            IDictionary<string, object> expectedParameters)
        {
            Mock<IDatabaseCommand> mockDatabaseCommand = CreateMockDatabaseCommandWithoutExecution(expectedParameters);
            mockDatabaseCommand.Setup(_ => _.ExecuteNonQueryAsync()).Returns(Task.CompletedTask);
            return mockDatabaseCommand;
        }

        private static Mock<IDatabaseCommand> CreateMockDatabaseCommandWithoutExecution(IDictionary<string, object> expectedParameters)
        {
            Mock<IDatabaseCommand> mockDatabaseCommand = new(MockBehavior.Strict);
            mockDatabaseCommand.SetupSet(_ => _.CommandText = It.IsAny<string>());
            mockDatabaseCommand.SetupSet(_ => _.Parameters = It.Is<IDictionary<string, object>>(parameters => DictionaryEquals(parameters, expectedParameters)));
            mockDatabaseCommand.Setup(_ => _.Dispose());
            return mockDatabaseCommand;
        }

        private static bool DictionaryEquals(IDictionary<string, object> dict1, IDictionary<string, object> dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, object> pair in dict1)
            {
                if (dict2.TryGetValue(pair.Key, out object value))
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
