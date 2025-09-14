using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveMgmt.Infrastructure.IntegrationTests.TestInfrastructure;
using Xunit;

namespace LeaveMgmt.Infrastructure.IntegrationTests.LeaveTypes;

[Collection(nameof(SqliteCollection))]
public class LeaveTypeRepositoryTests : IntegrationTestBase
{
    public LeaveTypeRepositoryTests(SqliteInMemoryFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Add_Then_GetById_Should_Roundtrip()
    {
        await ResetDbAsync();

        var lt = DomainBuilders.BuildLeaveType("Annual", "Annual leave", 15);

        await Fixture.DbContext.LeaveTypes.AddAsync(lt);
        await Fixture.DbContext.SaveChangesAsync();

        var found = await Fixture.LeaveTypeRepository.GetByIdAsync(lt.Id);
        found.IsSuccess.Should().BeTrue(found.Error);
        found.Value!.Id.Should().Be(lt.Id);
        found.Value!.Name.Should().Be(lt.Name);
    }

    [Fact]
    public async Task GetAll_Should_Return_Inserted()
    {
        await ResetDbAsync();

        var lt1 = DomainBuilders.BuildLeaveType("Annual", "Annual leave", 15);
        var lt2 = DomainBuilders.BuildLeaveType("Sick", "Sick leave", 10);

        await Fixture.DbContext.LeaveTypes.AddRangeAsync(lt1, lt2);
        await Fixture.DbContext.SaveChangesAsync();

        var res = await Fixture.LeaveTypeRepository.GetAllAsync();
        res.IsSuccess.Should().BeTrue(res.Error);
        res.Value!.Select(x => x.Name).Should().Contain(new[] { "Annual", "Sick" });
    }
}
