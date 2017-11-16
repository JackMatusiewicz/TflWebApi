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
open System.Net
open System.Text

open Suave.Web
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Cookie

open System
open System.Net
open System.Data
open Newtonsoft.Json
open System.Net.Http
open System.Configuration;
open System.Data.SqlClient;

type RowDto = {Rows : int}

let getRows () =
    try
        let connectionString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_CarParkDbConnection");
        let con = new SqlConnection(connectionString)
        con.Open()
        let query = "SELECT COUNT(*) as Rows FROM dbo.CarParkStats"
        use cmd = new SqlCommand(query, con)
        let count = cmd.ExecuteScalar() :?> int
        ({Rows = count}).ToString()
    with
        | ex -> ex.ToString()

let serverConfig = 
    let port = getBuildParamOrDefault "port" "8083" |> Sockets.Port.Parse
    { defaultConfig with bindings = [ HttpBinding.mk HTTP IPAddress.Loopback port ] }

let app = choose [
    GET >=> path "/rows" >=> (OK <| getRows ())
]

startWebServer serverConfig app
