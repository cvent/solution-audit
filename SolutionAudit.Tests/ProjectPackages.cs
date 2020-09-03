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
    public class ProjectPackages
    {
        private ISolution RoslynSolution { get; set; }
        private readonly FrameworkName _targetFramework = new FrameworkName(".NETFramework, Version=4.5");
        private Options Options { get; set; }

        [SetUp]
        public void Setup()
        {
            var solutionPath = Path.GetFullPath("test/ProjectPackages/ProjectPackages.sln");
            RoslynSolution = Solution.Load(solutionPath);
            Options = new Options {Project = true};
        }

        [Test]
        public void PackageReference()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("PackageReference"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.ProjectPackages.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> {"- Cvent.Framework.Localization => Project Reference"}, audit);
        }
    }
}
