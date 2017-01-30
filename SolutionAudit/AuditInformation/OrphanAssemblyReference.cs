using System.Collections.Generic;
using NuGet;
using NuGet.Runtime;

namespace SolutionAudit.AuditInformation
{
    public class OrphanAssemblyBinding : Wrapper<AssemblyBinding>
    {
        public OrphanAssemblyBinding(AssemblyBinding wrapped) : base(wrapped)
        {
        }

        public override string ToString()
        {
            return string.Format("& {0} (Only present as an assembly binding)", Wrapped.Name);
        }
    }
}