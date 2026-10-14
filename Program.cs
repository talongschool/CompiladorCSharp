using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
                "char" => "Tipo de dato caracter",
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
        "int", "float", "string", "char", "bool", "void",
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

        if (CurrentChar == '/' && NextChar == '/')
        {
            while (_posicion < _input.Length && CurrentChar != '\n' && CurrentChar != '\r')
            {
                sb.Append(CurrentChar);
                AvanzarPos();
            }

            return new Token(TipoToken.Comentario, sb.ToString(), linea, columna);
        }

        if (CurrentChar == '/' && NextChar == '*')
        {
            sb.Append("/*");
            AvanzarPos(); 
            AvanzarPos(); 

            while (_posicion < _input.Length)
            {
                if (CurrentChar == '*' && NextChar == '/')
                {
                    sb.Append("*/");
                    AvanzarPos(); 
                    AvanzarPos(); 
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

// ==========================================
// ANALIZADOR SINTÁCTICO (PARSER) C++
// ==========================================
public class Parser
{
    private readonly List<Token> _tokens;
    private int _pos = 0;
    private readonly List<string> _errores = new List<string>();

    public Parser(IEnumerable<Token> tokens)
    {
        _tokens = tokens.Where(t => t.Tipo != TipoToken.Blanco && t.Tipo != TipoToken.Comentario).ToList();
    }

    private Token Current => _pos < _tokens.Count ? _tokens[_pos] : _tokens[_tokens.Count - 1];

    private void Avanzar()
    {
        if (_pos < _tokens.Count - 1)
        {
            _pos++;
        }
    }

    private bool EsTipoDato(string valor)
    {
        return valor == "int" || valor == "float" || valor == "string" || valor == "char" || valor == "bool" || valor == "void";
    }

    private bool EsIdentificadorValidoToken(Token token)
    {
        // Acepta tanto un token de tipo Identificador como Keyword si el valor es 'main' u otra función reservada válida como identificador
        return token.Tipo == TipoToken.Identificador || 
               (token.Tipo == TipoToken.Keyword && token.Valor == "main");
    }

    private Token ConsumirIdentificador(string mensajeError = null)
    {
        if (EsIdentificadorValidoToken(Current))
        {
            Token t = Current;
            Avanzar();
            return t;
        }

        string msg = mensajeError ?? $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba un identificador, pero se encontró '{Current.Valor}'.";
        _errores.Add(msg);
        
        if (Current.Tipo != TipoToken.EOF)
        {
            Avanzar();
        }
        return Current;
    }

    private bool Coincide(TipoToken tipo, string valor = null)
    {
        if (Current.Tipo == tipo)
        {
            if (valor == null || Current.Valor == valor)
                return true;
        }
        return false;
    }

    private Token Consumir(TipoToken tipo, string valor = null, string mensajeError = null)
    {
        if (Coincide(tipo, valor))
        {
            Token t = Current;
            Avanzar();
            return t;
        }

        string esperado = valor ?? tipo.ToString();
        string msg = mensajeError ?? $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba '{esperado}', pero se encontró '{Current.Valor}'.";
        _errores.Add(msg);
        
        if (Current.Tipo != TipoToken.EOF)
        {
            Avanzar();
        }
        return Current;
    }

    public List<string> Analizar()
    {
        _errores.Clear();
        _pos = 0;

        try
        {
            while (_pos < _tokens.Count && Current.Tipo != TipoToken.EOF)
            {
                ParsearPrograma();
            }
        }
        catch (Exception ex)
        {
            _errores.Add($"[Error Crítico del Parser] {ex.Message}");
        }

        return _errores;
    }

    private void ParsearPrograma()
    {
        if (Current.Tipo == TipoToken.Keyword && EsTipoDato(Current.Valor))
        {
            string tipoDato = Current.Valor;
            Avanzar();

            Token id = ConsumirIdentificador($"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba un identificador (función o variable) después de '{tipoDato}'.");

            if (Coincide(TipoToken.Delimitador, "("))
            {
                Avanzar();
                ParsearParametros();
                Consumir(TipoToken.Delimitador, ")", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ')' cerrando los parámetros de la función.");
                ParsearBloque();
            }
            else if (Coincide(TipoToken.Delimitador, ";") || Coincide(TipoToken.Operador, "="))
            {
                if (Coincide(TipoToken.Operador, "="))
                {
                    Avanzar();
                    ParsearExpresion();
                }
                Consumir(TipoToken.Delimitador, ";", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ';' al final de la declaración de variable.");
            }
            else
            {
                _errores.Add($"[Error Sintáctico] Línea {id.Linea}, Columna {id.Columna}: Sintaxis inválida tras el identificador '{id.Valor}'.");
                Sincronizar();
            }
        }
        else
        {
            _errores.Add($"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba una declaración de tipo (int, float, void, etc.), se encontró '{Current.Valor}'.");
            Avanzar();
        }
    }

    private void ParsearParametros()
    {
        if (Coincide(TipoToken.Delimitador, ")")) return;

        do
        {
            if (Current.Tipo == TipoToken.Keyword && EsTipoDato(Current.Valor))
            {
                Avanzar();
                ConsumirIdentificador($"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba el nombre del parámetro.");
            }
            else if (Coincide(TipoToken.Keyword, "void") && Coincide(TipoToken.Delimitador, ")"))
            {
                Avanzar();
                break;
            }
            else
            {
                _errores.Add($"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Definición de parámetro inválida.");
                break;
            }

            if (Coincide(TipoToken.Delimitador, ","))
            {
                Avanzar();
            }
            else
            {
                break;
            }
        } while (_pos < _tokens.Count && !Coincide(TipoToken.Delimitador, ")"));
    }

    private void ParsearBloque()
    {
        Consumir(TipoToken.Delimitador, "{", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba '{{' para abrir el bloque de código.");
        while (_pos < _tokens.Count && !Coincide(TipoToken.Delimitador, "}") && !Coincide(TipoToken.EOF))
        {
            ParsearInstruccion();
        }
        Consumir(TipoToken.Delimitador, "}", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba '}}' para cerrar el bloque de código.");
    }

    private void ParsearInstruccion()
    {
        if (Coincide(TipoToken.Delimitador, ";"))
        {
            Avanzar();
            return;
        }

        if (Coincide(TipoToken.Delimitador, "{"))
        {
            ParsearBloque();
            return;
        }

        if (Current.Tipo == TipoToken.Keyword && EsTipoDato(Current.Valor))
        {
            string tipo = Current.Valor;
            Avanzar();
            ConsumirIdentificador($"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba un identificador tras el tipo '{tipo}'.");
            
            if (Coincide(TipoToken.Operador, "="))
            {
                Avanzar();
                ParsearExpresion();
            }
            Consumir(TipoToken.Delimitador, ";", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ';' al final de la instrucción.");
            return;
        }

        if (Coincide(TipoToken.Keyword, "if"))
        {
            Avanzar();
            Consumir(TipoToken.Delimitador, "(", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba '(' después de 'if'.");
            ParsearExpresion();
            Consumir(TipoToken.Delimitador, ")", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ')' después de la condición del 'if'.");
            ParsearInstruccion();

            if (Coincide(TipoToken.Keyword, "else"))
            {
                Avanzar();
                ParsearInstruccion();
            }
            return;
        }

        if (Coincide(TipoToken.Keyword, "while"))
        {
            Avanzar();
            Consumir(TipoToken.Delimitador, "(", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba '(' después de 'while'.");
            ParsearExpresion();
            Consumir(TipoToken.Delimitador, ")", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ')' después de la condición del 'while'.");
            ParsearInstruccion();
            return;
        }

        if (Coincide(TipoToken.Keyword, "for"))
        {
            Avanzar();
            Consumir(TipoToken.Delimitador, "(", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba '(' después de 'for'.");
            
            if (Current.Tipo == TipoToken.Keyword && EsTipoDato(Current.Valor))
            {
                Avanzar();
                ConsumirIdentificador();
            }
            else if (EsIdentificadorValidoToken(Current))
            {
                Avanzar();
            }

            if (Coincide(TipoToken.Operador, "="))
            {
                Avanzar();
                ParsearExpresion();
            }
            Consumir(TipoToken.Delimitador, ";", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ';' en la cabecera del 'for'.");

            ParsearExpresion();
            Consumir(TipoToken.Delimitador, ";", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ';' en la cabecera del 'for'.");

            ParsearExpresion();
            Consumir(TipoToken.Delimitador, ")", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ')' al cerrar la cabecera del 'for'.");

            ParsearInstruccion();
            return;
        }

        if (Coincide(TipoToken.Keyword, "return"))
        {
            Avanzar();
            if (!Coincide(TipoToken.Delimitador, ";"))
            {
                ParsearExpresion();
            }
            Consumir(TipoToken.Delimitador, ";", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ';' al final de la sentencia 'return'.");
            return;
        }

        ParsearExpresion();
        Consumir(TipoToken.Delimitador, ";", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ';' al final de la instrucción.");
    }

    private void ParsearExpresion() => ParsearAsignacion();

    private void ParsearAsignacion()
    {
        ParsearLogicaO();
        if (Coincide(TipoToken.Operador, "=") || Coincide(TipoToken.Operador, "+=") || 
            Coincide(TipoToken.Operador, "-=") || Coincide(TipoToken.Operador, "*=") || 
            Coincide(TipoToken.Operador, "/="))
        {
            Avanzar();
            ParsearAsignacion();
        }
    }

    private void ParsearLogicaO()
    {
        ParsearLogicaAnd();
        while (Coincide(TipoToken.Operador, "||")) { Avanzar(); ParsearLogicaAnd(); }
    }

    private void ParsearLogicaAnd()
    {
        ParsearIgualdad();
        while (Coincide(TipoToken.Operador, "&&")) { Avanzar(); ParsearIgualdad(); }
    }

    private void ParsearIgualdad()
    {
        ParsearRelacional();
        while (Coincide(TipoToken.Operador, "==") || Coincide(TipoToken.Operador, "!=")) { Avanzar(); ParsearRelacional(); }
    }

    private void ParsearRelacional()
    {
        ParsearAditiva();
        while (Coincide(TipoToken.Operador, "<") || Coincide(TipoToken.Operador, ">") || 
               Coincide(TipoToken.Operador, "<=") || Coincide(TipoToken.Operador, ">=")) { Avanzar(); ParsearAditiva(); }
    }

    private void ParsearAditiva()
    {
        ParsearMultiplicativa();
        while (Coincide(TipoToken.Operador, "+") || Coincide(TipoToken.Operador, "-")) { Avanzar(); ParsearMultiplicativa(); }
    }

    private void ParsearMultiplicativa()
    {
        ParsearUnaria();
        while (Coincide(TipoToken.Operador, "*") || Coincide(TipoToken.Operador, "/") || Coincide(TipoToken.Operador, "%")) { Avanzar(); ParsearUnaria(); }
    }

    private void ParsearUnaria()
    {
        if (Coincide(TipoToken.Operador, "!") || Coincide(TipoToken.Operador, "-") || 
            Coincide(TipoToken.Operador, "++") || Coincide(TipoToken.Operador, "--"))
        {
            Avanzar();
            ParsearUnaria();
        }
        else
        {
            ParsearPrimaria();
        }
    }

    private void ParsearPrimaria()
    {
        if (Coincide(TipoToken.Numero) || Coincide(TipoToken.String) || Coincide(TipoToken.Char) ||
            Coincide(TipoToken.Keyword, "true") || Coincide(TipoToken.Keyword, "false"))
        {
            Avanzar();
        }
        else if (EsIdentificadorValidoToken(Current))
        {
            Avanzar();

            if (Coincide(TipoToken.Delimitador, "("))
            {
                Avanzar();
                ParsearArgumentos();
                Consumir(TipoToken.Delimitador, ")", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ')' cerrando argumentos.");
            }
            else if (Coincide(TipoToken.Delimitador, "["))
            {
                Avanzar();
                ParsearExpresion();
                Consumir(TipoToken.Delimitador, "]", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ']' cerrando índice.");
            }
            else if (Coincide(TipoToken.Operador, "++") || Coincide(TipoToken.Operador, "--"))
            {
                Avanzar();
            }
        }
        else if (Coincide(TipoToken.Delimitador, "("))
        {
            Avanzar();
            ParsearExpresion();
            Consumir(TipoToken.Delimitador, ")", $"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Se esperaba ')' cerrando expresión.");
        }
        else
        {
            _errores.Add($"[Error Sintáctico] Línea {Current.Linea}, Columna {Current.Columna}: Expresión o término inválido: '{Current.Valor}'.");
            Avanzar();
        }
    }

    private void ParsearArgumentos()
    {
        if (Coincide(TipoToken.Delimitador, ")")) return;

        do
        {
            ParsearExpresion();
            if (Coincide(TipoToken.Delimitador, ","))
            {
                Avanzar();
            }
            else
            {
                break;
            }
        } while (_pos < _tokens.Count && !Coincide(TipoToken.Delimitador, ")"));
    }

    private void Sincronizar()
    {
        Avanzar();
        while (_pos < _tokens.Count)
        {
            if (_tokens[_pos - 1].Tipo == TipoToken.Delimitador && _tokens[_pos - 1].Valor == ";")
                return;

            if (Current.Tipo == TipoToken.Keyword && EsTipoDato(Current.Valor))
                return;

            Avanzar();
        }
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

            Console.WriteLine("\t=== TOKENIZADOR INICIADO ===\n");

            Tokenizador tokenizador = new Tokenizador(sourceCode);
            List<Token> tokens = tokenizador.Tokenizar().ToList();
            
            foreach (var token in tokens)
            {
                if (token.Tipo != TipoToken.Blanco)
                {
                    Console.WriteLine(token);
                }
            }
            
            Parser parser = new Parser(tokens);
            List<string> erroresSintacticos = parser.Analizar();

            if (erroresSintacticos.Count == 0)
            {
                Console.WriteLine("\n¡El código fuente es sintácticamente correcto.");
            }
            else
            {
                Console.WriteLine($"\nSe encontraron {erroresSintacticos.Count} errores de sintaxis:\n");
                foreach (var error in erroresSintacticos)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
        }
    }
}