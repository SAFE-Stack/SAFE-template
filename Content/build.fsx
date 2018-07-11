#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

-:cnd:noEmit
#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif
+:cnd:noEmit

open System

open Fake.Core
open Fake.DotNet
open Fake.IO

open SAFE.Build

let SAFE = SAFEBuild (fun x ->
    { x with
        RootPath = __SOURCE_DIRECTORY__
//#if (js-deps == "npm")
        JsDeps = NPM
//#else
        JsDeps = Yarn
//#endif
//#if (deploy == "docker")
        Docker =
            { DockerUser = "safe-template"
              DockerImageName = "safe-template" }
//#endif
    } )

Target.create "Clean" (fun _ ->
    SAFE.CleanBuildDirs ()
)

Target.create "InstallClient" (fun _ ->
    SAFE.RestoreClient ()
)

Target.create "RestoreServer" (fun _ ->
    SAFE.RestoreServer ()
)

Target.create "Build" (fun _ ->
    SAFE.BuildServer ()
    SAFE.BuildClient ()
)

Target.create "Run" (fun _ ->
    [ SAFE.RunServer; SAFE.RunClient; SAFE.RunBrowser ]
    |> SAFE.RunInParallel
)

//#if (deploy == "docker")
Target.create "Bundle" (fun _ ->
    SAFE.Docker.Budle ()
)

Target.create "Docker" (fun _ ->
    SAFE.Docker.Build ()
)
//#endif
//#if (deploy == "azure")
Target.create "Bundle" (fun _ ->
    SAFE.Azure.Bundle ()
)

Target.create "ArmTemplate" (fun _ ->
    SAFE.Azure.ProvisionARM ()
)

Target.create "AppService" (fun _ ->
    SAFE.Azure.Deploy ()
)
//#endif

open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "Build"
//#if (deploy == "docker")
    ==> "Bundle"
    ==> "Docker"
//#elseif (deploy == "azure")
    ==> "Bundle"
    ==> "ArmTemplate"
    ==> "AppService"
//#endif

"InstallClient"
    ==> "RestoreServer"
    ==> "Run"

Target.runOrDefault "Build"
