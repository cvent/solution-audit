using System;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using NuGet;

namespace SolutionAudit.AuditInformation
{
    public class MismatchedGuid : Wrapper<Guid>
    {
        public MismatchedGuid(Guid wrapped, String referenceName, String itemType) : base(wrapped)
        {
            ReferenceName = referenceName;
            ItemType = itemType;
        }

        private string ReferenceName { get; set; }
        private string ItemType { get; set; }

        public override string ToString()
        {
            return string.Format("& {0} (Bad {1} GUID)", ReferenceName, ItemType);
        }
    }
}