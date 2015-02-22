using System.Text;



using System;

namespace Nett.Parser {



internal sealed partial class Parser {
	public const int _EOF = 0;
	public const int _number = 1;
	public const int _sign = 2;
	public const int _identifier = 3;
	public const int _string = 4;
	public const int _mstring = 5;
	public const int _lstring = 6;
	public const int maxT = 9;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public readonly TomlTable parsed = new TomlTable();
	private readonly StringBuilder sb = new StringBuilder(32);



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void Toml() {
		string key; object val; 
		Key(out key);
		Expect(7);
		Value(out val);
		parsed.Add(key, val); 
	}

	void Key(out string val) {
		Expect(3);
		val = t.val; 
	}

	void Value(out object val) {
		long iv; val = null; 
		if (la.kind == 1 || la.kind == 2) {
			IntVal(out iv);
			val = iv; 
		} else if (la.kind == 4) {
			Get();
			val = ParseStringVal(t.val); 
		} else if (la.kind == 5) {
			Get();
			val = ParseMStringVal(t.val); 
		} else if (la.kind == 6) {
			Get();
			val = ParseLStringVal(t.val); 
		} else SynErr(10);
	}

	void IntVal(out long val) {
		bool neg = false; this.sb.Clear(); 
		if (la.kind == 2) {
			Get();
		}
		if(t.val == "-") neg = true; 
		Expect(1);
		sb.Append(t.val); 
		while (la.kind == 8) {
			Get();
			Expect(1);
			sb.Append(t.val); 
		}
		val = this.ParseIntVal(sb, neg); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Toml();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "number expected"; break;
			case 2: s = "sign expected"; break;
			case 3: s = "identifier expected"; break;
			case 4: s = "string expected"; break;
			case 5: s = "mstring expected"; break;
			case 6: s = "lstring expected"; break;
			case 7: s = "\"=\" expected"; break;
			case 8: s = "\"_\" expected"; break;
			case 9: s = "??? expected"; break;
			case 10: s = "invalid Value"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}