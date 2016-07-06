using System;
using Newtonsoft.Json;

namespace TodoItemWithEncryptedLocalStore
{
	public class ToDoItemWrapper : Java.Lang.Object
	{
		public ToDoItemWrapper (TodoItem item)
		{
			ToDoItem = item;
		}

		public TodoItem ToDoItem { get; private set; }
	}
}

