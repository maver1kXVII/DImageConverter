using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace DImgConv
{
    class ScriptContext : DbContext
    {
        public ScriptContext() : base("PyScriptDB") { }
        public DbSet<ScriptComposition> ScriptCompositions { get; set; }
        public DbSet<ScriptImport> ScriptImports { get; set; }
        public DbSet<ScriptFileParam> ScriptFileParams { get; set; }
        public DbSet<ScriptImgParam> ScriptImgParams { get; set; }
        public DbSet<ScriptFunction> ScriptFunctions { get; set; }
        public DbSet<ScriptImgParamItem> ScriptImgParamItems { get; set; }

        public ScriptImport GetImportByMD5(string hash)
        {
            ScriptImport[] imports = ScriptImports.ToArray();
            for (int i = 0; i < imports.Length; i++)
            {
                if (imports[i].MD5 == hash)
                    return imports[i];
            }
            return null;
        }

        public bool ScriptExists(string ext, string desc)
        {
            ScriptComposition[] c = ScriptCompositions.ToArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i].Extension == ext && c[i].Description == desc)
                    return true;
            }
            return false;
        }

        public string ScriptValidation()
        {
            string res = "";
            int errAll = 0;
            int errN = 5;
            bool gotW = false;
            bool gotH = false;
            bool gotBPP = false;
            bool gotWO = false;
            bool gotPF = false;

            ScriptComposition[] c = ScriptCompositions.ToArray();
            ScriptImgParam[] ip;
            for (int i = 0; i < c.Length; i++)
            {
                errN = 5;
                gotW = false;
                gotH = false;
                gotBPP = false;
                gotWO = false;
                gotPF = false;
                ScriptComposition sc = c[i];
                ip = ScriptImgParams.Include("ScriptImgParamItem").Where(x => x.ScriptCompositionID == sc.ID).ToArray();
                for (int j = 0; j < ip.Length; j++)
                {
                    if (ip[j].ScriptImgParamItem.Code.Contains("IMG_WIDTH") && !gotW)
                    {
                        if (ip[j].ScriptImgParamItem.Code[4] == 'W')
                        {
                            gotW = true;
                            errN--;
                        }
                    }
                    if (ip[j].ScriptImgParamItem.Code.Contains("IMG_HEIGHT") && !gotH)
                    {
                        if (ip[j].ScriptImgParamItem.Code[4] == 'H')
                        {
                            gotH = true;
                            errN--;
                        }
                    }
                    if (ip[j].ScriptImgParamItem.Code.Contains("IMG_BPP") && !gotBPP)
                    {
                        if (ip[j].ScriptImgParamItem.Code[4] == 'B')
                        {
                            gotBPP = true;
                            errN--;
                        }
                    }
                    if (ip[j].ScriptImgParamItem.Code.Contains("IMG_PIXEL_FORMAT") && !gotPF)
                    {
                        if (ip[j].ScriptImgParamItem.Code[4] == 'P')
                        {
                            gotPF = true;
                            errN--;
                        }
                    }
                    if (ip[j].ScriptImgParamItem.Code.Contains("WRITE_ORDER = List[System.String]()") && !gotWO)
                    {
                        gotWO = true;
                        errN--;
                    }
                }
                if (gotW&&gotH&&gotBPP&&gotPF&&gotWO)
                {
                    res += " + Скрипт формата " + c[i].Extension + " прошёл проверку.\n";
                }
                else
                {
                    res += " - Скрипт формата " + c[i].Extension + " не прошёл проверку: найдено ошибок - " + errN + ".\n";
                    if (!gotW)
                        res += "\tОтсутствует параметр IMG_WIDTH\n";
                    if (!gotH)
                        res += "\tОтсутствует параметр IMG_HEIGHT\n";
                    if (!gotBPP)
                        res += "\tОтсутствует параметр IMG_BPP\n";
                    if (!gotPF)
                        res += "\tОтсутствует параметр IMG_PIXEL_FORMAT\n";
                    if (!gotWO)
                        res += "\tОтсутствует параметр WRITE_ORDER\n";
                    errAll += errN;
                }
            }
            res += "Проверка модулей завершена: найдено ошибок - " + errAll + ".";
            return res;
        }

        public bool ImgParamItemExists(string hash)
        {
            ScriptImgParamItem[] c = ScriptImgParamItems.ToArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i].ID == hash)
                    return true;
            }
            return false;
        }

        public string GetFullScript(Guid CompositionID)
        {
            string code = "";
            ScriptComposition sc = GetCompositionById(CompositionID);
            code += sc.GetHalfScript();
            code += "\r\n" + GetFullImgParams(CompositionID);
            if (sc.ScriptFunction != null)
                code += sc.ScriptFunction.Code;
            return code;
        }

        public string GetFullImgParams(Guid CompositionID)
        {
            string code = "";
            ScriptImgParam[] c = ScriptImgParams.Include("ScriptImgParamItem")
                .Where(x => x.ScriptCompositionID == CompositionID)
                .OrderBy(x => x.Order)
                .ToArray();
            for (int i = 0; i < c.Length; i++)
            {
                code += c[i].ScriptImgParamItem.Code + "\r\n";
            }
            return code;
        }

        private ScriptComposition GetCompositionById(Guid ID)
        {
            return ScriptCompositions.First(p => p.ID == ID);
        }

        static ScriptContext()
        {
            Database.SetInitializer<ScriptContext>(new ScriptInitializer());
        }
    }
    //DropCreateDatabaseAlways<ScriptContext>
    class ScriptInitializer : DropCreateDatabaseIfModelChanges<ScriptContext> //DropCreateDatabaseAlways<ScriptContext>
    {
        protected override void Seed(ScriptContext db)
        {
            db.SaveChanges();
        }
    }
}
