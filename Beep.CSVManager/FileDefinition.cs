using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Beep.CSV.Logic
{
    public class FileDefinition
    {
        public FileDefinition()
        {

        }
        public string FilesFolder { get; set; }
        public List<CValue> Files { get; set; } = new List<CValue>();
        public int NoOfRecordsLimit { get; set; } = 100000;
        public ObservableCollection<CValue> Columns { get; set; } = new ObservableCollection<CValue>();
        public ObservableCollection<CValue> Wells { get; set; } = new ObservableCollection<CValue>();
    }
    public class CValue
    {
        public string Value { get; set; }
        public int idx { get; set; }
        public CValue()
        {

        }
    }
}
