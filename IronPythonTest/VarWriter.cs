using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using System.IO;

namespace DImgConv
{
    public class VarWriter
    {
        public static void WriteScriptVar(BinaryWriter binw, Microsoft.Scripting.Hosting.ScriptScope s, string pyVar)
        {
            if (s.ContainsVariable(pyVar + "_OFFSET"))
            {
                binw.BaseStream.Seek(s.GetVariable(pyVar + "_OFFSET"), SeekOrigin.Begin);
            }
            if (s.ContainsVariable(pyVar + "_TYPE"))
            {
                string type = s.GetVariable(pyVar + "_TYPE");
                switch (type)
                {
                    case "TRAILED_STRING":
                        char trail = char.Parse(s.GetVariable(pyVar + "_TRAIL"));

                        string value = s.GetVariable(pyVar);
                        char[] value2 = value.ToCharArray();
                        binw.Write(value2);
                        binw.Write(trail);
                        break;

                    default:
                        binw.Write(s.GetVariable(pyVar));
                        break;
                }
            }
            else
            {
                binw.Write(s.GetVariable(pyVar));
            }
        }

        public static void WriteInt32(BinaryWriter binw, Microsoft.Scripting.Hosting.ScriptScope s, string pyVar, int value)
        {
            if (s.ContainsVariable(pyVar + "_OFFSET"))
            {
                binw.BaseStream.Seek(s.GetVariable(pyVar + "_OFFSET"), SeekOrigin.Begin);
            }
            if (s.ContainsVariable(pyVar + "_TYPE"))
            {
                string type = s.GetVariable(pyVar + "_TYPE");
                switch (type)
                {
                    case "TRAILED_STRING":
                        char trail = char.Parse(s.GetVariable(pyVar + "_TRAIL"));
                        char[] value2 = value.ToString().ToCharArray();
                        binw.Write(value2);
                        binw.Write(trail);
                        break;

                    case "INT16":
                        binw.Write((Int16)value);
                        break;

                    default:
                        binw.Write(value);
                        break;
                }
            }
            else
            {
                binw.Write(value);
            }
        }
    }
}
