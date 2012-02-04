namespace LocalReportsEngine
{
    using System;

    /// <summary>
    /// Provides data for the Resolve event.
    /// </summary>
    /// <typeparam name="TKey">The type of the key of the resolvable item.</typeparam>
    /// <typeparam name="TValue">The type of the resolvable item.</typeparam>
    [Serializable]
    internal class ResolvableEventArgs<TKey, TValue> : EventArgs
    {
        /// <summary>
        /// The key of the item to be resolved.
        /// </summary>
        public readonly TKey ResolvingKey;

        /// <summary>
        /// Initializes a new instance of the ResolvableEventArgs class.
        /// </summary>
        /// <param name="key">They key of the item to be resolved.</param>
        public ResolvableEventArgs(TKey key)
        {
            ResolvingKey = key;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the item has been resolved.
        /// </summary>
        /// <remarks>The Resolve event will stop processing after the item is resolved.</remarks>
        public bool IsResolved { get; set; }

        /// <summary>
        /// Gets or sets the item that has been resolved.
        /// </summary>
        public TValue ResolvedItem { get; set; }
    }
}