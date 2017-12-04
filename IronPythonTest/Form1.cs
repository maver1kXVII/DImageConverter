using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using zlib;
using System.Data.Entity;

namespace DImgConv
{
    public partial class Form1 : Form
    {
        ImgInterpreter rdimg = new ImgInterpreter();

        string GlobalLanguage = "";

        bool dbScripts = false;
        ScriptContext pyDB;

        string[] Scripts;
        
        double size = 1.0;
        string path = "D:\\TXM\\lab04_test01.png";
        Bitmap img;
        Color[,] pixels;
        int h = 2048, w = 4096;

        string filter0 = "Все файлы (*.*)|*.*";
        string filter = "";

        public Form1(string[] args)
        {
            if (dbScripts)
            {
                pyDB = new ScriptContext();
                pyDB.Configuration.LazyLoadingEnabled = false;
                MakeDB();
                GetDBScripts();
            }
            else
                GetScripts();
            if (args.Length == 0)
            {
                InitializeComponent();
                FillLangList();
                FillModeList();
                button1.Visible = false;
                button2.Visible = false;
                img = new Bitmap(w, h);
                pixels = new Color[w, h];
                ApplyScriptsToFilters();
            }
            else
            {
                for (int i = 0; i < Scripts.Length; i++)
                {
                    if (Path.GetExtension(args[0]).ToUpper().Remove(0, 1)
                        == Scripts[i].Split('-')[0])
                    {
                        img = rdimg.Load(args[0], "scripts\\" + Scripts[i] + ".py");
                    }
                }

                for (int i = 0; i < Scripts.Length; i++)
                {
                    if (Path.GetExtension(args[1]).ToUpper().Remove(0, 1)
                        == Scripts[i].Split('-')[0])
                    {
                        rdimg.ExportImg(args[1], "scripts\\" + Scripts[i] + ".py");
                    }
                }

                this.Close();
            }
        }

        /// <summary>
        /// Fills the language selection combo box with options
        /// </summary>
        private void FillLangList()
        {
            tSCBLang.Items.Add("Русский");
            tSCBLang.Items.Add("English");
            tSCBLang.SelectedIndex = 1;
        }

        private void FillModeList()
        {
            tSCBMode.Items.Add(LangConst.ModePortable);
            tSCBMode.Items.Add(LangConst.ModeSegmented);
            tSCBMode.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResizeImg();
        }

        private void NewPicLoad()
        {
            pictureBox1.ImageLocation = path;
            pictureBox1.Load();
            ResizeImg();
        }

        /// <summary>
        /// Changes visible size of the loaded image
        /// </summary>
        private void ResizeImg()
        {
            if (size != 0)
                pictureBox1.Image = Operations.ResizeImg(img, size);
        }

        private void tBSize_TextChanged(object sender, EventArgs e)
        {
            size = double.Parse(tBSize.Text) / 100.0;
            ResizeImg();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                if (dbScripts)
                {
                    string code = "";
                    ScriptComposition[] t = pyDB.ScriptCompositions.Include("ScriptImport")
                        .Include("ScriptFileParam").Include("ScriptFunction").ToArray();
                    for (int i = 0; i < t.Length; i++)
                    {
                        if (Path.GetExtension(openFileDialog1.FileName).ToUpper().Remove(0, 1)
                            == t[i].Extension)
                        {
                            code = pyDB.GetFullScript(t[i].ID);
                            img = rdimg.Load(path, code);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Scripts.Length; i++)
                    {
                        if (Path.GetExtension(openFileDialog1.FileName).ToUpper().Remove(0, 1)
                            == Scripts[i].Split('-')[0])
                        {
                            img = rdimg.Load(path, "scripts\\" + Scripts[i] + ".py");
                        }
                    }
                }
                pictureBox1.Image = img;
                ResizeImg();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    for (int i = 0; i < Scripts.Length; i++)
                    {
                        if (Path.GetExtension(saveFileDialog1.FileName).ToUpper().Remove(0, 1)
                            == Scripts[i].Split('-')[0])
                        {
                            rdimg.Img = (Bitmap)pictureBox1.Image;
                            rdimg.ExportImg(saveFileDialog1.FileName, "scripts\\" + Scripts[i] + ".py");
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GetScripts()
        {
            string folder = "scripts";
            string[] elements;
            filter0 = "Все файлы (*.*)|*.*";
            filter = "";
            if (Directory.Exists(folder))
            {
                Scripts = Directory.GetFiles(folder, "*-*");

                for (int i = 0; i < Scripts.Length; i++)
                    Scripts[i] = Path.GetFileNameWithoutExtension(Scripts[i]);

                for (int i = 0; i < Scripts.Length; i++)
                {
                    elements = Scripts[i].Split('-');
                    filter += "|" + elements[1] + " (*." + elements[0] + ")|*." + elements[0];
                }
            }
        }

        private void ApplyScriptsToFilters()
        {
            openFileDialog1.Filter = filter0 + filter;
            saveFileDialog1.Filter = filter.Remove(0, 1);
        }

        private void flipVerticallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            img = Operations.FlipVertical(img);
            ResizeImg();
        }

        private void negativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            img = Operations.Negative(img);
            ResizeImg();
        }

        private void flipHorizontallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            img = Operations.FlipHorizontal(img);
            ResizeImg();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //Compression.LZW.TestCompress();
            System.IO.FileStream output = new System.IO.FileStream("CompressedZ1.txt", System.IO.FileMode.Create);
            zlib.ZOutputStream outputZ = new zlib.ZOutputStream(output, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
            System.IO.FileStream input = new System.IO.FileStream("InputZ1.txt", System.IO.FileMode.Open);
            try
            {
                CopyStream(input, outputZ);
            }
            finally
            {
                outputZ.Close();
                output.Close();
                input.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Compression.LZW.TestDecompres();
            //System.IO.FileStream outFileStream = new System.IO.FileStream("DecompressedZ1.txt", System.IO.FileMode.Create);
            //zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream);
            //System.IO.FileStream inFileStream = new System.IO.FileStream("CompressedZ1.txt", System.IO.FileMode.Open);
            System.IO.FileStream outFileStream = new System.IO.FileStream("DecompressedZ2.bin", System.IO.FileMode.Create);
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream);
            System.IO.FileStream inFileStream = new System.IO.FileStream("219outCompressed.bin", System.IO.FileMode.Open);
            
            try
            {
                CopyStream(inFileStream, outZStream);
            }
            finally
            {
                outZStream.Close();
                outFileStream.Close();
                inFileStream.Close();
            }
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        private void removeAlphaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            img = Operations.RemoveAlpha(img);
            ResizeImg();
        }

        public void MakeDB()
        {
            StreamReader sr;
            string folder = "scripts";
            string code = "";
            string[] codeParts;
            string[] paramParts;
            string[] elements;
            ScriptImport si = new ScriptImport { Code = "" };
            ScriptFileParam sfp;
            ScriptImgParam sip;
            ScriptImgParamItem sipi;
            ScriptFunction sf;
            ScriptComposition sc;

            if (Directory.Exists(folder))
            {
                Scripts = Directory.GetFiles(folder, "*-*");

                for (int i = 0; i < Scripts.Length; i++)
                {
                    elements = Path.GetFileNameWithoutExtension(Scripts[i]).Split('-');
                    sr = new StreamReader(Scripts[i]);
                    code = sr.ReadToEnd();
                    codeParts = code.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);

                    if (codeParts.Length >= 3 && !(pyDB.ScriptExists(elements[0], elements[1])))
                    {
                        si = pyDB.GetImportByMD5(GetMd5(codeParts[0]));
                        //if (codeParts[0] != si.Code)
                        if (si == null)
                        {
                            si = new ScriptImport { Code = codeParts[0], MD5 = GetMd5(codeParts[0]) };
                            pyDB.ScriptImports.Add(si);
                        }
                        sfp = new ScriptFileParam { Code = codeParts[1] };
                        pyDB.ScriptFileParams.Add(sfp);
                        //sip = new ScriptImgParam { Code = codeParts[2] };
                        //pyDB.ScriptImgParams.Add(sip);
                        if (codeParts.Length >= 4)
                        {
                            code = "";
                            for (int j = 3; j < codeParts.Length; j++)
                                code += codeParts[j];
                            sf = new ScriptFunction { Code = code };
                            pyDB.ScriptFunctions.Add(sf);
                            pyDB.SaveChanges();
                            sc = new ScriptComposition
                            {
                                Extension = elements[0],
                                Description = elements[1],
                                ScriptImport = pyDB.ScriptImports.First(p => p.ID == si.ID),
                                ScriptFileParam = pyDB.ScriptFileParams.First(p => p.ID == sfp.ID),
                                ScriptFunction = pyDB.ScriptFunctions.First(p => p.ID == sf.ID)
                            };
                        }
                        else
                        {
                            pyDB.SaveChanges();
                            sc = new ScriptComposition
                            {
                                Extension = elements[0],
                                Description = elements[1],
                                ScriptImport = pyDB.ScriptImports.First(p => p.ID == si.ID),
                                ScriptFileParam = pyDB.ScriptFileParams.First(p => p.ID == sfp.ID),
                                ScriptFunction = null
                            };
                        }
                        pyDB.ScriptCompositions.Add(sc);
                        paramParts = codeParts[2].Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        //List<ScriptImgParamItem> test = new List<ScriptImgParamItem>();
                        for (int j = 0; j < paramParts.Length; j++)
                        {
                            code = GetMd5(paramParts[j]);
                            if (!pyDB.ImgParamItemExists(code))
                            {
                                sipi = new ScriptImgParamItem { ID = code, Code = paramParts[j] };
                                pyDB.ScriptImgParamItems.Add(sipi);
                                //test.Add(sipi);
                            }
                            sip = new ScriptImgParam { ScriptCompositionID = sc.ID, ScriptImgParamItemID = code, Order = j };
                            pyDB.ScriptImgParams.Add(sip);
                            //sip = new ScriptImgParam { Code = codeParts[2] };
                            //pyDB.ScriptImgParams.Add(sip);
                        }
                        pyDB.SaveChanges();
                    }
                }
            }
        }

        private void GetDBScripts()
        {
            filter0 = "Все файлы (*.*)|*.*";
            filter = "";

            for (int i = 0; i < Scripts.Length; i++)
                Scripts[i] = Path.GetFileNameWithoutExtension(Scripts[i]);

            ScriptComposition[] t = pyDB.ScriptCompositions.ToArray();
            for (int i = 0; i < t.Length; i++)
            {
                filter += "|" + t[i].Description + " (*." + t[i].Extension + ")|*." + t[i].Extension;
            }
        }

        private void applySizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            img = Operations.ResizeImg(img, size);
            size = 1.0;
            ResizeImg();
        }

        private string GetMd5(string input)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] toBytes = Encoding.ASCII.GetBytes(input);
            toBytes = md5.ComputeHash(toBytes);
            return Encoding.ASCII.GetString(toBytes);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("DImgConverter v1.0.0 - an image converter with a support for user-made external image format modules.\n\nDeveloped by Kazantsev S.A.", "About");
        }

        private void validateScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(pyDB.ScriptValidation());
        }

        private void tSCBLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalLanguage = (string)tSCBLang.Items[tSCBLang.SelectedIndex];
            LangConst.UpdateLang(GlobalLanguage);
            Translate();
        }

        public void Translate()
        {
            fileToolStripMenuItem.Text = LangConst.MenuFile;
            operationsToolStripMenuItem.Text = LangConst.MenuOperations;
            adminToolStripMenuItem.Text = LangConst.MenuAdmin;
            helpToolStripMenuItem.Text = LangConst.MenuHelp;

            openToolStripMenuItem.Text = LangConst.MenuOpen;
            saveAsToolStripMenuItem.Text = LangConst.MenuSaveAs;
            modeToolStripMenuItem.Text = LangConst.MenuMode;

            label1.Text = LangConst.LabelSize;
        }
    }

    public class LangConst
    {
        public static string ModePortable;
        public static string ModeSegmented;

        public static string MenuFile;
        public static string MenuOperations;
        public static string MenuAdmin;
        public static string MenuHelp;

        public static string MenuOpen;
        public static string MenuSaveAs;
        public static string MenuMode;

        public static string LabelSize;

        public static void UpdateLang(string lang)
        {
            switch(lang)
            {
                case("Русский"):
                    ModePortable = "Портативный";
                    ModeSegmented = "Распределённый";

                    MenuFile = "Файл";
                    MenuOperations = "Операции";
                    MenuAdmin = "Администратор";
                    MenuHelp = "Помощь";

                    MenuOpen = "Открыть";
                    MenuSaveAs = "Сохранить как...";
                    MenuMode = "Режим";

                    LabelSize = "Масштаб";
                    break;

                //English is default
                default:
                    ModePortable = "Portable";
                    ModeSegmented = "Segmented";

                    MenuFile = "File";
                    MenuOperations = "Operations";
                    MenuAdmin = "Admin";
                    MenuHelp = "Help";

                    MenuOpen = "Open";
                    MenuSaveAs = "Save as...";
                    MenuMode = "Mode";

                    LabelSize = "Size";
                    break;
            }
        }
    }
}
