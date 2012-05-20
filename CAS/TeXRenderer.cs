using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace CAS
{
    class TeXRenderer
    {
        static string TEX_PATH = @"C:\Program Files\MiKTeX 2.9\miktex\bin";

        public Bitmap Render(Node node)
        {
            string tempPath = Path.GetTempPath();
            ProcessStartInfo si = new ProcessStartInfo(Path.Combine(TEX_PATH, "tex.exe"), "cas.tex");
            si.WorkingDirectory = tempPath;
            si.CreateNoWindow = true;
            si.UseShellExecute = false;
            si.RedirectStandardOutput = true;
            StreamWriter texWriter = File.CreateText(Path.Combine(tempPath, "cas.tex"));
            string tex = @"\nopagenumbers$" + texString(node) + @"$\end";
            texWriter.Write(tex);
            texWriter.Close();

            Process texProcess = Process.Start(si);
            texProcess.WaitForExit();
            File.Delete(Path.Combine(tempPath, "cas.tex"));

            si = new ProcessStartInfo(Path.Combine(TEX_PATH, "dvipng.exe"), "-T tight -D 150 cas.dvi");
            si.CreateNoWindow = true;
            si.RedirectStandardOutput = true;
            si.UseShellExecute = false;
            si.WorkingDirectory = tempPath;
            Process dvipngProcess = Process.Start(si);
            dvipngProcess.WaitForExit();

            File.Delete(Path.Combine(tempPath, "cas.dvi"));

            string pngPath = Path.Combine(tempPath, "cas1.png");
            string bitmapPath = Path.GetTempFileName();
            File.Copy(pngPath, bitmapPath, true);
            File.Delete(pngPath);
            Bitmap bitmap = new Bitmap(bitmapPath);

            return bitmap;
        }

        string texString(Node node)
        {
            string ret = "";
            switch(node.NodeType)
            {
                case Node.Type.Plus:
                    {
                        bool first = true;
                        foreach (Node child in node.Children)
                        {
                            if (!first)
                            {
                                ret += "+";
                            }
                            ret += texString(child);
                            first = false;
                        }
                    }
                    break;

                case Node.Type.Minus:
                    ret = texString(node.Children[0]) + "-";
                    if(node.Children[1].NodeType == Node.Type.Plus || node.Children[1].NodeType == Node.Type.Minus)
                    {
                        ret += "(" + texString(node.Children[1]) + ")";
                    }
                    else
                    {
                        ret += texString(node.Children[1]);
                    }
                    break;

                case Node.Type.Times:
                    {
                        bool first = true;
                        foreach (Node child in node.Children)
                        {
                            if (!first)
                            {
                                ret += @"\cdot ";
                            }

                            switch (child.NodeType)
                            {
                                case Node.Type.Plus:
                                case Node.Type.Minus:
                                    ret += "(" + texString(child) + ")";
                                    break;

                                default:
                                    ret += texString(child);
                                    break;
                            }
                            first = false;
                        }
                        break;
                    }

                case Node.Type.Divide:
                    ret = "{" + texString(node.Children[0]) + @"\over " + texString(node.Children[1]) + "}";
                    break;

                case Node.Type.Negative:
                    if (node.Children[0].NodeType == Node.Type.Plus || node.Children[0].NodeType == Node.Type.Minus)
                    {
                        ret = "-(" + texString(node.Children[0]) + ")";
                    }
                    else
                    {
                        ret = "-" + texString(node.Children[0]);
                    }
                    break;

                case Node.Type.Power:
                    if (node.Children[0].NodeType == Node.Type.Constant || node.Children[0].NodeType == Node.Type.Variable)
                    {
                        ret = texString(node.Children[0]) + "^{" + texString(node.Children[1]) + "}";
                    }
                    else
                    {
                        ret = "(" + texString(node.Children[0]) + ")^{" + texString(node.Children[1]) + "}";
                    }
                    break;

                case Node.Type.Function:
                    {
                        bool first = true;
                        ret = @"{\rm " + (string)node.Data + "} (";
                        foreach (Node arg in node.Children)
                        {
                            if (!first)
                            {
                                ret += ",";
                            }
                            ret += texString(arg);
                            first = false;
                        }
                        ret += ")";
                        break;
                    }

                default:
                    ret = node.Data.ToString();
                    break;
            }

            return ret;
        }
    }
}
