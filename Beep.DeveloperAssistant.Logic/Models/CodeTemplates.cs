using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Beep.DeveloperAssistant.Logic.Models
{
    public class CodeTemplates
    {
        public CodeTemplates() { GuidID = Guid.NewGuid().ToString(); }
       
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string TypeCode { get; set; }
        public string GuidID { get; }
        public string Using { get; set; }
        public string NameSpace { get; set; }
        public string ClassDefinition { get;set; }
        public string Header { get; set; }
        public string Footer { get;set; }
        public string Content { get; set; }
        public bool IsRepeatedProperty { get; set; }    =true;
        private string _FunctionHeader = string.Empty;
        public string FunctionHeader
        {
            get => _FunctionHeader; 
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    IsRepeatedProperty=true; _FunctionHeader = string.Empty;
                }
                else
                {
                    IsRepeatedProperty = false; _FunctionHeader = value;
                }
            }
        }
        public string ReferencedAssemblies { get; set; }

        
    }
    public enum TargetFramework
    {
        NetStandard20,
        NetCoreApp31,
        Net50
    }
}
