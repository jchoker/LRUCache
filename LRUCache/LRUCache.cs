using System;
using System.Collections.Generic;

namespace Choker
{
    /// <summary>
    /// An implementation of a generic LRU cache data structure using a doubly linkedlist and a hash-map.
    /// The mru (most recently used) entry is always stored right after a dummy head node while the lru entry is stored right before a dummy tail node of the list.
    /// Linked list node is implemented in an inner class inside the Cache as it will be only used by the cache as an implementation detail.
    /// Both Get & Put methods run in constant time.
    /// </summary>
    /// <date>11.02.20</date>
    /// <author>Jalal Choker, jalal.choker@gmail.com</author>

    public class LRUCache<TKey, TValue>
                where TKey : IEquatable<TKey>
                where TValue : IEquatable<TValue>
    {
        public LRUCache() : this(DefaultCapacity) { }

        public LRUCache(int capacity)
        {
            this.Capacity = capacity <= 0 ? throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be strictly positive") : capacity;
            this.Count = 0;

            this.lookup = new Dictionary<TKey, ListNode>(capacity);

            this.head = new ListNode();
            this.tail = new ListNode();
            this.head.Next = tail;
            this.tail.Previous = head;
        }

        public const int DefaultCapacity = 100;

        public int Capacity { get; private set; }
        public int Count { get; private set; }
        public bool IsEmpty => this.Count == 0;
        public bool IsFull => this.Count == this.Capacity;

        readonly Dictionary<TKey, ListNode> lookup; // constant time key to node lookup
        readonly ListNode head, tail; // O(1) add/update/delete using lookup

        public void Put(TKey key, TValue value)
        {
            if (lookup.TryGetValue(key, out var node))
            {
                if (!node.Value.Equals(value))
                    node.Value = value;

                MoveToHead(node);
            }
            else
            {
                if (this.IsFull) RemoveLRU(); // remove node tail.Previous

                node = new ListNode(key, value);
                this.lookup.Add(key, node);
                AddNode(node);
                this.Count++;
            }
        }

        void MoveToHead(ListNode node)
        {
            RemoveNode(node);

            AddNode(node);
        }

        static void RemoveNode(ListNode node)
        {
            var prv = node.Previous;
            var nxt = node.Next;
            prv.Next = nxt;
            nxt.Previous = prv;
        }

        // adds a node right after head
        void AddNode(ListNode node)
        {
            node.Next = this.head.Next;
            node.Next.Previous = node;
            head.Next = node;
            node.Previous = this.head;
        }

        void RemoveLRU()
        {
            var lru = tail.Previous;

            lookup.Remove(lru.Key);

            RemoveNode(lru);

            this.Count--;
        }

        public TValue Get(TKey key)
        {
            if (lookup.TryGetValue(key, out var node))
            {
                MoveToHead(node);
                return node.Value; ;
            }
            throw new KeyNotFoundException($"{key}");
        }

        public void IncreaseSize(int newCapacity)
        {
            if (newCapacity <= this.Capacity) throw new ArgumentException("New capacity must be greater than current capacity", nameof(newCapacity));
            this.Capacity = newCapacity;
        }

        public TKey MRU => !this.IsEmpty ? head.Next.Key : throw new InvalidOperationException("Cache is empty");
        public TKey LRU => !this.IsEmpty ? tail.Previous.Key : throw new InvalidOperationException("Cache is empty");


        // a custom convenience doubly linked list node
        private class ListNode
        {
            internal ListNode() : this(default, default) { } // serves the purpose of creating dummy head and tail

            internal ListNode(TKey key, TValue value, ListNode previous = null, ListNode next = null)
            {
                this.Key = key;
                this.Value = value;
                this.Previous = previous;
                this.Next = next;
            }

            public TKey Key { get; } // readonly
            public TValue Value { get; set; }
            public ListNode Previous { get; set; }
            public ListNode Next { get; set; }
        }
    }
}