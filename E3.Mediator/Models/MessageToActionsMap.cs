using System;
using System.Collections.Generic;
using System.Linq;

namespace E3.Mediator.Models
{
	/// <summary>
	/// This class is an implementation detail of the Mediator class.
	/// </summary>
	internal class MessageToActionsMap
	{
		#region Fields

		readonly Dictionary<object, List<WeakAction>> _map;

		#endregion // Fields

		#region Constructor

		internal MessageToActionsMap()
		{
			_map = new Dictionary<object, List<WeakAction>>();
		}

		#endregion // Constructor

		#region Methods

		internal void AddAction(object message, Action<object> callback)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			if (callback == null)
				throw new ArgumentNullException("callback");

			if (!_map.ContainsKey(message))
				_map[message] = new List<WeakAction>();

			if (_map[message].Any(cb => cb.GetMethod() == callback.Method))
			{
				return;
			}

			_map[message].Add(new WeakAction(callback));
		}

		internal List<Action<object>> GetActions(object message)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			if (!_map.ContainsKey(message))
				return null;

			List<WeakAction> weakActions = _map[message];
			List<Action<object>> actions = new List<Action<object>>();
			for (int i = weakActions.Count - 1; i > -1; --i)
			{
				WeakAction weakAction = weakActions[i];
				if (!weakAction.IsAlive)
					weakActions.RemoveAt(i);
				else
					actions.Add(weakAction.CreateAction());
			}

			this.RemoveMessageIfNecessary(weakActions, message);

			return actions;
		}

		void RemoveMessageIfNecessary(List<WeakAction> weakActions, object message)
		{
			if (weakActions.Count == 0)
				_map.Remove(message);
		}

		#endregion // Methods
	}
}
