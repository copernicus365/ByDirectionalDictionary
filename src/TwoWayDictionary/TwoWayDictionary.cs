using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TwoWayDictionary
{
    /// <summary>
    /// Represents a two-way one-to-one mapping between keys and values.
    /// Each key maps to exactly one value, and each value maps to exactly one key.
    /// This collection is not thread-safe.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the map.</typeparam>
    /// <typeparam name="TValue">The type of values in the map.</typeparam>
    public class TwoWayDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _forwardMap = [];
        private readonly Dictionary<TValue, TKey> _reverseMap = [];

        /// <summary>
        /// Gets the number of key-value pairs in the map.
        /// </summary>
        public int Count => _forwardMap.Count;

        /// <summary>
        /// Gets a collection containing the keys in the map.
        /// </summary>
        public ICollection<TKey> Keys => _forwardMap.Keys;

        /// <summary>
        /// Gets a collection containing the values in the map.
        /// </summary>
        public ICollection<TValue> Values => _forwardMap.Values;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to get or set.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="KeyNotFoundException">The key is not found when getting.</exception>
        /// <exception cref="ArgumentNullException">The key or value is null when setting.</exception>
        public TValue this[TKey key]
        {
            get => _forwardMap.TryGetValue(key, out var value)
                ? value
                : throw new KeyNotFoundException($"The key '{key}' was not found.");
            set => Set(key, value);
        }

        /// <summary>
        /// Adds a key-value pair to the map.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        /// <exception cref="ArgumentException">The key or value already exists in the map.</exception>
        /// <exception cref="ArgumentNullException">The key or value is null.</exception>
        public void Add(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            if (_forwardMap.ContainsKey(key))
                throw new ArgumentException($"Key '{key}' already exists.", nameof(key));

            if (_reverseMap.ContainsKey(value))
                throw new ArgumentException($"Value '{value}' already exists.", nameof(value));

            _forwardMap.Add(key, value);
            _reverseMap.Add(value, key);
        }

        /// <summary>
        /// Attempts to add a key-value pair to the map.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>true if the pair was added successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">The key or value is null.</exception>
        public bool TryAdd(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            if (_forwardMap.ContainsKey(key) || _reverseMap.ContainsKey(value))
                return false;

            _forwardMap.Add(key, value);
            _reverseMap.Add(value, key);
            return true;
        }

        /// <summary>
        /// Sets the value for the specified key, removing any existing mappings if necessary.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException">The key or value is null.</exception>
        public void Set(TKey key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            // Check if this exact mapping already exists
            if (_forwardMap.TryGetValue(key, out var existingValue) &&
                EqualityComparer<TValue>.Default.Equals(existingValue, value))
            {
                return; // No change needed
            }

            // Remove existing mappings
            RemoveByKey(key);
            RemoveByValue(value);

            // Add the new mapping
            _forwardMap[key] = value;
            _reverseMap[value] = key;
        }

        /// <summary>
        /// Gets the key associated with the specified value.
        /// </summary>
        /// <param name="value">The value to look up.</param>
        /// <returns>The key associated with the value.</returns>
        /// <exception cref="KeyNotFoundException">The value is not found.</exception>
        /// <exception cref="ArgumentNullException">The value is null.</exception>
        public TKey GetKey(TValue value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return _reverseMap.TryGetValue(value, out var key)
                ? key
                : throw new KeyNotFoundException($"The value '{value}' was not found.");
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="KeyNotFoundException">The key is not found.</exception>
        /// <exception cref="ArgumentNullException">The key is null.</exception>
        public TValue GetValue(TKey key)
        {
            ArgumentNullException.ThrowIfNull(key);

            return _forwardMap.TryGetValue(key, out var value)
                ? value
                : throw new KeyNotFoundException($"The key '{key}' was not found.");
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">When this method returns, contains the value associated with the key, if found.</param>
        /// <returns>true if the key was found; otherwise, false.</returns>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _forwardMap.TryGetValue(key, out value);
        }

        /// <summary>
        /// Attempts to get the key associated with the specified value.
        /// </summary>
        /// <param name="value">The value to look up.</param>
        /// <param name="key">When this method returns, contains the key associated with the value, if found.</param>
        /// <returns>true if the value was found; otherwise, false.</returns>
        public bool TryGetKey(TValue value, [MaybeNullWhen(false)] out TKey key)
        {
            return _reverseMap.TryGetValue(value, out key);
        }

        /// <summary>
        /// Removes the mapping with the specified key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>true if the key was found and removed; otherwise, false.</returns>
        public bool RemoveByKey(TKey key)
        {
            if (_forwardMap.TryGetValue(key, out var value))
            {
                _forwardMap.Remove(key);
                _reverseMap.Remove(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the mapping with the specified value.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>true if the value was found and removed; otherwise, false.</returns>
        public bool RemoveByValue(TValue value)
        {
            if (_reverseMap.TryGetValue(value, out var key))
            {
                _reverseMap.Remove(value);
                _forwardMap.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the mapping with the specified key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>true if the key was found and removed; otherwise, false.</returns>
        public bool Remove(TKey key) => RemoveByKey(key);

        /// <summary>
        /// Determines whether the map contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>true if the map contains the key; otherwise, false.</returns>
        public bool ContainsKey(TKey key) => _forwardMap.ContainsKey(key);

        /// <summary>
        /// Determines whether the map contains the specified value.
        /// </summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>true if the map contains the value; otherwise, false.</returns>
        public bool ContainsValue(TValue value) => _reverseMap.ContainsKey(value);

        /// <summary>
        /// Removes all mappings from the map.
        /// </summary>
        public void Clear()
        {
            _forwardMap.Clear();
            _reverseMap.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the key-value pairs in the map.
        /// </summary>
        /// <returns>An enumerator for the map.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _forwardMap.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the key-value pairs in the map.
        /// </summary>
        /// <returns>An enumerator for the map.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
