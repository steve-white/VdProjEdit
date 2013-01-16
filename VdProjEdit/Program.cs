using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace VdProjEdit
{
    /* This is by no means a complete application! Excuse the poor coding, it does what it needs to :) */
    class Program
    {
        static void Main(string[] args)
        {
            List<String> argsList = new List<string>(args);
            if (argsList.Count == 0)
            {
                Environment.ExitCode = -1;
                Err("Usage: VdProjEdit.exe VdProjFile AssemblyName ProductName Title");
                return;
            }
            else
            {
                String projFile = args[0];
                String assName = args[1];
                String prodName = args[2];
                String title = args[3];

                String[] vdProjTxtLines = File.ReadAllLines(projFile);
                String assemblyName = assName;
                String version = null;
                StringBuilder newTxt = new StringBuilder();
                List<String> newLines = new List<string>();
                            
                String srcAssFile = "\"SourcePath\" = \"..*:(..*)\"";
               
                foreach (String line in vdProjTxtLines)
                {

                    bool matchVer = Regex.IsMatch(line, srcAssFile);
                    if (matchVer)
                    {
                        System.Console.WriteLine("match: " + line);
                        if (line.Contains(assemblyName + ".exe"))
                        {
                            System.Console.WriteLine("match: " + line);
                            String assFile = @"C:\" + Regex.Replace(line, srcAssFile, "$1").Replace(" ","");
                            System.Console.WriteLine("assembly: " + assFile);
                            Assembly assObj = Assembly.LoadFile(assFile);
                            version = assObj.GetName().Version.ToString();
                            System.Console.WriteLine("version: " + version);
                        }
                    }
                }

                if (version == null)
                {
                    System.Console.WriteLine("Unable to find version field");
                    Environment.ExitCode = -2;
                    return;
                }
                        
                foreach (String line in vdProjTxtLines)
                {
                    String patternProdMatch = "(..*\"ProductVersion\" = \"..*:)..*(\")";

                    bool matchProdVer = Regex.IsMatch(line, patternProdMatch);
                    if (matchProdVer)
                    {
                        System.Console.WriteLine("match matchProdVer: " + line);
                        String verStr = VersionParse(version);
                        String replaceLine = Regex.Replace(line, patternProdMatch, "$1~" + verStr + "$2");
                        replaceLine = replaceLine.Replace("~", "");
                        System.Console.WriteLine("replaceLine: " + replaceLine);
                        newLines.Add(replaceLine);
                    }
                    else
                        newLines.Add(line);
                }
              
                String prodVerPatt = "(\"ProductName\" = \"..*:)" + prodName + "..*";
                String titleVerPatt = "(\"Title\" = \"..*:)(" + title + ")..*";
                System.Console.WriteLine("prodName= " + prodVerPatt);
                System.Console.WriteLine("prodTitle= " + titleVerPatt);

                foreach (String line in newLines)
                {
                    bool matchProdVerReplace = Regex.IsMatch(line, prodVerPatt);
                    bool matchTitleVerReplace = Regex.IsMatch(line, titleVerPatt);

                    if (matchProdVerReplace)
                    {
                        System.Console.WriteLine("match [ProductVersion] ProductName: " + line);

                        String newLine = Regex.Replace(line, prodVerPatt, "$1~" + prodName + " " + version + "\"");
                        newLine = newLine.Replace("~","");
                        System.Console.WriteLine("replaceLineProdName: " + newLine);
                        newTxt.AppendLine(newLine);
                    }
                    else
                    {
                        if (matchTitleVerReplace)
                        {
                            System.Console.WriteLine("match [ProductVersion] Title: " + line);
                            String newLine = Regex.Replace(line, titleVerPatt, "$1~" + title + " " + version + "\"");
                            newLine = newLine.Replace("~", "");
                            System.Console.WriteLine("replaceLineTitle: " + newLine);
                            newTxt.AppendLine(newLine);
                        }
                        else
                            newTxt.AppendLine(line);
                    }
                }
                File.WriteAllText(projFile, newTxt.ToString());
            }
        }

        private static string VersionParse(string version)
        { 
            int major = 0;
            int minor = 0;
            int build = 0;
            int revision = 0;

            String[] fields = version.Split('.');
            if (fields.Length >= 4)
            {
                major = int.Parse(fields[0]);
                minor = int.Parse(fields[1]);
                build = int.Parse(fields[2]);
                revision = int.Parse(fields[3]);
            }
            Version v = new Version(major, minor, build, revision);
            return v.Major + "." + v.Minor + "." + v.Build;
        }

        private static void Err(string p)
        {
            System.Console.WriteLine(p);
        }
    }
}
