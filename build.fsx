// include Fake lib
#r "packages/FAKE/tools/FakeLib.dll"
open System

open Fake
open Fake.AssemblyInfoFile
open Fake.EnvironmentHelper
open Fake.Git

// Properties
let buildDir = "./build/"
let testDir  = "./test/"
let packagingDir = "./packaging"
let testDlls = !! (testDir + "/*Test.dll")

// Packaging info
let authors = ["Dave Tucker"]
let title = "Sharp9P"
let description = "A 9P Client/Server Library written in C#"
let summary = description
let company = "Docker"
let copyright = "Copyright © Docker, Inc. 2016"
let versionBase = "0.1.4"
let buildNumber = environVarOrDefault "APPVEYOR_BUILD_NUMBER" "9999"
let version = sprintf "%s.%s" versionBase buildNumber
let commitHash = Information.getCurrentHash()
let accessKey = environVar "NUGET_API_KEY"
let push = environVarOrDefault "APPVEYOR_REPO_TAG" "false" |> Boolean.Parse

// Targets
Target "Clean" (fun _ ->
  CleanDirs [buildDir; testDir; packagingDir]
)

Target "SetVersions" (fun _ ->
  CreateCSharpAssemblyInfo "./Sharp9P/Properties/AssemblyInfo.cs"
    [ 
      Attribute.Title title
      Attribute.Description description
      Attribute.Guid "12dd4614-0f72-4deb-a9d1-37d825b9a07b"
      Attribute.Company company
      Attribute.Copyright copyright
      Attribute.Version version
      Attribute.FileVersion version
      Attribute.Metadata("githash", commitHash)
    ]
)

Target "CompileApp" (fun _ ->
  !! @"**/*.csproj"
  -- "*Test/*.csproj"
  |> MSBuildRelease buildDir "Build"
  |> Log "AppBuild-Output: "
)

Target "CompileTest" (fun _ ->
  !! @"*Test/*.csproj"
  |> MSBuildDebug testDir "Build"
  |> Log "TestBuild-Output: "
)

Target "TestApp" (fun _ ->
  testDlls
    |> Fake.Testing.NUnit3.NUnit3 (fun p -> 
      {p with
        ToolPath = "./packages/NUnit.ConsoleRunner/tools/nunit3-console.exe"
        ShadowCopy = false; 
        ResultSpecs = [testDir @@ "TestResults.xml"]})
)

Target "PackageApp" (fun _ ->
  let net45Dir = packagingDir @@ "lib/net45/"
  CopyFiles net45Dir [ (buildDir @@ "Sharp9P.dll")
                       (buildDir @@ "Sharp9P.pdb")
  ]
  CopyFile packagingDir "LICENSE"
  NuGet (fun p -> 
    {p with
      Files = [
              (buildDir + "*.dll", None, None)
              (buildDir + "*.pdb",None, None)
      ]
      Authors = authors
      Project = title
      Description = description                             
      OutputPath = packagingDir
      Summary = summary
      WorkingDir = packagingDir
      Version = version
      AccessKey = accessKey
      Publish = push
      PublishUrl = "https://api.nuget.org/v3/index.json"
      })
      "Sharp9P.nuspec"
)

// Dependencies
"Clean"
  ==> "SetVersions"
  ==> "CompileApp"
  ==> "CompileTest"
  ==> "TestApp"
  ==> "PackageApp"

// start build
RunTargetOrDefault "PackageApp"
