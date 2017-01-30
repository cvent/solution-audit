#I @"packages/FAKE/tools/"
#r @"FakeLib.dll"
open Fake

// The name of the project
// (used by name of a NuGet package)
let project = "Cvent.SolutionAudit"

// Short summary of the project
// (used as a short summary for NuGet package)
let summary = "An auditor for checking consistency and sanity for NuGet packages."

// List of author names (for NuGet package)
let authors = [ "Jonathan Morley" ]

let solutions = ["./SolutionAudit.sln"]
let testPackages = !! "./test/**/packages.config"
let testDlls = !! "./**/bin/**/*.Tests.dll"

Target "Clean" (fun _ ->
    MSBuildWithDefaults "Clean" solutions
    |> Log "AppClean-Output: "
)

Target "BuildSolutions" (fun _ ->
    MSBuildWithDefaults "Build" solutions
    |> Log "AppBuild-Output: "
)

Target "RestoreTestPackages" (fun _ ->
    testPackages
    |> Seq.iter (fun s -> RestorePackage (fun p ->
        { p with
            OutputPath = (DirectoryName (DirectoryName s)) @@ "packages"}) s)
)

Target "Test" (fun _ ->
     NUnit id testDlls
)

Target "NuPack" ( fun _ ->
    NuGet (fun p ->
        {p with
            Project = project
            Authors = authors
            Summary = summary
            Description = summary
            Version = "1.1.1"
            Publish = false
            Files = [
                    (@"..\SolutionAudit\bin\Release\*", Some "tools", None)
            ] })
      "SolutionAudit.nuspec"
)

Target "Default" DoNothing

"Clean"
    ==> "BuildSolutions"
    ==> "Default"
    ==> "RestoreTestPackages"
    ==> "Test"
    ==> "NuPack"

RunTargetOrDefault "Default"
