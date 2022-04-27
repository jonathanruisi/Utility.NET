// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ILinkedList.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 5:07 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Collections.Generic;

namespace JLR.Utility.NETFramework.Collections.LinkedList
{
	public interface ILinkedList<T> : IList<T>
	{
		// Properties
		LinkedListNode<T> First { get; }
		LinkedListNode<T> Last  { get; }

		// Indexers
		LinkedListNode<T> this[T item] { get; set; }

		// Methods
		void AddFirst(LinkedListNode<T> newNode);
		void AddLast(LinkedListNode<T> newNode);
		void InsertBefore(LinkedListNode<T> node, LinkedListNode<T> newNode);
		void InsertAfter(LinkedListNode<T> node, LinkedListNode<T> newNode);
		void Replace(LinkedListNode<T> node, LinkedListNode<T> newNode);
		void RemoveFirst();
		void RemoveLast();
		bool Remove(LinkedListNode<T> node);
		LinkedListNode<T> AddFirst(T value);
		LinkedListNode<T> AddLast(T value);
		LinkedListNode<T> InsertBefore(LinkedListNode<T> node, T value);
		LinkedListNode<T> InsertAfter(LinkedListNode<T> node, T value);
		LinkedListNode<T> Replace(LinkedListNode<T> node, T value);
		LinkedListNode<T> Find(T value);
		LinkedListNode<T> FindLast(T value);
	}
}