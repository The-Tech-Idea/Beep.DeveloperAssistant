using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using TheTechIdea;
using TheTechIdea.Beep;

namespace Beep.CSV.Logic
{
    public class ReadCSV
    {
      
        public DataTable DataTableFromCSV { get; set; }
        public FileDefinition fileDefinition { get; set; }=new FileDefinition();
        string CurrentFile;
        string CurrentWell;
        FileStream filestream;
        StreamWriter streamwriter;
        IDMEEditor dMEEditor;
        public ReadCSV(IDMEEditor pdMEEditor) { dMEEditor = pdMEEditor; }
        public void GetColumnsFromFolder(string folder, IProgress<PassedArgs> progress, CancellationToken token)
        {
            fileDefinition.Columns = new ObservableCollection<CValue>();
            fileDefinition.Wells  = new ObservableCollection<CValue>();
            int i = 0;
            GetFileDefinitonIfExist(folder);
            foreach (var files in Directory.GetFiles(folder))
            {
                if (!fileDefinition.Files.Any(o => o.Value.Equals(files, StringComparison.InvariantCultureIgnoreCase)))
                {
                    i += 1;
                    SendMessege(progress, token, $"Scanning File : {files}");
                    CValue x = new CValue() { idx = i, Value = files };
                    fileDefinition.Files.Add(x);
                    GetColumnsFromFile(files, progress, token);
                    dMEEditor.ConfigEditor.JsonLoader.Serialize(Path.Combine(folder, "File.json"), fileDefinition);
                }else
                    SendMessege(progress, token, $"Pass  File : {files}");

            }
            foreach (string dirs in Directory.EnumerateDirectories(folder))
            {
                GetColumnsFromFolder(dirs,  progress,  token);
            }
            
        } 
        public void GetFileDefinitonIfExist(string folder)
        {
            if(File.Exists(Path.Combine(folder, "File.json")))
            {
                fileDefinition= dMEEditor.ConfigEditor.JsonLoader.DeserializeSingleObject<FileDefinition>(Path.Combine(folder, "File.json"));
            }
        }
        private string CreateHeaderRow(string wellname)
        {
            string header = "";
            header = @$"{wellname},DateandTime,";
            List<string> cols = fileDefinition.Columns.Select(p => p.Value).ToList();
            for (int i = 0; i <= cols.Count-1; i++)
            {
                header += @$"{cols[i]},";
            }
            header=header.Remove(header.Length - 1);
            return header;
        }
        public void CreateCSVForWell(string folder,string wellname, IProgress<PassedArgs> progress, CancellationToken token)
        {
            string Fulltext;
           filestream = new FileStream(Path.Combine(folder, $"{wellname}.csv"), FileMode.OpenOrCreate);
            streamwriter = new StreamWriter(filestream);
            streamwriter.WriteLine(CreateHeaderRow(wellname));
            List<string> files = Directory.GetFiles(folder).OrderBy(p=>p).ToList();
            for (int k = 0; k <= files.Count-1; k++)
            {
                string filename = files[k];
                using (StreamReader sr = new StreamReader(filename))
                {
                    while (!sr.EndOfStream)
                    {
                        Fulltext = sr.ReadToEnd().ToString(); //read full file text
                        string[] rows = Fulltext.Split('\n'); //split full file text into rows

                        for (int i = 0; i < rows.Count() - 1; i++)
                        {
                            if (i > 0)
                            {
                                string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values
                                string columnName = rowValues[0];
                                string[] columnsplit = columnName.Split('.');
                                    if (!fileDefinition.Wells.Any(p => p.Value.Equals(columnsplit[0], System.StringComparison.InvariantCultureIgnoreCase)))
                                    {
                                        SendMessege(progress, token, $"Found Well : {columnsplit[0]}");
                                        CValue x = new CValue() { idx = i, Value = columnsplit[0] };
                                        fileDefinition.Wells.Add(x);
                                    }
                                    if (!fileDefinition.Columns.Any(p => p.Value.Equals(columnsplit[1], System.StringComparison.InvariantCultureIgnoreCase)))
                                    {
                                        SendMessege(progress, token, $"Found Column : {columnsplit[1]}");
                                        CValue x = new CValue() { idx = i, Value = columnsplit[1] };
                                        fileDefinition.Columns.Add(x);
                                    }
                               
                            }

                        }
                    }
                    sr.Close();
                }
           
            }
        }
        public ObservableCollection<CValue> GetColumnsFromFile(string filename ,IProgress<PassedArgs> progress, CancellationToken token)
        {
            string Fulltext;
            using (StreamReader sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    Fulltext = sr.ReadToEnd().ToString(); //read full file text
                    string[] rows = Fulltext.Split('\n'); //split full file text into rows

                    for (int i = 0; i < rows.Count() - 1; i++)
                    {
                        if (i > 0)
                        {
                            string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values
                            {
                                string columnName = rowValues[0];
                                string[] columnsplit = columnName.Split('.');
                                if (!fileDefinition.Wells.Any(p => p.Value.Equals(columnsplit[0], System.StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    SendMessege(progress, token, $"Found Well : {columnsplit[0]}");
                                    CValue x = new CValue() { idx = i, Value = columnsplit[0] };
                                    fileDefinition.Wells.Add(x);
                                }
                                if (!fileDefinition.Columns.Any(p => p.Value.Equals(columnsplit[1], System.StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    SendMessege(progress, token, $"Found Column : {columnsplit[1]}");
                                    CValue x = new CValue() { idx = i, Value = columnsplit[1] };
                                    fileDefinition.Columns.Add(x);
                                }
                            }
                        }
                       
                    }
                }
                sr.Close();
            }
            
            return fileDefinition.Columns;
        }
        public DataTable ReadCsvFile(string filename, string FileSaveWithPath)
        {
            DataTable dtCsv = new DataTable();
            string Fulltext;
            if (string.IsNullOrEmpty(filename))
            {
                // string FileSaveWithPath = Server.MapPath("\\Files\\Import" + System.DateTime.Now.ToString("ddMMyyyy_hhmmss") + ".csv");
                //  FileUpload1.SaveAs(FileSaveWithPath);
                using (StreamReader sr = new StreamReader(FileSaveWithPath))
                {
                    while (!sr.EndOfStream)
                    {
                        Fulltext = sr.ReadToEnd().ToString(); //read full file text
                        string[] rows = Fulltext.Split('\n'); //split full file text into rows
                        for (int i = 0; i < rows.Count() - 1; i++)
                        {
                            string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values
                            {
                                if (i == 0)
                                {
                                    for (int j = 0; j < rowValues.Count(); j++)
                                    {
                                        dtCsv.Columns.Add(rowValues[j]); //add headers
                                    }
                                }
                                else
                                {
                                    DataRow dr = dtCsv.NewRow();
                                    for (int k = 0; k < rowValues.Count(); k++)
                                    {
                                        dr[k] = rowValues[k].ToString();
                                    }
                                    dtCsv.Rows.Add(dr); //add other rows
                                }
                            }
                        }
                    }
                }
            }
            return dtCsv;
        }
        private void SendMessege(IProgress<PassedArgs> progress, CancellationToken token, string messege = null)
        {

            if (progress != null)
            {
                PassedArgs ps = new PassedArgs { EventType = "Update", ParameterString1 = messege };
                progress.Report(ps);
            }

        }

    }
   
}