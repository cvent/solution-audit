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

namespace SolutionAudit.AuditInformation
{
    public class IllegalFile
    {
        private string _filePath;

        public IllegalFile(string filePath)
        {
            _filePath = filePath;
        }

        public override string ToString()
        {
            return String.Format("Illegal solution file/folder must be removed: {0}", _filePath);
        }
    }
}
