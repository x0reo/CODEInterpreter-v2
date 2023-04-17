using Interpreter.Content;

namespace Interpreter;
public class CodeVisitor : CODEBaseVisitor<object?>
{
    public Dictionary<string, object?> IntVar { get; } = new();
    public Dictionary<string, object?> FloatVar { get; } = new();
    public Dictionary<string, object?> CharVar { get; } = new();
    public Dictionary<string, object?> BoolVar { get; } = new();
    public Dictionary<string, object?> Functions { get; } = new();
    
    public CodeVisitor()
    {
        Functions["DISPLAY"] = new Func<object?[], object?>(Write);
    }

    private object? Write(object?[] args)
    {
        foreach (var arg in args)
        {
            Console.WriteLine(arg);
        }

        return null;
    }
    
    public override object? VisitAssignmentList(CODEParser.AssignmentListContext context)
    {
        var varName = context.VARIABLENAME().Select(Visit).ToArray();
        return varName;
    }

    public override object? VisitAssignment(CODEParser.AssignmentContext context)
    {
        var varName = context.assignmentList().GetText();
        var assign = varName.Split('=');
        var value = Visit(context.expression());
        
        foreach (string s in assign)
        {
            if (IntVar.ContainsKey(s))
            {
                if (value is int)
                {
                    IntVar[s] = value;
                }
                else
                {
                    throw new Exception($"Invalid assignment of variable {varName}.");
                }
            }
            else if (FloatVar.ContainsKey(s))
            {
                if (value is float)
                {
                    FloatVar[s] = value;
                }
                else
                {
                    throw new Exception($"Invalid assignment of variable {varName}.");
                }
            }
            else if (CharVar.ContainsKey(s))
            {
                if (value is string | value is char)
                {
                    CharVar[s] = value;
                }
                else
                {
                    throw new Exception($"Invalid assignment of variable {varName}.");
                }
            }
            else if (BoolVar.ContainsKey(s))
            {
                if (value is "TRUE" || value is "FALSE")
                {
                    BoolVar[s] = value;
                }
                else
                {
                    throw new Exception($"Invalid assignment of variable {varName}.");
                }
            }
        }
        return null;
    }

    public override object? VisitFunctionCall(CODEParser.FunctionCallContext context)
    {
        var name = context.FUNCTIONNAME().GetText();
        var args = context.expression().Select(Visit).ToArray();

        var argType = context.expression(0).GetType().ToString();
        if (!Functions.ContainsKey(name))
            throw new Exception($"Function {name} is not defined");
        
        if (Functions[name] is not Func<object?[], object?> func)
            throw new Exception($"Function {name} is not a function");
        
        if(argType == "CODE_Interpreter.CODEParser+ConstantExpressionContext" && (args[0] is int || args[0] is float))
            throw new Exception($"Invalid operands for concatenation");
        
        return func(args);
    }

    public void DefaultDeclaration(string varDatatype, string varName)
    {
        if (varDatatype == "INT")
        {
            CharVar[varName] = 0;
        }
        if (varDatatype == "FLOAT")
        {
            CharVar[varName] = 0.0;
        }
        if (varDatatype == "CHAR")
        {
            CharVar[varName] = ' ';
        }
        if (varDatatype == "BOOL")
        {
            CharVar[varName] = null;
        }

        throw new Exception($"Invalid assignment of variable {varName}.");
    }
    
    public override object? VisitStatement(CODEParser.StatementContext context)
    {
        var newLine = context.NEWLINE().GetText();

        if (newLine != "\n")
            throw new Exception("Invalid Code Format");
        
        return base.VisitStatement(context);
    }
    
    public override object? VisitVardec(CODEParser.VardecContext context)
    {
        var declaratorList = context.declaratorlist().GetText();
        string[] variables = declaratorList.Split(',');
        var count = variables.Length;
        var varDatatype = context.DATATYPE().GetText();

        if (declaratorList.Contains('='))
        {
            for (int x = 0; x < count; x++)
            {
                string temp = variables[x];

                if (temp.Contains('='))
                {
                    string[] variable = temp.Split('=');
                    var varName = variable[0];
                    var value = variable[1];
                    int intValue;
                    float floatValue;
                    bool isNum = int.TryParse(value, out intValue), isFloat = float.TryParse(value, out floatValue);

                    if (!isNum && !isFloat)
                    {
                        if (value.Length == 3)
                        {
                            if (value.EndsWith('\'') && value.StartsWith('\''))
                                value = value[1..^1];
                            else
                                throw new Exception($"Variable {value} format is invalid");
                        }
                        else
                        {
                            if (value == "\"TRUE\"" || value == "\"FALSE\"")
                                value = value[1..^1];
                            else
                                throw new Exception($"Variable {value} format is invalid");
                        }
                    }

                    for (int i = 0; i < count; i++)
                    {
                        if (i == count - 1)
                        {
                            if (varDatatype == "INT" && isNum)
                            {
                                HasSameType(varName);
                                IntVar[varName] = intValue;
                            }
                            else if (varDatatype == "FLOAT" && isFloat)
                            {
                                HasSameType(varName);
                                FloatVar[varName] = floatValue;
                            }
                            else if (varDatatype == "CHAR" && !isNum)
                            {
                                HasSameType(varName);
                                CharVar[varName] = value;
                            }
                            else if (varDatatype == "BOOL" && (value == "TRUE" || value == "FALSE"))
                            {
                                HasSameType(varName);
                                BoolVar[varName] = value;
                            }
                            else
                            {
                                Console.WriteLine($"Variable {varName} expected to be {varDatatype}");
                            }
                            break;
                        }
                    }
                }
                else
                {
                    HasSameType(variables[x]);
                    DefaultDeclaration(varDatatype, variables[x]);
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                HasSameType(variables[i]);
                DefaultDeclaration(varDatatype, variables[i]);
            }
        }
        return null;
    }

    public override object? VisitVariablenameExpression(CODEParser.VariablenameExpressionContext context)
    {
        var varName = context.VARIABLENAME().GetText();
        
        if (IntVar.ContainsKey(varName))
        {
            return IntVar[varName];
        }
        if (FloatVar.ContainsKey(varName))
        {
            return FloatVar[varName];
        }
        if (CharVar.ContainsKey(varName))
        {
            return CharVar[varName];
        }
        if (BoolVar.ContainsKey(varName))
        {
            return BoolVar[varName];
        }

        throw new Exception($"Variable {varName} is not defined");
    }

    public override object? VisitConstant(CODEParser.ConstantContext context)
    {
        if (context.INTVAL() != null)
        {
            return int.Parse(context.INTVAL().GetText());
        }
        if (context.FLOATVAL() != null)
        {
            return float.Parse(context.FLOATVAL().GetText());
        }
        if (context.CHARVAL() is {} c)
        {
            return c.GetText()[1..^1];
        }
        if (context.BOOLVAL() != null)
        {
            return context.BOOLVAL().GetText() == "TRUE";
        }

        if (context.STRINGVAL() is { } s)
        {
            return s.GetText()[1..^1];
        }
        
        return null;
    }

    public override object? VisitNewlineopExpression(CODEParser.NewlineopExpressionContext context)
    {
        if (context.NEWLINEOP() != null)
            return "\n";
        
        return null;
    }

    public override object? VisitConcatenateExpression(CODEParser.ConcatenateExpressionContext context)
    {
        var left = Visit(context.expression(0))?.ToString();
        var right = Visit(context.expression(1))?.ToString();
        var op = context.concatOp().GetText();

        var leftValType = Visit(context.expression(0));
        var rightValType = Visit(context.expression(1));
        var leftType = context.expression(0).GetType().ToString();
        var rightType = context.expression(1).GetType().ToString();

        if(leftType == "CODE_Interpreter.CODEParser+ConstantExpressionContext" && (leftValType is int || leftValType is float))
            throw new Exception($"Invalid operands for concatenation");
        if(rightType == "CODE_Interpreter.CODEParser+ConstantExpressionContext" && (rightValType is int || rightValType is float)) 
            throw new Exception($"Invalid operands for concatenation");
        
        if (op == "&")
        {
            if (!string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(right))
            {
                return left + right;
            }
            throw new Exception($"Invalid operands for concatenation: {(string.IsNullOrEmpty(left) ? "left" : "right")} operand is null or empty.");
        }

        throw new Exception($"Invalid concatenation operator: '{op}'");
    }
    
    public override object? VisitAdditiveExpression(CODEParser.AdditiveExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        var op = context.addOp().GetText();

        return op switch
        {
            "+" => Add(left, right),
            "-" => Subtract(left, right),
            _ => throw new NotImplementedException()
        };
    }

    private object? Add(object? left, object? right)
    {   
        if (left is int l && right is int r)
            return l + r;

        if (left is float lf && right is float rf)
            return lf + rf;
        
        if (left is int lInt && right is float rFloat)
            return lInt + rFloat;
        
        if (left is int lFloat && right is float rInt)
            return lFloat + rInt;
        
        if (left is string || right is string)
            return $"{left}{right}";

        throw new NotImplementedException($"Cannot add values of types {left?.GetType()} and {right?.GetType()}");
    }
    private object? Subtract(object? left, object? right)
    {   
        if (left is int l && right is int r)
        {
            return l - r;
        }

        if (left is float lf && right is float rf)
        {
            return lf - rf;
        }

        throw new NotImplementedException($"Cannot add values of types {left?.GetType()} and {right?.GetType()}");
    }
    
    public void HasSameType(string varName)
    {
        bool hasSame = CharVar.ContainsKey(varName) || IntVar.ContainsKey(varName) || FloatVar.ContainsKey(varName) || BoolVar.ContainsKey(varName);

        if (hasSame)
            throw new Exception($"Multiple declaration of Variable {varName}");
    }
    
    public override object? VisitWhileBlock(CODEParser.WhileBlockContext context)
    {
        Func<object?, bool> condition = context.WHILE().GetText() == "WHILE"
                ? IsTrue
                : IsFalse
            ;

        if (condition(Visit(context.expression())))
        {
            do
            {
                Visit(context.block());
            } while (condition(Visit(context.expression())));
        }

        return null;
    }
    
    private bool IsTrue(object? value)
    {
        if (value is bool b)
            return b;

        throw new Exception("Value is not a boolean");
    }

    public bool IsFalse(object? value) => !IsTrue(value);
}