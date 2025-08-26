# EF Migrations Setup for Pathfinder Honor Manager API

## Overview

This project has been configured with Entity Framework Core migrations for safe, controlled database schema management. The setup includes a **baseline migration** approach for existing databases, ensuring compatibility with both local development and production deployment.

## What Was Implemented

### 1. EF Infrastructure
- ✅ Added `Microsoft.EntityFrameworkCore.Design` package
- ✅ Created baseline migration for existing database schema
- ✅ Configured local development environment
- ✅ Integrated with existing production deployment pipeline

### 2. Baseline Migration Approach
- ✅ **Existing Database Integration** - No database recreation required
- ✅ **Baseline Migration** - `20250826224824_InitialSchemaWithProperDeleteBehavior` captures current schema with safe delete behavior
- ✅ **Local Development Ready** - Migrations work with existing local database
- ✅ **Production Compatible** - Pipeline already configured for EF migrations

### 3. Safety Features
- ✅ **Zero automatic deletions** - EF only adds, never removes
- ✅ **Existing data preserved** - All current records remain intact
- ✅ **Migration history tracking** - `__EFMigrationsHistory` table properly configured
- ✅ **Pipeline control** - Migrations run automatically during deployment
- ✅ **Safe delete behavior** - Explicit configuration prevents unwanted cascade deletes

### 4. Delete Behavior Configuration
- ✅ **RESTRICT by default** - Prevents accidental data loss from parent deletions
- ✅ **Logical cascades only** - Pathfinder deletion removes their achievements/honors
- ✅ **Protected reference data** - Cannot delete clubs, honors, achievements if in use
- ✅ **Explicit configuration** - All relationships explicitly defined in `OnModelCreating`

## How It Works

### Local Development Setup
```bash
# 1. Ensure baseline migration is applied (one-time setup)
psql -h localhost -U dbuser -d pathfinder -c "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20250826224824_InitialSchemaWithProperDeleteBehavior', '9.0.8');"

# 2. Verify migration status
dotnet ef migrations list --project PathfinderHonorManager

# 3. Make model changes and create new migrations
dotnet ef migrations add AddNewFeature --project PathfinderHonorManager

# 4. Test locally
dotnet ef database update --project PathfinderHonorManager
```

### Production Deployment
The existing GitHub Actions pipeline (`.github/workflows/main_pathfinderhonormanager.yml`) already includes:
- **EF migrations integration** - `dotnet ef database update --connection "${{ secrets.PRODCONNECTIONSTRING }}"`
- **Automatic migration application** - Runs before app deployment
- **Zero manual intervention** - Migrations apply automatically

## Baseline Migration Details

### What Was Created
- **Migration**: `20250826224824_InitialSchemaWithProperDeleteBehavior`
- **Purpose**: Represents the current database schema state with safe delete behavior
- **Status**: Manually marked as applied in `__EFMigrationsHistory`
- **Approach**: Standard EF Core baseline migration pattern with explicit delete behavior configuration

### Why This Approach
1. **Existing Database** - Your database already has the schema
2. **No Recreation** - Avoids "relation already exists" errors
3. **Future Migrations** - Enables normal EF Core migration workflow
4. **Production Ready** - Pipeline will recognize this as baseline

## Development Workflow

### Creating New Migrations
```bash
# 1. Make model changes in your C# code
# 2. Create new migration
dotnet ef migrations add DescriptiveMigrationName --project PathfinderHonorManager

# 3. Review generated migration file
# 4. Test locally (optional)
dotnet ef database update --project PathfinderHonorManager

# 5. Commit and push
git add .
git commit -m "Add new feature with migration"
git push origin main
```

### Local Testing
```bash
# Check migration status
dotnet ef migrations list --project PathfinderHonorManager

# Apply pending migrations
dotnet ef database update --project PathfinderHonorManager

# Generate SQL script for review
dotnet ef migrations script --project PathfinderHonorManager --output migration.sql
```

## Configuration

### Required Environment Variables
- `PathfinderCS` - Database connection string
- `Auth0:Domain` - Auth0 domain
- `Auth0:Audience` - Auth0 audience
- `Auth0:ClientId` - Auth0 client ID

### Database Connection
- **Local**: `Host=localhost;Database=pathfinder;Username=dbuser;Password=yourpassword`
- **Production**: Configured via GitHub Secrets (`PRODCONNECTIONSTRING`)

## Monitoring & Troubleshooting

### Health Check Endpoints
- `/health` - Overall health status
- `/health/ready` - Readiness check
- `/health/live` - Liveness check

### Migration Status
```bash
# Check current migration status
dotnet ef migrations list --project PathfinderHonorManager

# Check database state
dotnet ef database update --dry-run --project PathfinderHonorManager
```

### Common Issues & Solutions

#### "Relation already exists" Error
**Cause**: Migration trying to create tables that already exist
**Solution**: Ensure baseline migration is marked as applied in `__EFMigrationsHistory`

```sql
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
VALUES ('20250826224824_InitialSchemaWithProperDeleteBehavior', '9.0.8');
```

#### Migration Not Found
**Cause**: Local database out of sync with migration files
**Solution**: Check `__EFMigrationsHistory` table and sync accordingly

## Best Practices

### Development
- ✅ **Baseline Migration** - One-time setup for existing databases
- ✅ **Descriptive Names** - Use clear migration names like `AddUserProfileTable`
- ✅ **Small Changes** - Keep migrations focused and incremental
- ✅ **Local Testing** - Test migrations locally before committing

### Deployment
- ✅ **Pipeline Integration** - Migrations run automatically
- ✅ **No Manual Steps** - Production deployment is fully automated
- ✅ **Health Monitoring** - Use health checks to verify deployment
- ✅ **Backup Strategy** - Ensure database backups before major changes

### Maintenance
- ✅ **Migration History** - Monitor `__EFMigrationsHistory` table
- ✅ **Clean Up** - Remove old migration files when no longer needed
- ✅ **Documentation** - Document complex schema changes
- ✅ **Version Control** - Keep migration files in source control

## Rollback Procedures

### Emergency Rollback
```bash
# 1. Stop application
# 2. Rollback database to previous migration
dotnet ef database update PreviousMigrationName --project PathfinderHonorManager

# 3. Redeploy previous application version
# 4. Verify data integrity
```

### Planned Rollback
```bash
# 1. Create rollback migration
dotnet ef migrations add RollbackFeature --project PathfinderHonorManager

# 2. Test rollback locally
# 3. Deploy through pipeline
# 4. Monitor health checks
```

## What's Different from Standard EF Setup

### Standard EF Setup
- Creates database from scratch
- First migration creates all tables
- Works with empty databases

### Our Baseline Approach
- Works with existing database
- Baseline migration represents current state
- Future migrations are incremental
- No database recreation required

## Support

For issues or questions:
1. Check migration status: `dotnet ef migrations list`
2. Verify `__EFMigrationsHistory` table contents
3. Test locally with development database
4. Check pipeline execution logs

## Next Steps

1. ✅ **Baseline Migration Complete** - Local development ready
2. ✅ **Production Pipeline Ready** - Migrations will run automatically
3. **Create New Features** - Add new migrations as needed
4. **Monitor Deployments** - Verify migrations apply successfully
5. **Team Documentation** - Share this workflow with your team

## Git Configuration

### .gitignore Updates
Added EF Core specific entries:
```
# Entity Framework Core
*.migration.sql
migration.sql
baseline-migration.sql
*.efmigrations
```

### What's Tracked
- ✅ `Migrations/` folder - All migration files
- ✅ `*.cs` migration files - Migration classes
- ✅ `*.Designer.cs` files - Migration designer files
- ✅ `PathfinderContextModelSnapshot.cs` - Model snapshot

### What's Ignored
- ❌ `*.migration.sql` - Generated SQL scripts
- ❌ `baseline-migration.sql` - Temporary baseline script
- ❌ `*.efmigrations` - EF Core artifacts 