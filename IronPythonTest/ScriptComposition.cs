using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace DImgConv
{
    class ScriptComposition
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        public Guid ScriptImportID { get; set; }
        public Guid ScriptFileParamID { get; set; }
        //public Guid ScriptImgParamID { get; set; }
        public Guid? ScriptFunctionID { get; set; }
        public string Extension { get; set; }
        public string Description { get; set; }
        public virtual ScriptImport ScriptImport { get; set; }
        public virtual ScriptFileParam ScriptFileParam { get; set; }
        //public virtual ScriptImgParam ScriptImgParam { get; set; }
        public virtual ScriptFunction ScriptFunction { get; set; }

        public string GetHalfScript()
        {
            string code = "";
            code += this.ScriptImport.Code;
            code += "\r\n" + this.ScriptFileParam.Code;
            return code;
        }
    }
}
