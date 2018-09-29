using System.Collections.Generic;
using System.Linq;

namespace TG
{
	public class TGExpression : IValue
	{
		public Dictionary<string, Parameter> Parameters = new Dictionary<string, Parameter>();
		public IValue ExpressionTree { get; set; }
		public double Value
		{
			get { return ExpressionTree.Value; }
		}
		public double[] MultiValue
		{
			get
			{
				var t = ExpressionTree as MultiParameterList;
				if (t != null)
				{
					double[] res = new double[t.Parameters.Length];
					for (int i = 0; i < res.Length; i++)
						res[i] = t.Parameters[i].Value;
					return res;
				}
				return null;
			}
		}
		public override string ToString()
		{
			return ExpressionTree.ToString();
		}
		public ExpressionDelegate ToDelegate(params string[] aParamOrder)
		{
			var parameters = new List<Parameter>(aParamOrder.Length);
			for (int i = 0; i < aParamOrder.Length; i++)
			{
				if (Parameters.ContainsKey(aParamOrder[i]))
					parameters.Add(Parameters[aParamOrder[i]]);
				else
					parameters.Add(null);
			}
			var parameters2 = parameters.ToArray();

			return (p) => Invoke(p, parameters2);
		}
		public MultiResultDelegate ToMultiResultDelegate(params string[] aParamOrder)
		{
			var parameters = new List<Parameter>(aParamOrder.Length);
			for (int i = 0; i < aParamOrder.Length; i++)
			{
				if (Parameters.ContainsKey(aParamOrder[i]))
					parameters.Add(Parameters[aParamOrder[i]]);
				else
					parameters.Add(null);
			}
			var parameters2 = parameters.ToArray();

			return (p) => InvokeMultiResult(p, parameters2);
		}
		double Invoke(double[] aParams, Parameter[] aParamList)
		{
			int count = System.Math.Min(aParamList.Length, aParams.Length);
			for (int i = 0; i < count; i++)
			{
				if (aParamList[i] != null)
					aParamList[i].Value = aParams[i];
			}
			return Value;
		}
		double[] InvokeMultiResult(double[] aParams, Parameter[] aParamList)
		{
			int count = System.Math.Min(aParamList.Length, aParams.Length);
			for (int i = 0; i < count; i++)
			{
				if (aParamList[i] != null)
					aParamList[i].Value = aParams[i];
			}
			return MultiValue;
		}
		public static TGExpression Parse(string aExpression)
		{
			return new TGExpressionParser().EvaluateExpression(aExpression);
		}

		public class ParameterException : System.Exception { public ParameterException(string aMessage) : base(aMessage) { } }
	}
	public delegate double ExpressionDelegate(params double[] aParams);
	public delegate double[] MultiResultDelegate(params double[] aParams);

}