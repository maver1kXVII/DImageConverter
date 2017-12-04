using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using System.IO;

namespace DImgConv
{
    public class VarReader
    {
        public static int ReadInt32(BinaryReader binr, Microsoft.Scripting.Hosting.ScriptScope s, string pyVar)
        {
            int rVar = 0;
            bool skip = false;

            FileStream fs = (FileStream)binr.BaseStream;
            string script = Path.GetExtension(fs.Name).Remove(0,1).ToUpper();
            
            if (script == pyVar.Split('_')[0])
                skip = true;

            if (s.ContainsVariable(pyVar + "_OFFSET"))
            {
                binr.BaseStream.Seek(s.GetVariable(pyVar + "_OFFSET"), SeekOrigin.Begin);
            }
            if (!skip && s.ContainsVariable(pyVar))
            {
                rVar = s.GetVariable(pyVar);
            }
            else
            {
                if (s.ContainsVariable(pyVar + "_TYPE"))
                {
                    string type = s.GetVariable(pyVar + "_TYPE");
                    switch (type)
                    {
                        case "TRAILED_STRING":
                            char trail = char.Parse(s.GetVariable(pyVar + "_TRAIL"));

                            string value = "";
                            char add = ' ';
                            while (true)
                            {
                                add = binr.ReadChar();
                                if (add != trail)
                                    value += add;
                                else
                                {
                                    if (!skip)
                                        rVar = int.Parse(value);
                                    break;
                                }
                            }
                            break;

                        case "INT16":
                            rVar = binr.ReadInt16();
                            break;

                        case "BYTE":
                            rVar = binr.ReadByte();
                            break;

                        default:
                            rVar = 0;
                            break;
                    }
                }
                else
                {
                    rVar = binr.ReadInt32();
                }
            }
            if (skip)
                return 0;
            return rVar;
        }
    }
}
