class: center, middle, inverse

# Solution Audit

---

# What is a 'Solution'?

<div class="mermaid">
graph TB
  subgraph Repository
    S1[Solution 1.sln]
    S2[Solution 2.sln]
    P1[Project 1.csproj]
    P2[Project 2.csproj]
    P3[Project 3.csproj]

    S1 --> P1
    S1 --> P2
    S2 --> P2
    S2 --> P3
  end
</div>

A .NET solution (`.sln` file) is a logical container for .NET projects (`.csproj` files).

---

# What are we 'Auditing'?

Solution Auditor will take a path to a solution file as an input.
It will evaluate that solution against a [set of rules](https://github.com/cvent/solution-audit/tree/master/SolutionAudit/AuditInformation):

1. [Duplicate Packages](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/DuplicatePackageReference.cs)
2. [Illegal Files](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/IllegalFile.cs)
3. [Illegal Elements](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/IllegalProjectFileElement.cs)
4. [Inconsistent Files](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/InconsistentFiles.cs)
5. [Inconsistent Versions](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/InconsistentVersionPackageReference.cs)
6. [Mismatched Guids](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/MismatchedGuid.cs)
7. [Missing Packages](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/MissingPackageDependency.cs)
8. [Orphan Assemblies](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/OrphanAssemblyReference.cs)
9. [Project Packages](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/ProjectPackage.cs)
10. [Snapshot Packages](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/SnapshotPackage.cs)
11. [Unused Packages](https://github.com/cvent/solution-audit/blob/master/SolutionAudit/AuditInformation/UnusedPackage.cs)

---

# Classic csproj vs. Modern csproj

The recommended approach to writing .NET project files has undergone some changes in recent years.

For many years, the `.csproj` XML-based format was the recommended way of defining the contents of a .NET project.

Around 2016, there were plans to migrate to a `project.json` format,
but this was replaced with a modern `.csproj` specification alongside the launch of VS 2017.

#### Major differences

|Classic `.csproj`                         | Modern `.csproj`                 |
|------------------------------------------|----------------------------------|
| Project GUID identifier                  | No project GUID identifier       |
| Project references include GUID          | Project referenced only by path  |
| Packages referenced in `packages.config` | Packages referenced in `.csproj` |
| Transitive packages included in project  | Only direct packages referenced  |
| Large amount of auto-generated fluff     | Trimmed down to essentials       |

---

class: center, middle, inverse

# Classic Solution Audit

---

# Package installation background

When NuGet installs a package, there are a few files that get modified:
1. `packages.config`
  - References to packages to download
2. `<PROJECT>.csproj`
  - References DLLs from packages
  - Calls scripts at build time from packages
3. `Web.config` / `App.config`
  - Binding redirects to recent versions of assemblies

---

.left-column[
## Classic Audits
### Duplicate Packages
]

.right-column[
The following files are not usually modified by hand,
and are therefore especially susceptible to incorrect merge conflicts.

* `packages.config` is usually modified via NuGet.
* `<PROJECT>.csproj` is usually modified via Visual Studio.

A common issue is duplicate entries arising from incorrect merge conflict resolution.
]

---

.left-column[
## Classic Audits
### Duplicate Packages
### Illegal Resources
]

.right-column[
Older versions of NuGet required the presence of few resources that are no longer necessary,
and could potentially interfere with NuGet operations.

#### Illegal Files
The `.nuget` solution directory is no longer required, and should be removed.

#### Illegal Elements
The `nuget.targets` references in `<PROJECT>.csproj` files are no longer required, and should be removed.
]

---

name: inconsistent-files

.left-column[
## Classic Audits
### Duplicate Packages
### Illegal Resources
### Inconsistent Files
]

.right-column[
As mentioned above, NuGet modifies the following files when installing packages:
1. `packages.config`
  - References to packages to download
2. `<PROJECT>.csproj`
  - References DLLs from packages
  - Calls scripts at build time from packages

For a variety of reasons, it may be the case that these files become 'inconsistent',
in that they refer to packages not mentioned in the other.
]

---

.left-column[
## Classic Audits
### Duplicate Packages
### Illegal Resources
### Inconsistent Files
### Inconsistent Versions
]

.right-column[

#### Compile Time

<div class="mermaid" style="max-width: 50%">
graph TB
  subgraph Solution
    P1[Project 1]
    P2[Project 2]
    PK1_1(Package 1 v1.0)
    PK1_2(Package 1 v2.0)
    A1_1["Package 1.dll (v1.0)"]
    A1_2["Package 1.dll (v2.0)"]

    P1 --> PK1_1
    PK1_1 --> A1_1
    P2 --> PK1_2
    PK1_2 --> A1_2
  end
</div>

#### Runtime

<div class="mermaid" style="max-width: 50%">
graph TB
  L1[Project 1.dll]
  L2[Project 2.dll]

  A1["Package 1.dll (v?)"]

  L1 --> A1
  L2 --> A1
</div>

[Assembly unification](https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/redirect-assembly-versions)
can give rise to unexpected versions of assemblies being used by projects.

To reduce the occurrence of this, package should only be specified as a single version across the entire solution.
]

---

.left-column[
## Classic Audits
### Duplicate Packages
### Illegal Resources
### Inconsistent Files
### Inconsistent Versions
### Mismatched Guids
]

.right-column[
Project files contain the following identifiers:
* Project Name
* Project GUID

Solution files contain references to projects based on the following attributes:
  - Project Name
  - Project GUID
  - Relative path to project file

All of the references should be internally consistent within the solution.
Out of the references above, the GUIDs seem to be the easiest to fall out of consistency,
and also the least obvious to identify.
]

---

.left-column[
## Classic Audits
### Duplicate Packages
### Illegal Resources
### Inconsistent Files
### Inconsistent Versions
### Mismatched Guids
### Missing Packages
]

.right-column[
<div class="mermaid">
graph LR
  P1(Package 1)
  P2(Package 2)
  P3(Package 3)

  P1 --> P2
  P2 --> P3
</div>

NuGet requires all transitive packages be listed in the `packages.config` file.

If a package is not installed correctly, it is easy for the transient dependencies to not be included.
]

---

.left-column[
## Classic Audits
### Duplicate Packages
### Illegal Resources
### Inconsistent Files
### Inconsistent Versions
### Mismatched Guids
### Missing Packages
### Orphan Assemblies
]

.right-column[
When a package is installed by NuGet, it may add binding redirects into the `Web.config` or `App.config` files.

If a package is subsequently removed from the project, NuGet _should_ remove the binding redirects too.

There is a potential for these files to not get correctly updated, and reference assemblies from packages that
are no longer present in the project.
]

---

.left-column[
## Classic Audits
### Duplicate Packages
### Illegal Resources
### Inconsistent Files
### Inconsistent Versions
### Mismatched Guids
### Missing Packages
### Orphan Assemblies
### Project Packages
]

.right-column[
<div class="mermaid">
graph TB
  subgraph Solution
    P1[Project 1]
    P2[Project 2]
    PK2("Package 2<br/>(created from Project 2)")

    P1 --> P2
    P2 --> PK2
    P1 -- Not allowed --> PK2
  end
</div>

Projects in a solution that depend on each other, should do so with project dependencies.

If dependencies between projects are instead done via packages, then it can be hard to test changes.
]

---

.left-column[
## Classic Audits
### Duplicate Packages
### Illegal Resources
### Inconsistent Files
### Inconsistent Versions
### Mismatched Guids
### Missing Packages
### Orphan Assemblies
### Project Packages
### Snapshot Packages
]

.right-column[
Pre-release versions of packages should not be depended upon by projects.

When performing a `nuget pack` operation in a library, this assertion is enforced by NuGet.
However, in an application, the `nuget pack` operation may not be used, and it can be
difficult to ensure that pre-release versions are not used.
]

---

.left-column[
## Classic Audits
### Duplicate Packages
### Illegal Resources
### Inconsistent Files
### Inconsistent Versions
### Mismatched Guids
### Missing Packages
### Orphan Assemblies
### Project Packages
### Snapshot Packages
### Unused packages
]

.right-column[
Packages can be 'used' in the following ways:
1. Depended upon by another package.
2. Contains an assembly that is referenced in the project library

Packages that are not 'used' should be removed.
]

---

class: center, middle, inverse

# Modern Solution Audit

---

# Modern Solution Audit

We should now be able to take the opportunity to refactor Solution Auditor by:
* Supporting the modern csproj format as project files in solutions
* Removing some of the features that are no longer necessary with the modern `.csproj` format
* While still retaining those features that still provide value.

---

.left-column[
## Modern Audits
### Duplicate Packages
]

.right-column[

Package references now only reside in a single location, within the `.csproj` file.

This reduces, but does not eliminate the potential for duplicate entries.
]

---

.left-column[
## Modern Audits
### Duplicate Packages
### Illegal Resources
]

.right-column[
Projects that have been migrated from classic `.csproj` may still carry with them `.nuget`
directories from the older version of NuGet.
]

---

.left-column[
## Modern Audits
### Duplicate Packages
### Illegal Resources
### ~~Inconsistent Files~~
]

.right-column[
There is now only a single file storing package references.

Consistency between files is therefore no longer a concern.
]

---

.left-column[
## Modern Audits
### Duplicate Packages
### Illegal Resources
### ~~Inconsistent Files~~
### Inconsistent Versions
]

.right-column[
Assembly unification can still potentially cause issues.

We should maintain this check.
]

---

.left-column[
## Modern Audits
### Duplicate Packages
### Illegal Resources
### ~~Inconsistent Files~~
### Inconsistent Versions
### ~~Mismatched Guids~~
]

.right-column[
While the solution file still carries a GUID referencing the project,
the project file no longer has an identifying Project GUID.

Consistency between them is therefore no longer a concern.
]

---

.left-column[
## Modern Audits
### Duplicate Packages
### Illegal Resources
### ~~Inconsistent Files~~
### Inconsistent Versions
### ~~Mismatched Guids~~
### ~~Missing Packages~~
]

.right-column[
Transitive packages no longer have to be explicitly listed as dependencies.

While this may introduce other types of issues,
it does remove the need to check for missing, dependent packages.
]

---

.left-column[
## Modern Audits
### Duplicate Packages
### Illegal Resources
### ~~Inconsistent Files~~
### Inconsistent Versions
### ~~Mismatched Guids~~
### ~~Missing Packages~~
### Orphan Assemblies
]

.right-column[
`App.config` and `Web.config` still store Binding Redirects,
and could still potentially cause issues.

We should maintain this check.
]

---

.left-column[
## Modern Audits
### Duplicate Packages
### Illegal Resources
### ~~Inconsistent Files~~
### Inconsistent Versions
### ~~Mismatched Guids~~
### ~~Missing Packages~~
### Orphan Assemblies
### Project Packages
]

.right-column[
Configuring Project dependencies as Package dependencies is still going to cause
difficulties while testing changes.

We should maintain this check.
]

---

.left-column[
## Modern Audits
### Duplicate Packages
### Illegal Resources
### ~~Inconsistent Files~~
### Inconsistent Versions
### ~~Mismatched Guids~~
### ~~Missing Packages~~
### Orphan Assemblies
### Project Packages
### Snapshot Packages
]

.right-column[
Snapshot packages (especially in applications) are still undesirable.

We should maintain this check.
]

---

.left-column[
## Modern Audits
### Duplicate Packages
### Illegal Resources
### ~~Inconsistent Files~~
### Inconsistent Versions
### ~~Mismatched Guids~~
### ~~Missing Packages~~
### Orphan Assemblies
### Project Packages
### Snapshot Packages
### Unused packages
]

.right-column[
Unused packages will need to be altered slightly due to project files now only referencing direct Package dependencies.

Otherwise, this check is still useful.
]

---

class: center, middle, inverse

# Questions
