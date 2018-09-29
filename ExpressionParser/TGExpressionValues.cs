using System.Collections.Generic;
using System.Linq;

namespace TG
{
	public interface IValue
	{
		double Value { get; }
	}
	public class Number : IValue
	{
		private double m_Value;
		public double Value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}
		public Number(double aValue)
		{
			m_Value = aValue;
		}
		public override string ToString()
		{
			return "" + m_Value + "";
		}
	}
	public class OperationSum : IValue
	{
		private IValue[] m_Values;
		public double Value
		{
			get { return m_Values.Select(v => v.Value).Sum(); }
		}
		public OperationSum(params IValue[] aValues)
		{
			// collapse unnecessary nested sum operations.
			List<IValue> v = new List<IValue>(aValues.Length);
			foreach (var I in aValues)
			{
				var sum = I as OperationSum;
				if (sum == null)
					v.Add(I);
				else
					v.AddRange(sum.m_Values);
			}
			m_Values = v.ToArray();
		}
		public override string ToString()
		{
			return "( " + string.Join(" + ", m_Values.Select(v => v.ToString()).ToArray()) + " )";
		}
	}
	public class OperationProduct : IValue
	{
		private IValue[] m_Values;
		public double Value
		{
			get { return m_Values.Select(v => v.Value).Aggregate((v1, v2) => v1 * v2); }
		}
		public OperationProduct(params IValue[] aValues)
		{
			m_Values = aValues;
		}
		public override string ToString()
		{
			return "( " + string.Join(" * ", m_Values.Select(v => v.ToString()).ToArray()) + " )";
		}

	}
	public class OperationPower : IValue
	{
		private IValue m_Value;
		private IValue m_Power;
		public double Value
		{
			get { return System.Math.Pow(m_Value.Value, m_Power.Value); }
		}
		public OperationPower(IValue aValue, IValue aPower)
		{
			m_Value = aValue;
			m_Power = aPower;
		}
		public override string ToString()
		{
			return "( " + m_Value + "^" + m_Power + " )";
		}

	}
	public class OperationNegate : IValue
	{
		private IValue m_Value;
		public double Value
		{
			get { return -m_Value.Value; }
		}
		public OperationNegate(IValue aValue)
		{
			m_Value = aValue;
		}
		public override string ToString()
		{
			return "( -" + m_Value + " )";
		}

	}
	public class OperationReciprocal : IValue
	{
		private IValue m_Value;
		public double Value
		{
			get { return 1.0 / m_Value.Value; }
		}
		public OperationReciprocal(IValue aValue)
		{
			m_Value = aValue;
		}
		public override string ToString()
		{
			return "( 1/" + m_Value + " )";
		}

	}

	public class MultiParameterList : IValue
	{
		private IValue[] m_Values;
		public IValue[] Parameters { get { return m_Values; } }
		public double Value
		{
			get { return m_Values.Select(v => v.Value).FirstOrDefault(); }
		}
		public MultiParameterList(params IValue[] aValues)
		{
			m_Values = aValues;
		}
		public override string ToString()
		{
			return string.Join(", ", m_Values.Select(v => v.ToString()).ToArray());
		}
	}

	public class CustomFunction : IValue
	{
		private IValue[] m_Params;
		private System.Func<double[], double> m_Delegate;
		private string m_Name;
		public double Value
		{
			get
			{
				if (m_Params == null)
					return m_Delegate(null);
				return m_Delegate(m_Params.Select(p => p.Value).ToArray());
			}
		}
		public CustomFunction(string aName, System.Func<double[], double> aDelegate, params IValue[] aValues)
		{
			m_Delegate = aDelegate;
			m_Params = aValues;
			m_Name = aName;
		}
		public override string ToString()
		{
			if (m_Params == null)
				return m_Name;
			return m_Name + "( " + string.Join(", ", m_Params.Select(v => v.ToString()).ToArray()) + " )";
		}
	}
	public class Parameter : Number
	{
		public string Name { get; private set; }
		public override string ToString()
		{
			return Name + "[" + base.ToString() + "]";
		}
		public Parameter(string aName) : base(0)
		{
			Name = aName;
		}
	}
}