// --------------------------------------------------------------------------------------
// Start up Suave.io
// --------------------------------------------------------------------------------------

#r "../packages/FAKE/tools/FakeLib.dll"
open System.Windows.Forms
#r "../packages/Suave/lib/net40/Suave.dll"
#r "Newtonsoft.Json"
#r "System.Net.Http.dll"
#r "System.Configuration"
#r "System.Data"

open Fake
open System.Net

open Suave
open Suave.Web
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.Successful

open Newtonsoft.Json

open System
open System.Net
open System.Data
open System.Net.Http
open System.Configuration;
open System.Data.SqlClient;

type Rows = {TotalRows : int}

type CarParkDatabaseRecord = {
    bayCount : uint32
    free : uint32
    occupied : uint32
    bayType : string
    carParkId : string
    timestamp : System.DateTime
}

let constructSqlParameter (name : string) (paramType : SqlDbType) (value : obj) =
    let p = SqlParameter(name, paramType)
    p.Value <- value
    p

let getOpenConnection () =
    let connectionString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_CarParkDbConnection");
    let con = new SqlConnection(connectionString)
    con.Open()
    con

let getRows () = 
    try
        use  con = getOpenConnection ()
        let query = "SELECT COUNT(*) as Rows FROM dbo.CarParkStats"
        use cmd = new SqlCommand(query, con)
        let count = cmd.ExecuteScalar() :?> int
        JsonConvert.SerializeObject({TotalRows = count})
    with
        | ex -> sprintf "%s" (ex.ToString())

let constructRecord (reader : SqlDataReader) = 
    {
        bayCount = (uint32 (reader.GetInt32(reader.GetOrdinal("BayCount"))))
        free = (uint32 (reader.GetInt32(reader.GetOrdinal("FreeSpaces"))))
        occupied = (uint32 (reader.GetInt32(reader.GetOrdinal("OccupiedSpaces"))))
        bayType = reader.GetString(reader.GetOrdinal("BayType"))
        carParkId = reader.GetString(reader.GetOrdinal("CarParkId"))
        timestamp = reader.GetDateTime(reader.GetOrdinal("InsertTime"))
    }

let getData ((f,t) : string * string) =
    let fromDate = DateTime.Parse(f)
    let toDate = DateTime.Parse(t)
    use con = getOpenConnection ()
    let query = "SELECT * FROM dbo.CarParkStats a WHERE a.BayType != 'Disabled' AND a.InsertTime BETWEEN @from AND @to"
    use cmd = new SqlCommand(query, con)
    cmd.Parameters.Add(constructSqlParameter "@from" (SqlDbType.DateTime2) fromDate) |> ignore
    cmd.Parameters.Add(constructSqlParameter "@to" (SqlDbType.DateTime2) toDate) |> ignore
    let reader = cmd.ExecuteReader()
    if reader.HasRows then
        let d = [
                    while reader.Read()
                        do yield constructRecord reader
                ]
        OK <| JsonConvert.SerializeObject(d)
    else  OK <| "[]"

let serverConfig = 
    let port = getBuildParamOrDefault "port" "8083" |> Sockets.Port.Parse
    { defaultConfig with bindings = [ HttpBinding.create HTTP IPAddress.Loopback port ] }

let app = choose [
            GET >=> path "/rows" >=> request (fun _ -> OK <| getRows ()) >=> Writers.setMimeType "application/json; charset=utf-8"
            pathScan "/data/from/%s/to/%s" getData >=> Writers.setMimeType "application/json; charset=utf-8"
        ]

startWebServer serverConfig app
