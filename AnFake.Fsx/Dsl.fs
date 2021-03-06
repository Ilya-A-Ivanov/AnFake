﻿[<AutoOpen>]
module AnFake.Fsx.Dsl

open System
open System.IO
open System.Collections.Generic
open AnFake.Core
open System.Runtime.CompilerServices

let inline (~~) (path: string) = FileSystem.AsPath(path)

let inline (!!) (wildcardedPath: string) = FileSystem.AsFileSet(wildcardedPath)

let inline (%%) (basePath: string, wildcardedPath: string) = FileSystem.AsFileSetFrom(wildcardedPath, basePath)

let inline (!!!) (wildcardedPath: string) = FileSystem.AsFolderSet(wildcardedPath)

let inline (=>) target (action: unit -> unit) = TargetExtension.AsTarget(target).Do(fun _ -> action ())

let inline (&=>) (target: Target) (action: unit -> unit) = target.OnFailure(action)

let inline (|=>) (target: Target) (action: unit -> unit) = target.Finally(action)

let inline (<==) target (dependencies: IEnumerable<string>) = TargetExtension.AsTarget(target).DependsOn(dependencies)

let inline (==>) (predecessor: string) successor = 
    TargetExtension.AsTarget(successor).DependsOn(predecessor) |> ignore
    successor

let skipErrors (target: Target) = target.SkipErrors()

let failIfAnyWarning (target: Target) = target.FailIfAnyWarning()

let partialSucceedIfAnyWarning (target: Target) = target.PartialSucceedIfAnyWarning()

let nullInt () = new Nullable<Int32>()

let nullLong () = new Nullable<Int64>()

let nullBool () = new Nullable<bool>()

let doNothing = fun () -> ()

[<Extension>]
type FsxHelper () =
    [<Extension>]
    static member inline Set(props: IDictionary<System.String, System.String>, nameValue: (string * string) list) = 
        for (name, value) in nameValue do
            props.Item(name) <- value

    [<Extension>]
    static member inline Get(props: IDictionary<System.String, System.String>, name: string) = 
        if not <| props.ContainsKey(name) then
            failwithf "Required parameters '%s' is missed." name
        props.Item(name)

    [<Extension>]
    static member inline Get(props: IDictionary<System.String, System.String>, name: string, defValue: string) = 
        if not <| props.ContainsKey(name) then 
            defValue
        else
            props.Item(name)
