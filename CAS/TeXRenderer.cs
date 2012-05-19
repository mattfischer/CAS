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

        public Bitmap Render(Expression expression)
        {
            string tempPath = Path.GetTempPath();
            ProcessStartInfo si = new ProcessStartInfo(Path.Combine(TEX_PATH, "tex.exe"), "cas.tex");
            si.WorkingDirectory = tempPath;
            si.CreateNoWindow = true;
            si.UseShellExecute = false;
            si.RedirectStandardOutput = true;
            StreamWriter texWriter = File.CreateText(Path.Combine(tempPath, "cas.tex"));
            string tex = @"\nopagenumbers$" + texString(expression) + @"$\end";
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

        string texString(Expression e)
        {
            string ret = "";
            switch(e.ExpressionType)
            {
                case Expression.Type.Plus:
                    {
                        bool first = true;
                        foreach (Expression child in e.Children)
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

                case Expression.Type.Minus:
                    ret = texString(e.Children[0]) + "-";
                    if(e.Children[1].ExpressionType == Expression.Type.Plus || e.Children[1].ExpressionType == Expression.Type.Minus)
                    {
                        ret += "(" + texString(e.Children[1]) + ")";
                    }
                    else
                    {
                        ret += texString(e.Children[1]);
                    }
                    break;

                case Expression.Type.Times:
                    {
                        bool first = true;
                        foreach (Expression child in e.Children)
                        {
                            if (!first)
                            {
                                ret += @"\cdot ";
                            }

                            switch (child.ExpressionType)
                            {
                                case Expression.Type.Plus:
                                case Expression.Type.Minus:
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

                case Expression.Type.Divide:
                    ret = "{" + texString(e.Children[0]) + @"\over " + texString(e.Children[1]) + "}";
                    break;

                case Expression.Type.Negative:
                    if (e.Children[0].ExpressionType == Expression.Type.Plus || e.Children[0].ExpressionType == Expression.Type.Minus)
                    {
                        ret = "-(" + texString(e.Children[0]) + ")";
                    }
                    else
                    {
                        ret = "-" + texString(e.Children[0]);
                    }
                    break;

                case Expression.Type.Power:
                    if (e.Children[0].ExpressionType == Expression.Type.Constant || e.Children[0].ExpressionType == Expression.Type.Variable)
                    {
                        ret = texString(e.Children[0]) + "^{" + texString(e.Children[1]) + "}";
                    }
                    else
                    {
                        ret = "(" + texString(e.Children[0]) + ")^{" + texString(e.Children[1]) + "}";
                    }
                    break;

                case Expression.Type.Function:
                    {
                        bool first = true;
                        ret = (string)e.Data + "(";
                        foreach (Expression arg in e.Children)
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
                    ret = e.Data.ToString();
                    break;
            }

            return ret;
        }
    }
}
