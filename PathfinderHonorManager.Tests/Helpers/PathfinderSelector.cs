using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Model.Enum;

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
}

