using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Bugger.Helpers
{
    public static class Operations
    {

        private static Dictionary<String, Func<double, double, double>> validOperations = new Dictionary<String, Func<double, double, double>>
        {
            { "+", Add },
            { "-", Sub },
            { "*", Mult },
            { "/", Div },
            { "%", Mod }
        };

        public static double PerformComputation(String sentence)
        {
            if (string.IsNullOrEmpty(sentence) || sentence.Length < 3) { return 0; }

            var seperatedValues = new List<string>();
            SeperateValues(seperatedValues, sentence);

            // Initial check, because multiplication and division have a higher priority 
            // than addition and subtraction. 
            if (seperatedValues.Count > 3)
            {
                int i = 0;
                while (i < seperatedValues.Count-1)
                {
                    if (seperatedValues[i + 1].Equals("*") || seperatedValues[i + 1].Equals("/"))
                    {
                        ReplaceSegment(seperatedValues, i);
                    }
                    else
                    {
                        i++;
                    }
                }
                while (seperatedValues.Count > 3)
                {
                    ReplaceSegment(seperatedValues, 0);
                }
            }
            double x, y;
            try
            {
                x = double.Parse(seperatedValues[0]);
                y = double.Parse(seperatedValues[2]);
            }
            catch (FormatException e)
            {
                return 0;
            }
            return validOperations[seperatedValues[1]](x, y);
        }

        private static void SeperateValues(List<String> list, String sentence)
        {
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < sentence.Length; i++)
            {
                if (validOperations.ContainsKey(sentence[i].ToString()))
                {
                    if (i==0) { buffer.Append("0"); }
                    list.Add(buffer.ToString());
                    list.Add(sentence[i].ToString());
                    buffer = new StringBuilder();
                }
                else
                {
                    buffer.Append(sentence[i]);
                }
            }
            list.Add(buffer.ToString());
        }

        private static void ReplaceSegment(List<String> list, int startIdx)
        {
            if (list.Count < 3) { return; }
            String s = "";
            s += list[startIdx+0];
            s += list[startIdx+1];
            s += list[startIdx+2];
            list.RemoveAt(startIdx);
            list.RemoveAt(startIdx);
            list[startIdx] = Operations.PerformComputation(s).ToString(CultureInfo.CurrentCulture);
        }

        private static double Add(double x, double y)
        {
            return x + y;
        }

        private static double Sub(double x, double y)
        {
            return x - y;
        }

        private static double Mult(double x, double y)
        {
            return x * y;
        }

        private static double Div(double x, double y)
        {
            return x / y;
        }

        private static double Mod(double x, double y)
        {
            return x % y;
        }
    }
}
