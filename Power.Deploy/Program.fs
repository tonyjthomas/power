module Power.Deploy

open System
open Farmer
open Farmer.Builders

[<Literal>]
let rgName = "RG_NAME"

[<Literal>]
let vmName = "VM_NAME"

let resolvedRgName = Environment.GetEnvironmentVariable rgName
let resolvedVmName = Environment.GetEnvironmentVariable vmName

let func =
    functions {
        name $"func-{resolvedRgName}"
        service_plan_name $"plan-{resolvedRgName}"
        system_identity
        setting rgName resolvedRgName
        setting vmName resolvedVmName
        zip_deploy "package.zip"
    }

let deployment =
    arm {
        location Location.EastUS
        add_resource func
    }

printf "Beginning ARM deployment..."

match deployment |> Deploy.tryExecute resolvedRgName [] with
| Ok _ -> $"Deployment to completed."
| Error e -> $"Deployment failed with error: {e}."
|> printfn "%s"
