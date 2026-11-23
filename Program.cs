using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;

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
                "+=" => "Suma y asigna", "-=" => "Resta y asigna", "*=" => "Multiplica y asigna",
                "/=" => "Divide y asigna", "%=" => "Módulo y asigna",
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

    private static readonly HashSet<string> DoubleOperators = new(StringComparer.Ordinal)
    {
        "==", "!=", "<=", ">=", "&&", "||", "++", "--", "+=", "-=", "*=", "/=", "%="
    };

    public Tokenizador(string input)
    {
        _input = input ?? string.Empty;
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
            else if (c == '"')
            {
                yield return LeerString(inicioLinea, inicioColumna);
            }
            else if (c == '\'')
            {
                yield return LeerChar(inicioLinea, inicioColumna);
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
            else
            {
                yield return new Token(TipoToken.Error, $"{c} (Error: Símbolo no reconocido)", inicioLinea, inicioColumna);
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
                return new Token(TipoToken.Error, sb.ToString() + " (Error: Identificador o número inválido)", linea, columna);
            }
            else
            {
                break;
            }
        }

        if (tieneError)
        {
            return new Token(TipoToken.Error, sb.ToString() + " (Error: Estructura numérica inválida)", linea, columna);
        }

        return new Token(TipoToken.Numero, sb.ToString(), linea, columna);
    }

    private Token LeerOperador(int linea, int columna)
    {
        char c = CurrentChar;
        char next = NextChar;
        string combined = $"{c}{next}";

        if (DoubleOperators.Contains(combined))
        {
            AvanzarPos();
            AvanzarPos();
            return new Token(TipoToken.Operador, combined, linea, columna);
        }

        AvanzarPos();
        return new Token(TipoToken.Operador, c.ToString(), linea, columna);
    }

    private Token LeerString(int linea, int columna)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append('"');
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

            if (c == '"')
            {
                sb.Append(c);
                AvanzarPos();
                return new Token(TipoToken.String, sb.ToString(), linea, columna);
            }

            sb.Append(c);
            AvanzarPos();
        }

        return new Token(TipoToken.Error, sb.ToString() + " (Error: Cadena sin cerrar)", linea, columna);
    }

    private Token LeerChar(int linea, int columna)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append('\'');
        AvanzarPos();

        int caracteresUtiles = 0;
        bool cerrado = false;

        while (_posicion < _input.Length)
        {
            char c = CurrentChar;
            if (c == '\\' && NextChar != '\0')
            {
                sb.Append(c);
                AvanzarPos();
                sb.Append(CurrentChar);
                AvanzarPos();
                caracteresUtiles++;
                continue;
            }

            if (c == '\'')
            {
                sb.Append(c);
                AvanzarPos();
                cerrado = true;
                break;
            }

            sb.Append(c);
            AvanzarPos();
            caracteresUtiles++;
        }

        if (!cerrado)
        {
            return new Token(TipoToken.Error, sb.ToString() + " (Error: Carácter sin cerrar)", linea, columna);
        }

        if (caracteresUtiles != 1)
        {
            return new Token(TipoToken.Error, sb.ToString() + " (Error: Literal char debe contener exactamente un carácter)", linea, columna);
        }

        return new Token(TipoToken.Char, sb.ToString(), linea, columna);
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

// AST: Árbol de Sintaxis Abstracta

public abstract record NodoAST(int Linea, int Columna);

public record Programa(List<NodoAST> Declaraciones) : NodoAST(0, 0);

public record Funcion(
    string TipoRetorno,
    string Nombre,
    List<Parametro> Parametros,
    Bloque Cuerpo,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record Parametro(
    string Tipo,
    string Nombre,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record Bloque(
    List<NodoAST> Instrucciones,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record DeclaracionVariable(
    string Tipo,
    string Nombre,
    NodoAST? ValorInicial,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record Asignacion(
    string Nombre,
    string Operador,
    NodoAST Valor,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record Condicional(
    NodoAST Condicion,
    Bloque Entonces,
    NodoAST? Sino,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record BucleWhile(
    NodoAST Condicion,
    Bloque Cuerpo,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record BucleFor(
    NodoAST? Inicializacion,
    NodoAST? Condicion,
    NodoAST? Incremento,
    Bloque Cuerpo,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record Retorno(
    NodoAST? Valor,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record LlamadaFuncion(
    string Nombre,
    List<NodoAST> Argumentos,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record Literal(
    object Valor,
    string TipoLiteral,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record Identificador(
    string Nombre,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record ExpresionBinaria(
    NodoAST Izquierda,
    string Operador,
    NodoAST Derecha,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

public record ExpresionUnaria(
    string Operador,
    NodoAST Operando,
    bool EsPrefijo,
    int Linea,
    int Columna) : NodoAST(Linea, Columna);

// Excepciones del Compilador

public class ParserException : Exception
{
    public int Linea { get; }
    public int Columna { get; }

    public ParserException(string mensaje, int linea, int columna)
        : base($"Error sintáctico [L:{linea} C:{columna}]: {mensaje}")
    {
        Linea = linea;
        Columna = columna;
    }
}

public class SemanticException : Exception
{
    public int Linea { get; }
    public int Columna { get; }

    public SemanticException(string mensaje, int linea, int columna)
        : base($"Error semántico [L:{linea} C:{columna}]: {mensaje}")
    {
        Linea = linea;
        Columna = columna;
    }
}

// TABLA DE SÍMBOLOS CON SOBRECARGA

public enum TipoSimbolo
{
    Variable,
    Parametro,
    Funcion
}

public class Simbolo
{
    public string Nombre { get; }
    public string Tipo { get; }
    public TipoSimbolo Categoria { get; }
    public List<string>? TiposParametros { get; }

    public Simbolo(string nombre, string tipo, TipoSimbolo categoria, List<string>? tiposParametros = null)
    {
        Nombre = nombre;
        Tipo = tipo;
        Categoria = categoria;
        TiposParametros = tiposParametros;
    }

    public static string ObtenerClaveFuncion(string nombre, IEnumerable<string> tiposParams)
    {
        return $"{nombre}({string.Join(",", tiposParams)})";
    }
}

public class TablaSimbolos
{
    private readonly Dictionary<string, Simbolo> _tabla = new();
    public TablaSimbolos? Padre { get; }

    public TablaSimbolos(TablaSimbolos? padre = null)
    {
        Padre = padre;
    }

    public IEnumerable<Simbolo> SimbolosLocales => _tabla.Values;

    public bool Insertar(string clave, Simbolo simbolo)
    {
        if (_tabla.ContainsKey(clave))
        {
            return false;
        }
        _tabla[clave] = simbolo;
        return true;
    }

    public Simbolo? Buscar(string clave)
    {
        if (_tabla.TryGetValue(clave, out var simbolo))
        {
            return simbolo;
        }
        return Padre?.Buscar(clave);
    }
}

// Mostrar AST

public static class AstPrinter
{
    public static void Imprimir(NodoAST nodo, int nivel = 0)
    {
        string indent = new string(' ', nivel * 2);
        switch (nodo)
        {
            case Programa p:
                Console.WriteLine($"{indent}Programa ({p.Declaraciones.Count} declaraciones)");
                foreach (var d in p.Declaraciones) Imprimir(d, nivel + 1);
                break;
            case Funcion f:
                Console.WriteLine($"{indent}Funcion: {f.TipoRetorno} {f.Nombre} ({f.Parametros.Count} params)");
                foreach (var p in f.Parametros) Imprimir(p, nivel + 1);
                Imprimir(f.Cuerpo, nivel + 1);
                break;
            case Parametro p:
                Console.WriteLine($"{indent}Parametro: {p.Tipo} {p.Nombre}");
                break;
            case Bloque b:
                Console.WriteLine($"{indent}Bloque ({b.Instrucciones.Count} instrucciones)");
                foreach (var i in b.Instrucciones) Imprimir(i, nivel + 1);
                break;
            case DeclaracionVariable d:
                Console.WriteLine($"{indent}DeclaracionVariable: {d.Tipo} {d.Nombre}");
                if (d.ValorInicial != null) Imprimir(d.ValorInicial, nivel + 1);
                break;
            case Asignacion a:
                Console.WriteLine($"{indent}Asignacion: {a.Nombre} {a.Operador}");
                Imprimir(a.Valor, nivel + 1);
                break;
            case Condicional c:
                Console.WriteLine($"{indent}Condicional:");
                Imprimir(c.Condicion, nivel + 1);
                Console.WriteLine($"{indent}  Entonces:");
                Imprimir(c.Entonces, nivel + 1);
                if (c.Sino != null)
                {
                    Console.WriteLine($"{indent}  Sino:");
                    Imprimir(c.Sino, nivel + 1);
                }
                break;
            case BucleWhile w:
                Console.WriteLine($"{indent}BucleWhile:");
                Imprimir(w.Condicion, nivel + 1);
                Imprimir(w.Cuerpo, nivel + 1);
                break;
            case BucleFor f:
                Console.WriteLine($"{indent}BucleFor:");
                if (f.Inicializacion != null) Imprimir(f.Inicializacion, nivel + 1);
                if (f.Condicion != null) Imprimir(f.Condicion, nivel + 1);
                if (f.Incremento != null) Imprimir(f.Incremento, nivel + 1);
                Imprimir(f.Cuerpo, nivel + 1);
                break;
            case Retorno r:
                Console.WriteLine($"{indent}Retorno:");
                if (r.Valor != null) Imprimir(r.Valor, nivel + 1);
                break;
            case LlamadaFuncion l:
                Console.WriteLine($"{indent}LlamadaFuncion: {l.Nombre}");
                foreach (var a in l.Argumentos) Imprimir(a, nivel + 1);
                break;
            case Literal lit:
                Console.WriteLine($"{indent}Literal: {lit.Valor} ({lit.TipoLiteral})");
                break;
            case Identificador id:
                Console.WriteLine($"{indent}Identificador: {id.Nombre}");
                break;
            case ExpresionBinaria b:
                Console.WriteLine($"{indent}ExpresionBinaria: {b.Operador}");
                Imprimir(b.Izquierda, nivel + 1);
                Imprimir(b.Derecha, nivel + 1);
                break;
            case ExpresionUnaria u:
                Console.WriteLine($"{indent}ExpresionUnaria: {u.Operador} ({(u.EsPrefijo ? "prefijo" : "postfijo")})");
                Imprimir(u.Operando, nivel + 1);
                break;
            default:
                Console.WriteLine($"{indent}{nodo.GetType().Name}");
                break;
        }
    }
}

// PARSER POR DESCENSO RECURSIVO

public class Parser
{
    private readonly List<Token> _tokens;
    private int _posicion;

    private static readonly HashSet<string> TiposDato = new(StringComparer.Ordinal)
    {
        "int", "float", "string", "bool", "char"
    };

    private static readonly HashSet<string> TiposRetorno = new(StringComparer.Ordinal)
    {
        "int", "float", "string", "bool", "char", "void"
    };

    private static readonly HashSet<string> OperadoresAsignacion = new(StringComparer.Ordinal)
    {
        "=", "+=", "-=", "*=", "/=", "%="
    };

    public Parser(IEnumerable<Token> tokens)
    {
        _tokens = tokens.Where(t => t.Tipo != TipoToken.Comentario).ToList();
        _posicion = 0;
    }

    private Token Actual => _posicion < _tokens.Count ? _tokens[_posicion] : _tokens[_tokens.Count - 1];
    private Token Anterior => _posicion > 0 ? _tokens[_posicion - 1] : _tokens[0];
    private bool EsFin => Actual.Tipo == TipoToken.EOF;

    private bool EsIdentificador() => Actual.Tipo == TipoToken.Identificador || (Actual.Tipo == TipoToken.Keyword && Actual.Valor == "main");
    private bool EsTipoDato() => Actual.Tipo == TipoToken.Keyword && TiposDato.Contains(Actual.Valor);
    private bool EsTipoRetorno() => Actual.Tipo == TipoToken.Keyword && TiposRetorno.Contains(Actual.Valor);
    private bool EsOperadorAsignacion(string valor) => OperadoresAsignacion.Contains(valor);

    public Programa Parse()
    {
        var declaraciones = new List<NodoAST>();
        while (!EsFin)
        {
            declaraciones.Add(ParseDeclaracionGlobal());
        }
        return new Programa(declaraciones);
    }

    private NodoAST ParseDeclaracionGlobal()
    {
        if (_posicion + 2 < _tokens.Count)
        {
            string tercero = _tokens[_posicion + 2].Valor;
            if (tercero == "(")
            {
                return ParseFuncion();
            }
        }
        return ParseDeclaracionVariable();
    }

    private Funcion ParseFuncion()
    {
        int linea = Actual.Linea;
        int columna = Actual.Columna;
        string tipo = ParseTipoRetorno();
        if (!EsIdentificador()) Error($"Se esperaba un identificador para la función, se encontró '{Actual.Valor}'");
        string nombre = Avanzar().Valor;
        ConsumirValor("(");
        List<Parametro> parametros = new List<Parametro>();
        if (!Verificar(TipoToken.Delimitador, ")"))
        {
            parametros = ParseParametros();
        }
        ConsumirValor(")");
        Bloque cuerpo = ParseBloque();
        return new Funcion(tipo, nombre, parametros, cuerpo, linea, columna);
    }

    private List<Parametro> ParseParametros()
    {
        var parametros = new List<Parametro>();
        do
        {
            parametros.Add(ParseParametro());
        } while (Coincidir(","));
        return parametros;
    }

    private Parametro ParseParametro()
    {
        int linea = Actual.Linea;
        int columna = Actual.Columna;
        string tipo = ParseTipoDato();
        if (!EsIdentificador()) Error($"Se esperaba un identificador para el parámetro, se encontró '{Actual.Valor}'");
        string nombre = Avanzar().Valor;
        return new Parametro(tipo, nombre, linea, columna);
    }

    private string ParseTipoDato()
    {
        if (EsTipoDato()) return Avanzar().Valor;
        Error($"Se esperaba un tipo de dato válido, se encontró '{Actual.Valor}'");
        return string.Empty;
    }

    private string ParseTipoRetorno()
    {
        if (EsTipoRetorno()) return Avanzar().Valor;
        Error($"Se esperaba un tipo de retorno válido, se encontró '{Actual.Valor}'");
        return string.Empty;
    }

    private Bloque ParseBloque()
    {
        int linea = Actual.Linea;
        int columna = Actual.Columna;
        ConsumirValor("{");
        var instrucciones = new List<NodoAST>();
        while (!Verificar(TipoToken.Delimitador, "}") && !EsFin)
        {
            instrucciones.Add(ParseInstruccion());
        }
        ConsumirValor("}");
        return new Bloque(instrucciones, linea, columna);
    }

    private NodoAST ParseInstruccion()
    {
        if (Verificar(TipoToken.Delimitador, "{")) return ParseBloque();
        if (Coincidir("if")) return ParseCondicional();
        if (Coincidir("while")) return ParseBucleWhile();
        if (Coincidir("for")) return ParseBucleFor();
        if (Coincidir("return")) return ParseRetorno();
        if (EsTipoDato()) return ParseDeclaracionVariable();

        if (EsIdentificador() && _posicion + 1 < _tokens.Count && EsOperadorAsignacion(_tokens[_posicion + 1].Valor))
        {
            return ParseAsignacionConPuntoYComa();
        }

        NodoAST expr = ParseExpresion();
        ConsumirValor(";");
        return expr;
    }

    private DeclaracionVariable ParseDeclaracionVariable()
    {
        int linea = Actual.Linea;
        int columna = Actual.Columna;
        string tipo = ParseTipoDato();
        if (!EsIdentificador()) Error($"Se esperaba un identificador de variable, se encontró '{Actual.Valor}'");
        string nombre = Avanzar().Valor;
        NodoAST? valorInicial = null;
        if (Coincidir("="))
        {
            valorInicial = ParseExpresion();
        }
        ConsumirValor(";");
        return new DeclaracionVariable(tipo, nombre, valorInicial, linea, columna);
    }

    private Asignacion ParseAsignacionConPuntoYComa()
    {
        Asignacion asignacion = ParseAsignacionInterna();
        ConsumirValor(";");
        return asignacion;
    }

    private Asignacion ParseAsignacionSinPuntoYComa() => ParseAsignacionInterna();

    private Asignacion ParseAsignacionInterna()
    {
        int linea = Actual.Linea;
        int columna = Actual.Columna;
        if (!EsIdentificador()) Error($"Se esperaba un identificador para la asignación, se encontró '{Actual.Valor}'");
        string nombre = Avanzar().Valor;

        if (!Coincidir("=", "+=", "-=", "*=", "/=", "%="))
        {
            Error($"Se esperaba un operador de asignación, se encontró '{Actual.Valor}'");
        }
        string operador = Anterior.Valor;
        NodoAST valor = ParseExpresion();
        return new Asignacion(nombre, operador, valor, linea, columna);
    }

    private Condicional ParseCondicional()
    {
        int linea = Anterior.Linea;
        int columna = Anterior.Columna;
        ConsumirValor("(");
        NodoAST condicion = ParseExpresion();
        ConsumirValor(")");
        Bloque entonces = ParseBloque();
        NodoAST? sino = null;
        if (Coincidir("else"))
        {
            if (VerificarValor("if"))
            {
                Avanzar();
                sino = ParseCondicional();
            }
            else
            {
                sino = ParseBloque();
            }
        }
        return new Condicional(condicion, entonces, sino, linea, columna);
    }

    private BucleWhile ParseBucleWhile()
    {
        int linea = Anterior.Linea;
        int columna = Anterior.Columna;
        ConsumirValor("(");
        NodoAST condicion = ParseExpresion();
        ConsumirValor(")");
        Bloque cuerpo = ParseBloque();
        return new BucleWhile(condicion, cuerpo, linea, columna);
    }

    private BucleFor ParseBucleFor()
    {
        int linea = Anterior.Linea;
        int columna = Anterior.Columna;
        ConsumirValor("(");
        NodoAST? inicializacion = ParseInicializacionFor();
        ConsumirValor(";");
        NodoAST? condicion = ParseCondicionFor();
        ConsumirValor(";");
        NodoAST? incremento = ParseIncrementoFor();
        ConsumirValor(")");
        Bloque cuerpo = ParseBloque();
        return new BucleFor(inicializacion, condicion, incremento, cuerpo, linea, columna);
    }

    private NodoAST? ParseInicializacionFor()
    {
        if (Verificar(TipoToken.Delimitador, ";")) return null;
        if (EsTipoDato())
        {
            int linea = Actual.Linea;
            int columna = Actual.Columna;
            string tipo = ParseTipoDato();
            if (!EsIdentificador()) Error($"Se esperaba un identificador, se encontró '{Actual.Valor}'");
            string nombre = Avanzar().Valor;
            NodoAST? valorInicial = null;
            if (Coincidir("=")) valorInicial = ParseExpresion();
            return new DeclaracionVariable(tipo, nombre, valorInicial, linea, columna);
        }
        if (EsIdentificador() && _posicion + 1 < _tokens.Count && EsOperadorAsignacion(_tokens[_posicion + 1].Valor))
        {
            return ParseAsignacionSinPuntoYComa();
        }
        return ParseExpresion();
    }

    private NodoAST? ParseCondicionFor() => Verificar(TipoToken.Delimitador, ";") ? null : ParseExpresion();

    private NodoAST? ParseIncrementoFor()
    {
        if (Verificar(TipoToken.Delimitador, ")")) return null;
        if (EsIdentificador() && _posicion + 1 < _tokens.Count && EsOperadorAsignacion(_tokens[_posicion + 1].Valor))
        {
            return ParseAsignacionSinPuntoYComa();
        }
        return ParseExpresion();
    }

    private Retorno ParseRetorno()
    {
        int linea = Anterior.Linea;
        int columna = Anterior.Columna;
        NodoAST? valor = null;
        if (!Verificar(TipoToken.Delimitador, ";"))
        {
            valor = ParseExpresion();
        }
        ConsumirValor(";");
        return new Retorno(valor, linea, columna);
    }

    private LlamadaFuncion ParseLlamadaFuncion()
    {
        int linea = Actual.Linea;
        int columna = Actual.Columna;
        if (!EsIdentificador()) Error($"Se esperaba un identificador de función, se encontró '{Actual.Valor}'");
        string nombre = Avanzar().Valor;
        ConsumirValor("(");
        var argumentos = new List<NodoAST>();
        if (!Verificar(TipoToken.Delimitador, ")"))
        {
            argumentos = ParseArgumentos();
        }
        ConsumirValor(")");
        return new LlamadaFuncion(nombre, argumentos, linea, columna);
    }

    private List<NodoAST> ParseArgumentos()
    {
        var args = new List<NodoAST>();
        do
        {
            args.Add(ParseExpresion());
        } while (Coincidir(","));
        return args;
    }

    private NodoAST ParseExpresion() => ParseExpresionOr();

    private NodoAST ParseExpresionOr()
    {
        NodoAST izquierda = ParseExpresionAnd();
        while (Coincidir("||"))
        {
            Token op = Anterior;
            NodoAST derecha = ParseExpresionAnd();
            izquierda = new ExpresionBinaria(izquierda, op.Valor, derecha, op.Linea, op.Columna);
        }
        return izquierda;
    }

    private NodoAST ParseExpresionAnd()
    {
        NodoAST izquierda = ParseExpresionIgualdad();
        while (Coincidir("&&"))
        {
            Token op = Anterior;
            NodoAST derecha = ParseExpresionIgualdad();
            izquierda = new ExpresionBinaria(izquierda, op.Valor, derecha, op.Linea, op.Columna);
        }
        return izquierda;
    }

    private NodoAST ParseExpresionIgualdad()
    {
        NodoAST izquierda = ParseExpresionComparacion();
        while (Coincidir("==", "!="))
        {
            Token op = Anterior;
            NodoAST derecha = ParseExpresionComparacion();
            izquierda = new ExpresionBinaria(izquierda, op.Valor, derecha, op.Linea, op.Columna);
        }
        return izquierda;
    }

    private NodoAST ParseExpresionComparacion()
    {
        NodoAST izquierda = ParseExpresionSuma();
        while (Coincidir("<", ">", "<=", ">="))
        {
            Token op = Anterior;
            NodoAST derecha = ParseExpresionSuma();
            izquierda = new ExpresionBinaria(izquierda, op.Valor, derecha, op.Linea, op.Columna);
        }
        return izquierda;
    }

    private NodoAST ParseExpresionSuma()
    {
        NodoAST izquierda = ParseExpresionMult();
        while (Coincidir("+", "-"))
        {
            Token op = Anterior;
            NodoAST derecha = ParseExpresionMult();
            izquierda = new ExpresionBinaria(izquierda, op.Valor, derecha, op.Linea, op.Columna);
        }
        return izquierda;
    }

    private NodoAST ParseExpresionMult()
    {
        NodoAST izquierda = ParseExpresionUnaria();
        while (Coincidir("*", "/", "%"))
        {
            Token op = Anterior;
            NodoAST derecha = ParseExpresionUnaria();
            izquierda = new ExpresionBinaria(izquierda, op.Valor, derecha, op.Linea, op.Columna);
        }
        return izquierda;
    }

    private NodoAST ParseExpresionUnaria()
    {
        if (Coincidir("!", "-", "++", "--"))
        {
            Token op = Anterior;
            NodoAST operando = ParseExpresionUnaria();
            return new ExpresionUnaria(op.Valor, operando, true, op.Linea, op.Columna);
        }
        return ParseExpresionPostfija();
    }

    private NodoAST ParseExpresionPostfija()
    {
        NodoAST primario = ParsePrimario();
        while (Coincidir("++", "--"))
        {
            Token op = Anterior;
            primario = new ExpresionUnaria(op.Valor, primario, false, op.Linea, op.Columna);
        }
        return primario;
    }

    private NodoAST ParsePrimario()
    {
        if (Verificar(TipoToken.Numero))
        {
            Token t = Avanzar();
            object valor = t.Valor.Contains('.')
                ? double.Parse(t.Valor, CultureInfo.InvariantCulture)
                : int.Parse(t.Valor, CultureInfo.InvariantCulture);
            return new Literal(valor, t.Valor.Contains('.') ? "float" : "int", t.Linea, t.Columna);
        }
        if (Verificar(TipoToken.String))
        {
            Token t = Avanzar();
            return new Literal(t.Valor, "string", t.Linea, t.Columna);
        }
        if (Verificar(TipoToken.Char))
        {
            Token t = Avanzar();
            return new Literal(t.Valor, "char", t.Linea, t.Columna);
        }
        if (Coincidir("true", "false"))
        {
            Token t = Anterior;
            bool val = bool.Parse(t.Valor);
            return new Literal(val, "bool", t.Linea, t.Columna);
        }
        if (Coincidir("("))
        {
            NodoAST expr = ParseExpresion();
            ConsumirValor(")");
            return expr;
        }
        if (EsIdentificador())
        {
            if (_posicion + 1 < _tokens.Count && _tokens[_posicion + 1].Valor == "(")
            {
                return ParseLlamadaFuncion();
            }
            Token t = Avanzar();
            return new Identificador(t.Valor, t.Linea, t.Columna);
        }

        Error($"Expresión primaria inválida o inesperada: '{Actual.Valor}'");
        return null!;
    }

    private bool Verificar(TipoToken tipo) => Actual.Tipo == tipo;
    private bool Verificar(TipoToken tipo, string valor) => Actual.Tipo == tipo && Actual.Valor == valor;
    private bool VerificarValor(string valor) => Actual.Valor == valor;

    private bool Coincidir(params string[] valores)
    {
        foreach (var v in valores)
        {
            if (VerificarValor(v))
            {
                Avanzar();
                return true;
            }
        }
        return false;
    }

    private Token Avanzar()
    {
        if (!EsFin) _posicion++;
        return Anterior;
    }

    private Token ConsumirValor(string valor)
    {
        if (VerificarValor(valor)) return Avanzar();
        throw new ParserException($"Se esperaba '{valor}', se encontró '{Actual.Valor}' ({Actual.Tipo})", Actual.Linea, Actual.Columna);
    }

    private void Error(string mensaje) => throw new ParserException(mensaje, Actual.Linea, Actual.Columna);
}

// Semantico

public class AnalizadorSemantico
{
    private TablaSimbolos _tablaActual = new();
    private string _tipoFuncionActual = "void";

    public void Analizar(Programa programa)
    {
        // Pasada 1: Registrar firmas de funciones para llamadas adelantadas y sobrecarga
        foreach (var decl in programa.Declaraciones)
        {
            if (decl is Funcion func)
            {
                var tiposParams = func.Parametros.Select(p => p.Tipo).ToList();
                string clave = Simbolo.ObtenerClaveFuncion(func.Nombre, tiposParams);
                var simboloFunc = new Simbolo(func.Nombre, func.TipoRetorno, TipoSimbolo.Funcion, tiposParams);

                if (!_tablaActual.Insertar(clave, simboloFunc))
                {
                    throw new SemanticException($"La función '{func.Nombre}' con esos tipos de parámetros ya ha sido declarada.", func.Linea, func.Columna);
                }
            }
        }

        // Pasada 2: Validación semántica
        foreach (var decl in programa.Declaraciones)
        {
            Visitar(decl);
        }
    }

    private void Visitar(NodoAST nodo)
    {
        switch (nodo)
        {
            case Funcion f:
                VisitarFuncion(f);
                break;
            case DeclaracionVariable dv:
                VisitarDeclaracionVariable(dv);
                break;
            case Bloque b:
                VisitarBloque(b);
                break;
            case Asignacion a:
                VisitarAsignacion(a);
                break;
            case Condicional c:
                VisitarCondicional(c);
                break;
            case BucleWhile w:
                VisitarBucleWhile(w);
                break;
            case BucleFor fo:
                VisitarBucleFor(fo);
                break;
            case Retorno r:
                VisitarRetorno(r);
                break;
            case ExpresionBinaria eb:
                VisitarExpresionBinaria(eb);
                break;
            case ExpresionUnaria eu:
                VisitarExpresionUnaria(eu);
                break;
            case LlamadaFuncion lf:
                VisitarLlamadaFuncion(lf);
                break;
            case Identificador id:
                VisitarIdentificador(id);
                break;
            case Literal:
                break;
        }
    }

    private void VisitarFuncion(Funcion f)
    {
        string tipoAnterior = _tipoFuncionActual;
        _tipoFuncionActual = f.TipoRetorno;
        _tablaActual = new TablaSimbolos(_tablaActual);

        try
        {
            foreach (var param in f.Parametros)
            {
                var simboloParam = new Simbolo(param.Nombre, param.Tipo, TipoSimbolo.Parametro);
                if (!_tablaActual.Insertar(param.Nombre, simboloParam))
                {
                    throw new SemanticException($"El parámetro '{param.Nombre}' ya está declarado en la función.", param.Linea, param.Columna);
                }
            }

            Visitar(f.Cuerpo);

            if (f.TipoRetorno != "void" && !GarantizaRetorno(f.Cuerpo))
            {
                throw new SemanticException($"La función '{f.Nombre}' de tipo '{f.TipoRetorno}' debe retornar un valor en todos los caminos de ejecución.", f.Linea, f.Columna);
            }
        }
        finally
        {
            _tablaActual = _tablaActual.Padre ?? _tablaActual;
            _tipoFuncionActual = tipoAnterior;
        }
    }

    private bool GarantizaRetorno(NodoAST nodo)
    {
        switch (nodo)
        {
            case Retorno:
                return true;
            case Bloque b:
                foreach (var inst in b.Instrucciones)
                {
                    if (GarantizaRetorno(inst)) return true;
                }
                return false;
            case Condicional c:
                if (c.Sino == null) return false;
                return GarantizaRetorno(c.Entonces) && GarantizaRetorno(c.Sino);
            default:
                return false;
        }
    }

    private void VisitarBloque(Bloque b)
    {
        _tablaActual = new TablaSimbolos(_tablaActual);
        try
        {
            foreach (var instruccion in b.Instrucciones)
            {
                Visitar(instruccion);
            }
        }
        finally
        {
            _tablaActual = _tablaActual.Padre ?? _tablaActual;
        }
    }

    private void VisitarDeclaracionVariable(DeclaracionVariable dv)
    {
        if (dv.ValorInicial != null)
        {
            Visitar(dv.ValorInicial);
            string tipoExpr = ObtenerTipoExpresion(dv.ValorInicial);
            ValidarAsignacionTipos(dv.Tipo, tipoExpr, dv.Linea, dv.Columna);
        }

        var simbolo = new Simbolo(dv.Nombre, dv.Tipo, TipoSimbolo.Variable);
        if (!_tablaActual.Insertar(dv.Nombre, simbolo))
        {
            throw new SemanticException($"La variable '{dv.Nombre}' ya ha sido declarada en este ámbito.", dv.Linea, dv.Columna);
        }
    }

    private void VisitarAsignacion(Asignacion a)
    {
        var simbolo = _tablaActual.Buscar(a.Nombre);
        if (simbolo == null)
        {
            throw new SemanticException($"La variable '{a.Nombre}' no ha sido declarada.", a.Linea, a.Columna);
        }
        if (simbolo.Categoria == TipoSimbolo.Funcion)
        {
            throw new SemanticException($"No se puede asignar un valor a la función '{a.Nombre}'.", a.Linea, a.Columna);
        }

        Visitar(a.Valor);
        string tipoValor = ObtenerTipoExpresion(a.Valor);

        if (a.Operador != "=")
        {
            string opBase = a.Operador[..^1];
            string tipoResultante = EvaluarOperacionBinaria(simbolo.Tipo, opBase, tipoValor, a.Linea, a.Columna);
            ValidarAsignacionTipos(simbolo.Tipo, tipoResultante, a.Linea, a.Columna);
        }
        else
        {
            ValidarAsignacionTipos(simbolo.Tipo, tipoValor, a.Linea, a.Columna);
        }
    }

    private void VisitarCondicional(Condicional c)
    {
        Visitar(c.Condicion);
        string tipoCond = ObtenerTipoExpresion(c.Condicion);
        if (tipoCond != "bool")
        {
            throw new SemanticException($"La condición del 'if' debe ser de tipo 'bool', se encontró '{tipoCond}'.", c.Linea, c.Columna);
        }

        Visitar(c.Entonces);
        if (c.Sino != null)
        {
            Visitar(c.Sino);
        }
    }

    private void VisitarBucleWhile(BucleWhile w)
    {
        Visitar(w.Condicion);
        string tipoCond = ObtenerTipoExpresion(w.Condicion);
        if (tipoCond != "bool")
        {
            throw new SemanticException($"La condición del 'while' debe ser de tipo 'bool', se encontró '{tipoCond}'.", w.Linea, w.Columna);
        }
        Visitar(w.Cuerpo);
    }

    private void VisitarBucleFor(BucleFor fo)
    {
        _tablaActual = new TablaSimbolos(_tablaActual);
        try
        {
            if (fo.Inicializacion != null) Visitar(fo.Inicializacion);

            if (fo.Condicion != null)
            {
                Visitar(fo.Condicion);
                string tipoCond = ObtenerTipoExpresion(fo.Condicion);
                if (tipoCond != "bool")
                {
                    throw new SemanticException($"La condición del 'for' debe ser de tipo 'bool', se encontró '{tipoCond}'.", fo.Linea, fo.Columna);
                }
            }

            if (fo.Incremento != null) Visitar(fo.Incremento);
            Visitar(fo.Cuerpo);
        }
        finally
        {
            _tablaActual = _tablaActual.Padre ?? _tablaActual;
        }
    }

    private void VisitarRetorno(Retorno r)
    {
        string tipoValorRetorno = "void";
        if (r.Valor != null)
        {
            Visitar(r.Valor);
            tipoValorRetorno = ObtenerTipoExpresion(r.Valor);
        }

        if (_tipoFuncionActual == "void" && r.Valor != null)
        {
            throw new SemanticException("Una función de tipo 'void' no puede retornar un valor.", r.Linea, r.Columna);
        }
        if (_tipoFuncionActual != "void" && r.Valor == null)
        {
            throw new SemanticException($"Se esperaba un valor de retorno de tipo '{_tipoFuncionActual}'.", r.Linea, r.Columna);
        }
        if (_tipoFuncionActual != "void" && r.Valor != null)
        {
            ValidarAsignacionTipos(_tipoFuncionActual, tipoValorRetorno, r.Linea, r.Columna);
        }
    }

    private void VisitarExpresionBinaria(ExpresionBinaria eb)
    {
        Visitar(eb.Izquierda);
        Visitar(eb.Derecha);
        InferirTipoBinario(eb);
    }

    private void VisitarExpresionUnaria(ExpresionUnaria eu)
    {
        Visitar(eu.Operando);
        string tipoOp = ObtenerTipoExpresion(eu.Operando);

        if (eu.Operador == "!")
        {
            if (tipoOp != "bool")
                throw new SemanticException($"El operador unario '!' requiere un operando de tipo 'bool', se encontró '{tipoOp}'.", eu.Linea, eu.Columna);
        }
        else if (eu.Operador == "-")
        {
            if (tipoOp != "int" && tipoOp != "float")
                throw new SemanticException($"El operador unario '-' requiere un operando numérico, se encontró '{tipoOp}'.", eu.Linea, eu.Columna);
        }
        else if (eu.Operador == "++" || eu.Operador == "--")
        {
            if (eu.Operando is not Identificador)
                throw new SemanticException($"El operador '{eu.Operador}' requiere una variable modificable.", eu.Linea, eu.Columna);

            if (tipoOp != "int" && tipoOp != "float")
                throw new SemanticException($"El operador '{eu.Operador}' requiere un operando numérico, se encontró '{tipoOp}'.", eu.Linea, eu.Columna);
        }
    }

    private void VisitarLlamadaFuncion(LlamadaFuncion lf)
    {
        var tiposArgs = new List<string>();
        foreach (var arg in lf.Argumentos)
        {
            Visitar(arg);
            tiposArgs.Add(ObtenerTipoExpresion(arg));
        }

        ResolverSobrecarga(lf.Nombre, tiposArgs, lf.Linea, lf.Columna);
    }

    private void VisitarIdentificador(Identificador id)
    {
        var simbolo = _tablaActual.Buscar(id.Nombre);
        if (simbolo == null)
        {
            throw new SemanticException($"La variable o identificador '{id.Nombre}' no ha sido declarada.", id.Linea, id.Columna);
        }
    }

    private Simbolo ResolverSobrecarga(string nombre, List<string> tiposArgs, int linea, int columna)
    {
        string claveExacta = Simbolo.ObtenerClaveFuncion(nombre, tiposArgs);
        var exacta = _tablaActual.Buscar(claveExacta);
        if (exacta != null && exacta.Categoria == TipoSimbolo.Funcion)
        {
            return exacta;
        }

        var candidatos = new List<Simbolo>();
        TablaSimbolos? scope = _tablaActual;
        while (scope != null)
        {
            foreach (var sim in scope.SimbolosLocales)
            {
                if (sim.Categoria == TipoSimbolo.Funcion &&
                    sim.Nombre == nombre &&
                    sim.TiposParametros != null &&
                    sim.TiposParametros.Count == tiposArgs.Count &&
                    !candidatos.Any(c => c.TiposParametros!.SequenceEqual(sim.TiposParametros)))
                {
                    bool compatible = true;
                    for (int i = 0; i < tiposArgs.Count; i++)
                    {
                        string esperado = sim.TiposParametros[i];
                        string real = tiposArgs[i];
                        if (esperado != real && !(esperado == "float" && real == "int"))
                        {
                            compatible = false;
                            break;
                        }
                    }
                    if (compatible)
                    {
                        candidatos.Add(sim);
                    }
                }
            }
            scope = scope.Padre;
        }

        if (candidatos.Count == 1)
        {
            return candidatos[0];
        }

        if (candidatos.Count > 1)
        {
            throw new SemanticException($"Llamada ambigua a la función '{nombre}' con argumentos ({string.Join(", ", tiposArgs)}).", linea, columna);
        }

        throw new SemanticException($"No existe ninguna función '{nombre}' compatible con los argumentos especificados ({string.Join(", ", tiposArgs)}).", linea, columna);
    }

    private string ObtenerTipoExpresion(NodoAST nodo)
    {
        return nodo switch
        {
            Literal lit => lit.TipoLiteral,
            Identificador id => _tablaActual.Buscar(id.Nombre)?.Tipo ?? "error",
            LlamadaFuncion lf => BuscarTipoLlamada(lf),
            ExpresionUnaria eu => ObtenerTipoExpresion(eu.Operando),
            ExpresionBinaria eb => InferirTipoBinario(eb),
            _ => "error"
        };
    }

    private string BuscarTipoLlamada(LlamadaFuncion lf)
    {
        var tiposArgs = lf.Argumentos.Select(ObtenerTipoExpresion).ToList();
        return ResolverSobrecarga(lf.Nombre, tiposArgs, lf.Linea, lf.Columna).Tipo;
    }

    private string InferirTipoBinario(ExpresionBinaria eb)
    {
        string tIzq = ObtenerTipoExpresion(eb.Izquierda);
        string tDer = ObtenerTipoExpresion(eb.Derecha);
        return EvaluarOperacionBinaria(tIzq, eb.Operador, tDer, eb.Linea, eb.Columna);
    }

    private string EvaluarOperacionBinaria(string tIzq, string operador, string tDer, int linea, int columna)
    {
        // Operadores lógicos
        if (operador == "&&" || operador == "||")
        {
            if (tIzq != "bool" || tDer != "bool")
                throw new SemanticException($"El operador lógico '{operador}' requiere operandos booleanos, se encontró '{tIzq}' y '{tDer}'.", linea, columna);
            return "bool";
        }

        // Operadores de igualdad
        if (operador == "==" || operador == "!=")
        {
            bool ambosNum = (tIzq == "int" || tIzq == "float") && (tDer == "int" || tDer == "float");
            if (!ambosNum && tIzq != tDer)
                throw new SemanticException($"Tipos incompatibles para la comparación '{operador}': '{tIzq}' y '{tDer}'.", linea, columna);
            return "bool";
        }

        // Operadores de orden
        if (operador == "<" || operador == ">" || operador == "<=" || operador == ">=")
        {
            bool ambosNum = (tIzq == "int" || tIzq == "float") && (tDer == "int" || tDer == "float");
            if (!ambosNum)
                throw new SemanticException($"El operador relacional '{operador}' requiere operandos numéricos, se encontró '{tIzq}' y '{tDer}'.", linea, columna);
            return "bool";
        }

        // Operadores aritméticos
        if (operador == "+" || operador == "-" || operador == "*" || operador == "/" || operador == "%")
        {
            if (tIzq == "string" && tDer == "string" && operador == "+")
                return "string";

            bool izqNum = tIzq == "int" || tIzq == "float";
            bool derNum = tDer == "int" || tDer == "float";

            if (!izqNum || !derNum)
                throw new SemanticException($"El operador aritmético '{operador}' no se puede aplicar a los tipos '{tIzq}' y '{tDer}'.", linea, columna);

            if (tIzq == "float" || tDer == "float") return "float";
            return "int";
        }

        throw new SemanticException($"Operador binario desconocido '{operador}'.", linea, columna);
    }

    private void ValidarAsignacionTipos(string tipoEsperado, string tipoReal, int linea, int columna)
    {
        if (tipoEsperado == tipoReal) return;
        if (tipoEsperado == "float" && tipoReal == "int") return;

        throw new SemanticException($"Incompatibilidad de tipos: no se puede convertir de '{tipoReal}' a '{tipoEsperado}'.", linea, columna);
    }
}

// Main

class Program
{
    static void EsperarTeclaSiEsPosible()
    {
        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }

    static void Main(string[] args)
    {
        string path = args.Length > 0
            ? args[0]
            : Path.Combine(AppContext.BaseDirectory, "prueba.txt");

        if (!File.Exists(path))
        {
            Console.WriteLine($"No se encontró el archivo '{path}'.");
            Console.WriteLine($"Directorio base: {AppContext.BaseDirectory}");
            return;
        }

        try
        {
            string sourceCode = File.ReadAllText(path);

            Console.WriteLine("\tTokenizador iniciado\n");
            Tokenizador tokenizador = new Tokenizador(sourceCode);
            var tokens = tokenizador.Tokenizar().ToList();

            foreach (var token in tokens)
            {
                if (token.Tipo != TipoToken.Blanco)
                {
                    Console.WriteLine(token);
                }
            }

            if (tokens.Any(t => t.Tipo == TipoToken.Error))
            {
                Console.WriteLine("\n[ERROR LÉXICO] Se encontraron tokens inválidos durante la tokenización.");
                EsperarTeclaSiEsPosible();
                return;
            }

            Console.WriteLine("\n\tParser iniciado\n");
            Parser parser = new Parser(tokens);
            Programa programa = parser.Parse();
            Console.WriteLine("Análisis sintáctico exitoso");

            Console.WriteLine("\n\tAnalizador Semántico iniciado\n");
            AnalizadorSemantico semantico = new AnalizadorSemantico();
            semantico.Analizar(programa);
            Console.WriteLine("Análisis semántico exitoso (Verificación de tipos, ámbitos y retornos correcta)");

            Console.WriteLine("\n\tAST resultante\n");
            AstPrinter.Imprimir(programa);

            EsperarTeclaSiEsPosible();
        }
        catch (ParserException ex)
        {
            Console.WriteLine($"\n[ERROR SINTÁCTICO] {ex.Message}");
            EsperarTeclaSiEsPosible();
        }
        catch (SemanticException ex)
        {
            Console.WriteLine($"\n[ERROR SEMÁNTICO] {ex.Message}");
            EsperarTeclaSiEsPosible();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
            EsperarTeclaSiEsPosible();
        }
    }
}