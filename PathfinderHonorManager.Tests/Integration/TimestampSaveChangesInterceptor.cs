using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PathfinderHonorManager.Tests.Integration
{
    public class TimestampSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyTimestamps(eventData);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyTimestamps(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void ApplyTimestamps(DbContextEventData eventData)
        {
            var context = eventData.Context;
            if (context == null)
            {
                return;
            }

            var now = DateTime.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Added))
            {
                SetIfDefault(entry, "Created", now);
                SetIfDefault(entry, "Updated", now);
                SetIfDefault(entry, "CreateTimestamp", now);
            }

            foreach (var entry in context.ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Modified))
            {
                SetIfDefault(entry, "Updated", now);
            }
        }

        private static void SetIfDefault(EntityEntry entry, string propertyName, DateTime value)
        {
            var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);
            if (property == null)
            {
                return;
            }

            if (property.CurrentValue is DateTime current && current != default)
            {
                return;
            }

            property.CurrentValue = value;
        }
    }
}
