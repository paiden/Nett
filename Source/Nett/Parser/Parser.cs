using System.Text;
using System.Collections.Generic;



using System;

namespace Nett.Parser {



internal sealed partial class Parser {
	public const int _EOF = 0;
	public const int _plus = 1;
	public const int _minus = 2;
	public const int _letters = 3;
	public const int _number = 4;
	public const int _string = 5;
	public const int _mstring = 6;
	public const int _lstring = 7;
	public const int _mlstring = 8;
	public const int _true = 9;
	public const int _false = 10;
	public const int _ao = 11;
	public const int _ac = 12;
	public const int _as = 13;
	public const int _us = 14;
	public const int maxT = 22;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

private readonly StringBuilder psb = new StringBuilder(32);
	public readonly TomlTable parsed = new TomlTable();
	private TomlTable current;



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
		this.current = this.parsed;
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
		while (la.kind == 3 || la.kind == 11) {
			if (la.kind == 3) {
				KeyValuePair(out key, out val);
				this.current.Add(key, val); 
			} else {
				TomlTable();
			}
		}
	}

	void KeyValuePair(out string key, out object val) {
		Key(out key);
		Expect(15);
		Value(out val);
	}

	void TomlTable() {
		string tableName = null; string key = null; object val = null; 
		Expect(11);
		Key(out tableName);
		Expect(12);
		var t = new TomlTable(tableName); this.current.Add(t.Name, t); this.current = t; 
		while (la.kind == 3 || la.kind == 11) {
			if (la.kind == 11) {
				TomlTable();
			} else {
				KeyValuePair(out key, out val);
			}
		}
	}

	void Key(out string val) {
		val = ""; psb.Clear(); 
		BareKey(out val);
	}

	void Value(out object val) {
		val = null; 
		if (la.kind == 5) {
			Get();
			val = ParseStringVal(t.val); 
		} else if (la.kind == 9 || la.kind == 10) {
			BoolVal(out val);
		} else if (la.kind == 6) {
			Get();
			val = ParseMStringVal(t.val); 
		} else if (la.kind == 7) {
			Get();
			val = ParseLStringVal(t.val); 
		} else if (la.kind == 8) {
			Get();
			val = ParseMLStringVal(t.val); 
		} else if (la.kind == 11) {
			Array(out val);
		} else if (NotADateTime()) {
			NumVal(out val);
		} else if (la.kind == 4) {
			DateTimeVal(out val);
		} else SynErr(23);
	}

	void BareKey(out string val) {
		val = null; this.psb.Clear(); 
		Expect(3);
		this.psb.Append(t.val); 
		while (StartOf(1)) {
			if (la.kind == 3) {
				Get();
				this.psb.Append(t.val); 
			} else if (la.kind == 4) {
				Get();
				this.psb.Append(t.val); 
			} else if (la.kind == 2) {
				Get();
				this.psb.Append(t.val); 
			} else {
				Get();
				this.psb.Append(t.val); 
			}
		}
		val = psb.ToString(); 
	}

	void BoolVal(out object val) {
		val = null; 
		if (la.kind == 9) {
			Get();
			val = true; 
		} else if (la.kind == 10) {
			Get();
			val = false; 
		} else SynErr(24);
	}

	void Array(out object val) {
		object v = null; val = null; var a = new TomlArray(); 
		Expect(11);
		Value(out v);
		a.Add(v); 
		while (la.kind == 13) {
			Get();
			Value(out v);
			a.Add(v); 
		}
		Expect(12);
		val = a; 
	}

	void NumVal(out object val) {
		bool neg = false; string sv = null; this.psb.Clear(); 
		if (la.kind == 1 || la.kind == 2) {
			Sign(out sv);
		}
		if(sv == "-") neg = true; 
		IntNumS();
		val = this.ParseIntVal(this.psb, neg); 
		if (la.kind == 16 || la.kind == 17 || la.kind == 18) {
			FloatPart(neg, out val);
		}
	}

	void DateTimeVal(out object val) {
		val = null; string sv = null; this.psb.Clear(); 
		Year();
		Expect(2);
		this.psb.Append(t.val); 
		Month();
		Expect(2);
		this.psb.Append(t.val); 
		Day();
		Expect(19);
		this.psb.Append(t.val); 
		Hour();
		Expect(20);
		this.psb.Append(t.val); 
		Minute();
		Expect(20);
		this.psb.Append(t.val); 
		Second();
		if (la.kind == 21) {
			Get();
			this.psb.Append(t.val); 
		} else if (la.kind == 1 || la.kind == 2) {
			Sign(out sv);
			this.psb.Append(sv); 
			Hour();
			Expect(20);
			this.psb.Append(t.val); 
			Minute();
		} else SynErr(25);
		val = DateTime.Parse(this.psb.ToString()); 
	}

	void Sign(out string val) {
		val = null; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 2) {
			Get();
			val = t.val; 
		} else SynErr(26);
	}

	void IntNumS() {
		Expect(4);
		psb.Append(t.val); 
		while (la.kind == 14) {
			Get();
			Expect(4);
			psb.Append(t.val); 
		}
	}

	void FloatPart(bool neg, out object val) {
		val = null; string sv = null; 
		if (la.kind == 16 || la.kind == 17) {
			if (la.kind == 16) {
				Get();
			} else {
				Get();
			}
			this.psb.Append(t.val); 
			if (la.kind == 1 || la.kind == 2) {
				Sign(out sv);
			}
			this.psb.Append(sv); 
			IntNumS();
			val = this.ParseFloatVal(this.psb, neg); 
		} else if (la.kind == 18) {
			Get();
			this.psb.Append(t.val); 
			IntNumS();
			this.psb.Append(t.val); 
			if (la.kind == 16 || la.kind == 17) {
				if (la.kind == 16) {
					Get();
				} else {
					Get();
				}
				this.psb.Append(t.val); 
				if (la.kind == 1 || la.kind == 2) {
					Sign(out sv);
				}
				this.psb.Append(sv); 
				IntNumS();
			}
			val = this.ParseFloatVal(this.psb, neg); 
		} else SynErr(27);
	}

	void Year() {
		Expect(4);
		this.psb.Append(t.val); 
	}

	void Month() {
		Expect(4);
		this.psb.Append(t.val); 
	}

	void Day() {
		Expect(4);
		this.psb.Append(t.val); 
	}

	void Hour() {
		Expect(4);
		this.psb.Append(t.val); 
	}

	void Minute() {
		Expect(4);
		this.psb.Append(t.val); 
	}

	void Second() {
		Expect(4);
		this.psb.Append(t.val); 
		if (la.kind == 18) {
			Get();
			this.psb.Append(t.val); 
			Expect(4);
			this.psb.Append(t.val); 
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Toml();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x}

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
			case 1: s = "plus expected"; break;
			case 2: s = "minus expected"; break;
			case 3: s = "letters expected"; break;
			case 4: s = "number expected"; break;
			case 5: s = "string expected"; break;
			case 6: s = "mstring expected"; break;
			case 7: s = "lstring expected"; break;
			case 8: s = "mlstring expected"; break;
			case 9: s = "true expected"; break;
			case 10: s = "false expected"; break;
			case 11: s = "ao expected"; break;
			case 12: s = "ac expected"; break;
			case 13: s = "as expected"; break;
			case 14: s = "us expected"; break;
			case 15: s = "\"=\" expected"; break;
			case 16: s = "\"e\" expected"; break;
			case 17: s = "\"E\" expected"; break;
			case 18: s = "\".\" expected"; break;
			case 19: s = "\"T\" expected"; break;
			case 20: s = "\":\" expected"; break;
			case 21: s = "\"Z\" expected"; break;
			case 22: s = "??? expected"; break;
			case 23: s = "invalid Value"; break;
			case 24: s = "invalid BoolVal"; break;
			case 25: s = "invalid DateTimeVal"; break;
			case 26: s = "invalid Sign"; break;
			case 27: s = "invalid FloatPart"; break;

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