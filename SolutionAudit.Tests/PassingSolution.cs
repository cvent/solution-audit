// Copyright 2017 Cvent, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
