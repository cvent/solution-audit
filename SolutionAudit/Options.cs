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

using CommandLine;

namespace SolutionAudit
{
    public class Options
    {
        private bool _unused;
        private bool _project;
        private bool _missing;
        private bool _inconsistent;
        private bool _snapshot;
        private bool _fileDiff;
        private bool _redirectMismatch;
        private bool _nugetTargets;
        private bool _all = true;

        [Value(0, Required = true)]
        public string SolutionPath { get; set; }

        public bool All
        {
            get { return _all; }
        }

        [Option('u', "unused", DefaultValue = false, HelpText = "Show unused dependencies [-].")]
        public bool Unused
        {
            get { return _unused || All; }
            set
            {
                if (_unused != value) _all = false;
                _unused = value;
            }
        }

        [Option('p', "project", DefaultValue = false, HelpText = "Show references that are brought in as nuget packages when a project reference would suffice [-].")]
        public bool Project
        {
            get { return _project || All; }
            set
            {
                if (_project != value) _all = false;
                _project = value;
            }
        }

        [Option('m', "missing", DefaultValue = false, HelpText = "Show missing packages [+].")]
        public bool Missing
        {
            get { return _missing || All; }
            set
            {
                if (_missing != value) _all = false;
                _missing = value;
            }
        }

        [Option('i', "inconsistent", DefaultValue = false, HelpText = "Show inconsistent versions of packages [=].")]
        public bool Inconsistent
        {
            get { return _inconsistent || All; }
            set
            {
                if (_inconsistent != value) _all = false;
                _inconsistent = value;
            }
        }

        [Option('s', "snapshot", DefaultValue = false, HelpText = "Show snapshot packages [$].")]
        public bool Snapshot
        {
            get { return _snapshot || All; }
            set
            {
                if (_snapshot != value) _all = false;
                _snapshot = value;
            }
        }

        [Option('f', "fileDiff", DefaultValue = false, HelpText = "Give information on out of sync packages/assemblies between csproj, packages.config and orphan binding redirects [&].")]
        public bool FileDiff
        {
            get { return _fileDiff || All; }
            set
            {
                if (_fileDiff != value) _all = false;
                _fileDiff = value;
            }
        }

        
        [Option('r', "redirectMismatch", DefaultValue = false, HelpText = "Give information on mismatched assembly versions between csproj and binding redirect targets [^].")]
        public bool RedirectMismatch
        {
            get { return _redirectMismatch || All; }
            set
            {
                if (_redirectMismatch != value) _all = false;
                _redirectMismatch = value;
            }
        }

        [Option('t', "nugetTargets", DefaultValue = false, HelpText = "Alert on any projects that import the Nuget targets or if a Nuget.targets file is present in the solution.")]
        public bool NugetTargets
        {
            get { return _nugetTargets || All; }
            set
            {
                if (_nugetTargets != value) _all = false;
                _nugetTargets = value;
            }
        }

        [Option('N', "nugetCommands", DefaultValue = false, HelpText = "Show output as nuget commands that need to be run, instead of files.")]
        public bool NugetCommands { get; set; }

        [Option('U', "unusedUsings", DefaultValue = false, HelpText = "Take into account unused usings when calculating the unused packages (use this with the -i flag).")]
        public bool UnusedUsings { get; set; }

        [Option('I', "allInconsistent", DefaultValue = false, HelpText = "Get all inconsistent packages (default: Just packages with strongly named assemblies).")]
        public bool AllInconsistent { get; set; }
    }
}
