namespace LocalReportsEngine
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class Resolvable<TKey, TValue> : MarshalByRefObject
    {
        protected Dictionary<TKey, TValue> ResolvedItems { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Resolvable()
            : this(null)
        {
        }

        public Resolvable(EventHandler<ResolvableEventArgs<TKey, TValue>> defaultResolve)
            : this(defaultResolve, null)
        {
        }

        public Resolvable(EventHandler<ResolvableEventArgs<TKey, TValue>> defaultResolve, IEqualityComparer<TKey> comparer)
        {
            if (defaultResolve != null)
                Resolve += defaultResolve;

            if (comparer != null)
                ResolvedItems = new Dictionary<TKey, TValue>(comparer);
            else
                ResolvedItems = new Dictionary<TKey, TValue>();
        }

        public event EventHandler<ResolvableEventArgs<TKey, TValue>> Resolve;

        protected void OnResolve(ResolvableEventArgs<TKey, TValue> args)
        {
            var resolveEvent = Resolve;
            if (resolveEvent == null)
                return;

            foreach(EventHandler<ResolvableEventArgs<TKey, TValue>> invoke in resolveEvent.GetInvocationList())
            {
                invoke(this, args);
                if (args.IsResolved)
                    break;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key of the item being resolved.</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                // Cache hit?
                TValue value;
                if (ResolvedItems.TryGetValue(key, out value))
                    return value;

                // Cache miss
                var args = new ResolvableEventArgs<TKey, TValue>(key);

                OnResolve(args);

                if (args.IsResolved)
                {
                    ResolvedItems.Add(args.ResolvingKey, args.ResolvedItem);
                    return args.ResolvedItem;
                }
            
                throw new ArgumentOutOfRangeException("key", key, "Unable to resolve");
            }
        }

        public void ForEachResolved(Action<KeyValuePair<TKey, TValue>> action)
        {
            foreach (var kvp in ResolvedItems)
                action(kvp);
        }
    }
}