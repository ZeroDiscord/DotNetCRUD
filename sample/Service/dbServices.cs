using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Text.Json;



public class dbServices{
    IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    //MySqlConnection conn = null; // this will store the connection which will be persistent 
    private readonly Dictionary<string, string> _acceptReferalURL = new Dictionary<string, string>();
    MySqlConnection connPrimary = null; // this will store the connection which will be persistent 
    MySqlConnection connReadOnly = null;

    public  dbServices() // constructor
    {
        //_appsettings=appsettings;
        // connectDBPrimary();
        // connectDBReadOnly();
    } 
        public string connectDBPrimary()
    {   
        try
        {
            connPrimary = new MySqlConnection(appsettings["db:connStrPrimary"]);
            connPrimary.Open();
            return "Connected";
        }
        catch (Exception ex)
        {
            //throw new ErrorEventArgs(ex); // check as this will throw exception error
            Console.WriteLine(ex);
            return ex.ToString();
        }
    }
    private void connectDBReadOnly()
    {  
        try
        {
            connReadOnly = new MySqlConnection(appsettings["db2:connStrReadOnly"]);
            connReadOnly.Open();
        }
        catch (Exception ex)
        {
            //throw new ErrorEventArgs(ex); // check as this will throw exception error
            Console.WriteLine(ex);
        }
    }


    public DataSet getDataSQL(string sq)
    {
        try
        {
            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(appsettings["db:connStrPrimary"]);
            MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(sq, conn);
            MySql.Data.MySqlClient.MySqlDataAdapter dta = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd);
            cmd.CommandTimeout = 300;
            DataSet d = new DataSet();
            dta.Fill(d); 
            return d;
        }
        catch (Exception ex)
        {
            return null;
        }

    }


    public List<List<object[]>> executeSQL(string sq, MySqlParameter[] prms)
    {
        var allTables = new List<List<object[]>>();

        try
        {
            // Ensure the connection is open
            using (var conn = new MySqlConnection(appsettings["db:connStrPrimary"])) // Use the connection string
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sq;
                    if (prms != null)
                        cmd.Parameters.AddRange(prms);

                    using (var trans = conn.BeginTransaction())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            using (var dr = cmd.ExecuteReader())
                            {
                                do
                                {
                                    var tblRows = new List<object[]>();
                                    while (dr.Read())
                                    {
                                        var values = new object[dr.FieldCount];
                                        dr.GetValues(values);
                                        tblRows.Add(values);
                                    }
                                    allTables.Add(tblRows);
                                } while (dr.NextResult());
                            } 
                            trans.Commit(); // Commit the transaction if no errors
                        }
                        catch
                        {
                            trans.Rollback(); // Rollback on error 
                            conn.Close();
                            throw; // Rethrow to be handled outside
                        }
                        finally{ 
                            conn.Close();
                        }
                    }
                } 
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null; // Handle the exception appropriately
        }

        return allTables;
    }




    public List<Dictionary<string, object>[]> ExecuteSQLName(string sq, MySqlParameter[] prms)
    {
        MySqlTransaction transaction = null;
        List<Dictionary<string, object>[]> allTables = new List<Dictionary<string, object>[]>();

        try
        {
            using (var conn = new MySqlConnection(appsettings["db:connStrPrimary"])) // Use the connection string
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sq;
                    if (prms != null)
                        cmd.Parameters.AddRange(prms);

                    using (var trans = conn.BeginTransaction())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                do
                                {
                                    List<Dictionary<string, object>> tblRows = new List<Dictionary<string, object>>();

                                    while (reader.Read())
                                    {
                                        Dictionary<string, object> values = new Dictionary<string, object>();

                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            string columnName = reader.GetName(i);
                                            object columnValue = reader.GetValue(i);
                                            values[columnName] = columnValue;
                                        }

                                        tblRows.Add(values);
                                    }

                                    allTables.Add(tblRows.ToArray());
                                } while (reader.NextResult());
                            }

                            trans.Commit(); // Commit the transaction if no errors
                            conn.Close();
                        }
                        catch
                        {

                            trans.Rollback(); // Rollback on error
                            conn.Close();
                            throw; // Rethrow to be handled outside
                        }
                        finally {
                            conn.Close();
                        }
                    }
                }
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            transaction?.Rollback();
            //connPrimary?.Close(); 
            return null;
        } 

        return allTables;
    }


    public List<List<Object[]>>  executeSQLpcmdb(string sq,MySqlParameter[] prms) // this will return the database response the last partameter is to allow selection of connectio id
    {

            MySqlTransaction trans=null;
             List<List<Object[]>> allTables=new List<List<Object[]>>();

        MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(appsettings["db:connStrPrimary"]);
        try
        {
            // if (connReadOnly == null)
            //    connectDBReadOnly();
            conn.Open();
            trans = conn.BeginTransaction();

            var cmd = conn.CreateCommand();
            cmd.CommandText = sq;
            if (prms != null)
                cmd.Parameters.AddRange(prms);

            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                do
                {
                    List<Object[]> tblRows = new List<Object[]>();
                    while (dr.Read())
                    {
                        object[] values = new object[dr.FieldCount]; // create an array with sixe of field count
                        dr.GetValues(values); // save all values here
                        tblRows.Add(values); // add this to the list array
                    }
                    allTables.Add(tblRows);
                } while (dr.NextResult());
            }
            Console.Write("Database Operation Completed Successfully");
            trans.Commit(); // check thee functions 
            return allTables; // if success return allTables
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            trans.Rollback(); // check these functions
            conn.Close();
            return null; // if error return null
        }
        finally {
            conn.Close();
        } 
    }
    public async Task<string> ApiCall(requestData request,string endPoint)
        {
            // Serialize the entire request object
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var jsons = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);

            // var url = "http://170.170.170.22:7454/api/AI/apiToApiCall";
            var url = _acceptReferalURL["acceptReferalURL"] + endPoint;

            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            using var httpClient = new HttpClient(httpClientHandler);
            var content = new StringContent(jsons, Encoding.UTF8, "application/json");

            // Send the API request
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            else
            {
                // Handle unsuccessful request (log, throw exception, etc.)
                Console.WriteLine($"API request failed with status code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                return string.Empty; // Or throw an exception
            }
        }
}