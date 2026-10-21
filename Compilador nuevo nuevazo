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

// El AST: Árbol de Sintaxis Abstracta.

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

// Excepción para errores sintácticos.

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

// Impresor del AST, para verificar visualmente la estructura.

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

// Parser por descenso recursivo completo.

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

    public Parser(IEnumerable<Token> tokens)
    {
        _tokens = tokens.Where(t => t.Tipo != TipoToken.Comentario).ToList();
        _posicion = 0;
    }

    private Token Actual => _posicion < _tokens.Count ? _tokens[_posicion] : _tokens[_tokens.Count - 1];
    private Token Anterior => _posicion > 0 ? _tokens[_posicion - 1] : _tokens[0];
    private bool EsFin => Actual.Tipo == TipoToken.EOF;

    private bool EsIdentificador()
    {
        return Actual.Tipo == TipoToken.Identificador || (Actual.Tipo == TipoToken.Keyword && Actual.Valor == "main");
    }

    private bool EsTipoDato()
    {
        return Actual.Tipo == TipoToken.Keyword && TiposDato.Contains(Actual.Valor);
    }

    private bool EsTipoRetorno()
    {
        return Actual.Tipo == TipoToken.Keyword && TiposRetorno.Contains(Actual.Valor);
    }

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
        // Mira hacia adelante: tipo identificador "("  o  tipo identificador ("=" | ";")
        // _posicion + 0 = tipo
        // _posicion + 1 = identificador
        // _posicion + 2 = "(" (función) | "=" | ";" (variable)
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
        if (EsTipoDato())
        {
            return Avanzar().Valor;
        }
        Error($"Se esperaba un tipo de dato válido, se encontró '{Actual.Valor}'");
        return string.Empty;
    }

    private string ParseTipoRetorno()
    {
        if (EsTipoRetorno())
        {
            return Avanzar().Valor;
        }
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
        if (Verificar(TipoToken.Delimitador, "{"))
        {
            return ParseBloque();
        }
        if (Coincidir("if"))
        {
            return ParseCondicional();
        }
        if (Coincidir("while"))
        {
            return ParseBucleWhile();
        }
        if (Coincidir("for"))
        {
            return ParseBucleFor();
        }
        if (Coincidir("return"))
        {
            return ParseRetorno();
        }
        if (EsTipoDato())
        {
            return ParseDeclaracionVariable();
        }

        // Detección de asignación: IDENTIFICADOR seguido de operador de asignación
        if (EsIdentificador() && _posicion + 1 < _tokens.Count)
        {
            string siguienteValor = _tokens[_posicion + 1].Valor;
            if (siguienteValor == "=" || siguienteValor == "+=" || siguienteValor == "-=" || siguienteValor == "*=" || siguienteValor == "/=")
            {
                return ParseAsignacionConPuntoYComa();
            }
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

    // Variante con punto y coma final (para instrucciones sueltas).
    private Asignacion ParseAsignacionConPuntoYComa()
    {
        Asignacion asignacion = ParseAsignacionInterna();
        ConsumirValor(";");
        return asignacion;
    }

    // Variante sin punto y coma final (para el for).
    private Asignacion ParseAsignacionSinPuntoYComa()
    {
        return ParseAsignacionInterna();
    }

    // Núcleo de la asignación, sin consumir el ";" final.
    private Asignacion ParseAsignacionInterna()
    {
        int linea = Actual.Linea;
        int columna = Actual.Columna;
        if (!EsIdentificador()) Error($"Se esperaba un identificador para la asignación, se encontró '{Actual.Valor}'");
        string nombre = Avanzar().Valor;

        if (!Coincidir("=", "+=", "-=", "*=", "/="))
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

        // Declaración con tipo: int i = 0
        if (EsTipoDato())
        {
            int linea = Actual.Linea;
            int columna = Actual.Columna;
            string tipo = ParseTipoDato();
            if (!EsIdentificador()) Error($"Se esperaba un identificador, se encontró '{Actual.Valor}'");
            string nombre = Avanzar().Valor;
            NodoAST? valorInicial = null;
            if (Coincidir("="))
            {
                valorInicial = ParseExpresion();
            }
            return new DeclaracionVariable(tipo, nombre, valorInicial, linea, columna);
        }

        // Asignación: i = 0
        if (EsIdentificador() && _posicion + 1 < _tokens.Count)
        {
            string siguiente = _tokens[_posicion + 1].Valor;
            if (siguiente == "=" || siguiente == "+=" || siguiente == "-=" || siguiente == "*=" || siguiente == "/=")
            {
                return ParseAsignacionSinPuntoYComa();
            }
        }

        // Cualquier otra expresión (llamadas a función, etc.)
        return ParseExpresion();
    }

    private NodoAST? ParseCondicionFor()
    {
        if (Verificar(TipoToken.Delimitador, ";")) return null;
        return ParseExpresion();
    }

    private NodoAST? ParseIncrementoFor()
    {
        if (Verificar(TipoToken.Delimitador, ")")) return null;

        // Asignación: i = i + 1, i += 1
        if (EsIdentificador() && _posicion + 1 < _tokens.Count)
        {
            string siguiente = _tokens[_posicion + 1].Valor;
            if (siguiente == "=" || siguiente == "+=" || siguiente == "-=" || siguiente == "*=" || siguiente == "/=")
            {
                return ParseAsignacionSinPuntoYComa();
            }
        }

        // Expresión: i++, ++i, llamada, etc.
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

    // Jerarquía de expresiones por precedencia de operadores
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
        if (Coincidir("++", "--"))
        {
            Token op = Anterior;
            return new ExpresionUnaria(op.Valor, primario, false, op.Linea, op.Columna);
        }
        return primario;
    }

    private NodoAST ParsePrimario()
    {
        int linea = Actual.Linea;
        int columna = Actual.Columna;

        if (Verificar(TipoToken.Numero))
        {
            Token t = Avanzar();
            object valor = t.Valor.Contains('.')
                ? double.Parse(t.Valor, System.Globalization.CultureInfo.InvariantCulture)
                : int.Parse(t.Valor);
            return new Literal(valor, "number", t.Linea, t.Columna);
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

    // Utilidades.

    private bool Verificar(TipoToken tipo) => Actual.Tipo == tipo;

    private bool Verificar(TipoToken tipo, string valor)
        => Actual.Tipo == tipo && Actual.Valor == valor;

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
        throw new ParserException(
            $"Se esperaba '{valor}', se encontró '{Actual.Valor}' ({Actual.Tipo})",
            Actual.Linea,
            Actual.Columna);
    }

    private void Error(string mensaje)
    {
        throw new ParserException(mensaje, Actual.Linea, Actual.Columna);
    }
}

// El programa principal.

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
            var tokens = tokenizador.Tokenizar().ToList();

            foreach (var token in tokens)
            {
                if (token.Tipo != TipoToken.Blanco)
                {
                    Console.WriteLine(token);
                }
            }

            Console.WriteLine("\n\tParser iniciado\n");

            Parser parser = new Parser(tokens);
            Programa programa = parser.Parse();

            Console.WriteLine("¡Análisis sintáctico exitoso!");

            Console.WriteLine("\n\tAST resultante\n");
            AstPrinter.Imprimir(programa);

            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
        catch (ParserException ex)
        {
            Console.WriteLine($"\n[ERROR SINTÁCTICO] {ex.Message}");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
            Console.ReadKey();
        }
    }
}
