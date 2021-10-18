using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.DataAccess;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace PathfinderHonorManager.Tests.DataFixtures
{
    public class PathfinderValidatorInMemoryTests : PathfinderValidatorTests, IDisposable
    {
        private readonly DbConnection _connection;

        public PathfinderValidatorInMemoryTests()
            : base(
                new DbContextOptionsBuilder<PathfinderContext>()
                    .UseSqlite(CreateInMemoryDatabase())
                    .Options)
        {
            _connection = RelationalOptionsExtension.Extract(ContextOptions).Connection;
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Data Source=Sharable;Mode=Memory;Cache=Shared");

            connection.Open();

            return connection;
        }

        public void Dispose() => _connection.Dispose();

    }
}
