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
using System.IO;
using System.Linq;
using CommandLine;

namespace SolutionAudit
{
    public static class Program
    {

        public static int Main(string[] args)
        {
            var options = Parser.Default.ParseArguments<Options>(args);
            if (options.Errors.Any()) return 2;
            if (!File.Exists(options.Value.SolutionPath)) return 3;

            var solutionPath = Path.GetFullPath(options.Value.SolutionPath);

            var solution = new AuditSolution(solutionPath, options.Value);

            Console.Write(solution.ToString());

            var result = solution.PassAudit ? 0 : 1;

            Console.ReadKey();

            return result;
        }
    }
}
