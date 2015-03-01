using System.Text;



using System;

namespace Nett.Parser {



internal sealed partial class Parser {
	public const int _EOF = 0;
	public const int _sign = 1;
	public const int _letters = 2;
	public const int _number = 3;
	public const int _string = 4;
	public const int _mstring = 5;
	public const int _lstring = 6;
	public const int _mlstring = 7;
	public const int _true = 8;
	public const int _false = 9;
	public const int maxT = 15;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public readonly TomlTable parsed = new TomlTable();
	private readonly StringBuilder psb = new StringBuilder(32);



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
		Expect(10);
		Value(out val);
		parsed.Add(key, val); 
	}

	void Key(out string val) {
		val = ""; psb.Clear(); 
		Expect(2);
		this.psb.Append(t.val); 
		while (la.kind == 2 || la.kind == 3) {
			if (la.kind == 2) {
				Get();
				this.psb.Append(t.val); 
			} else {
				Get();
				this.psb.Append(t.val); 
			}
		}
		val = psb.ToString(); 
	}

	void Value(out object val) {
		val = null; 
		switch (la.kind) {
		case 4: {
			Get();
			val = ParseStringVal(t.val); 
			break;
		}
		case 5: {
			Get();
			val = ParseMStringVal(t.val); 
			break;
		}
		case 6: {
			Get();
			val = ParseLStringVal(t.val); 
			break;
		}
		case 7: {
			Get();
			val = ParseMLStringVal(t.val); 
			break;
		}
		case 1: case 3: {
			NumVal(out val);
			break;
		}
		case 8: case 9: {
			BoolVal(out val);
			break;
		}
		default: SynErr(16); break;
		}
	}

	void NumVal(out object val) {
		bool neg = false; this.psb.Clear(); 
		if (la.kind == 1) {
			Get();
		}
		if(t.val == "-") neg = true; 
		IntNumS();
		val = this.ParseIntVal(this.psb, neg); 
		if (la.kind == 12 || la.kind == 13 || la.kind == 14) {
			FloatPart(neg, out val);
		}
	}

	void BoolVal(out object val) {
		val = null; 
		if (la.kind == 8) {
			Get();
			val = true; 
		} else if (la.kind == 9) {
			Get();
			val = false; 
		} else SynErr(17);
	}

	void IntNumS() {
		Expect(3);
		psb.Append(t.val); 
		while (la.kind == 11) {
			Get();
			Expect(3);
			psb.Append(t.val); 
		}
	}

	void FloatPart(bool neg, out object val) {
		val = null; 
		if (la.kind == 12 || la.kind == 13) {
			if (la.kind == 12) {
				Get();
			} else {
				Get();
			}
			this.psb.Append(t.val); 
			if (la.kind == 1) {
				Get();
				this.psb.Append(t.val); 
			}
			IntNumS();
			val = this.ParseFloatVal(this.psb, neg); 
		} else if (la.kind == 14) {
			Get();
			this.psb.Append(t.val); 
			IntNumS();
			this.psb.Append(t.val); 
			if (la.kind == 12 || la.kind == 13) {
				if (la.kind == 12) {
					Get();
				} else {
					Get();
				}
				this.psb.Append(t.val); 
				if (la.kind == 1) {
					Get();
					this.psb.Append(t.val); 
				}
				IntNumS();
			}
			val = this.ParseFloatVal(this.psb, neg); 
		} else SynErr(18);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Toml();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x}

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
			case 1: s = "sign expected"; break;
			case 2: s = "letters expected"; break;
			case 3: s = "number expected"; break;
			case 4: s = "string expected"; break;
			case 5: s = "mstring expected"; break;
			case 6: s = "lstring expected"; break;
			case 7: s = "mlstring expected"; break;
			case 8: s = "true expected"; break;
			case 9: s = "false expected"; break;
			case 10: s = "\"=\" expected"; break;
			case 11: s = "\"_\" expected"; break;
			case 12: s = "\"e\" expected"; break;
			case 13: s = "\"E\" expected"; break;
			case 14: s = "\".\" expected"; break;
			case 15: s = "??? expected"; break;
			case 16: s = "invalid Value"; break;
			case 17: s = "invalid BoolVal"; break;
			case 18: s = "invalid FloatPart"; break;

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