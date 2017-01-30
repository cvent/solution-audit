using System.Collections.Generic;
using NuGet;

namespace SolutionAudit.AuditInformation
{
    public class InconsistentFiles : Wrapper<PackageReference>
    {
        public InconsistentFiles(PackageReference wrapped, params string[] existsInFiles) : base(wrapped)
        {
            ExistsInFiles = existsInFiles;
        }

        private IEnumerable<string> ExistsInFiles { get; set; }

        public override string ToString()
        {
            return string.Format("& {0} (Only present in {1})", Wrapped, string.Join(", ", ExistsInFiles));
        }
    }
}