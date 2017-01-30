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

            return solution.PassAudit ? 0 : 1;
        }
    }
}