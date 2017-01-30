using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SolutionAudit.Tests
{
    [TestFixture]
    class FailingSolution
    {
        private AuditSolution _solution;

        [SetUp]
        public void Setup()
        {
            var solutionPath = Path.GetFullPath("../../../test/NugetTargets/NugetTargets.sln");
            var options = new Options { NugetTargets = true };
            _solution = new AuditSolution(solutionPath, options);
        }

        [Test]
        public void SolutionPassesAudit()
        {
            var solutionPath = Path.GetFullPath("../../../test/ProjectPackages/ProjectPackages.sln");
            var options = new Options { NugetTargets = true };
            var solution = new AuditSolution(solutionPath, options);
            Assert.That(solution.PassAudit, Is.True);
            Assert.That(solution.GetErrors(), Is.EqualTo(""));
            Assert.That(solution.IllegalSolutionFiles.Count(), Is.EqualTo(0));
        }

        [Test]
        public void SolutionFailsAudit()
        {
            Assert.That(_solution.PassAudit, Is.False);
        }

        [Test]
        public void SolutionProjects()
        {
            Assert.That(_solution.Projects.Count(), Is.EqualTo(2));
        }

        [Test]
        public void IllegalSolutionFiles()
        {
            Assert.That(_solution.IllegalSolutionFiles.Count(), Is.EqualTo(1));
            Assert.That(_solution.IllegalSolutionFiles.First().ToString(),
                Is.StringContaining(@"\NugetTargets\.nuget"));
        }

        [Test]
        public void SolutionErrors()
        {
            var errorText = _solution.GetErrors();
            Assert.That(errorText, Is.StringContaining(@"\NugetTargets\.nuget"));

            foreach (var project in _solution.Projects.Where(p => !p.PassAudit()))
            {
                Assert.That(errorText, Is.StringContaining(project.ToString()));
            }
        }

    }
}
