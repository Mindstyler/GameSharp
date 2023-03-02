namespace GameSharp;

internal static class GameSharpCompiler
{
    private static readonly Stack<string> _operatorStack = new();
    private static readonly List<string> _postfix = new();
    private static readonly List<string> _tokens = new();

    internal static void Tokenize(string expression)
    {
        // place to accumulate digits
        string number = string.Empty;

        // loop by char through code
        foreach (char c in expression)
        {
            // operators can end tokens as well as being one
            if (IsOperator(c.ToString()) || "()".Contains(c))
            {
                if (number != string.Empty)
                {
                    _tokens.Add(number);
                    number = string.Empty;
                }

                _tokens.Add(c.ToString());
            }
            // spaces end numbers
            else if (c == ' ')
            {
                if (number != string.Empty)
                {
                    _tokens.Add(number);
                    number = string.Empty;
                }
            }
            // digits can combine, so store each one
            else if ("0123456789".Contains(c))
            {
                number += c;
            }
            else
            {
                // barf if you encounter something not a digit, space, or operator
                throw new /*Parse*/Exception($"Unexpected character '{c}'.");
            }
        }

        // add the last token if there is one
        if (number != string.Empty)
        {
            _tokens.Add(number);
        }
    }

    internal static bool IsOperator(string token) => "+-*/÷^%".Contains(token);

    internal static List<string> Parse()
    {
        foreach (string token in _tokens)
        {
            if (IsOperator(token))
            {
                PopOperators(token);
                _operatorStack.Push(token);
            }
            else if (token == "(")
            {
                // parents only go on stack temporarily, as they're not ops
                _operatorStack.Push(token);
            }
            else if (token == ")")
            {
                while (_operatorStack.Peek() != "(")
                {
                    _postfix.Add(_operatorStack.Pop());
                }

                _operatorStack.Pop();
            }
            else if (int.TryParse(token, out _))
            {
                _postfix.Add(token);
            }
            else
            {
                throw new /*Parse*/Exception($"Unrecognized token: {token}.");
            }
        }

        while (_operatorStack.Count > 0)
        {
            _postfix.Add(_operatorStack.Pop());
        }

        return _postfix;
    }

    private static void PopOperators(string token)
    {
        Operator cur = OperatorFromString(token);

        // if there are no operators, get out
        if (_operatorStack.Count == 0)
        {
            return;
        }

        try
        {
            for (Operator top = OperatorFromString(_operatorStack.Peek());
                 (_operatorStack.Count > 0 && GreaterPrecedence(top, cur));
                 top = OperatorFromString(_operatorStack.Peek()))
            {
                _postfix.Add(_operatorStack.Pop());
            }
        }
        catch (/*Parse*/Exception)
        {
            // it's a parenthesis, which can't be parsed as an operator
            return;
        }

        static Operator OperatorFromString(string token)
        {
            return token switch
            {
                "-" => new Operator { Associativity = Operator.Associativities.Left, Precedence = 0 },
                "/" or "÷" => new Operator { Associativity = Operator.Associativities.Left, Precedence = 1 },
                "+" => new Operator { Associativity = Operator.Associativities./*Associative*/Left, Precedence = 0 },
                "*" => new Operator { Associativity = Operator.Associativities./*Associative*/Left, Precedence = 1 },
                "^" => new Operator { Associativity = Operator.Associativities./*Associative*/Left, Precedence = 1 }
            };
        }

        static bool GreaterPrecedence(Operator a, Operator b)
        {
            return (a.Precedence > b.Precedence) || ((a.Precedence == b.Precedence) && (b.Associativity == Operator.Associativities.Left));
        }
    }
}
