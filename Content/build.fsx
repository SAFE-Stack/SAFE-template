#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open Fake.Core

Target.initEnvironment ()

Target.create "Clean" (fun _ ->
    SAFE.Core.clean ()
)

Target.create "InstallClient" (fun _ ->
    SAFE.Core.installClient ()
)

Target.create "Build" (fun _ ->
    SAFE.Core.build ()
)

Target.create "Bundle" (fun _ ->
    SAFE.Core.bundle ()
)

Target.create "Run" (fun _ ->
    SAFE.Core.run ()
)

// plugin targets

open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "Build"
    ==> "Bundle"

"Clean"
    ==> "InstallClient"
    ==> "Run"

// plugin operators

Target.runOrDefaultWithArguments "Build"