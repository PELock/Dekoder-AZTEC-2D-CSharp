using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Runtime.Serialization.Json
{
	internal class JavaScriptReader
	{
		TextReader r;
		int line = 1, column = 0;
//		bool raise_on_number_error; // FIXME: use it

		public JavaScriptReader (TextReader reader, bool raiseOnNumberError)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			this.r = reader;
//			raise_on_number_error = raiseOnNumberError;
		}

		public object Read ()
		{
			object v = ReadCore ();
			SkipSpaces ();
			if (ReadChar () >= 0)
				throw JsonError (String.Format ("extra characters in JSON input"));
			return v;
		}

		object ReadCore ()
		{
			SkipSpaces ();
			int c = PeekChar ();
			if (c < 0)
				throw JsonError ("Incomplete JSON input");
			switch (c) {
			case '[':
				ReadChar ();
                List<object> list = new List<object>();
				SkipSpaces ();
				if (PeekChar () == ']') {
					ReadChar ();
					return list;
				}
				while (true) {
					list.Add (ReadCore ());
					SkipSpaces ();
					c = PeekChar ();
					if (c != ',')
						break;
					ReadChar ();
					continue;
				}
				if (ReadChar () != ']')
					throw JsonError ("JSON array must end with ']'");
				return list.ToArray ();
			case '{':
				ReadChar ();
                Dictionary<string, object> obj = new Dictionary<string, object>();
				SkipSpaces ();
				if (PeekChar () == '}') {
					ReadChar ();
					return obj;
				}
				while (true) {
					SkipSpaces ();
					if (PeekChar () == '}') {
						ReadChar ();
						break;
					}
					string name = ReadStringLiteral ();
					SkipSpaces ();
					Expect (':');
					SkipSpaces ();
					obj [name] = ReadCore (); // it does not reject duplicate names.
					SkipSpaces ();
					c = ReadChar ();
					if (c == ',')
						continue;
					if (c == '}')
						break;
				}
#if true
				int idx = 0;
				KeyValuePair<string, object> [] ret = new KeyValuePair<string, object>[obj.Count];
				foreach (KeyValuePair <string, object> kvp in obj)
					ret [idx++] = kvp;

				return ret;
#else
                // C# 2.0 does not support extension methods
                //return obj.ToArray ();
                return Enumerable.ToArray(obj);
#endif
			case 't':
				Expect ("true");
				return true;
			case 'f':
				Expect ("false");
				return false;
			case 'n':
				Expect ("null");
				// FIXME: what should we return?
				return (string) null;
			case '"':
				return ReadStringLiteral ();
			default:
				if ('0' <= c && c <= '9' || c == '-')
					return ReadNumericLiteral ();
				else
					throw JsonError (String.Format ("Unexpected character '{0}'", (char) c));
			}
		}

		int peek;
		bool has_peek;
		bool prev_lf;

		int PeekChar ()
		{
			if (!has_peek) {
				peek = r.Read ();
				has_peek = true;
			}
			return peek;
		}

		int ReadChar ()
		{
			int v = has_peek ? peek : r.Read ();

			has_peek = false;

			if (prev_lf) {
				line++;
				column = 0;
				prev_lf = false;
			}

			if (v == '\n')
				prev_lf = true;
			column++;

			return v;
		}

		void SkipSpaces ()
		{
			while (true) {
				switch (PeekChar ()) {
				case ' ': case '\t': case '\r': case '\n':
					ReadChar ();
					continue;
				default:
					return;
				}
			}
		}

		// It could return either int, long or decimal, depending on the parsed value.
		object ReadNumericLiteral ()
		{
            StringBuilder sb = new StringBuilder();
			
			//bool negative = false;
			if (PeekChar () == '-') {
				//negative = true;
				sb.Append ((char) ReadChar ());
			}

			int c;
			int x = 0;
			bool zeroStart = PeekChar () == '0';
			for (; ; x++) {
				c = PeekChar ();
				if (c < '0' || '9' < c)
					break;
				sb.Append ((char) ReadChar ());
				if (zeroStart && x == 1)
					throw JsonError ("leading zeros are not allowed");
			}
			if (x == 0) // Reached e.g. for "- "
				throw JsonError ("Invalid JSON numeric literal; no digit found");

			// fraction
			bool hasFrac = false;
			int fdigits = 0;
			if (PeekChar () == '.') {
				hasFrac = true;
				sb.Append ((char) ReadChar ());
				if (PeekChar () < 0)
					throw JsonError ("Invalid JSON numeric literal; extra dot");
				while (true) {
					c = PeekChar ();
					if (c < '0' || '9' < c)
						break;
					sb.Append ((char) ReadChar ());
					fdigits++;
				}
				if (fdigits == 0)
					throw JsonError ("Invalid JSON numeric literal; extra dot");
			}

			c = PeekChar ();
			if (c != 'e' && c != 'E') {
				if (!hasFrac) {
					int valueInt;
					if (int.TryParse (sb.ToString (), NumberStyles.Float, CultureInfo.InvariantCulture, out valueInt))
						return valueInt;
					
					long valueLong;
					if (long.TryParse (sb.ToString (), NumberStyles.Float, CultureInfo.InvariantCulture, out valueLong))
						return valueLong;
					
					ulong valueUlong;
					if (ulong.TryParse (sb.ToString (), NumberStyles.Float, CultureInfo.InvariantCulture, out valueUlong))
						return valueUlong;
				}
				decimal valueDecimal;
				if (decimal.TryParse (sb.ToString (), NumberStyles.Float, CultureInfo.InvariantCulture, out valueDecimal) && valueDecimal != 0)
					return valueDecimal;
			} else {
				// exponent
				sb.Append ((char) ReadChar ());
				if (PeekChar () < 0)
					throw new ArgumentException ("Invalid JSON numeric literal; incomplete exponent");
			
				c = PeekChar ();
				if (c == '-') {
					sb.Append ((char) ReadChar ());
				}
				else if (c == '+')
					sb.Append ((char) ReadChar ());

				if (PeekChar () < 0)
					throw JsonError ("Invalid JSON numeric literal; incomplete exponent");
				while (true) {
					c = PeekChar ();
					if (c < '0' || '9' < c)
						break;
					sb.Append ((char) ReadChar ());
				}
			}

			return double.Parse (sb.ToString (), NumberStyles.Float, CultureInfo.InvariantCulture);
		}

		StringBuilder vb = new StringBuilder ();

		string ReadStringLiteral ()
		{
			if (PeekChar () != '"')
				throw JsonError ("Invalid JSON string literal format");

			ReadChar ();
			vb.Length = 0;
			while (true) {
				int c = ReadChar ();
				if (c < 0)
					throw JsonError ("JSON string is not closed");
				if (c == '"')
					return vb.ToString ();
				else if (c != '\\') {
					vb.Append ((char) c);
					continue;
				}

				// escaped expression
				c = ReadChar ();
				if (c < 0)
					throw JsonError ("Invalid JSON string literal; incomplete escape sequence");
				switch (c) {
				case '"':
				case '\\':
				case '/':
					vb.Append ((char) c);
					break;
				case 'b':
					vb.Append ('\x8');
					break;
				case 'f':
					vb.Append ('\f');
					break;
				case 'n':
					vb.Append ('\n');
					break;
				case 'r':
					vb.Append ('\r');
					break;
				case 't':
					vb.Append ('\t');
					break;
				case 'u':
					ushort cp = 0;
					for (int i = 0; i < 4; i++) {
						cp <<= 4;
						if ((c = ReadChar ()) < 0)
							throw JsonError ("Incomplete unicode character escape literal");
						if ('0' <= c && c <= '9')
							cp += (ushort) (c - '0');
						if ('A' <= c && c <= 'F')
							cp += (ushort) (c - 'A' + 10);
						if ('a' <= c && c <= 'f')
							cp += (ushort) (c - 'a' + 10);
					}
					vb.Append ((char) cp);
					break;
				default:
					throw JsonError ("Invalid JSON string literal; unexpected escape character");
				}
			}
		}

		void Expect (char expected)
		{
			int c;
			if ((c = ReadChar ()) != expected)
				throw JsonError (String.Format ("Expected '{0}', got '{1}'", expected, (char) c));
		}

		void Expect (string expected)
		{
			for (int i = 0; i < expected.Length; i++)
				if (ReadChar () != expected [i])
					throw JsonError (String.Format ("Expected '{0}', differed at {1}", expected, i));
		}

		Exception JsonError (string msg)
		{
			return new ArgumentException (String.Format ("{0}. At line {1}, column {2}", msg, line, column));
		}
	}
}
