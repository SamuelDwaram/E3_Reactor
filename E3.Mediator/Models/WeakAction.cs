using System;
using System.Reflection;

namespace E3.Mediator.Models
{
	/// <summary>
	/// This class is an implementation detail of the MessageToActionsMap class.
	/// </summary>
	internal class WeakAction : WeakReference
	{
		readonly MethodInfo _method;

		internal WeakAction(Action<object> action)
			: base(action.Target)
		{
			_method = action.Method;
		}

		internal MethodInfo GetMethod()
		{
			return _method;
		}

		internal Action<object> CreateAction()
		{
			if (!base.IsAlive)
				return null;

			try
			{
				// Rehydrate into a real Action
				// object, so that the method
				// can be invoked on the target.

				return Delegate.CreateDelegate(
					typeof(Action<object>),
					base.Target,
					_method.Name)
					as Action<object>;
			}
			catch
			{
				return null;
			}
		}
	}

}
