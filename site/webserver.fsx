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
open Suave.Successful
open Suave.Web
open System.Net

open System
open System.Net
open System.Data
open System.Net.Http
open System.Configuration;
open System.Data.SqlClient;

type Rows = {TotalRows : int}

let getRows () = 
    try
        let connectionString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_CarParkDbConnection");
        let con = new SqlConnection(connectionString)
        con.Open()
        let query = "SELECT COUNT(*) as Rows FROM dbo.CarParkStats"
        use cmd = new SqlCommand(query, con)
        let count = cmd.ExecuteScalar() :?> int
        ({TotalRows = count}).ToString()
    with
        | ex -> sprintf "%s" (ex.ToString())    

let serverConfig = 
    let port = getBuildParamOrDefault "port" "8083" |> Sockets.Port.Parse
    { defaultConfig with bindings = [ HttpBinding.create HTTP IPAddress.Loopback port ] }

let app = choose [
            GET >=> path "/rows" >=> OK (getRows ())
        ]

startWebServer serverConfig (OK (getRows ()))