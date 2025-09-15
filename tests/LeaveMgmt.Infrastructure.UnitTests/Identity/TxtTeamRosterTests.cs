// File: LeaveMgmt.Infrastructure.UnitTests/Identity/TxtTeamRosterTests.cs
using System.IO;
using System.Collections.Generic;
using FluentAssertions;
using LeaveMgmt.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Xunit;

public class TxtTeamRosterTests
{
    [Fact] // Unit
    public void Roster_Should_Read_CSV_And_Map_Roles()
    {
        // arrange temp files
        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "roster_tests_" + Guid.NewGuid()));
        var dev = Path.Combine(dir.FullName, "Dev.txt");
        var mgmt = Path.Combine(dir.FullName, "Managment.txt");
        var sup = Path.Combine(dir.FullName, "Support.txt");

        File.WriteAllText(dev,
@"Team Members
Full Name,Employee Number,Email Address,Cellphone Number
Ella Jefferson,2005,ellajefferson@acme.com,+27 55 979 367");

        File.WriteAllText(mgmt,
@"Team Members
Full Name,Employee Number,Email Address,Cellphone Number
Colin Horton,3,colinhorton@acme.com,+27 ...");

        File.WriteAllText(sup,
@"Team Members
Full Name,Employee Number,Email Address,Cellphone Number
Amy Burns,2012,amyburns@acme.com,+27 ...");

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Teams:Dev"] = dev,
                ["Teams:Management"] = mgmt,
                ["Teams:Support"] = sup
            }).Build();

        var roster = new TxtTeamRoster(cfg);

        // act/assert
        roster.TryGetRolesFor("ellajefferson@acme.com", out var roles1).Should().BeTrue();
        roles1.Should().Contain("Employee");

        roster.TryGetRolesFor("colinhorton@acme.com", out var roles2).Should().BeTrue();
        roles2.Should().Contain("Manager");

        roster.TryGetRolesFor("amyburns@acme.com", out var roles3).Should().BeTrue();
        roles3.Should().Contain("Support");
    }
}
