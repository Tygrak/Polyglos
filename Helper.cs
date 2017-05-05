using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Polyglos{
    public static class PolyglosHelper{
        public static string CalculateString(string toCalculate){
            Stack<String> stack = new Stack<String>();
            string value = "";
            for (int i = 0; i < toCalculate.Length; i++){
                String s = toCalculate.Substring(i, 1);
                char chr = s.ToCharArray()[0];
                if (!char.IsDigit(chr) && chr != '.' && value != ""){
                    stack.Push(value);
                    value = "";
                }
                if (s.Equals("(")){
                    string innerExp = "";
                    i++; //Fetch Next Character
                    int bracketCount=0;
                    for (; i < toCalculate.Length; i++){
                        s = toCalculate.Substring(i, 1);
                        if (s.Equals("("))
                            bracketCount++;
                        if (s.Equals(")"))
                            if (bracketCount == 0)
                                break;
                            else
                                bracketCount--;
                        innerExp += s;
                    }
                    stack.Push(CalculateString(innerExp));
                }
                else if (s.Equals("+")) stack.Push(s);
                else if (s.Equals("-")) stack.Push(s);
                else if (s.Equals("*")) stack.Push(s);
                else if (s.Equals("/")) stack.Push(s);
                else if (s.Equals("sqrt")) stack.Push(s);
                else if (s.Equals(")")){
                }
                else if (char.IsDigit(chr) || chr == '.'){
                    value += s;
                    if (value.Split('.').Length > 2)
                        throw new Exception("Invalid decimal.");
                    if (i == (toCalculate.Length - 1))
                        stack.Push(value);
                }
                else
                    throw new Exception("Invalid character.");
            }
            double result = 0;
            while (stack.Count >= 3){
                double right = Convert.ToDouble(stack.Pop());
                string op = stack.Pop();
                double left = Convert.ToDouble(stack.Pop());
                if (op == "+") result = left + right;
                else if (op == "+") result = left + right;
                else if (op == "-") result = left - right;
                else if (op == "*") result = left * right;
                else if (op == "/") result = left / right;
                stack.Push(result.ToString());
            }
            return stack.Pop();
        }

        public static int CalculateStringInt(string toCalculate){
            return (int) Math.Round(Convert.ToDouble(CalculateString(toCalculate)));
        }

        public static float CalculateStringFloat(string toCalculate){
            return (float) Convert.ToDouble(CalculateString(toCalculate));
        }
    }
}