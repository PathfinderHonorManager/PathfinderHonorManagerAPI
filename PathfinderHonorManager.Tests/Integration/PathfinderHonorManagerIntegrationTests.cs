using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Model.Enum;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Tests.Integration
{
    [TestFixture]
    public class PathfinderHonorManagerIntegrationTests
    {
        [Test]
        public async Task ClubAndPathfinderFlow_CreatesAndFetchesPathfinder()
        {
            using var factory = new IntegrationTestWebAppFactory();
            await factory.InitializeAsync();
            await SeedPathfinderClassAsync(factory);

            using var client = factory.CreateClient();

            var clubResponse = await client.PostAsJsonAsync(
                "/api/clubs",
                new Incoming.ClubDto
                {
                    Name = "Integration Club",
                    ClubCode = TestAuthHandler.ClubCode
                });

            Assert.That(clubResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var createdClub = await clubResponse.Content.ReadFromJsonAsync<Outgoing.ClubDto>();
            Assert.That(createdClub, Is.Not.Null);

            var pathfinderResponse = await client.PostAsJsonAsync(
                "/api/pathfinders",
                new Incoming.PathfinderDto
                {
                    FirstName = "Jamie",
                    LastName = "Doe",
                    Email = "jamie.doe@example.com",
                    Grade = 5,
                    IsActive = true
                });

            Assert.That(pathfinderResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var createdPathfinder = await pathfinderResponse.Content.ReadFromJsonAsync<Outgoing.PathfinderDto>();
            Assert.That(createdPathfinder, Is.Not.Null);

            var getResponse = await client.GetAsync($"/api/pathfinders/{createdPathfinder.PathfinderID}");
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var fetchedPathfinder = await getResponse.Content.ReadFromJsonAsync<Outgoing.PathfinderDependantDto>();
            Assert.That(fetchedPathfinder, Is.Not.Null);
            Assert.That(fetchedPathfinder.FirstName, Is.EqualTo("Jamie"));
            Assert.That(fetchedPathfinder.ClubName, Is.EqualTo(createdClub.Name));

            var listResponse = await client.GetAsync("/api/pathfinders?showInactive=true");
            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var list = await listResponse.Content.ReadFromJsonAsync<List<Outgoing.PathfinderDependantDto>>();
            Assert.That(list, Is.Not.Null);
            Assert.That(list.Any(p => p.PathfinderID == createdPathfinder.PathfinderID), Is.True);
        }

        [Test]
        public async Task HonorsFlow_AssignsAndUpdatesHonorStatus()
        {
            using var factory = new IntegrationTestWebAppFactory();
            await factory.InitializeAsync();
            await SeedPathfinderClassAsync(factory);
            await SeedHonorStatusesAsync(factory);

            using var client = factory.CreateClient();

            var clubResponse = await client.PostAsJsonAsync(
                "/api/clubs",
                new Incoming.ClubDto
                {
                    Name = "Honor Club",
                    ClubCode = TestAuthHandler.ClubCode
                });

            Assert.That(clubResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var pathfinderResponse = await client.PostAsJsonAsync(
                "/api/pathfinders",
                new Incoming.PathfinderDto
                {
                    FirstName = "Alex",
                    LastName = "Runner",
                    Email = "alex.runner@example.com",
                    Grade = 6,
                    IsActive = true
                });

            Assert.That(pathfinderResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var createdPathfinder = await pathfinderResponse.Content.ReadFromJsonAsync<Outgoing.PathfinderDto>();
            Assert.That(createdPathfinder, Is.Not.Null);

            var honorResponse = await client.PostAsJsonAsync(
                "/api/honors",
                new Incoming.HonorDto
                {
                    Name = "Trail Basics",
                    Level = 1,
                    PatchFilename = "trail.png",
                    WikiPath = new Uri("https://example.com/trail")
                });

            Assert.That(honorResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var createdHonor = await honorResponse.Content.ReadFromJsonAsync<Honor>();
            Assert.That(createdHonor, Is.Not.Null);

            var assignResponse = await client.PostAsJsonAsync(
                $"/api/pathfinders/{createdPathfinder.PathfinderID}/pathfinderhonors",
                new Incoming.PostPathfinderHonorDto
                {
                    HonorID = createdHonor.HonorID,
                    Status = HonorStatus.Planned.ToString()
                });

            Assert.That(assignResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var assignedHonor = await assignResponse.Content.ReadFromJsonAsync<Outgoing.PathfinderHonorDto>();
            Assert.That(assignedHonor, Is.Not.Null);
            Assert.That(assignedHonor.Status, Is.EqualTo(HonorStatus.Planned.ToString()));

            var updateResponse = await client.PutAsJsonAsync(
                $"/api/pathfinders/{createdPathfinder.PathfinderID}/pathfinderhonors/{createdHonor.HonorID}",
                new Incoming.PutPathfinderHonorDto
                {
                    Status = HonorStatus.Earned.ToString()
                });

            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var updatedHonor = await updateResponse.Content.ReadFromJsonAsync<Outgoing.PathfinderHonorDto>();
            Assert.That(updatedHonor, Is.Not.Null);
            Assert.That(updatedHonor.Status, Is.EqualTo(HonorStatus.Earned.ToString()));
            Assert.That(updatedHonor.Earned, Is.Not.Null);
        }

        [Test]
        public async Task HonorsFlow_FiltersByStatus()
        {
            using var factory = new IntegrationTestWebAppFactory();
            await factory.InitializeAsync();
            await SeedPathfinderClassAsync(factory);
            await SeedHonorStatusesAsync(factory);

            using var client = factory.CreateClient();

            var clubResponse = await client.PostAsJsonAsync(
                "/api/clubs",
                new Incoming.ClubDto
                {
                    Name = "Status Club",
                    ClubCode = TestAuthHandler.ClubCode
                });

            Assert.That(clubResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var pathfinderResponse = await client.PostAsJsonAsync(
                "/api/pathfinders",
                new Incoming.PathfinderDto
                {
                    FirstName = "Riley",
                    LastName = "Status",
                    Email = "riley.status@example.com",
                    Grade = 5,
                    IsActive = true
                });

            Assert.That(pathfinderResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var createdPathfinder = await pathfinderResponse.Content.ReadFromJsonAsync<Outgoing.PathfinderDto>();
            Assert.That(createdPathfinder, Is.Not.Null);

            var honorResponse1 = await client.PostAsJsonAsync(
                "/api/honors",
                new Incoming.HonorDto
                {
                    Name = "Status Honor 1",
                    Level = 1,
                    PatchFilename = "status1.png",
                    WikiPath = new Uri("https://example.com/status1")
                });

            var honorResponse2 = await client.PostAsJsonAsync(
                "/api/honors",
                new Incoming.HonorDto
                {
                    Name = "Status Honor 2",
                    Level = 1,
                    PatchFilename = "status2.png",
                    WikiPath = new Uri("https://example.com/status2")
                });

            Assert.That(honorResponse1.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(honorResponse2.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var honor1 = await honorResponse1.Content.ReadFromJsonAsync<Honor>();
            var honor2 = await honorResponse2.Content.ReadFromJsonAsync<Honor>();
            Assert.That(honor1, Is.Not.Null);
            Assert.That(honor2, Is.Not.Null);

            var assignPlanned = await client.PostAsJsonAsync(
                $"/api/pathfinders/{createdPathfinder.PathfinderID}/pathfinderhonors",
                new Incoming.PostPathfinderHonorDto
                {
                    HonorID = honor1.HonorID,
                    Status = HonorStatus.Planned.ToString()
                });

            var assignEarned = await client.PostAsJsonAsync(
                $"/api/pathfinders/{createdPathfinder.PathfinderID}/pathfinderhonors",
                new Incoming.PostPathfinderHonorDto
                {
                    HonorID = honor2.HonorID,
                    Status = HonorStatus.Earned.ToString()
                });

            Assert.That(assignPlanned.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(assignEarned.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var statusResponse = await client.GetAsync("/api/pathfinders/pathfinderhonors?status=Planned");
            Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var honors = await statusResponse.Content.ReadFromJsonAsync<List<Outgoing.PathfinderHonorDto>>();
            Assert.That(honors, Is.Not.Null);
            Assert.That(honors.Count, Is.EqualTo(1));
            Assert.That(honors[0].Status, Is.EqualTo(HonorStatus.Planned.ToString()));
        }

        [Test]
        public async Task PathfindersFlow_BulkUpdateReturnsPerItemStatuses()
        {
            using var factory = new IntegrationTestWebAppFactory();
            await factory.InitializeAsync();
            await SeedPathfinderClassAsync(factory);

            using var client = factory.CreateClient();

            var clubResponse = await client.PostAsJsonAsync(
                "/api/clubs",
                new Incoming.ClubDto
                {
                    Name = "Bulk Club",
                    ClubCode = TestAuthHandler.ClubCode
                });

            Assert.That(clubResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var pathfinderResponse = await client.PostAsJsonAsync(
                "/api/pathfinders",
                new Incoming.PathfinderDto
                {
                    FirstName = "Bulk",
                    LastName = "Update",
                    Email = "bulk.update@example.com",
                    Grade = 6,
                    IsActive = true
                });

            Assert.That(pathfinderResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var createdPathfinder = await pathfinderResponse.Content.ReadFromJsonAsync<Outgoing.PathfinderDto>();
            Assert.That(createdPathfinder, Is.Not.Null);

            var bulkPayload = new[]
            {
                new Incoming.BulkPutPathfinderDto
                {
                    Items = new[]
                    {
                        new Incoming.BulkPutPathfinderItemDto
                        {
                            PathfinderId = createdPathfinder.PathfinderID,
                            Grade = 6,
                            IsActive = true
                        },
                        new Incoming.BulkPutPathfinderItemDto
                        {
                            PathfinderId = Guid.NewGuid(),
                            Grade = 6,
                            IsActive = true
                        }
                    }
                }
            };

            var bulkResponse = await client.PutAsJsonAsync("/api/pathfinders", bulkPayload);
            Assert.That(bulkResponse.StatusCode, Is.EqualTo(HttpStatusCode.MultiStatus));

            var payload = await bulkResponse.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(payload);
            var statuses = document.RootElement.EnumerateArray()
                .Select(item => item.GetProperty("status").GetInt32())
                .ToList();

            Assert.That(statuses, Does.Contain(StatusCodes.Status200OK));
            Assert.That(statuses, Does.Contain(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task HonorsFlow_ReturnsBadRequestForInvalidPatchFilename()
        {
            using var factory = new IntegrationTestWebAppFactory();
            await factory.InitializeAsync();

            using var client = factory.CreateClient();

            var honorResponse = await client.PostAsJsonAsync(
                "/api/honors",
                new Incoming.HonorDto
                {
                    Name = "Invalid Patch",
                    Level = 1,
                    PatchFilename = "invalid.txt",
                    WikiPath = new Uri("https://example.com/invalid")
                });

            Assert.That(honorResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AuthorizationBlocks_CreatePathfinderWithoutPermission()
        {
            var permissions = new[] { "read:pathfinders" };
            using var factory = new IntegrationTestWebAppFactory(permissions);
            await factory.InitializeAsync();

            using var client = factory.CreateClient();

            var pathfinderResponse = await client.PostAsJsonAsync(
                "/api/pathfinders",
                new Incoming.PathfinderDto
                {
                    FirstName = "Nope",
                    LastName = "Denied",
                    Email = "denied@example.com",
                    Grade = 5,
                    IsActive = true
                });

            Assert.That(pathfinderResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        private static async Task SeedPathfinderClassAsync(IntegrationTestWebAppFactory factory)
        {
            await SeedAsync(factory, async db =>
            {
                if (!await db.PathfinderClasses.AnyAsync())
                {
                    db.PathfinderClasses.Add(new PathfinderClass
                    {
                        Grade = 5,
                        ClassName = "Class for Grade 5"
                    });
                    db.PathfinderClasses.Add(new PathfinderClass
                    {
                        Grade = 6,
                        ClassName = "Class for Grade 6"
                    });
                    await db.SaveChangesAsync();
                }
            });
        }

        private static async Task SeedHonorStatusesAsync(IntegrationTestWebAppFactory factory)
        {
            await SeedAsync(factory, async db =>
            {
                if (!await db.PathfinderHonorStatuses.AnyAsync())
                {
                    db.PathfinderHonorStatuses.AddRange(new[]
                    {
                        new PathfinderHonorStatus
                        {
                            StatusCode = (int)HonorStatus.Planned,
                            Status = HonorStatus.Planned.ToString()
                        },
                        new PathfinderHonorStatus
                        {
                            StatusCode = (int)HonorStatus.Earned,
                            Status = HonorStatus.Earned.ToString()
                        },
                        new PathfinderHonorStatus
                        {
                            StatusCode = (int)HonorStatus.Awarded,
                            Status = HonorStatus.Awarded.ToString()
                        }
                    });

                    await db.SaveChangesAsync();
                }
            });
        }

        private static async Task SeedAsync(IntegrationTestWebAppFactory factory, Func<PathfinderContext, Task> seeder)
        {
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PathfinderContext>();
            await seeder(dbContext);
        }
    }
}
