using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DotNetXtensions;

/// <summary>
/// Represents a two-way one-to-one mapping between keys and values.
/// Each key maps to exactly one value, and each value maps to exactly one key.
/// This collection is not thread-safe.
/// </summary>
/// <typeparam name="TKey">The type of keys in the map.</typeparam>
/// <typeparam name="TValue">The type of values in the map.</typeparam>
public class BidirectionalDictionary<TKey, TValue> :
	IDictionary<TKey, TValue>,
	IEnumerable<KeyValuePair<TKey, TValue>>
	where TKey : notnull
	where TValue : notnull
{
	private readonly Dictionary<TKey, TValue> _fmap;
	private readonly Dictionary<TValue, TKey> _rmap;

	/// <summary>
	/// True (default) forces the **setter** to not throw when input value being set already exists,
	/// and is mapped to *a different key*. *Force* means: Force this condition to silently update,
	/// no exception if value was set to a different key.
	/// False means: that should never happen, I want an exception, a value should never have a different key
	/// (one could manually remove a key / value beforehand, but this shouldn't happen / be updated silently).
	/// Compared to the Guava's ByMap, `Force` set to true makes the setter act like his `forcePut`, and like his
	/// `put` when false.
	/// <para />
	/// In summary:
	/// `Force`: "I don't care what key owned that value before — evict it (if already exists) and update it to be owned by THIS new key"
	/// `!Force`: "I'm updating this key's mapping, but if this value already existed, it MUST NOT already be set to a
	/// DIFFERENT KEY, scream if they differ"
	/// </summary>
	public bool Force { get; set; } = true;

	/// <summary>
	/// When false, adding or setting will throw if the key or value equals the default value for its type
	/// (e.g. <c>0</c> for <c>int</c>, <c>Guid.Empty</c> for <c>Guid</c>, <c>null</c> for reference types).
	/// Defaults to true — opt-out only.
	/// </summary>
	public bool AllowDefaults { get; set; } = true;

	/// <summary>Constructor</summary>
	public BidirectionalDictionary()
	{
		_fmap = [];
		_rmap = [];
	}

	/// <summary>Constructor accepting key / value comparers.</summary>
	/// <param name="keyComparer">Key comparer, or null to use default for type</param>
	/// <param name="valueComparer">Value comparer, or null to use default for type</param>
	public BidirectionalDictionary(
		IEqualityComparer<TKey>? keyComparer = null,
		IEqualityComparer<TValue>? valueComparer = null)
	{
		_fmap = new Dictionary<TKey, TValue>(keyComparer);
		_rmap = new(valueComparer);
	}

	bool _tvalsEqual(TValue? x, TValue? y) => _rmap.Comparer.Equals(x, y);
	bool _tkeysEqual(TKey? x, TKey? y) => _fmap.Comparer.Equals(x, y);

	void _checkDefaults(TKey key, TValue value)
	{
		//if(!AllowDefaults) return; // <-- or could move this here. better perf tho to ignore a method call on hotspots (Set)
		if(EqualityComparer<TKey>.Default.Equals(key, default!))
			throw new ArgumentException($"Key '{key}' is the default value for its type, which is disallowed.", nameof(key));
		if(EqualityComparer<TValue>.Default.Equals(value, default!))
			throw new ArgumentException($"Value '{value}' is the default value for its type, which is disallowed.", nameof(value));
	}

	/// <summary>
	/// Gets the number of key-value pairs in the map.
	/// </summary>
	public int Count => _fmap.Count;

	/// <summary>
	/// For internal consistency purposes, gets whether the forward and reverse maps have the same count.
	/// Note that exceptions that stop control flow can break this (ie updates are not transactions).
	/// </summary>
	public bool CountsAreEqual => _fmap.Count == _rmap.Count;

	/// <summary>
	/// Gets a collection containing the keys in the map.
	/// </summary>
	public ICollection<TKey> Keys => _fmap.Keys;

	/// <summary>
	/// Gets a collection containing the values in the map.
	/// Takes the values from the forward map.
	/// </summary>
	public ICollection<TValue> Values => _fmap.Values;

	/// <summary>Member required for IDictionary, hardcoded FALSE.</summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// Gets or sets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key to get or set.</param>
	/// <returns>The value associated with the key.</returns>
	/// <exception cref="KeyNotFoundException">The key is not found when getting.</exception>
	/// <exception cref="ArgumentNullException">The key or value is null when setting.</exception>
	public TValue this[TKey key] {
		get => _fmap.TryGetValue(key, out var value)
			 ? value
			 : throw new KeyNotFoundException($"The key '{key}' was not found.");
		set => Set(key, value, Force);
	}

	/// <summary>
	/// Gets the key associated with the specified value.
	/// </summary>
	public TKey this[TValue value] {
		get => GetKey(value);
	}

	/// <summary>
	/// Adds a key-value pair to the map. Indirection to <see cref="Add(TKey, TValue)"/>, which see for further docs.
	/// </summary>
	/// <param name="item">Key value to add</param>
	public void Add(KeyValuePair<TKey, TValue> item)
		=> Add(item.Key, item.Value);

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

		if(!AllowDefaults)
			_checkDefaults(key, value);

		if(_fmap.ContainsKey(key))
			throw new ArgumentException($"Key '{key}' already exists.", nameof(key));

		if(_rmap.ContainsKey(value))
			throw new ArgumentException($"Value '{value}' already exists.", nameof(value));

		_fmap.Add(key, value);
		_rmap.Add(value, key);
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
		if(!AllowDefaults)
			_checkDefaults(key, value);

		if(_fmap.ContainsKey(key) || _rmap.ContainsKey(value))
			return false;

		_fmap.Add(key, value);
		_rmap.Add(value, key);
		return true;
	}

	/// <summary>
	/// The all important setter method. It will update the mapping for the specified key
	/// to the new value, and also update the reverse mapping accordingly. Force rules will
	/// be determined by the current value of the <see cref="Force"/> property, which see for details.
	/// This method is called by the indexer setter.
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="value">Value</param>
	public void Set(TKey key, TValue value)
		=> Set(key, value, Force);

	/// <summary>
	/// The all important setter method. It will update the mapping for the specified key
	/// to the new value, and also update the reverse mapping accordingly.
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="value">Value</param>
	/// <param name="force">See notes on the <see cref="Force"/> property.</param>
	public void Set(TKey key, TValue value, bool force)
	{
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		bool keyExists = _fmap.TryGetValue(key, out TValue? currentValue);
		if(keyExists && _tvalsEqual(value, currentValue))
			return;

		if(!AllowDefaults)
			_checkDefaults(key, value);

		// does the *new* value already exist? AND if so is its key different?
		// IMPORTANT: check conflict BEFORE modifying anything, so a throw leaves the dictionary untouched
		bool valueExistsWithDiffKey = _rmap.TryGetValue(value, out TKey? conflictKey) && !_tkeysEqual(conflictKey, key);
		if(valueExistsWithDiffKey) {
			if(!force)
				throw new ArgumentException($"Value '{value}' is already mapped to key '{conflictKey}'.", nameof(value));
			if(conflictKey != null)
				_fmap.Remove(conflictKey);
		}

		// if keyExists AND == value, RETURNED. So at this point keyExists === keyExists_ValueDiffered, must remove from reverse map
		if(keyExists)
			_rmap.Remove(currentValue!);

		_fmap[key] = value;
		_rmap[value] = key;
	}

	/// <summary>
	/// Attempts to set the mapping for <paramref name="key"/> to <paramref name="value"/>.
	/// Unlike <see cref="Set(TKey, TValue, bool)"/>, this method never throws on a conflict and never
	/// silently evicts a conflicting mapping — instead it returns <see langword="false"/> and leaves
	/// the dictionary completely untouched, giving the caller full knowledge and control.
	/// <para />
	/// Returns <see langword="true"/> when the mapping was set, OR, when it was already set to the same value (!).
	/// Returns <see langword="false"/> when <paramref name="value"/> is already owned by a different key,
	/// in which case <paramref name="conflictKey"/> contains that key and the dictionary is unchanged.
	/// On <see langword="true"/>, <paramref name="conflictKey"/> is always <see langword="default"/>.
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="value">Value</param>
	/// <param name="conflictKey">When this method returns <see langword="false"/>, contains the key
	/// that currently owns <paramref name="value"/>. Always <see langword="default"/> on <see langword="true"/>.</param>
	/// <returns><see langword="true"/> if the mapping was set successfully; <see langword="false"/> if a conflict was detected.</returns>
	public bool TrySet(TKey key, TValue value, out TKey? conflictKey)
	{
		// unfort we have to duplicate much code with Set, would have been nice to fully share, but best in end
		// not to. MUST remember however to sync core Set logic where applicable between these two
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(value);

		bool keyExists = _fmap.TryGetValue(key, out TValue? currentValue);

		if(keyExists && _tvalsEqual(value, currentValue)) {
			conflictKey = default;
			return true;
		}

		// The exact key→value mapping does not exist yet

		if(!AllowDefaults)
			_checkDefaults(key, value);

		// does the *new* value already exist? AND if so is its key different?
		// IMPORTANT: check conflict BEFORE modifying anything, so a false return leaves the dictionary untouched
		bool valueExistsWithDiffKey = _rmap.TryGetValue(value, out conflictKey) && !_tkeysEqual(conflictKey, key);
		if(valueExistsWithDiffKey)
			return false;

		// all checks passed — now commit
		if(keyExists)
			_rmap.Remove(currentValue!);

		conflictKey = default;
		_fmap[key] = value;
		_rmap[value] = key;

		return true;
	}

	/// <summary>
	/// Removes the mapping with the specified key.
	/// </summary>
	/// <param name="key">The key to remove.</param>
	/// <returns>true if the key was found and removed; otherwise, false.</returns>
	public bool RemoveByKey(TKey key)
	{
		if(_fmap.TryGetValue(key, out var value)) {
			_fmap.Remove(key);
			_rmap.Remove(value);
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
		if(_rmap.TryGetValue(value, out var key)) {
			_rmap.Remove(value);
			_fmap.Remove(key);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Removes the mapping with the specified key. Indirection call to <see cref="RemoveByKey(TKey)"/>.
	/// </summary>
	/// <param name="key">The key to remove.</param>
	/// <returns>true if the key was found and removed; otherwise, false.</returns>
	public bool Remove(TKey key)
		=> RemoveByKey(key);

	/// <summary>
	/// Removes the mapping with the specified key OR value.
	/// Member of `IDictionary`, so we call *both*
	/// <see cref="RemoveByKey(TKey)"/> and <see cref="RemoveByValue(TValue)"/>.
	/// </summary>
	/// <param name="item">Key value pair</param>
	/// <returns>True if either map Remove returned true.</returns>
	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		bool keyRemoved = RemoveByKey(item.Key);
		bool valRemoved = RemoveByValue(item.Value);
		return keyRemoved || valRemoved;
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

		return _rmap.TryGetValue(value, out var key)
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

		return _fmap.TryGetValue(key, out TValue? value)
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
		=> _fmap.TryGetValue(key, out value);

	/// <summary>
	/// Attempts to get the key associated with the specified value.
	/// </summary>
	/// <param name="value">The value to look up.</param>
	/// <param name="key">When this method returns, contains the key associated with the value, if found.</param>
	/// <returns>true if the value was found; otherwise, false.</returns>
	public bool TryGetKey(TValue value, [MaybeNullWhen(false)] out TKey key)
		=> _rmap.TryGetValue(value, out key);

	/// <summary>
	/// Determines whether the maps contain the specified key and value.
	/// This member is needed for `IDictionary` implementation. It checks
	/// *both* maps key values, but not each map's value for a match. Hard
	/// decision but that seems the best option, another function can be added
	/// for full consistency checking purposes.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public bool Contains(KeyValuePair<TKey, TValue> item)
		=> ContainsKey(item.Key) && ContainsValue(item.Value);

	/// <summary>
	/// Determines whether the map contains the specified key.
	/// </summary>
	/// <param name="key">The key to locate.</param>
	/// <returns>true if the map contains the key; otherwise, false.</returns>
	public bool ContainsKey(TKey key)
		=> _fmap.ContainsKey(key);

	/// <summary>
	/// Determines whether the map contains the specified value.
	/// </summary>
	/// <param name="value">The value to locate.</param>
	/// <returns>true if the map contains the value; otherwise, false.</returns>
	public bool ContainsValue(TValue value)
		=> _rmap.ContainsKey(value);

	/// <summary>
	/// Removes all mappings from the map.
	/// </summary>
	public void Clear()
	{
		_fmap.Clear();
		_rmap.Clear();
	}

	/// <summary>
	/// Returns an enumerator that iterates through the key-value pairs in the map.
	/// </summary>
	/// <returns>An enumerator for the map.</returns>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		=> _fmap.GetEnumerator();

	/// <summary>
	/// Returns an enumerator that iterates through the key-value pairs in the map.
	/// </summary>
	/// <returns>An enumerator for the map.</returns>
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();


	/// <summary>
	/// Copies the elements of the ICollection to the array, starting at the particular array index.
	/// </summary>
	/// <param name="array">Array to copy to</param>
	/// <param name="arrayIndex">The starting index in array to start copying to</param>
	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		=> ((ICollection<KeyValuePair<TKey, TValue>>)_fmap).CopyTo(array, arrayIndex);

	/// <summary>
	/// Adds all entries from <paramref name="source"/> using the current <see cref="Force"/> property.
	/// Equivalent to calling <see cref="AddRange(IEnumerable{KeyValuePair{TKey,TValue}}, bool)"/> with <see cref="Force"/>.
	/// </summary>
	public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> source)
		=> AddRange(source, Force);

	/// <summary>
	/// Adds all entries from <paramref name="source"/>.
	/// When <paramref name="force"/> is <see langword="true"/>, value conflicts silently evict the existing mapping (calls <see cref="Set(TKey, TValue, bool)"/>).
	/// When <paramref name="force"/> is <see langword="false"/>, any duplicate key or value throws <see cref="ArgumentException"/> (calls <see cref="Add"/>).
	/// </summary>
	public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> source, bool force)
	{
		ArgumentNullException.ThrowIfNull(source);
		foreach(var kvp in source)
			if(!force)
				Add(kvp.Key, kvp.Value);
			else
				Set(kvp.Key, kvp.Value, true);
	}

	/// <summary>
	/// Adds all entries from <paramref name="source"/> using <see cref="TrySet"/> semantics — never throws, never evicts.
	/// Entries whose value is already owned by a different key are skipped and collected into
	/// <paramref name="conflicts"/> as <see cref="KVConflict{TKey,TValue}"/> entries,
	/// or <see langword="null"/> if there were none.
	/// </summary>
	public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> source, out List<KVConflict<TKey, TValue>>? conflicts)
	{
		ArgumentNullException.ThrowIfNull(source);
		conflicts = null;
		foreach(var kvp in source) {
			if(!TrySet(kvp.Key, kvp.Value, out TKey? conflictKey)) {
				conflicts ??= [];
				conflicts.Add(new KVConflict<TKey, TValue>(kvp.Key, kvp.Value, conflictKey!));
			}
		}
	}
}

/// <summary>
/// Extension methods for creating a <see cref="BidirectionalDictionary{TKey, TValue}"/> from existing sequences.
/// </summary>
public static class BidirectionalDictionaryX
{
	/// <summary>
	/// Creates a <see cref="BidirectionalDictionary{TKey, TValue}"/> from <paramref name="source"/>.
	/// By default any duplicate key or value throws (<paramref name="force"/> = <see langword="false"/>).
	/// Set <paramref name="force"/> to <see langword="true"/> to silently evict conflicting mappings
	/// (last in wins).
	/// If you want to skip conflicts but capture them for inspection, use the <c>out conflicts</c> overload.
	/// <para/>
	/// Note: <paramref name="force"/> sets the <see cref="BidirectionalDictionary{TKey,TValue}.Force"/> property,
	/// but if so desired, this can be changed after the fact on the returned dictionary.
	/// </summary>
	/// <param name="source">Source key-value pairs.</param>
	/// <param name="force">When <see langword="true"/>, key value conflicts silently evict the existing mapping.
	/// When <see langword="false"/> (default), any duplicate key or value throws.</param>
	/// <param name="allowDefaults">Sets the <see cref="BidirectionalDictionary{TKey,TValue}.AllowDefaults"/> property
	/// (else that defaults to its normal default).</param>
	/// <param name="keyComparer">Key comparer, or null to use the default for the type.</param>
	public static BidirectionalDictionary<TKey, TValue> ToBidirectionalDictionary<TKey, TValue>(
		this IEnumerable<KeyValuePair<TKey, TValue>> source,
		bool force = false,
		bool? allowDefaults = null,
		IEqualityComparer<TKey>? keyComparer = null)
		where TKey : notnull
		where TValue : notnull
	{
		BidirectionalDictionary<TKey, TValue> map = new(keyComparer) { Force = force };
		if(allowDefaults.HasValue)
			map.AllowDefaults = allowDefaults.Value;
		map.AddRange(source, force);
		return map;
	}

	/// <summary>
	/// Creates a <see cref="BidirectionalDictionary{TKey, TValue}"/> from <paramref name="source"/>
	/// using <see cref="BidirectionalDictionary{TKey,TValue}.TrySet"/> semantics — never throws, never evicts.
	/// Entries whose value is already owned by a different key are skipped and collected into
	/// <paramref name="conflicts"/> as <see cref="KVConflict{TKey,TValue}"/> entries,
	/// or <see langword="null"/> if there were none. So for conflicts, first in wins.
	/// The returned map has <see cref="BidirectionalDictionary{TKey,TValue}.Force"/> set to <see langword="true"/>.
	/// </summary>
	/// <param name="source">Source key-value pairs.</param>
	/// <param name="conflicts">The skipped conflicts, or <see langword="null"/> if none.</param>
	/// <param name="allowDefaults">Sets the <see cref="BidirectionalDictionary{TKey,TValue}.AllowDefaults"/> property
	/// (else that defaults to its normal default).</param>
	/// <param name="keyComparer">Key comparer, or null to use the default for the type.</param>
	public static BidirectionalDictionary<TKey, TValue> ToBidirectionalDictionary<TKey, TValue>(
		this IEnumerable<KeyValuePair<TKey, TValue>> source,
		out List<KVConflict<TKey, TValue>>? conflicts,
		bool? allowDefaults = null,
		IEqualityComparer<TKey>? keyComparer = null)
		where TKey : notnull
		where TValue : notnull
	{
		BidirectionalDictionary<TKey, TValue> map = new(keyComparer) { Force = true };
		if(allowDefaults.HasValue)
			map.AllowDefaults = allowDefaults.Value;
		map.AddRange(source, out conflicts);
		return map;
	}

	/// <summary>
	/// Creates a <see cref="BidirectionalDictionary{TKey, TValue}"/> from <paramref name="source"/>
	/// using key and value selector functions.
	/// By default any duplicate key or value throws (<paramref name="force"/> = <see langword="false"/>).
	/// Set <paramref name="force"/> to <see langword="true"/> to silently evict conflicting mappings
	/// (last in wins).
	/// </summary>
	/// <param name="source">Source sequence.</param>
	/// <param name="keySelector">Function to extract the key from each element.</param>
	/// <param name="valueSelector">Function to extract the value from each element.</param>
	/// <param name="force">When <see langword="true"/>, key value conflicts silently evict the existing mapping.
	/// When <see langword="false"/> (default), any duplicate key or value throws.</param>
	/// <param name="allowDefaults">Sets the <see cref="BidirectionalDictionary{TKey,TValue}.AllowDefaults"/> property
	/// (else that defaults to its normal default).</param>
	/// <param name="keyComparer">Key comparer, or null to use the default for the type.</param>
	public static BidirectionalDictionary<TKey, TValue> ToBidirectionalDictionary<T, TKey, TValue>(
		this IEnumerable<T> source,
		Func<T, TKey> keySelector,
		Func<T, TValue> valueSelector,
		bool force = false,
		bool? allowDefaults = null,
		IEqualityComparer<TKey>? keyComparer = null)
		where TKey : notnull
		where TValue : notnull
	{
		BidirectionalDictionary<TKey, TValue> map = new(keyComparer) { Force = force };
		if(allowDefaults.HasValue)
			map.AllowDefaults = allowDefaults.Value;
		map.AddRange(source.Select(item => new KeyValuePair<TKey, TValue>(keySelector(item), valueSelector(item))), force);
		return map;
	}

	/// <summary>
	/// Creates a <see cref="BidirectionalDictionary{TKey, TValue}"/> from <paramref name="source"/>
	/// using key and value selector functions, with <see cref="BidirectionalDictionary{TKey,TValue}.TrySet"/> semantics —
	/// never throws, never evicts. Entries whose value is already owned by a different key are skipped and
	/// collected into <paramref name="conflicts"/>, or <see langword="null"/> if there were none.
	/// The returned map has <see cref="BidirectionalDictionary{TKey,TValue}.Force"/> set to <see langword="true"/>.
	/// </summary>
	/// <param name="source">Source sequence.</param>
	/// <param name="keySelector">Function to extract the key from each element.</param>
	/// <param name="valueSelector">Function to extract the value from each element.</param>
	/// <param name="conflicts">The skipped conflicts, or <see langword="null"/> if none.</param>
	/// <param name="allowDefaults">Sets the <see cref="BidirectionalDictionary{TKey,TValue}.AllowDefaults"/> property
	/// (else that defaults to its normal default).</param>
	/// <param name="keyComparer">Key comparer, or null to use the default for the type.</param>
	public static BidirectionalDictionary<TKey, TValue> ToBidirectionalDictionary<T, TKey, TValue>(
		this IEnumerable<T> source,
		Func<T, TKey> keySelector,
		Func<T, TValue> valueSelector,
		out List<KVConflict<TKey, TValue>>? conflicts,
		bool? allowDefaults = null,
		IEqualityComparer<TKey>? keyComparer = null)
		where TKey : notnull
		where TValue : notnull
	{
		BidirectionalDictionary<TKey, TValue> map = new(keyComparer) { Force = true };
		if(allowDefaults.HasValue)
			map.AllowDefaults = allowDefaults.Value;
		map.AddRange(source.Select(item => new KeyValuePair<TKey, TValue>(keySelector(item), valueSelector(item))), out conflicts);
		return map;
	}
}

/// <summary>
/// Represents a value conflict encountered when bulk-loading a <see cref="BidirectionalDictionary{TKey,TValue}"/>.
/// <c>Value</c> was already owned by <c>ConflictKey</c>, so the entry <c>Key→Value</c> was skipped.
/// </summary>
public readonly record struct KVConflict<TKey, TValue>(TKey Key, TValue Value, TKey ConflictKey)
	where TKey : notnull
	where TValue : notnull;
