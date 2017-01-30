using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Mono.Cecil;
using NuGet;
using SolutionAudit.AuditInformation;

namespace SolutionAudit.Extensions
{
    static class NuGet
    {
        public static bool IsStronglyNamed(this IPackage package)
        {
            return package.Assemblies().Any(a => a.Name.HasPublicKey);
        }

        private static IEnumerable<IPackage> GetDependencies(this IPackageMetadata package, IEnumerable<IPackage> packageList, FrameworkName targetFramework)
        {
            var dependentIds = package.GetCompatiblePackageDependencies(targetFramework).Select(d => d.Id);
            return packageList.Where(p => dependentIds.Contains(p.Id));
        }

        public static IPackage GetPackage(this PackageReference packageReference, IPackageRepository repository)
        {
            try {
                return PackageHelper.ResolvePackage(repository, packageReference.Id, packageReference.Version);
            }
            catch (InvalidOperationException)
            {
                Console.Error.WriteLine("Could not find package {0}; you probably need to perform a nuget restore.", packageReference);
                return null;
            }
        }

        public static IEnumerable<IPackage> GetDirectPackages(this IEnumerable<IPackage> packages, FrameworkName targetFramework)
        {
            var packageList = packages as IList<IPackage> ?? packages.ToList();

            //Get all packages that have parents
            var indirectPackages = packageList.SelectMany(package => package.GetDependencies(packageList, targetFramework));

            return packageList.Except(indirectPackages);
        }

        public static IEnumerable<SnapshotPackage> GetSnapshotPackages(this IEnumerable<IPackage> references)
        {
            return references.Where(r => !r.IsReleaseVersion()).Select(r => new SnapshotPackage(r));
        }

        private static IEnumerable<AssemblyDefinition> Assemblies(this IPackage package)
        {
            return package.AssemblyReferences
                .Where(
                    reference =>
                    {
                        var extension = Path.GetExtension(reference.Name);
                        return extension != null && Constants.AssemblyReferencesExtensions.Contains(extension.ToLower());
                    })
                .Select(a => AssemblyDefinition.ReadAssembly(a.GetStream()));
        }

        public static IEnumerable<TypeDefinition> GetTypes(this IPackage package)
        {
            return package.Assemblies().SelectMany(a => a.MainModule.GetTypes());
        }

        public static IEnumerable<string> GetAssemblyPaths(this IPackage package)
        {
            var packageString = String.Format("{0}.{1}", package.Id, package.Version);
            return package.AssemblyReferences.Select(r => Path.Combine(packageString, r.Path));
        }
    }
}
