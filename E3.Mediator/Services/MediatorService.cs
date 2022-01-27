using System;
using System.Collections.Generic;
using System.Windows.Threading;
using E3.Mediator.Models;

namespace E3.Mediator.Services
{
	/// <summary>
	/// Provides loosely-coupled messaging between
	/// various colleagues.  All references to objects
	/// are stored weakly, to prevent memory leaks.
	/// </summary>
	public class MediatorService : DispatcherObject
	{
		readonly MessageToActionsMap _messageToCallbacksMap = new MessageToActionsMap();

		public MediatorService()
		{
			if (base.Dispatcher == null)
				throw new InvalidOperationException("Cannot create Mediator on a thread that does not have a Dispatcher.");
		}

		public void Register(object message, Action<object> callback)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			if (callback == null)
				throw new ArgumentNullException("callback");

			if (callback.Target == null)
				throw new ArgumentException("The 'callback' delegate must reference an instance method.");

			_messageToCallbacksMap.AddAction(message, callback);
		}

		public void NotifyColleagues(object message, object parameter)
		{
			if (base.CheckAccess())
			{
				List<Action<object>> actions =
					_messageToCallbacksMap.GetActions(message);

				if (actions != null)
					actions.ForEach(action => action(parameter));
			}
			else
			{
				base.Dispatcher.BeginInvoke((Action)delegate
				{
					this.NotifyColleagues(message, parameter);
				},
				DispatcherPriority.Send);
			}
		}
	}
}
