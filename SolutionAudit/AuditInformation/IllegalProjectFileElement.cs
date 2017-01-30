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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;

namespace SolutionAudit.AuditInformation
{
    public class IllegalProjectFileElement
    {
        public static IllegalProjectFileElement FromTarget(ProjectTargetInstance target)
        {
            return new IllegalProjectFileElement(new TargetDescriptor(target));
        }

        public static IllegalProjectFileElement FromElement(ProjectTargetInstanceChild targetChild)
        {
            return new IllegalProjectFileElement(new TargetChildDescriptor(targetChild));
        }

        public static IllegalProjectFileElement FromImport(ProjectImportElement import)
        {
            return new IllegalProjectFileElement(new ImportDescriptor(import));
        }

        private IProjectFileElementDescriptor _descriptor;

        private IllegalProjectFileElement(IProjectFileElementDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public override string ToString()
        {
            return string.Format("The {0} element at {1} must be removed.",
                _descriptor.ElementString,
                _descriptor.Location.LocationString);
        }

        private interface IProjectFileElementDescriptor
        {
            String ElementString { get; }
            Microsoft.Build.Construction.ElementLocation Location { get; }
        }

        private class TargetDescriptor : IProjectFileElementDescriptor
        {
            private ProjectTargetInstance _target;

            public TargetDescriptor(ProjectTargetInstance target)
            {
                _target = target;
            }

            public string ElementString
            {
                get { return string.Format("<Target Name=\"{0}\" ...>", _target.Name); }
            }

            public Microsoft.Build.Construction.ElementLocation Location
            {
                get { return _target.Location; }
            }
        }

        private class TargetChildDescriptor : IProjectFileElementDescriptor
        {
            private ProjectTargetInstanceChild _child;

            public TargetChildDescriptor(ProjectTargetInstanceChild child)
            {
                _child = child;
            }

            public string ElementString
            {
                get { return string.Format(@"<Error Condition=""!Exists('$(SolutionDir)\.nuget\NuGet.targets')"" ...>", _child.Condition); }
            }

            public Microsoft.Build.Construction.ElementLocation Location
            {
                get { return _child.Location; }
            }
        }

        private class ImportDescriptor : IProjectFileElementDescriptor
        {
            private ProjectImportElement _import;

            public ImportDescriptor(ProjectImportElement import)
            {
                _import = import;
            }

            public string ElementString
            {
                get { return string.Format("<Import Project=\"{0}\" ...>", _import.Project); }
            }

            public Microsoft.Build.Construction.ElementLocation Location
            {
                get { return _import.Location; }
            }
        }
    }
}
