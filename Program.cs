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
            /*Glos cGlos = new Glos("pes", GlosLang.C, "#include <stdio.h>\nint main(int argc,char *argv[]){printf(\"Hello!\");}");
            cGlos.CreateFile();
            cGlos.RunFile();
            cGlos.ReadOutput();*/
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
        public int position = 0;

        public PolyglosProgram(string program){
            curr = this;
            this.program = program;
        }

        public string GetNextCommand(){
            int nextPos = program.IndexOf(";", position);
            if(nextPos == -1){
                return null;
            }
            nextPos += 1;
            if(program.Substring(position, nextPos-position).Contains("@{@")){
                nextPos = program.IndexOf("@}@;", position) + 4;
            }
            string nextCommand = program.Substring(position, nextPos-position);
            position = nextPos;
            nextCommand = nextCommand.Trim();
            return nextCommand;
        }

        public void RunProgram(){
            //Console.WriteLine(program);
            string nextCommand = "";
            int breaker = 0;
            while(nextCommand != null){
                nextCommand = GetNextCommand();
                Console.WriteLine(breaker.ToString() + " : " + nextCommand);
                breaker++;
                if(breaker > 500 || nextCommand == null){
                    Console.WriteLine("End.");
                    break;
                }
                EvaluateCommand(nextCommand);
            }
        }

        public void EvaluateCommand(string command){
            command = command.Remove(command.Length-1);
            if(command.StartsWith("int ")){
                int pos = command.IndexOf(" ", 3);
                string name = command.Substring(4, pos-2);
                name = name.Trim();
                pos = command.IndexOf("=")+1;
                string value = command.Substring(pos);
                value = value.Trim();
                variables.Add(new IntVar(name, PolyglosHelper.CalculateStringInt(value)));
            } else if(command.StartsWith("glos ")){
                int pos = command.IndexOf(" ", 5);
                string name = command.Substring(5, pos-5);
                name = name.Trim();
                pos = command.IndexOf("=", pos)+1;
                int glosStartPos = command.IndexOf("@{@", pos);
                int glosEndPos = command.IndexOf("@}@", glosStartPos+3);
                string glosType = command.Substring(pos, glosStartPos-pos);
                glosType = glosType.Trim();
                string glosContent = command.Substring(glosStartPos+5, glosEndPos-glosStartPos-6);
                //Console.WriteLine("glos " + name + " = " + glosType + "@{@" + glosContent + "@}@");
                if(glosType == "langC"){
                    variables.Add(new Glos(name, GlosLang.C, glosContent));
                } else if(glosType == "langPython"){
                    variables.Add(new Glos(name, GlosLang.Python, glosContent));
                } else if(glosType == "textfile"){
                    variables.Add(new Glos(name, GlosLang.Text, glosContent));
                }
            } else{
                if(command.Contains(".")){
                    int pos = command.IndexOf(".");
                    string varName = command.Substring(0, pos);
                    int id = variables.FindIndex(x => x.GetName() == varName);
                    //Console.WriteLine(varName + ", id: " + id);
                    if(id != -1){
                        int paramPos = command.IndexOf("(", pos);
                        string funcName = command.Substring(pos+1, paramPos-pos-1);
                        if(variables[id].GetType() == typeof(IntVar)){
                            IntVar var = (IntVar) variables[id];
                        } else if(variables[id].GetType() == typeof(Glos)){
                            Glos var = (Glos) variables[id];
                            if(funcName == "RunLoud"){
                                var.CreateFile();
                                var.RunFile();
                                Console.WriteLine(var.output);
                            } else if(funcName == "Run"){
                                var.CreateFile();
                                var.RunFile();
                            }
                        }
                    }
                } else if(command.Contains("=")){
                    int pos = command.IndexOf("=");
                    string varName = command.Substring(0, pos);
                    varName = varName.Trim();
                    int id = variables.FindIndex(x => x.GetName() == varName);
                    //Console.WriteLine(varName + ", id: " + id);
                    if(id != -1){
                        string value = command.Substring(pos+1, command.Length-pos-1);
                        value = ReplaceVariables(value);
                        value = value.Trim();
                        if(variables[id].GetType() == typeof(IntVar)){
                            IntVar var = (IntVar) variables[id];
                            var.value = PolyglosHelper.CalculateStringInt(value);
                        }
                    }
                }
            }
        }

        public string ReplaceVariables(string toReplace){
            for (int i = 0; i < variables.Count; i++){
                if(variables[i].GetType() == typeof(IntVar)){
                    IntVar var = (IntVar) variables[i];
                    toReplace = toReplace.Replace("@"+var.GetName(), var.value.ToString());
                }
            }
            return toReplace;
        }
    }

    public class Glos : GlosVar{
        public string glosProgram = "";
        public GlosLang lang;
        public string include;
        public Process process;
        public string fileLocation;
        public string output;

        public Glos(string name, GlosLang lang, string glosText){
            this.name = name;
            this.lang = lang;
            this.glosProgram = glosText;
            this.include = "#include <stdio.h>\n";
            AddDefaults();
        }

        public Glos(string name, GlosLang lang, string glosText, string include){
            this.name = name;
            this.lang = lang;
            this.glosProgram = glosText;
            if(!include.Contains("#include <stdio.h>")){
                this.include = "#include <stdio.h>\n";
            }
            this.include += include;
            AddDefaults();
        }

        private void AddDefaults(){
            if(lang == GlosLang.C && !glosProgram.Contains("int main")){
                glosProgram = glosProgram.Insert(0, "int main(int argc,char *argv[]){\n");
                if(include != null){
                    glosProgram = glosProgram.Insert(0, include);
                }
                glosProgram += "}";
            }
        }

        public void CreateFile(){
            string glosText = glosProgram;
            glosText = PolyglosProgram.curr.ReplaceVariables(glosText);
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
                        FileName = "gcc",
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
                output = process.StandardOutput.ReadToEnd();
            } else if(lang == GlosLang.Python){
                process = new Process{
                    StartInfo = new ProcessStartInfo{
                        FileName = "python",
                        Arguments = fileLocation,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                output = process.StandardOutput.ReadToEnd();
            }
        }
    }

    public enum GlosLang{
        C, Python, Text
    }
}
