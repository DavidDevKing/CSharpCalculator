using System.Text;

namespace CalculatorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string ans = "";

            while (true)
            {
                Console.WriteLine("Enter An Expression: ");
                string expression = ReadLine() ?? "";
                expression = expression.Replace("ans", ans == "" ? "0" : ans);
                if (Arithmetic.Evaluate(expression, out string solution)) ans = solution;
                Console.WriteLine("ans = " +solution + "\n");
            }
        }

        static string ReadLine(string baseString = "")
        {
            StringBuilder buffer = new();
            bool pressedKey = false;
            
            if (baseString != "")
            {
                buffer.Append(baseString);
                Console.Write(baseString);
            }
            while (true)
            {

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return buffer.ToString();
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Length > 0)
                    {
                        buffer.Remove(buffer.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    if (!pressedKey && Arithmetic.isOperator(keyInfo.KeyChar))
                    {
                        buffer.Append("ans");
                        Console.Write("ans");
                    }
                    pressedKey = true;
                    buffer.Append(keyInfo.KeyChar);
                    Console.Write(keyInfo.KeyChar);
                }
            }
        }
    }
}