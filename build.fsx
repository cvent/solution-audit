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

#I @"packages/FAKE/tools/"
#r @"FakeLib.dll"
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.NuGet
open Fake.IO.Globbing.Operators
open Fake.DotNet.Testing
open Fake.IO
open System.Net
open NUnit3

// Get Tls 1.2 working for nuget restores with old nuget executables
ServicePointManager.SecurityProtocol <- ServicePointManager.SecurityProtocol ||| SecurityProtocolType.Tls12

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

Target.create "Clean" (fun _ ->
    MSBuild.runWithDefaults "Clean" solutions |> (fun _ -> ())
)

Target.create "Build" (fun _ ->
    MSBuild.runWithDefaults "Build" solutions |> (fun _ -> ())
)

Target.create "RestoreTestPackages" (fun _ ->
    testPackages
    |> Seq.iter (fun s -> Restore.RestorePackage (fun p ->
        { p with
            OutputPath = Path.combine (Path.getDirectory (Path.getDirectory s)) "packages"}) s)
)

Target.create "Test" (fun _ ->
    NUnit3.run ( fun p ->
        {p with
            Domain = NUnit3DomainModel.DefaultDomainModel
            ToolPath = "packages/NUnit.ConsoleRunner/tools/nunit3-console.exe"
        })
        testDlls
)

Target.create "NuPack" ( fun _ ->
    NuGet.NuGetPack (fun p ->
        {p with
            Project = project
            Authors = authors
            Summary = summary
            Description = summary
            Version = "1.3.0"
            Publish = false
            Files = [
                    (@"..\SolutionAudit\bin\Release\*", Some "tools", None)
            ] })
      "SolutionAudit.nuspec"
)

"Clean"
    ==> "Build"
    ==> "RestoreTestPackages"
    ==> "Test"
    ==> "NuPack"

Target.runOrDefault "Build"