# BidirectionalDictionary

[![NuGet](https://img.shields.io/nuget/v/DotNetXtensions.BidirectionalDictionary.svg)](https://www.nuget.org/packages/DotNetXtensions.BidirectionalDictionary)

A high-performance bidirectional dictionary implementation for C# that maintains one-to-one relationships between keys and values, allowing O(1) lookups in both directions. A carefully thought out implementation, amongst which is a configurable setter behavior via the `Force` property: controls whether setting a `key`'s value silently evicts any conflicting reverse mapping (force mode, the default, ie the more lenient), or throws when `value` is already owned by a different `key`, aligned with important other BiMap libraries, like Guava's (Java based) `BiMap`. An `AllowDefaults` property (default: `true`) optionally rejects keys or values that equal their type's default — `0` for `int`, `Guid.Empty` for `Guid`, etc.

## Acknowledgments

This library is a fork of [ashishkarn](https://github.com/ashishkarn)'s excellent [TwoWayDictionary](https://github.com/ashishkarn/TwoWayDictionary) — "a robust, reusable bidirectional mapping solution for the C# community". Key modifications include: configurable setter behavior via `Force`/`Set(key, value, force)`, an optimized `Set` implementation, the reverse indexer `this[TValue]`, custom comparer support, `AllowDefaults` guard, implements `IDictionary<TKey, TValue>` now, etc.

## Features

- **Bidirectional Lookups**: Retrieve values by keys or keys by values with O(1) time complexity
- **One-to-One Mapping**: Enforces unique keys and unique values - each key maps to exactly one value and vice versa
- **Type-Safe**: Generic implementation with compile-time type checking
- **Configurable Setter Behavior**: The `Force` property controls whether conflicting reverse mappings are silently evicted or cause an exception
- **Default Value Guard**: The `AllowDefaults` property (default: `true`) can be set to `false` to reject keys or values equal to their type's default (`0`, `Guid.Empty`, `false`, etc.)
- **Comprehensive API**: Support for Add, Set, Remove, Contains, and Try* variants
- **Bulk Loading**: `AddRange` and `ToBidirectionalDictionary` extension methods convert any existing dictionary or sequence, with three conflict strategies: throw, force-evict, or collect conflicts via `KVConflict<TKey, TValue>`
- **IEnumerable Support**: Iterate through key-value pairs with foreach
- **Well-Tested**: Comprehensive unit test coverage, 100% passing
- **Zero Dependencies**: No external dependencies beyond .NET 8.0+
- **Multi-Target**: Supports .NET 8.0 and .NET 10.0

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package DotNetXtensions.BidirectionalDictionary
```

Or via Package Manager Console:

```powershell
Install-Package DotNetXtensions.BidirectionalDictionary
```

## Quick Start

```csharp
using DotNetXtensions;

// Create a two-way dictionary
BidirectionalDictionary<int, string> map = [];

// Add. Like with normal Dictionary, checks for conflicts. Throws if key OR value already exists
map.Add(101, "Alice");
map.Add(102, "Bob");

// Indexer (=Set()). Add is a bit lighter when you know you are adding new entries
map[103] = "Charlie";

// Indexer call above is an indirection call to, and thus is identical, to:
map.Set(103, "Charlie", Force)`).

// Set. Can be an addition (new key/value), but also can be an Update
map.Set(120, "Sophie");

// Forward lookup (key -> value)
string name = map[101];  // "Alice"

// Reverse lookup (value -> key)
int id = map["Bob"];         // 102 (reverse indexer)
int id2 = map.GetKey("Bob"); // 102 (explicit method)

// Check existence
bool hasUser = map.ContainsKey(101);      // true
bool hasName = map.ContainsValue("Bob");  // true

// Remove mappings
map.RemoveByKey(102);
// or
map.RemoveByValue("Charlie");
```

TrySet with conflict awareness

```csharp
BidirectionalDictionary<int, string> map = [];

map.Add(110, "Rudolph");
map[115] = "Zog";
map.Set(120, "Sophie");

Equal(3, map.Count); // verify 3 items exist

True(map.TrySet(120, "Sophia", out int conflictKey)); // success, conflictKey = 0 (default)

False(map.TrySet(220, "Sophia", out conflictKey));
// fail! conflictKey = 120 ... key 120 is already paired with value "Sophia"
// op is as fast as lightning, with zero exception

Equal(3, map.Count); // verify still only 3 items, failed TrySet didn't add

False(map.ContainsValue("Sophie")); // updated "Sophie" value is gone

True(map.TryGetKey("Sophia", out int ownerKey) && ownerKey == 120);
```


## API Documentation

### Adding Elements

```csharp
// Add - throws if key or value already exists
map.Add(key, value);

// TryAdd - returns false if key or value already exists
bool success = map.TryAdd(key, value);

// Set - updates mapping, behavior on value conflict controlled by Force property
map.Set(key, value);

// Set with explicit force override (ignores Force property for this call)
map.Set(key, value, force: true);   // forcePut: silently evict conflicting mapping
map.Set(key, value, force: false);  // strict put: throw if value owned by different key

// Indexer set - same as Set (uses Force property)
map[key] = value;

// TrySet - returns false (without throwing, without modifying the dictionary) if value is
// owned by a different key; on false, conflictKey identifies the conflicting key
bool set = map.TrySet(key, value, out TKey? conflictKey);
```

### Creating from an Existing Dictionary or Collection

Any `IEnumerable<KeyValuePair<TKey, TValue>>` — including `Dictionary<TKey, TValue>`, LINQ projections, lists of `KeyValuePair`, etc. — can be converted via the `ToBidirectionalDictionary` extension methods, or bulk-loaded into an existing map via `AddRange`.

Three conflict strategies are available, consistent with the rest of the API:

```csharp
Dictionary<int, string> source = new() { [1] = "one", [2] = "two", [3] = "three" };

// Strict (default, force: false) — throws on any duplicate value, consistent with ToDictionary()
BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary();

// Force — silently evicts conflicting mappings, last in wins
BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(force: true);

// Collect conflicts — never throws, never evicts; skipped entries returned in conflicts list
BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(
    out List<KVConflict<int, string>>? conflicts);

if(conflicts != null) {
    foreach(KVConflict<int, string> c in conflicts)
        WriteLine($"Skipped {c.Key}→{c.Value}: value already owned by key {c.ConflictKey}");
}

// Optional: case-insensitive key comparer
BidirectionalDictionary<string, int> map = source.ToBidirectionalDictionary(
    keyComparer: StringComparer.OrdinalIgnoreCase);
```

Any `IEnumerable<T>` can also be converted directly using key and value selectors — the same three conflict strategies apply:

```csharp
record User(int Id, string Name);

List<User> users = [new(1, "Alice"), new(2, "Bob"), new(3, "Charlie")];

// Strict (default) — throws on duplicate value
BidirectionalDictionary<int, string> map = users.ToBidirectionalDictionary(x => x.Id, x => x.Name);

// Force — last in wins on conflict
BidirectionalDictionary<int, string> map = users.ToBidirectionalDictionary(x => x.Id, x => x.Name, force: true);

// Collect conflicts
BidirectionalDictionary<int, string> map = users.ToBidirectionalDictionary(
    x => x.Id, x => x.Name, out List<KVConflict<int, string>>? conflicts);
```

`AddRange` provides the same three strategies as instance methods, for loading into an existing (possibly already-populated) map. In fact the extension methods ultimately call `AddRange` methods:

```csharp
// Useful when you need to configure the map first (comparers, AllowDefaults, etc.)
BidirectionalDictionary<int, string> map = new() { AllowDefaults = false };

map.AddRange(source);                // uses map.Force property
map.AddRange(source, force: false);  // strict — throws on conflict
map.AddRange(source, force: true);   // evict conflicts silently

map.AddRange(source, out List<KVConflict<int, string>>? conflicts); // collect conflicts
```

### Setter Behavior: the `Force` Property

The `Force` property (default: `true`) controls what happens when you set a key's value and the new value is **already mapped to a different key**:

```csharp
var map = new BidirectionalDictionary<int, string>();
map.Add(1, "Alice");
map.Add(2, "Bob");

// Force = true (default): silently evicts the conflicting mapping (1→"Alice" removed)
map.Force = true;
map[2] = "Alice";  // map now contains only: 2→"Alice"

// Force = false: throws ArgumentException instead
map.Force = false;
map[2] = "Alice";  // throws: "Value 'Alice' is already mapped to key '1'"
```

This maps to Guava's `BiMap` terminology: `Force = true` behaves like `forcePut`, `Force = false` behaves like `put`. The `Set(key, value, bool force)` overload lets you override this per-call regardless of the property.

### Default Value Guard: the `AllowDefaults` Property

The `AllowDefaults` property (default: `true`) controls whether keys or values equal to their type's default are permitted. Set it to `false` to enforce that all entries carry meaningful, non-default values — useful when `0`, `Guid.Empty`, `false`, and similar sentinel-like defaults would be ambiguous or invalid in your domain. On first look, this may not seem like a big deal, but when you consider, with a 1 to 1 restriction like this type has, it makes sense to never allow it. We still leave that in your hands, and thus have even kept this to `true` by default, but the point is, this is actually a significant issue.

```csharp
// AllowDefaults = true (default): 0, Guid.Empty, false, etc. are accepted
var map = new BidirectionalDictionary<string, int>();
map.Add("pending", 0);  // fine

// AllowDefaults = false: throws ArgumentException for any default key or value
var strictMap = new BidirectionalDictionary<string, int> { AllowDefaults = false };
strictMap.Add("user", 0);    // throws: value 0 is the default for int
strictMap.Add("user", 42);   // fine

// Also guards keys
var idMap = new BidirectionalDictionary<int, string> { AllowDefaults = false };
idMap.Add(0, "alice");   // throws: key 0 is the default for int
idMap.Add(42, "alice");  // fine

// Works for any value type
var guidMap = new BidirectionalDictionary<string, Guid> { AllowDefaults = false };
guidMap.Add("x", Guid.Empty);   // throws
guidMap.Add("x", Guid.NewGuid()); // fine
```

The guard applies to any adds or sets (`Add`, `TryAdd`, and `Set`, indexer setter, etc). `AllowDefaults` and `Force` are independent — both can be set freely.

### TrySet: Conflict-Aware Setting

`TrySet` is a third option alongside `Force = true` and `Force = false`. Rather than silently evicting a conflicting mapping or throwing an exception, it returns `false` and leaves the dictionary **completely untouched**, letting the caller decide what to do next.

This is a very useful feature, because the Set already had to do this check anyways. It allows you to have `!force` functionality (not silent overwrite of a conflict), but to allow the dictionary itself to do the work of checking if there will be a conflict.

The `out TKey? conflictKey` parameter tells you exactly which key already owns the conflicting value:

```csharp
var map = new BidirectionalDictionary<int, string>();
map.Add(1, "Alice");
map.Add(2, "Bob");

// Change key 1 to pair with a NEW value ("Charlie") — no conflict, sets 1 → "Charlie"
if(!map.TrySet(1, "Charlie", out int conflictKey))
    Console.WriteLine($"Conflict: 'Charlie' already owned by key {conflictKey}");
// map now: 1→"Charlie", 2→"Bob"

// "Bob" is already owned by key 2 — conflict
if(!map.TrySet(3, "Bob", out conflictKey))
    Console.WriteLine($"Conflict: 'Bob' already owned by key {conflictKey}"); // → key 2
// dictionary unchanged: still 1→"Charlie", 2→"Bob"
```

Return value semantics:
- **`true`** — the mapping is now set (either it was just written, or it was already exactly `key → value`). `conflictKey` is always `default`.
- **`false`** — `value` is already owned by a different key. The dictionary is **unchanged** — no partial modifications, no evictions. `conflictKey` is the key that owns `value`.

This is the right choice when:
- You want `Force = false` semantics (no silent evictions) but also don't want an exception — you need to handle the conflict gracefully in normal control flow
- The conflict is a recoverable, expected condition (e.g. user-driven data entry, protocol negotiation)
- You want to avoid the extra lookup you'd need to manually pre-check for a conflict before calling `Set`

`AllowDefaults` is still respected: null or default keys/values throw `ArgumentNullException` / `ArgumentException` regardless.

### Retrieving Elements

```csharp
// Get value by key (throws KeyNotFoundException if not found)
TValue value = map[key];
TValue value = map.GetValue(key);

// Get key by value (throws KeyNotFoundException if not found)
TKey key = map[value];         // reverse indexer
TKey key = map.GetKey(value);  // explicit method

// TryGet* variants - safe versions that don't throw
bool found = map.TryGetValue(key, out TValue value);
bool found = map.TryGetKey(value, out TKey key);
```

### Removing Elements

```csharp
// Remove by key
bool removed = map.RemoveByKey(key);
bool removed = map.Remove(key);  // alias for RemoveByKey

// Remove by value
bool removed = map.RemoveByValue(value);

// Clear all
map.Clear();
```

### Checking Existence

```csharp
bool hasKey = map.ContainsKey(key);
bool hasValue = map.ContainsValue(value);
```

### Custom Comparers

```csharp
// Case-insensitive lookups in both directions
var map = new BidirectionalDictionary<string, string>(
    keyComparer: StringComparer.OrdinalIgnoreCase,
    valueComparer: StringComparer.OrdinalIgnoreCase);

map.Add("US", "United States");
string name = map["us"];             // "United States"
string code = map["united states"];  // "US"
```

### Collection Properties

```csharp
int count = map.Count;
ICollection<TKey> keys = map.Keys;
ICollection<TValue> values = map.Values;
```

### Enumeration

```csharp
foreach (var pair in map)
{
    Console.WriteLine($"{pair.Key} -> {pair.Value}");
}
```

## Use Cases

### User ID to Username Mapping
```csharp
BidirectionalDictionary<int, string> userMap = [];
userMap.Add(1001, "john_doe");
userMap.Add(1002, "jane_smith");

// Find user by ID
string username = userMap[1001];  // "john_doe"

// Find ID by username
int userId = userMap["jane_smith"];  // 1002
```

### Country Code to Country Name
```csharp
BidirectionalDictionary<string, string> countryMap = [];
countryMap.Add("US", "United States");
countryMap.Add("UK", "United Kingdom");
countryMap.Add("CA", "Canada");

// Get full name from code
string fullName = countryMap["US"];  // "United States"

// Get code from full name
string code = countryMap.GetKey("Canada");  // "CA"
```

### Coordinate to Entity Mapping (Game Development)
```csharp
BidirectionalDictionary<Vector3, GameObject> positionMap = [];
positionMap.Add(new Vector3(0, 0, 0), player);
positionMap.Add(new Vector3(10, 0, 5), enemy);

// Find entity at position
GameObject entity = positionMap[new Vector3(0, 0, 0)];

// Find position of entity
Vector3 position = positionMap[player];
```

## Performance Characteristics

| Operation | Time Complexity | Space Complexity |
|-----------|----------------|------------------|
| Add | O(1) | O(n) |
| Remove | O(1) | O(n) |
| Get by Key | O(1) | - |
| Get by Value | O(1) | - |
| Contains | O(1) | - |
| Clear | O(n) | - |

**Space Complexity**: The map maintains two internal dictionaries, so memory usage is approximately 2x that of a standard Dictionary.

## Thread Safety

**This collection is not thread-safe.** If you need to access the map from multiple threads, you must provide your own synchronization mechanism (e.g., `lock` statements or `ReaderWriterLockSlim`).

## Type Constraints

Both `TKey` and `TValue` must be non-nullable reference types or value types. The `notnull` constraint ensures type safety and prevents null-related runtime errors.

```csharp
// Valid
BidirectionalDictionary<int, string> map1 = [];
BidirectionalDictionary<Guid, MyClass> map2 = [];

// Invalid - nullable types not allowed
// BidirectionalDictionary<int?, string> map3 = [];
```

Note that while `0`, `Guid.Empty`, and similar are valid by default, you can opt in to rejecting them via `AllowDefaults = false` — see the [Default Value Guard](#default-value-guard-the-allowdefaults-property) section.

## Requirements

- .NET 8.0 or later (also targets .NET 10.0)
- C# 12 or later

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please file an issue on the GitHub repository.
