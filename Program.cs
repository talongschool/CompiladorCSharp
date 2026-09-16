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
    Char,
    Comentario,
    Operador,
    Delimitador,
    Error,
    EOF,
    Desconocido
}

public record Token(TipoToken Tipo, string Valor, int Linea, int Columna)
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
            TipoToken.Char => "Caracter individual",
            TipoToken.Error => "Error léxico (Símbolo desconocido o no cerrado)",
            TipoToken.Comentario => "Comentario en el codigo",
            TipoToken.EOF => "Final del archivo.",
            _ => string.Empty
        };
    }

    public override string ToString()
    {
        return $"[L:{Linea:D3} C:{Columna:D3}] Tipo = {Tipo,-14} || Valor = \"{Valor}\"";
    }
}

public class Tokenizador
{
    private readonly string _input;
    private int _posicion;
    private int _linea = 1;
    private int _columna = 1;

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

    private void AvanzarPos()
    {
        if (_posicion >= _input.Length) return;
        if (CurrentChar == '\n')
        {
            _linea++;
            _columna = 1;
        }
        else
        {
            _columna++;
        }

        _posicion++;
    }

    public IEnumerable<Token> Tokenizar()
    {
        while (_posicion < _input.Length)
        {
            char c = CurrentChar;
            int inicioLinea = _linea;
            int inicioColumna = _columna;
            if (char.IsWhiteSpace(c))
            {
                AvanzarPos();
            }
            else if (char.IsLetter(c) || c == '_')
            {
                yield return LeerIdentificadorOrKeyword(inicioLinea, inicioColumna);
            }
            else if (c == '/' && (NextChar == '/' || NextChar == '*'))
            {
                yield return LeerComentario(inicioLinea, inicioColumna);
            }
            else if (c == '"' || c == '\'')
            {
                yield return LeerCadena(inicioLinea, inicioColumna);
            }
            else if (char.IsDigit(c))
            {
                yield return LeerNumero(inicioLinea, inicioColumna);
            }
            else if ("+-*/=<>!&|^%".Contains(c))
            {
                yield return LeerOperador(inicioLinea, inicioColumna);
            }
            else if ("(){}[].,;:".Contains(c))
            {
                yield return new Token(TipoToken.Delimitador, c.ToString(), inicioLinea, inicioColumna);
                AvanzarPos();
            }
            else {
                yield return new Token(TipoToken.Error, c.ToString(), inicioLinea, inicioColumna);
                AvanzarPos();
            }
        }
        yield return new Token(TipoToken.EOF, "", _linea, _columna);
    }

    private Token LeerIdentificadorOrKeyword(int linea, int columna)
    {
        StringBuilder sb = new StringBuilder();
        while (_posicion < _input.Length && (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_'))
        {
            sb.Append(CurrentChar);
            AvanzarPos();
        }

        string text = sb.ToString();

        return new Token(
            Keywords.Contains(text) ? TipoToken.Keyword : TipoToken.Identificador,
            text,
            linea,
            columna
        );
    }

    private Token LeerNumero(int linea, int columna)
    {
        StringBuilder sb = new StringBuilder();
        bool punto = false;
        bool tieneError = false;

        while (_posicion < _input.Length)
        {
            char c = CurrentChar;

            if (char.IsDigit(c))
            {
                sb.Append(c);
                AvanzarPos();
            }
            else if (c == '.')
            {
                if (!punto && char.IsDigit(NextChar))
                {
                    punto = true;
                    sb.Append(c);
                    AvanzarPos();
                }
                else
                {
                    tieneError = true;
                    sb.Append(c);
                    AvanzarPos();
                }
            }
            else if (char.IsLetter(c) || c == '_')
            {
                tieneError = true;
                while (_posicion < _input.Length && (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_' || CurrentChar == '.'))
                {
                    sb.Append(CurrentChar);
                    AvanzarPos();
                }
                return new Token(TipoToken.Error, sb.ToString() + " (Error: Identificador inválido)", linea, columna);
            }
            else
            {
                break;
            }
        }

        if (tieneError)
        {
            return new Token(TipoToken.Error, sb.ToString() + " (Error: Estructura numérica inválida, múltiples puntos)", linea, columna);
        }

        return new Token(TipoToken.Numero, sb.ToString(), linea, columna);
    }

    private Token LeerOperador(int linea, int columna)
    {
        char c = CurrentChar;
        char next = NextChar;
        string combined = $"{c}{next}";

        string[] doubleOperadors = { "==", "!=", "<=", ">=", "&&", "||", "++", "--", "+=", "-=", "*=", "/=" };

        if (Array.Exists(doubleOperadors, op => op == combined))
        {
            AvanzarPos();
            AvanzarPos();
            return new Token(TipoToken.Operador, combined, linea, columna);
        }

        AvanzarPos();
        return new Token(TipoToken.Operador, c.ToString(), linea, columna);
    }

    private Token LeerCadena(int linea, int columna)
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
                
                TipoToken tipo = delimitador == '\'' ? TipoToken.Char : TipoToken.String;
                return new Token(tipo, sb.ToString(), linea, columna);
            }

            sb.Append(c);
            AvanzarPos();
        }
        return new Token(TipoToken.Error, sb.ToString(), linea, columna);
    }

    private Token LeerComentario(int linea, int columna)
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

            return new Token(TipoToken.Comentario, sb.ToString(), linea, columna);
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
                    return new Token(TipoToken.Comentario, sb.ToString(), linea, columna);
                }

                sb.Append(CurrentChar);
                AvanzarPos();
            }

            return new Token(TipoToken.Error, sb.ToString() + " (Error: Faltó cerrar con */)", linea, columna);
        }

        return new Token(TipoToken.Blanco, " ", linea, columna);
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

            Console.WriteLine("\tTokenizador iniciado\n");

            Tokenizador tokenizador = new Tokenizador(sourceCode);
            
            foreach (var token in tokenizador.Tokenizar())
            {
                if (token.Tipo != TipoToken.Blanco)
                {
                    Console.WriteLine(token);
                }
            }
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
        }
    }
}