using System.Data.OleDb;
using System.IO;
using System.Windows;
using HollingerBox.Data;
using HollingerBox.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HollingerBox
{
    public partial class MainWindow : Window
    {
        // Path to your Access .mdb file
        private const string AccessDbPath = @"C:\HollingerBox\EthicsXp_data.mdb";

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Generic method to read all columns from a given table in Access, 
        /// returning a list of Dictionary(column -> value).
        /// </summary>
        private List<Dictionary<string, object>> ReadTableFromAccess(string tableName)
        {
            var result = new List<Dictionary<string, object>>();

            // For .mdb (Jet); if .accdb, use ACE OLE DB 12.0
            var connStr = $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={AccessDbPath};User Id=admin;Password=;";
            // If you have an .accdb file, it might be:
            // var connStr = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={AccessDbPath};Persist Security Info=False;";

            using (var conn = new OleDbConnection(connStr))
            {
                conn.Open();
                string sql = $"SELECT * FROM [{tableName}]";
                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rowDict = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string colName = reader.GetName(i);
                            object val = reader.GetValue(i);
                            rowDict[colName] = val;
                        }
                        result.Add(rowDict);
                    }
                }
            }

            return result;
        }

        // ---------------- BUTTON HANDLERS FOR OTHER TABLES ---------------- //

        private void BtnInquiry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = ReadTableFromAccess("Inquiry"); // table name in Access
                using (var scope = App.ServiceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<EthicsContext>();

                    // Map dictionary rows to Inquiry entities
                    var items = data.Select(row => new Inquiry
                    {
                        Subcommittee = row["Subcommittee"]?.ToString(),
                        LongName = row["Long Name"]?.ToString(),
                        Password = row["password"]?.ToString()
                    }).ToList();

                    db.Inquiry.AddRange(items);
                    db.SaveChanges();
                }

                MessageBox.Show("Inquiry imported successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing Inquiry: {ex.Message}");
            }
        }

        private void BtnCongress_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = ReadTableFromAccess("Congress");
                using (var scope = App.ServiceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<EthicsContext>();

                    // Map dictionary rows to Congress entities
                    var items = data.Select(row => new Congress
                    {
                        CongressNo = row["CongressNo"] is int cno
                                     ? cno
                                     : Convert.ToInt32(row["CongressNo"]),
                        Years = row["Years"]?.ToString(),
                        // "Default" might be boolean or numeric in Access
                        DefaultVal = ConvertYesNo(row["Default"]),
                        YearLabel = row["Year Label"]?.ToString(),
                        Committee = row["Committee"]?.ToString()
                    }).ToList();

                    db.Congress.AddRange(items);
                    db.SaveChanges();
                }

                MessageBox.Show("Congress imported successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing Congress: {ex.Message}");
            }
        }

        private void BtnArchivist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = ReadTableFromAccess("Archivist");
                using (var scope = App.ServiceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<EthicsContext>();

                    var items = data.Select(row => new Archivist
                    {
                        Ric = row["Ric"]?.ToString(),
                        First = row["First"]?.ToString(),
                        Last = row["Last"]?.ToString(),
                        LoggedIn = ConvertYesNo(row["Logged in"]),
                        Password = row["password"]?.ToString()
                    }).ToList();

                    db.Archivist.AddRange(items);
                    db.SaveChanges();
                }

                MessageBox.Show("Archivist imported successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing Archivist: {ex.Message}");
            }
        }

        private void BtnDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = ReadTableFromAccess("Docs");

                using (var scope = App.ServiceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<EthicsContext>();

                    // Convert each row into a Docs entity
                    var items = data.Select(row =>
                    {
                        // Possibly trim strings to avoid trailing spaces
                        string docDesc = row["Doc Descrip"]?.ToString()?.Trim();
                        string action = row["Action"]?.ToString()?.Trim();
                        string hasc = row["HASC Key"]?.ToString()?.Trim();
                        string user = row["User ID"]?.ToString()?.Trim();

                        // If "Key" is auto-increment in Access, we can store it, but EF might not re-use it
                        int keyVal = row["Key"] is int k ? k : Convert.ToInt32(row["Key"]);

                        return new Docs
                        {
                            Key = keyVal,
                            DocDescrip = docDesc,
                            Action = action,
                            HascKey = hasc,
                            UserID = user
                        };
                    }).ToList();

                    // Where we'll log failures
                    string jsonPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        "FailedRecordsDocs.json"
                    );
                    if (File.Exists(jsonPath)) File.Delete(jsonPath); // fresh start

                    int successCount = 0;
                    int counter = 0;

                    foreach (var docRecord in items)
                    {
                        counter++;
                        db.Docs.Add(docRecord);

                        try
                        {
                            db.SaveChanges();
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            // Build a fail-info object
                            var failInfo = new FailedDocsRecord
                            {
                                RecordNumber = counter,
                                KeyValue = docRecord.Key,
                                HascKey = docRecord.HascKey,
                                UserID = docRecord.UserID,
                                OuterExceptionMessage = ex.Message,
                                InnerExceptionMessage = ex.InnerException?.Message ?? ""
                            };

                            // Detach the failing entity from EF
                            db.ChangeTracker.Entries<Docs>()
                              .Where(e => e.Entity == docRecord)
                              .ToList()
                              .ForEach(e => e.State = Microsoft.EntityFrameworkCore.EntityState.Detached);

                            // Append JSON line to the fail file
                            var jsonFailLine = System.Text.Json.JsonSerializer.Serialize(failInfo);
                            File.AppendAllText(jsonPath, jsonFailLine + Environment.NewLine);
                        }
                    }

                    MessageBox.Show($"Docs import finished.\n" +
                                    $"Successfully imported: {successCount} records.\n" +
                                    $"Check {jsonPath} on Desktop for failures (if any).");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing Docs: {ex.Message}");
            }
        }


        // ---------------- BUTTON HANDLER FOR ARCHIVE (RECORD-BY-RECORD) ---------------- //

        private void BtnArchive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Read all rows from Access
                var data = ReadTableFromAccess("Archive");

                using (var scope = App.ServiceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<EthicsContext>();

                    // 2. Convert each row to an Archive record
                    var items = data.Select(row =>
                    {
                        // Trim or fix fields here
                        string hasc = row["HASC Key"]?.ToString()?.Trim();
                        string sub = row["Subcommittee"]?.ToString()?.Trim();
                        string stat = row["Status"]?.ToString()?.Trim();
                        string holl = row["Hollinger Box Key"]?.ToString()?.Trim();
                        string note = row["Note"]?.ToString()?.Trim();

                        int archiveNo = Convert.ToInt32(row["Archive No"]);
                        int congressVal = Convert.ToInt32(row["Congress"]);

                        short? boxLabelWOC = null;
                        if (row["Box Label without congress"] != DBNull.Value)
                        {
                            if (row["Box Label without congress"] is short sh)
                                boxLabelWOC = sh;
                            else
                                boxLabelWOC = Convert.ToInt16(row["Box Label without congress"]);
                        }

                        return new Archive
                        {
                            HascKey = hasc,
                            Subcommittee = sub,
                            ArchiveNo = archiveNo,
                            Congress = congressVal,
                            Classified = ConvertYesNo(row["Classified"]),
                            Status = stat,
                            HollingerBoxKey = holl,
                            Printed = ConvertYesNo(row["Printed"]),
                            Note = note,
                            BoxLabelWithoutCongress = boxLabelWOC,
                            BoxLabelProblem = ConvertYesNo(row["Box Label problem"]),
                            DocFound = ConvertYesNo(row["docFound"]),
                            Label1 = row["label1"]?.ToString()?.Trim(),
                            Label2 = row["label2"]?.ToString()?.Trim(),
                            Label3 = row["label3"]?.ToString()?.Trim(),
                            Label4 = row["label4"]?.ToString()?.Trim()
                        };
                    }).ToList();

                    // We'll keep track of how many succeeded for a final message
                    int successCount = 0;

                    // Prepare a path for the fail log
                    string jsonPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        "FailedRecordsArchive.json"
                    );

                    // Optionally: Clear any previous fail logs so we start fresh
                    if (File.Exists(jsonPath)) File.Delete(jsonPath);

                    int counter = 0;
                    foreach (var arch in items)
                    {
                        counter++;
                        db.Archive.Add(arch);

                        try
                        {
                            db.SaveChanges();
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            // Create the fail info
                            var failInfo = new FailedArchiveRecord
                            {
                                RecordNumber = counter,
                                HascKey = arch.HascKey,
                                Subcommittee = arch.Subcommittee,
                                Congress = arch.Congress,
                                OuterExceptionMessage = ex.Message,
                                InnerExceptionMessage = ex.InnerException?.Message ?? ""
                            };

                            // Detach the failing entity so we don't keep trying it
                            db.ChangeTracker.Entries<Archive>()
                              .Where(e => e.Entity == arch)
                              .ToList()
                              .ForEach(e => e.State = Microsoft.EntityFrameworkCore.EntityState.Detached);

                            // Now let's append it to the JSON file immediately
                            // We'll write one JSON object per line. 
                            var jsonFailLine = System.Text.Json.JsonSerializer.Serialize(failInfo);
                            File.AppendAllText(jsonPath, jsonFailLine + Environment.NewLine);

                            // If you want to keep going, do nothing here
                            // If you want to stop on the first fail, break or return
                        }
                    }

                    MessageBox.Show($"Archive import finished.\n" +
                                    $"Successfully imported: {successCount} records.\n" +
                                    $"Check your Desktop/FailedRecordsArchive.json for any failures.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing Archive: {ex.Message}");
            }
        }


        // Helper method to convert Yes/No columns from Access into bool?
        private bool? ConvertYesNo(object accessValue)
        {
            if (accessValue == null || accessValue == DBNull.Value)
                return null;

            if (accessValue is bool b)
                return b;

            if (accessValue is short s)
                return s != 0;

            if (accessValue is int i)
                return i != 0;

            return null;
        }

        // A little record class to store info about failed Archive rows
        private class FailedArchiveRecord
        {
            public int RecordNumber { get; set; }
            public string HascKey { get; set; }
            public string Subcommittee { get; set; }
            public int Congress { get; set; }
            public string OuterExceptionMessage { get; set; }
            public string InnerExceptionMessage { get; set; }
        }

        private class FailedDocsRecord
        {
            public int RecordNumber { get; set; }
            public int KeyValue { get; set; }
            public string HascKey { get; set; }
            public string UserID { get; set; }
            public string OuterExceptionMessage { get; set; }
            public string InnerExceptionMessage { get; set; }
        }


    }
}
