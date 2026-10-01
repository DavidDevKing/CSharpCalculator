class Arithmetic
{
    ///<summary>
    /// Evaluates an expression from a string
    /// </summary>
    /// <param name="expression">The expression string to be evaluated</param>
    /// <param name="solution">Output parameter that holds the solution to the expression</param>
    /// <returns> True if evaluation is successful</returns>
    public static bool Evaluate(string expression, out string solution)
    {
        solution = "";
        static bool EmptyExpression(ref string solution)
        {
            solution = "Empty Expression";
            return false;
        }
        static bool SyntaxError(ref string solution)
        {
            solution = "Syntax Error";
            return false;
        }
        static bool MathError(ref string solution)
        {
            solution = "Math Error";
            return false;
        }

        string parsedExpression = expression.Replace(" ", "");
        if (parsedExpression == "") return EmptyExpression(ref solution);

        // Handle syntax errors
        for (int i = 0; i < parsedExpression.Length; i++)
        {
            char cur = parsedExpression[i];
            if (!(isNumber(cur) || isOperator(cur) || isParentheses(cur))) return SyntaxError(ref solution);

            if (i == 0)
            {
                if (cur == '+' || cur == '*' || cur == '/' || cur == 'E') return SyntaxError(ref solution);
            }
            else
            {
                if (cur == '(' && (isNumber(parsedExpression[i-1]) || parsedExpression[i-1] == ')'))
                {
                    parsedExpression = parsedExpression.Insert(i, "*");
                }
            }
            if (i == parsedExpression.Length-1)
            {
                if (isOperator(cur) || cur == 'E') return SyntaxError(ref solution);
            }
            else
            {
                if (cur == 'E' && (!isNumber(parsedExpression[i-1]) || parsedExpression[i+1] != '+' && parsedExpression[i+1] != '-')) return SyntaxError(ref solution);
            }
        }



        // Evaluate Parentheses
        string innerExpression = "";
        int encapsulations = 0;

        // Extract the expression from the parentheses
        for (int i = 0; i < parsedExpression.Length; i++)
        {
            if (parsedExpression[i] == ')')
            {
                encapsulations--;
                if (encapsulations < 0) return SyntaxError(ref solution);
                if (encapsulations == 0)
                {
                    
                    // Evaluate the inner expression
                    if (!Evaluate(innerExpression, out string sol))
                    {
                        if (sol != "Empty Expression") return MathError(ref solution);
                    }
                    parsedExpression = parsedExpression.Remove(i-innerExpression.Length-1, innerExpression.Length+2);
                    i -= innerExpression.Length+1;
                    parsedExpression = parsedExpression.Insert(i, sol == "Empty Expression" ? "" : sol);
                    if (parsedExpression == "") break;

                    // Clear the inner expression
                    innerExpression = "";

                    // Clamp the index
                    i = i > parsedExpression.Length-1 ? parsedExpression.Length-1 : (i < 0 ? 0 : i);
                }
            }

            if (encapsulations > 0) innerExpression += parsedExpression[i];
            
            if (parsedExpression[i] == '(') encapsulations++;

        }
        // Throw an error if we didn't close all parentheses
        if (encapsulations != 0) return SyntaxError(ref solution);

        // Check again if the expression is empty
        if (parsedExpression == "") return EmptyExpression(ref solution);


        // Exponents
        for(int i = parsedExpression.Length-1; i >= 0; i--)
        {
            if (parsedExpression[i] == '^')
            {
                if (!parseValuesFromExpression(parsedExpression, i, out double[] values, out int[] distances)) return SyntaxError(ref solution);
                

                double express = Math.Pow(values[0], values[1]);

                // delete old expression
                parsedExpression = parsedExpression.Remove(i+1, distances[1]);
                parsedExpression = parsedExpression.Remove(i-distances[0], distances[0]+1);
                i-=distances[0];

                // substitute new value
                parsedExpression = parsedExpression.Insert(i,express.ToString());
                
                // Math error if we get infinity
                if (!double.IsFinite(express)) return MathError(ref solution);
            }
        }



        // Multiplication and Division
        for(int i = 0; i < parsedExpression.Length; i++)
        {
            if (parsedExpression[i] == '*' || parsedExpression[i] == '/')
            {
                if (!parseValuesFromExpression(parsedExpression, i, out double[] values, out int[] distances)) return SyntaxError(ref solution);
                
                // Handle division by zero
                if (parsedExpression[i] == '/' && values[1] == 0)
                {
                    solution = "Divide By Zero Error";
                    return false;
                }


                double express = parsedExpression[i] == '*' ? values[0]*values[1] : values[0]/values[1];

                // delete old expression
                parsedExpression = parsedExpression.Remove(i+1, distances[1]);
                parsedExpression = parsedExpression.Remove(i-distances[0], distances[0]+1);
                i-=distances[0];

                // substitute new value
                parsedExpression = parsedExpression.Insert(i,express.ToString());

                // Math error if we get infinity
                if (!double.IsFinite(express)) return MathError(ref solution);
            }
        }


        // Addition and Subtraction
        for(int i = 0; i < parsedExpression.Length; i++)
        {
            if ((parsedExpression[i] == '+' || parsedExpression[i] == '-' && i > 0) && parsedExpression[i-1] != 'E')
            {
                if (!parseValuesFromExpression(parsedExpression, i, out double[] values, out int[] distances)) return SyntaxError(ref solution);
                double express = parsedExpression[i] == '+' ? values[0]+values[1] : values[0]-values[1];

                // delete old expression
                parsedExpression = parsedExpression.Remove(i+1, distances[1]);
                parsedExpression = parsedExpression.Remove(i-distances[0], distances[0]+1);
                i-=distances[0];

                // substitute new value
                parsedExpression = parsedExpression.Insert(i,express.ToString());

                // Math error if we get infinity
                if (!double.IsFinite(express)) return MathError(ref solution);
            }
        }

        solution = parsedExpression;
        // Clean up any errors we missed in the initial checks like decimals.
        if (!double.TryParse(solution, out _)) return SyntaxError(ref solution);
        return true;
    }


    ///<summary>
    /// Looks for the values on both sides of an expression given an expression string and the index of the operator.
    /// </summary>
    /// <param name="expression">A string containing the full expression.</param>
    /// <param name="operatorIndex">The index of the operator we check values from both sides from.</param>
    /// <param name="values">Output parameter that holds the 2 values of the expression. 0 for left and 1 for right.</param>
    /// <param name="distances">Output parameter that holds the distances of the 2 values from the expression. 0 for left and 1 for right</param>
    /// <returns>True if parsing was successful</returns>
    /// <code>
    /// char expression = '1+125*17';
    /// bool isSuccessful = parseValuesFromExpression(expression, 5, out double[] values, out double distances);
    /// </code>
    private static bool parseValuesFromExpression(string expression, int operatorIndex, out double[] values, out int[] distances)
    {
        values = [0,0];
        distances = [0,0];

        char opp = expression[operatorIndex];
        // Left value
        string leftValueString = "";
        for (int i = operatorIndex-1; i >= 0; i--)
        {
            char cur = expression[i];
            if (isOperator(cur) || isParentheses(cur))
            {
                if (i == 0)
                {
                    if (cur == '-' && opp != '^') leftValueString += cur;
                    break;
                }
                char prev = expression[i-1];
                if (cur == '-')
                {
                    if (prev != 'E')
                    {
                        if (opp != '^') leftValueString += cur;
                        break;
                    }
                }
                else if (cur == '+')
                {
                    if (prev != 'E') break;
                }
                else break;
            }
            leftValueString += cur;
        }
        char[] arr = leftValueString.ToCharArray();
        Array.Reverse(arr);
        leftValueString = new string (arr);
        if (!double.TryParse(leftValueString, out double leftValue)) return false;
        int leftDistance = leftValueString.Length;
        distances[0] = leftDistance;
        values[0] = leftValue;

        // Right Value
        string rightValueString = "";
        bool foundMinus = false;
        for (int i = operatorIndex+1; i < expression.Length; i++)
        {
            if (isOperator(expression[i]) || isParentheses(expression[i]))
            {
                if (expression[i] == '+')
                {
                    if (expression[i-1] != 'E') break;
                }
                else if (expression[i] == '-')
                {
                    if (foundMinus && expression[i-1] != 'E') break; 
                    if (expression[i-1] !='E') foundMinus = true;
                }
                else break;
            }
            rightValueString += expression[i];
        }
        if (!double.TryParse(rightValueString, out double rightValue)) return false;
        int rightDistance = rightValueString.Length;
        distances[1] = rightDistance;
        values[1] = rightValue;


        return true;
    }



    ///<summary>
    /// Checks whether or not the input character is a number or a decimal point.
    /// </summary>
    /// <param name="input">The input character to check.</param>
    /// <returns> True if input is a number or decimal point.</returns>
    /// <code>
    /// char input = '7';
    /// bool check = isNumber(input);
    /// </code>
    private static bool isNumber(char input)
    {
        return input == '0' || input == '1' || input == '2' || input == '3' || input == '4' || input == '5' || input == '6' || input == '7' || input == '8' || input == '9' || input == '.' || input == 'E';
    }



    ///<summary>
    /// Checks whether or not the input is an arithmetic operator.
    /// </summary>
    /// <param name="input">The input character to check.</param>
    /// <returns> True if input is any of the arithmetic operators +, -, * or /.</returns>
    /// <code>
    /// char input = '-';
    /// bool check = isOperator(input);
    /// </code>
    public static bool isOperator(char input)
    {
        return input == '+' || input == '-' || input == '*' || input == '/' || input == '^';
    }



    ///<summary>
    /// Checks whether or not the input is a parenthesis.
    /// </summary>
    /// <param name="input">The input character to check.</param>
    /// <returns> True if input is a left parentheses or right parentheses.</returns>
    /// <code>
    /// char input = '(';
    /// bool check = isParentheses(input);
    /// </code>
    private static bool isParentheses(char input)
    {
        return input == '(' || input == ')';
    }
}
