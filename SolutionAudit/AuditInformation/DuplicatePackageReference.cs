using System.Collections.Generic;
using NuGet;

namespace SolutionAudit.AuditInformation
{
    public class DuplicatePackageReference : Wrapper<PackageReference>
    {
        public DuplicatePackageReference(PackageReference wrapped, string fileName, string dllName = null) : base(wrapped)
        {
            FileName = fileName;
            DllName = dllName;
        }

        private string FileName { get; set; }
        private string DllName { get; set; }

        public override string ToString()
        {
            var dllName = DllName == null ? string.Empty : "/" + DllName;
            return string.Format("& {0}{2} (More than 1 entry in {1})", Wrapped, FileName, dllName);
        }
    }
}