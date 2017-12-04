using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using IronPython.Hosting;
using System.IO;

namespace DImgConv
{
    class ImgInterpreter
    {
        string path;
        Bitmap img;
        Microsoft.Scripting.Hosting.ScriptEngine py;
        Microsoft.Scripting.Hosting.ScriptScope s;
        string script;
        FileStream fs;

        int bpp;
        int BPP
        {
            get
            {
                return bpp;
            }
            set
            {
                /*if (pixelFormat.Length == 4)
                    alpha = true;
                else
                    alpha = false;*/

                switch(value)
                {
                    case(24):
                        bpc = 8;
                        break;

                    case (32):
                        bpc = 8;
                        if (pixelFormat.Length < 4)
                            pixelFormat += "A";
                        break;

                    default:
                        break;
                }

                bpp = value;
            }
        }
        string pixelFormat;

        //bool alpha;
        int bpc;

        public Bitmap Img 
        { 
            get
            {
                return img;
            }
            set
            {
                img = value;
            }
        }

        public ImgInterpreter()
        {
            path = "";
            script = "scripts\\_default.py";
            py = Python.CreateEngine(); // allows to run ironpython programs
            s = py.CreateScope(); // you need this to get the variables
        }

        /// <summary>
        /// Imports the chosen image
        /// </summary>
        /// <param name="imgpath">path to image</param>
        /// <param name="scriptpath">script path</param>
        /// <returns></returns>
        public Bitmap Load(string imgpath, string scriptpath)
        {
            py = Python.CreateEngine();
            s = py.CreateScope(); 
            
            path = imgpath;
            img = new Bitmap(4, 4);
            script = scriptpath;
            if (scriptpath.Length > 100)
                py.Execute(script, s);
            else
                py.ExecuteFile(script, s);
            LoadImg();
            return img;
        }

        /// <summary>
        /// Loads the image
        /// </summary>
        public void LoadImg()
        {
            BinaryReader binr;
            fs = new FileStream(path, FileMode.Open);
            binr = new BinaryReader(fs);
            int w = 0, h = 0;
            pixelFormat = s.GetVariable("IMG_PIXEL_FORMAT");
            pixelFormat = pixelFormat.ToUpper();
            BPP = VarReader.ReadInt32(binr, s, "IMG_BPP");

            List<string> order = s.GetVariable("WRITE_ORDER");

            for (int q = 0; q < order.Count; q++)
            {
                switch (order[q])
                {
                    case "FILE_MAGIC":
                        int maglen = s.GetVariable("FILE_MAGIC_LEN");
                        List<Byte> magic = binr.ReadBytes(maglen).ToList();
                        List<Byte> magic2 = s.GetVariable("FILE_MAGIC");
                        //if (magic.Equals(magic2))
                        //    return;
                        break;
                    case "IMG_WIDTH":
                        w = VarReader.ReadInt32(binr, s, order[q]);
                        break;
                    case "IMG_HEIGHT":
                        h = VarReader.ReadInt32(binr, s, order[q]);
                        break;
                    case "IMG_BPP":
                        BPP = VarReader.ReadInt32(binr, s, order[q]);
                        break;
                    case "IMG_PIXELS":
                        if (s.ContainsVariable("IMG_PIXEL_FORMAT"))
                        {
                            byte[] rgb;
                            int size = 0;
                            if (s.ContainsVariable("IMG_UNEVEN_PADDING"))
                            {
                                if (s.GetVariable("IMG_UNEVEN_PADDING"))
                                {
                                    int w1 = 0;
                                    if (w % 2 > 0)
                                        w1 = 1;
                                    size = (w + w1) * h * (bpc * pixelFormat.Length / 8);
                                }
                                else
                                    size = w * h * (bpc * pixelFormat.Length / 8);
                            }
                            else
                                size = w * h * (bpc * pixelFormat.Length / 8);

                            if (s.ContainsVariable("IMG_PIXELS_LEN"))
                            {
                                size = s.GetVariable("IMG_PIXELS_LEN");
                            }

                            if (s.ContainsVariable("IMG_PIXELS_LEN_OFFSET"))
                            {
                                binr.BaseStream.Seek(s.GetVariable("IMG_PIXELS_LEN_OFFSET"), SeekOrigin.Begin);
                                size = VarReader.ReadInt32(binr, s, "IMG_PIXELS_LEN");
                            }

                            if (s.ContainsVariable(order[q] + "_OFFSET"))
                                binr.BaseStream.Seek(s.GetVariable(order[q] + "_OFFSET"), SeekOrigin.Begin);

                            rgb = binr.ReadBytes(size);

                            if (s.ContainsVariable("IMG_DEFLATE"))
                            {
                                if (s.GetVariable("IMG_DEFLATE"))
                                {
                                    System.IO.FileStream inFile = new System.IO.FileStream("CompZ.bin", System.IO.FileMode.Create);
                                    BinaryWriter binZ = new BinaryWriter(inFile);
                                    binZ.Write(rgb);
                                    //binZ.Flush();
                                    binZ.Close();
                                    inFile.Close();

                                    System.IO.FileStream outFileStream = new System.IO.FileStream("DecompZ.bin", System.IO.FileMode.Create);
                                    zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream);
                                    System.IO.FileStream inFileStream = new System.IO.FileStream("CompZ.bin", System.IO.FileMode.Open);
                                    
                                    try
                                    {
                                        byte[] buffer = new byte[2000];
                                        int len;
                                        while ((len = inFileStream.Read(buffer, 0, 2000)) > 0)
                                        {
                                            outZStream.Write(buffer, 0, len);
                                        }
                                        outZStream.Flush();
                                    }
                                    finally
                                    {
                                        outZStream.Close();
                                        outFileStream.Close();
                                        inFileStream.Close();
                                    }

                                    System.IO.FileStream inFile2 = new System.IO.FileStream("DecompZ.bin", System.IO.FileMode.Open);
                                    BinaryReader binZ2 = new BinaryReader(inFile2);
                                    //rgb = new byte[inFile2.Length];
                                    //inFile2.Read(rgb, 0, (int)inFile2.Length);
                                    rgb = binZ2.ReadBytes((int)inFile2.Length);
                                    binZ2.Close();
                                    inFile2.Close();
                                }
                            }

                            if (s.ContainsVariable("IMG_PIXELS_HANDLER"))
                            {
                                if (s.GetVariable("IMG_PIXELS_HANDLER"))
                                {
                                    var ProcessPixels = s.GetVariable("ProcessPixels");
                                    IList<object> rgbp = new List<object>();
                                    for (int i = 0; i < rgb.Length; i++)
                                        rgbp.Add(rgb[i]);
                                    List<byte> rgbl = ProcessPixels(rgbp, w, h, BPP);
                                    rgb = rgbl.ToArray();
                                }
                            }

                            img = new Bitmap(w, h);
                            Color[,] pixels = Render(rgb, h, w);
                            for (int j = 0; j < h; j++)
                            {
                                for (int i = 0; i < w; i++)
                                {
                                    img.SetPixel(i, j, pixels[i, j]);
                                }
                            }
                        }
                        break;
                    default:
                        VarReader.ReadInt32(binr, s, order[q]);
                        break;
                }
            }
            binr.Close();
            fs.Close();
        }

        private Color[,] Render(byte[] rgb, int h, int w)
        {
            Color[,] pixels = new Color[w, h];
            //int red = 0, green = 0, blue = 0, alpha = 255;
            bool upsideDown = false;
            int colors = pixelFormat.Length;

            if (s.ContainsVariable("IMG_UPSIDE_DOWN"))
                if (s.GetVariable("IMG_UPSIDE_DOWN"))
                    upsideDown = true;

            int degreeOfParallelism = Environment.ProcessorCount;
            Task[] tasks = new Task[degreeOfParallelism];

            for (int taskNumber = 0; taskNumber < degreeOfParallelism; taskNumber++)
            {
                int taskNumberCopy = taskNumber;
                tasks[taskNumber] = Task.Factory.StartNew(
                    () =>
                    {
                        for (int j = taskNumberCopy * h / degreeOfParallelism; j < (taskNumberCopy + 1) * h / degreeOfParallelism; j++)
                        {
                            int red = 0, green = 0, blue = 0, alpha = 255;
                            for (int i = 0; i < w; i++)
                            {
                                if (i < w)
                                {
                                    for (int c = 0; c < colors; c++)
                                    {
                                        if (pixelFormat[c] == 'B')
                                            blue = rgb[j * w * colors + i * colors + c];
                                        if (pixelFormat[c] == 'G')
                                            green = rgb[j * w * colors + i * colors + c];
                                        if (pixelFormat[c] == 'R')
                                            red = rgb[j * w * colors + i * colors + c];
                                        if (pixelFormat[c] == 'A')
                                            alpha = rgb[j * w * colors + i * colors + c];
                                    }
                                    if (upsideDown)
                                        pixels[i, h - 1 - j] = Color.FromArgb(alpha, red, green, blue);
                                    else
                                        pixels[i, j] = Color.FromArgb(alpha, red, green, blue);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    });
            }

            Task.WaitAll(tasks);
            return pixels;
        }

        /// <summary>
        /// Export image to chosen format
        /// </summary>
        /// <param name="path">output path</param>
        /// <param name="scriptpath">script path</param>
        public void ExportImg(string path, string scriptpath)
        {
            py = Python.CreateEngine();
            s = py.CreateScope(); 
            //py.ExecuteFile(scriptpath, s);
            if (scriptpath.Length > 100)
                py.Execute(scriptpath, s);
            else
                py.ExecuteFile(scriptpath, s);

            BinaryWriter binr;
            fs = new FileStream(path, FileMode.Create);
            binr = new BinaryWriter(fs);

            pixelFormat = s.GetVariable("IMG_PIXEL_FORMAT");
            pixelFormat = pixelFormat.ToUpper();
            if (pixelFormat.Length < 4 && BPP == 32)
                pixelFormat += "A";

            int w1 = 0;
            List<string> order = s.GetVariable("WRITE_ORDER");

            for (int i = 0; i < order.Count; i++)
            {
                switch (order[i])
                {
                    case "FILE_MAGIC":
                        List<Byte> magic2 = s.GetVariable("FILE_MAGIC");
                        for (int j = 0; j < magic2.Count; j++)
                            binr.Write(magic2[j]);
                        break;
                    case "FILE_SIZE":
                        /*if (img.Width % 2 > 0)
                            w1 = 1;
                        binr.Seek(s.GetVariable(order[i] + "_OFFSET"), SeekOrigin.Begin);
                        binr.Write(0x36 + ((img.Width + w1) * img.Height * bpp / 8));*/
                        break;
                    case "IMG_PIXELS_LEN":
                        if (img.Width % 2 > 0)
                            w1 = 1;
                        binr.Seek(s.GetVariable(order[i] + "_OFFSET"), SeekOrigin.Begin);
                        binr.Write((img.Width + w1) * img.Height * BPP / 8);
                        break;
                    /*case "FILE_HEADER_SIZE":
                        binr.Seek(s.GetVariable("FILE_HEADER_SIZE_OFFSET"), SeekOrigin.Begin);
                        binr.Write(s.GetVariable("FILE_HEADER_SIZE"));
                        break;*/
                    case "IMG_WIDTH":
                        //binr.Seek(s.GetVariable(order[i] + "_OFFSET"), SeekOrigin.Begin);
                        //binr.Write(img.Width);
                        VarWriter.WriteInt32(binr, s, order[i], img.Width);
                        break;
                    case "IMG_HEIGHT":
                        //binr.Seek(s.GetVariable(order[i] + "_OFFSET"), SeekOrigin.Begin);
                        //binr.Write(img.Height);
                        VarWriter.WriteInt32(binr, s, order[i], img.Height);
                        break;
                    case "IMG_BPP":
                        binr.Seek(s.GetVariable(order[i] + "_OFFSET"), SeekOrigin.Begin);
                        binr.Write(BPP);
                        break;
                    case "IMG_PIXELS":
                        if (s.ContainsVariable("IMG_PIXELS_OFFSET"))
                            binr.Seek(s.GetVariable("IMG_PIXELS_OFFSET"), SeekOrigin.Begin);
                        //int red = 0, green = 0, blue = 0;
                        w1 = 0;
                        if (s.ContainsVariable("IMG_UNEVEN_PADDING"))
                        {
                            if (s.GetVariable("IMG_UNEVEN_PADDING"))
                            {
                                if (img.Width % 2 > 0)
                                    w1 = 1;
                            }
                        }
                        if (s.ContainsVariable("IMG_UPSIDE_DOWN"))
                            if (s.GetVariable("IMG_UPSIDE_DOWN"))
                                img = Operations.FlipVertical(img);

                        List<byte> rgb = new List<byte>();
                        if (s.ContainsVariable("IMG_PIXELS_HANDLER"))
                        {
                            if (s.GetVariable("IMG_PIXELS_HANDLER"))
                            {
                                for (int j = 0; j < img.Height; j++)
                                {
                                    for (int q = 0; q < img.Width + w1; q++)
                                    {
                                        if (q < img.Width)
                                        {
                                            for (int c = 0; c < pixelFormat.Length; c++)
                                            {
                                                if (pixelFormat[c] == 'B')
                                                    rgb.Add(img.GetPixel(q, j).B);
                                                if (pixelFormat[c] == 'G')
                                                    rgb.Add(img.GetPixel(q, j).G);
                                                if (pixelFormat[c] == 'R')
                                                    rgb.Add(img.GetPixel(q, j).R);
                                                if (pixelFormat[c] == 'A')
                                                    rgb.Add(img.GetPixel(q, j).A);
                                            }
                                        }
                                    }
                                }
                                var SavePixels = s.GetVariable("SavePixels");
                                IList<object> rgbp = new List<object>();
                                for (int z = 0; z < rgb.Count; z++)
                                    rgbp.Add(rgb[z]);
                                List<byte> rgbl = SavePixels(rgbp, img.Width, img.Height, BPP);
                                if (rgbl.Count < img.Width * img.Height)
                                    binr.Write(rgb.ToArray());
                                else
                                    binr.Write(rgbl.ToArray());
                            }
                        }
                        else
                        {
                            for (int j = 0; j < img.Height; j++)
                            {
                                for (int q = 0; q < img.Width + w1; q++)
                                {
                                    if (q < img.Width)
                                    {
                                        for (int c = 0; c < pixelFormat.Length; c++)
                                        {
                                            if (pixelFormat[c] == 'B')
                                                binr.Write(img.GetPixel(q, j).B);
                                            if (pixelFormat[c] == 'G')
                                                binr.Write(img.GetPixel(q, j).G);
                                            if (pixelFormat[c] == 'R')
                                                binr.Write(img.GetPixel(q, j).R);
                                            if (pixelFormat[c] == 'A')
                                                binr.Write(img.GetPixel(q, j).A);
                                        }
                                    }
                                    else
                                    {
                                        for (int c = 0; c < pixelFormat.Length; c++)
                                        {
                                            binr.Write((byte)0);
                                        }
                                    }
                                }
                            }
                        }
                        if (s.ContainsVariable("IMG_UPSIDE_DOWN"))
                            if (s.GetVariable("IMG_UPSIDE_DOWN"))
                                img = Operations.FlipVertical(img);
                        break;
                    default:
                        //binr.Seek(s.GetVariable(order[i] + "_OFFSET"), SeekOrigin.Begin);
                        //binr.Write(s.GetVariable(order[i]));
                        VarWriter.WriteScriptVar(binr, s, order[i]);
                        break;
                }
                
            }
            if (order.Contains("FILE_SIZE"))
            {
                int size = (int)binr.BaseStream.Position;
                binr.Seek(s.GetVariable("FILE_SIZE_OFFSET"), SeekOrigin.Begin);
                binr.Write(size);
            }
            binr.Close();
            fs.Close();
        }
    }
}
