using System;

namespace PathfinderHonorManager.Service
{
    public class AchievementSyncOptions
    {
        public TimeSpan ProcessingInterval { get; set; } = TimeSpan.FromMinutes(5);
        
        public int MaxBatchSize { get; set; } = 50;
        
        public int MaxConcurrency { get; set; } = 5;
        
        public bool RunAuditOnStartup { get; set; } = true;
    }
}

