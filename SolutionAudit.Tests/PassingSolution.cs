using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SolutionAudit.Tests
{
    [TestFixture]
    class PassingSolution
    {
        private AuditSolution _solution;

        [SetUp]
        public void Setup()
        {
            var solutionPath = Path.GetFullPath("../../../test/ProjectPackages/ProjectPackages.sln");
            var options = new Options { NugetTargets = true };
            _solution = new AuditSolution(solutionPath, options);
        }

        [Test]
        public void SolutionPassesAudit()
        {
            Assert.That(_solution.PassAudit, Is.True);
        }

        [Test]
        public void SolutionProjects()
        {
            Assert.That(_solution.Projects.Count(), Is.EqualTo(2));
        }

        [Test]
        public void IllegalSolutionFiles()
        {
            Assert.That(_solution.IllegalSolutionFiles.Count(), Is.EqualTo(0));
        }

        [Test]
        public void SolutionErrors()
        {
            Assert.That(_solution.GetErrors(), Is.EqualTo(""));
        }

    }
}
