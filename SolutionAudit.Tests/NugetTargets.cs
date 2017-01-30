using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NUnit.Framework;
using Roslyn.Services;


namespace SolutionAudit.Tests
{
    [TestFixture]
    class NugetTargets
    {
        private ISolution RoslynSolution { get; set; }
        private readonly FrameworkName _targetFramework = new FrameworkName(".NETFramework, Version=4.5");
        private Options Options { get; set; }

        [SetUp]
        public void Setup()
        {
            var solutionPath = Path.GetFullPath("../../../test/NugetTargets/NugetTargets.sln");
            RoslynSolution = Solution.Load(solutionPath);
            Options = new Options { NugetTargets = true };
        }

        [Test]
        public void WithoutTargets()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("WithoutTargets"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            Assert.IsEmpty(auditProject.IllegalNugetTargets);
        }

        [Test]
        public void WithTargets()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("WithTargets"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var issues = auditProject.IllegalNugetTargets.Select(a => a.ToString());

            var importIssues = issues.Where(a => a.Contains("<Import"));
            Assert.That(importIssues.Count(), Is.EqualTo(1));
            Assert.That(importIssues.First(), Is.StringContaining(@"<Import Project=""$(SolutionDir)\.nuget\NuGet.targets"" ...>"));
            Assert.That(importIssues.First(), Is.StringContaining(@"\test\NugetTargets\WithTargets\WithTargets.csproj (60,3)"));

            var targetIssues = issues.Where(a => a.Contains("<Error"));
            Assert.That(targetIssues.Count(), Is.EqualTo(1));
            Assert.That(targetIssues.First(), Is.StringContaining(@"<Error Condition=""!Exists('$(SolutionDir)\.nuget\NuGet.targets')"" ...>"));
            Assert.That(targetIssues.First(), Is.StringContaining(@"\test\NugetTargets\WithTargets\WithTargets.csproj (65,5)"));
        }
    }
}