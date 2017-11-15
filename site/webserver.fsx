// --------------------------------------------------------------------------------------
// Start up Suave.io
// --------------------------------------------------------------------------------------

#r "../packages/FAKE/tools/FakeLib.dll"
#r "../packages/Suave/lib/net40/Suave.dll"
#r "Newtonsoft.Json"
#r "System.Net.Http.dll"
#r "System.Configuration"
#r "System.Data"

open Fake
open Suave
open Suave.Http.Successful
open Suave.Web
open Suave.Types
open System.Net

open System
open System.Net
open System.Data
open Newtonsoft.Json
open System.Net.Http
open System.Configuration;
open System.Data.SqlClient;

let getRows () = OK "TEST3"

let serverConfig = 
    let port = getBuildParamOrDefault "port" "8083" |> Sockets.Port.Parse
    { defaultConfig with bindings = [ HttpBinding.mk HTTP IPAddress.Loopback port ] }

startWebServer serverConfig (getRows ())
