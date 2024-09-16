using System;
using System.Collections.Generic;
using System.Linq;
using PathfinderHonorManager.Model;

namespace PathfinderHonorManager.Tests.Helpers;

public class PathfinderSelectorHelper
{
    private readonly List<Pathfinder> _pathfinders;
    private readonly List<PathfinderHonor> _pathfinderHonors;

    public PathfinderSelectorHelper(List<Pathfinder> pathfinders, List<PathfinderHonor> pathfinderHonors)
    {
        _pathfinders = pathfinders ?? throw new ArgumentNullException(nameof(pathfinders));
        _pathfinderHonors = pathfinderHonors ?? throw new ArgumentNullException(nameof(pathfinderHonors));
    }

    public Guid SelectPathfinderId(bool withHonors)
    {
        if (withHonors)
        {
            var pathfinderWithHonors = _pathfinders.FirstOrDefault(p => _pathfinderHonors.Any(ph => ph.PathfinderID == p.PathfinderID));
            if (pathfinderWithHonors == null)
            {
                throw new InvalidOperationException("No pathfinder with honors found.");
            }
            return pathfinderWithHonors.PathfinderID;
        }
        else
        {
            var pathfinderWithoutHonors = _pathfinders.FirstOrDefault(p => !_pathfinderHonors.Any(ph => ph.PathfinderID == p.PathfinderID));
            if (pathfinderWithoutHonors == null)
            {
                throw new InvalidOperationException("No pathfinder without honors found.");
            }
            return pathfinderWithoutHonors.PathfinderID;
        }
        
    }
        public List<Guid> SelectUniquePathfinderIds(int count, bool? withHonors = null)
    {
        var filteredPathfinders = _pathfinders.Where(p => 
            withHonors == null || 
            (withHonors.Value ? _pathfinderHonors.Any(ph => ph.PathfinderID == p.PathfinderID) : !_pathfinderHonors.Any(ph => ph.PathfinderID == p.PathfinderID)))
            .ToList();

        var pathfinderIds = filteredPathfinders
            .Select(p => p.PathfinderID)
            .Distinct()
            .Take(count)
            .ToList();

        if (pathfinderIds.Count < count)
        {
            throw new InvalidOperationException("Not enough unique pathfinders found with the specified criteria.");
        }

        return pathfinderIds;
    }
}