module Power

open System

open Azure.Identity
open Azure.ResourceManager
open Azure.ResourceManager.Compute
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

let getVm =
    let rgName =
        Environment.GetEnvironmentVariable "RG_NAME"

    let vmName =
        Environment.GetEnvironmentVariable "VM_NAME"

    let credentials = new AzureCliCredential()
    let client = new ArmClient(credentials)

    task {
        let! subscription = client.GetDefaultSubscriptionAsync()
        let! resourceGroups = subscription.GetResourceGroups().GetAsync(rgName)
        let rg = resourceGroups.Value
        let! vm = rg.GetVirtualMachines().GetAsync(vmName)
        return vm.Value
    }

[<FunctionName("start")>]
let start ([<HttpTrigger(AuthorizationLevel.Function, "post")>] req: HttpRequest) (log: ILogger) =
    let vm = getVm.Result
    let operation = vm.PowerOn(waitForCompletion = true)

    if operation.HasCompleted then
        OkObjectResult("Completed")
    else
        OkObjectResult("Ongoing")

[<FunctionName("stop")>]
let stop ([<HttpTrigger(AuthorizationLevel.Function, "post")>] req: HttpRequest) (log: ILogger) =
    let vm = getVm.Result
    let operation = vm.PowerOff(waitForCompletion = true)

    if operation.HasCompleted then
        OkObjectResult("Completed")
    else
        OkObjectResult("Ongoing")

[<FunctionName("status")>]
let status ([<HttpTrigger(AuthorizationLevel.Function, "post")>] req: HttpRequest) (log: ILogger) =
    let vm = getVm.Result
    let view = vm.InstanceView().Value
    OkObjectResult(view.Statuses)
