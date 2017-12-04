using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace DImgConv
{
    class ScriptImgParam //: ScriptBaseEntity 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        public Guid ScriptCompositionID { get; set; }
        public string ScriptImgParamItemID { get; set; }
        public int Order { get; set; }
        public virtual ScriptComposition ScriptComposition { get; set; }
        public virtual ScriptImgParamItem ScriptImgParamItem { get; set; }
    }
}
