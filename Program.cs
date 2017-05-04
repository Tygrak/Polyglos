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
        public string program = "";
        public List<GlosVar> variables = new List<GlosVar>();

        public PolyglosProgram(string program){
            this.program = program;
        }

        public void RunProgram(){

        }
    }

    public class Glos{
        public string glosProgram = "";
        public Process process;

        public Glos(string glosText){
            this.glosProgram = glosText;
        }

    }
}
