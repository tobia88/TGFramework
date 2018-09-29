using System.Collections.Generic;
using System.Linq;

namespace TG
{
	public class TGExpressionParser
	{
		private List<string> m_BracketHeap = new List<string>();
		private Dictionary<string, System.Func<double>> m_Consts = new Dictionary<string, System.Func<double>>();
		private Dictionary<string, System.Func<double[], double>> m_Funcs = new Dictionary<string, System.Func<double[], double>>();
		private TGExpression m_Context;

		public TGExpressionParser()
		{
			var rnd = new System.Random();
			m_Consts.Add("PI", () => System.Math.PI);
			m_Consts.Add("e", () => System.Math.E);
			m_Funcs.Add("sqrt", (p) => System.Math.Sqrt(p.FirstOrDefault()));
			m_Funcs.Add("abs", (p) => System.Math.Abs(p.FirstOrDefault()));
			m_Funcs.Add("ln", (p) => System.Math.Log(p.FirstOrDefault()));
			m_Funcs.Add("floor", (p) => System.Math.Floor(p.FirstOrDefault()));
			m_Funcs.Add("ceiling", (p) => System.Math.Ceiling(p.FirstOrDefault()));
			m_Funcs.Add("round", (p) => System.Math.Round(p.FirstOrDefault()));

			m_Funcs.Add("sin", (p) => System.Math.Sin(p.FirstOrDefault()));
			m_Funcs.Add("cos", (p) => System.Math.Cos(p.FirstOrDefault()));
			m_Funcs.Add("tan", (p) => System.Math.Tan(p.FirstOrDefault()));

			m_Funcs.Add("asin", (p) => System.Math.Asin(p.FirstOrDefault()));
			m_Funcs.Add("acos", (p) => System.Math.Acos(p.FirstOrDefault()));
			m_Funcs.Add("atan", (p) => System.Math.Atan(p.FirstOrDefault()));
			m_Funcs.Add("atan2", (p) => System.Math.Atan2(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
			//System.Math.Floor
			m_Funcs.Add("min", (p) => System.Math.Min(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
			m_Funcs.Add("max", (p) => System.Math.Max(p.FirstOrDefault(), p.ElementAtOrDefault(1)));
			m_Funcs.Add("rnd", (p) =>
			{
				if (p.Length == 2)
					return p[0] + rnd.NextDouble() * (p[1] - p[0]);
				if (p.Length == 1)
					return rnd.NextDouble() * p[0];
				return rnd.NextDouble();
			});
		}

		public void AddFunc(string aName, System.Func<double[], double> aMethod)
		{
			if (m_Funcs.ContainsKey(aName))
				m_Funcs[aName] = aMethod;
			else
				m_Funcs.Add(aName, aMethod);
		}

		public void AddConst(string aName, System.Func<double> aMethod)
		{
			if (m_Consts.ContainsKey(aName))
				m_Consts[aName] = aMethod;
			else
				m_Consts.Add(aName, aMethod);
		}
		public void RemoveFunc(string aName)
		{
			if (m_Funcs.ContainsKey(aName))
				m_Funcs.Remove(aName);
		}
		public void RemoveConst(string aName)
		{
			if (m_Consts.ContainsKey(aName))
				m_Consts.Remove(aName);
		}

		int FindClosingBracket(ref string aText, int aStart, char aOpen, char aClose)
		{
			int counter = 0;
			for (int i = aStart; i < aText.Length; i++)
			{
				if (aText[i] == aOpen)
					counter++;
				if (aText[i] == aClose)
					counter--;
				if (counter == 0)
					return i;
			}
			return -1;
		}

		void SubstitudeBracket(ref string aExpression, int aIndex)
		{
			int closing = FindClosingBracket(ref aExpression, aIndex, '(', ')');
			if (closing > aIndex + 1)
			{
				string inner = aExpression.Substring(aIndex + 1, closing - aIndex - 1);
				m_BracketHeap.Add(inner);
				string sub = "&" + (m_BracketHeap.Count - 1) + ";";
				aExpression = aExpression.Substring(0, aIndex) + sub + aExpression.Substring(closing + 1);
			}
			else throw new ParseException("Bracket not closed!");
		}

		IValue Parse(string aExpression)
		{
			aExpression = aExpression.Trim();
			int index = aExpression.IndexOf('(');
			while (index >= 0)
			{
				SubstitudeBracket(ref aExpression, index);
				index = aExpression.IndexOf('(');
			}
			if (aExpression.Contains(','))
			{
				string[] parts = aExpression.Split(',');
				List<IValue> exp = new List<IValue>(parts.Length);
				for (int i = 0; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(Parse(s));
				}
				return new MultiParameterList(exp.ToArray());
			}
			else if (aExpression.Contains('+'))
			{
				string[] parts = aExpression.Split('+');
				List<IValue> exp = new List<IValue>(parts.Length);
				for (int i = 0; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(Parse(s));
				}
				if (exp.Count == 1)
					return exp[0];
				return new OperationSum(exp.ToArray());
			}
			else if (aExpression.Contains('-'))
			{
				string[] parts = aExpression.Split('-');
				List<IValue> exp = new List<IValue>(parts.Length);
				if (!string.IsNullOrEmpty(parts[0].Trim()))
					exp.Add(Parse(parts[0]));
				for (int i = 1; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(new OperationNegate(Parse(s)));
				}
				if (exp.Count == 1)
					return exp[0];
				return new OperationSum(exp.ToArray());
			}
			else if (aExpression.Contains('*'))
			{
				string[] parts = aExpression.Split('*');
				List<IValue> exp = new List<IValue>(parts.Length);
				for (int i = 0; i < parts.Length; i++)
				{
					exp.Add(Parse(parts[i]));
				}
				if (exp.Count == 1)
					return exp[0];
				return new OperationProduct(exp.ToArray());
			}
			else if (aExpression.Contains('/'))
			{
				string[] parts = aExpression.Split('/');
				List<IValue> exp = new List<IValue>(parts.Length);
				if (!string.IsNullOrEmpty(parts[0].Trim()))
					exp.Add(Parse(parts[0]));
				for (int i = 1; i < parts.Length; i++)
				{
					string s = parts[i].Trim();
					if (!string.IsNullOrEmpty(s))
						exp.Add(new OperationReciprocal(Parse(s)));
				}
				return new OperationProduct(exp.ToArray());
			}
			else if (aExpression.Contains('^'))
			{
				int pos = aExpression.IndexOf('^');
				var val = Parse(aExpression.Substring(0, pos));
				var pow = Parse(aExpression.Substring(pos + 1));
				return new OperationPower(val, pow);
			}
			int pPos = aExpression.IndexOf("&");
			if (pPos > 0)
			{
				string fName = aExpression.Substring(0, pPos);
				foreach (var M in m_Funcs)
				{
					if (fName == M.Key)
					{
						var inner = aExpression.Substring(M.Key.Length);
						var param = Parse(inner);
						var multiParams = param as MultiParameterList;
						IValue[] parameters;
						if (multiParams != null)
							parameters = multiParams.Parameters;
						else
							parameters = new IValue[] { param };
						return new CustomFunction(M.Key, M.Value, parameters);
					}
				}
			}
			foreach (var C in m_Consts)
			{
				if (aExpression == C.Key)
				{
					return new CustomFunction(C.Key, (p) => C.Value(), null);
				}
			}
			int index2a = aExpression.IndexOf('&');
			int index2b = aExpression.IndexOf(';');
			if (index2a >= 0 && index2b >= 2)
			{
				var inner = aExpression.Substring(index2a + 1, index2b - index2a - 1);
				int bracketIndex;
				if (int.TryParse(inner, out bracketIndex) && bracketIndex >= 0 && bracketIndex < m_BracketHeap.Count)
				{
					return Parse(m_BracketHeap[bracketIndex]);
				}
				else
					throw new ParseException("Can't parse substitude token");
			}
			double doubleValue;
			if (double.TryParse(aExpression, out doubleValue))
			{
				return new Number(doubleValue);
			}
			if (ValidIdentifier(aExpression))
			{
				if (m_Context.Parameters.ContainsKey(aExpression))
					return m_Context.Parameters[aExpression];
				var val = new Parameter(aExpression);
				m_Context.Parameters.Add(aExpression, val);
				return val;
			}

			throw new ParseException("Reached unexpected end within the parsing tree");
		}

		private bool ValidIdentifier(string aExpression)
		{
			aExpression = aExpression.Trim();
			if (string.IsNullOrEmpty(aExpression))
				return false;
			if (aExpression.Length < 1)
				return false;
			if (aExpression.Contains(" "))
				return false;
			if (!"abcdefghijklmnopqrstuvwxyz§$".Contains(char.ToLower(aExpression[0])))
				return false;
			if (m_Consts.ContainsKey(aExpression))
				return false;
			if (m_Funcs.ContainsKey(aExpression))
				return false;
			return true;
		}

		public TGExpression EvaluateExpression(string aExpression)
		{
			var val = new TGExpression();
			m_Context = val;
			val.ExpressionTree = Parse(aExpression);
			m_Context = null;
			m_BracketHeap.Clear();
			return val;
		}

		public double Evaluate(string aExpression)
		{
			return EvaluateExpression(aExpression).Value;
		}
		public static double Eval(string aExpression)
		{
			return new TGExpressionParser().Evaluate(aExpression);
		}

		public class ParseException : System.Exception { public ParseException(string aMessage) : base(aMessage) { } }
	}
}