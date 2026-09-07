using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

public enum TipoToken
{
    Blanco,
    Keyword,
    Identificador,
    Numero,
    String,
    Comentario,
    Operador,
    Delimitador,
    Desconocido
}

public record Token(TipoToken Tipo, string Valor)
{
    public string GetDescription()
    {
        return Tipo switch
        {
            TipoToken.Keyword => Valor switch
            {   
                "main" => "Funcion principal del programa",
                "if" => "Condicional (si)",
                "else" => "Condicional (sino)",
                "while" => "Bucle (mientras)",
                "for" => "Bucle (para)",
                "return" => "Retorno de función",
                "int" => "Tipo de dato entero",
                "float" => "Tipo de dato decimal",
                "string" => "Tipo de dato cadena de texto",
                "bool" => "Tipo de dato booleano",
                "void" => "Tipo de retorno vacío",
                "true" => "Valor booleano verdadero",
                "false" => "Valor booleano falso",
                _ => "Palabra reservada"
            },
            TipoToken.Operador => Valor switch
            {
                "+" => "Suma", "-" => "Resta", "*" => "Multiplicación", "/" => "División", "%" => "Módulo",
                "=" => "Asignación", "==" => "Igualdad", "!=" => "Diferencia",
                "<" => "Menor que", ">" => "Mayor que", "<=" => "Menor o igual que", ">=" => "Mayor o igual que",
                "&&" => "AND lógico", "||" => "OR lógico", "!" => "NOT lógico",
                "++" => "Incremento", "--" => "Decremento",
                "+=" => "Suma y asigna", "-=" => "Resta y asigna", "*=" => "Multiplica y asigna", "/=" => "Divide y asigna",
                "&" => "AND a nivel de bits", "|" => "OR a nivel de bits", "^" => "XOR a nivel de bits",
                _ => "Operador"
            },
            TipoToken.Delimitador => Valor switch
            {
                "(" => "Paréntesis de apertura", ")" => "Paréntesis de cierre",
                "{" => "Llave de apertura", "}" => "Llave de cierre",
                "[" => "Corchete de apertura", "]" => "Corchete de cierre",
                "," => "Coma (Separador)", "." => "Punto (Acceso)",
                ";" => "Punto y coma (Fin de instrucción)", ":" => "Dos puntos",
                _ => "Delimitador"
            },
            TipoToken.Identificador => "Identificador definido por el usuario",
            TipoToken.Numero => Valor.Contains('.') 
                ? "Valor numérico decimal (float)" 
                : "Valor numérico entero (int)",
            TipoToken.Desconocido => "Símbolo no reconocido",
            TipoToken.String => "Cadena de texto.",
            TipoToken.Comentario => "Comentario en el codigo",
            _ => string.Empty
        };
    }

    public override string ToString()
    {
        return $"Tipo = {Tipo,-14} || Valor = \"{Valor}\" || {GetDescription()}";
    }
}

public class Tokenizador
{
    private readonly string _input;
    private int _posicion;

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "main", "if", "else", "while", "for", "return", 
        "int", "float", "string", "bool", "void", 
        "true", "false"
    };

    public Tokenizador(string input)
    {
        _input = input;
        _posicion = 0;
    }

    private char CurrentChar => _posicion < _input.Length ? _input[_posicion] : '\0';
    private char NextChar => (_posicion + 1) < _input.Length ? _input[_posicion + 1] : '\0';
    private void AvanzarPos() => _posicion++;

    public List<Token> Tokenizar()
    {
        List<Token> tokens = new List<Token>();

        while (_posicion < _input.Length)
        {
            char c = CurrentChar;

            if (char.IsWhiteSpace(c))
            {
                tokens.Add(new Token(TipoToken.Blanco, c.ToString()));
                AvanzarPos();
            }
            else if (char.IsLetter(c))
            {
                tokens.Add(LeerIdentificadorOrKeyword());
            }
            else if (c == '/' && (NextChar == '/' || NextChar == '*'))
            {
                tokens.Add(LeerComentario());
            }
            else if (c == '"' || c == '\'')
            {
                tokens.Add(LeerCadena());
            }
            else if (char.IsDigit(c))
            {
                tokens.Add(LeerNumero());
            }
            else if ("+-*/=<>!&|^%".Contains(c))
            {
                tokens.Add(LeerOperador());
            }
            else if ("(){}[].,;:".Contains(c))
            {
                tokens.Add(new Token(TipoToken.Delimitador, c.ToString()));
                AvanzarPos();
            }
            else
            {
                tokens.Add(new Token(TipoToken.Desconocido, c.ToString()));
                AvanzarPos();
            }
        }

        return tokens;
    }

    private Token LeerIdentificadorOrKeyword()
    {
        StringBuilder sb = new StringBuilder();
        while (_posicion < _input.Length && char.IsLetterOrDigit(CurrentChar))
        {
            sb.Append(CurrentChar);
            AvanzarPos();
        }

        string text = sb.ToString();

        if (Keywords.Contains(text))
        {
            return new Token(TipoToken.Keyword, text);
        }

        return new Token(TipoToken.Identificador, text);
    }

    private Token LeerNumero()
    {
        StringBuilder sb = new StringBuilder();
        bool punto = false;

        while (_posicion < _input.Length)
        {
            char c = CurrentChar;

            if (char.IsDigit(c))
            {
                sb.Append(c);
                AvanzarPos();
            }
            else if (c == '.' && !punto && char.IsDigit(NextChar))
            {
                punto = true;
                sb.Append(c);
                AvanzarPos();
            }
            else
            {
                break;
            }
        }

        return new Token(TipoToken.Numero, sb.ToString());
    }

    private Token LeerOperador()
    {
        char c = CurrentChar;
        char next = NextChar;
        string combined = $"{c}{next}";

        string[] doubleOperadors = { "==", "!=", "<=", ">=", "&&", "||", "++", "--", "+=", "-=", "*=", "/=" };

        if (Array.Exists(doubleOperadors, op => op == combined))
        {
            AvanzarPos(); 
            AvanzarPos(); 
            return new Token(TipoToken.Operador, combined);
        }

        AvanzarPos(); 
        return new Token(TipoToken.Operador, c.ToString());
    }
    
    private Token LeerCadena()
    {
        char delimitador = CurrentChar; 
        StringBuilder sb = new StringBuilder();
        
        sb.Append(delimitador);
        AvanzarPos();

        while (_posicion < _input.Length)
        {
            char c = CurrentChar;
            
            if (c == '\\' && NextChar != '\0')
            {
                sb.Append(c);        
                AvanzarPos();
                sb.Append(CurrentChar); 
                AvanzarPos();
                continue;
            }
            
            if (c == delimitador)
            {
                sb.Append(c);
                AvanzarPos();
                return new Token(TipoToken.String, sb.ToString());
            }

            sb.Append(c);
            AvanzarPos();
        }
        
        return new Token(TipoToken.Desconocido, sb.ToString());
    }
    
    private Token LeerComentario()
    {
        StringBuilder sb = new StringBuilder();

        // Single line
        if (CurrentChar == '/' && NextChar == '/')
        {
            while (_posicion < _input.Length && CurrentChar != '\n' && CurrentChar != '\r')
            {
                sb.Append(CurrentChar);
                AvanzarPos();
            }
            return new Token(TipoToken.Comentario, sb.ToString());
        }

        // Multilinea
        if (CurrentChar == '/' && NextChar == '*')
        {
            sb.Append("/*");
            AvanzarPos(); // Consume /
            AvanzarPos(); // Consume *

            while (_posicion < _input.Length)
            {
                if (CurrentChar == '*' && NextChar == '/')
                {
                    sb.Append("*/");
                    AvanzarPos(); // Consume *
                    AvanzarPos(); // Consume /
                    return new Token(TipoToken.Comentario, sb.ToString());
                }

                sb.Append(CurrentChar);
                AvanzarPos();
            }
            
            return new Token(TipoToken.Desconocido, sb.ToString());
        }

        return new Token(TipoToken.Desconocido, string.Empty);
    }
}

class Program
{
    static void Main(string[] args)
    {
        string path = $"{AppContext.BaseDirectory}/prueba.txt";

        if (!File.Exists(path))
        {
            Console.WriteLine($" No se encontró el archivo '{path}' en el directorio actual.");
            Console.WriteLine($"CWD: {AppContext.BaseDirectory}");
            return;
        }

        try
        {
            string sourceCode = File.ReadAllText(path);

            Console.WriteLine("\t\tokenizador iniciado\n");

            Tokenizador tokenizador = new Tokenizador(sourceCode);
            List<Token> tokens = tokenizador.Tokenizar();

            foreach (var token in tokens)
            {
                if (token.Tipo != TipoToken.Blanco)
                {
                    Console.WriteLine(token);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
        }
    }
}