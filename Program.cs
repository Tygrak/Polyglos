using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Polyglos{
    public class Program{
        public static void Main(string[] args){
            string path = "";
            if(args.Length > 0){
                if(args[0].Contains(":")){
                    path = args[0];
                } else{
                    path = Directory.GetCurrentDirectory()+"\\"+args[0];
                }
            } else{
                Console.WriteLine("Choose file to run: ");
                string line = Console.ReadLine();
                if(line.Contains(":")){
                    path = line;
                } else{
                    path = Directory.GetCurrentDirectory()+"\\"+line;
                }
            }
            //path = Directory.GetCurrentDirectory()+"\\test.~-";
            string inputText = "";
            try{
                inputText = File.ReadAllText(path);
            } catch{
                Console.WriteLine("File not found.");
                return;
            }
            PolyglosProgram prog = new PolyglosProgram(inputText);
            prog.RunProgram();
            Glos cGlos = new Glos("pes", GlosLang.C, "#include <stdio.h>\nint main(int argc,char *argv[]){printf(\"Hello!\");}");
            cGlos.CreateFile();
            cGlos.RunFile();
            cGlos.ReadOutput();
        }
    }

    public class GlosVar{
        protected string name;
        public GlosVar(){
            name = "";
        }

        public virtual string GetName(){
            return name;
        }
    }

    public class IntVar : GlosVar{
        public int value = 0;
        public IntVar(string name, int value){
            this.name = name;
            this.value = value;
        }
    }

    public class PolyglosProgram{
        public static PolyglosProgram curr;
        public string program = "";
        public List<GlosVar> variables = new List<GlosVar>();

        public PolyglosProgram(string program){
            curr = this;
            this.program = program;
        }

        public void RunProgram(){

        }
    }

    public class Glos : GlosVar{
        public string glosProgram = "";
        public GlosLang lang;
        public Process process;
        public string fileLocation;

        public Glos(string name, GlosLang lang, string glosText){
            this.name = name;
            this.lang = lang;
            this.glosProgram = glosText;
        }

        public void CreateFile(){
            string glosText = glosProgram;
            List<GlosVar> variables = PolyglosProgram.curr.variables;
            for (int i = 0; i < variables.Count; i++){
                if(variables[i].GetType() == typeof(IntVar)){
                    IntVar var = (IntVar) variables[i];
                    glosText = glosText.Replace("@"+var.GetName(), var.value.ToString());
                }
            }
            string path = Directory.GetCurrentDirectory()+"\\"+name;
            if(lang == GlosLang.C) path += ".c";
            else if(lang == GlosLang.Python) path += ".py";
            else{path += ".txt";}
            File.WriteAllText(path, glosText);
            fileLocation = path;
        }

        public void RunFile(){
            if(lang == GlosLang.C){
                Process bProcess = new Process{
                    StartInfo = new ProcessStartInfo{
                        FileName = "gcc.exe",
                        Arguments = fileLocation + " -o " + name,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                bProcess.Start();
                bProcess.WaitForExit();
                process = new Process{
                    StartInfo = new ProcessStartInfo{
                        FileName = name,
                        Arguments = fileLocation,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
            }
        }

        public void ReadOutput(){
            Console.WriteLine(process.StandardOutput.ReadToEnd());
        }
    }

    public enum GlosLang{
        C, Python
    }
}
