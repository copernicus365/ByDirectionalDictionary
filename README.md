# TwoWayDictionary

A high-performance bidirectional dictionary implementation for C# that maintains one-to-one relationships between keys and values, allowing O(1) lookups in both directions.

## Features

- **Bidirectional Lookups**: Retrieve values by keys or keys by values with O(1) time complexity
- **One-to-One Mapping**: Enforces unique keys and unique values - each key maps to exactly one value and vice versa
- **Type-Safe**: Generic implementation with compile-time type checking
- **Comprehensive API**: Support for Add, Set, Remove, Contains, and Try* variants
- **IEnumerable Support**: Iterate through key-value pairs with foreach
- **Well-Tested**: Comprehensive unit test coverage (40+ tests, 100% passing)
- **Zero Dependencies**: No external dependencies beyond .NET 6.0+
- **Multi-Target**: Supports .NET 6.0 and .NET 9.0

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package TwoWayDictionary
```

Or via Package Manager Console:

```powershell
Install-Package TwoWayDictionary
```

## Quick Start

```csharp
using TwoWayDictionary;

// Create a two-way dictionary
var userMap = new TwoWayDictionary<int, string>();

// Add mappings
userMap.Add(101, "Alice");
userMap.Add(102, "Bob");
userMap.Add(103, "Charlie");

// Forward lookup (key -> value)
string name = userMap[101];  // "Alice"

// Reverse lookup (value -> key)
int id = userMap.GetKey("Bob");  // 102

// Check existence
bool hasUser = userMap.ContainsKey(101);      // true
bool hasName = userMap.ContainsValue("Bob");  // true

// Remove mappings
userMap.RemoveByKey(102);
// or
userMap.RemoveByValue("Charlie");
```

## API Documentation

### Adding Elements

```csharp
// Add - throws if key or value already exists
map.Add(key, value);

// TryAdd - returns false if key or value already exists
bool success = map.TryAdd(key, value);

// Set - replaces existing mappings if necessary
map.Set(key, value);

// Indexer set - same as Set
map[key] = value;
```

### Retrieving Elements

```csharp
// Get value by key (throws KeyNotFoundException if not found)
TValue value = map[key];
TValue value = map.GetValue(key);

// Get key by value (throws KeyNotFoundException if not found)
TKey key = map.GetKey(value);

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
var userMap = new TwoWayDictionary<int, string>();
userMap.Add(1001, "john_doe");
userMap.Add(1002, "jane_smith");

// Find user by ID
string username = userMap[1001];  // "john_doe"

// Find ID by username
int userId = userMap.GetKey("jane_smith");  // 1002
```

### Country Code to Country Name
```csharp
var countryMap = new TwoWayDictionary<string, string>();
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
var positionMap = new TwoWayDictionary<Vector3, GameObject>();
positionMap.Add(new Vector3(0, 0, 0), player);
positionMap.Add(new Vector3(10, 0, 5), enemy);

// Find entity at position
GameObject entity = positionMap[new Vector3(0, 0, 0)];

// Find position of entity
Vector3 position = positionMap.GetKey(player);
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
var map1 = new TwoWayDictionary<int, string>();
var map2 = new TwoWayDictionary<Guid, MyClass>();

// Invalid - nullable types not allowed
// var map3 = new TwoWayDictionary<int?, string>();
```

## Requirements

- .NET 6.0 or later
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

## Acknowledgments

This library was created to provide a robust, reusable bidirectional mapping solution for the C# community.

## Support

If you encounter any issues or have questions, please file an issue on the GitHub repository.
