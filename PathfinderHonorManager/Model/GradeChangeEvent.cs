using System;

namespace PathfinderHonorManager.Model
{
    public class GradeChangeEvent
    {
        public Guid PathfinderId { get; set; }
        public int? OldGrade { get; set; }
        public int? NewGrade { get; set; }
        public DateTime QueuedAt { get; set; }

        public GradeChangeEvent()
        {
            QueuedAt = DateTime.UtcNow;
        }

        public GradeChangeEvent(Guid pathfinderId, int? oldGrade, int? newGrade)
        {
            PathfinderId = pathfinderId;
            OldGrade = oldGrade;
            NewGrade = newGrade;
            QueuedAt = DateTime.UtcNow;
        }
    }
}

