using Xunit;

using static Xunit.Assert;

namespace ByDirectionalDictionary.Tests;

public class CoreTests
{
	[Fact]
	public void Add_ValidKeyValue_AddsSuccessfully()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		map.Add(1, "one");

		// Assert
		Single(map);
		Equal("one", map[1]);
		Equal(1, map.GetKey("one"));
	}

	[Fact]
	public void Add_DuplicateKey_ThrowsArgumentException()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		var exception = Throws<ArgumentException>(() => map.Add(1, "two"));
		Contains("Key", exception.Message);
	}

	[Fact]
	public void Add_DuplicateValue_ThrowsArgumentException()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		var exception = Throws<ArgumentException>(() => map.Add(2, "one"));
		Contains("Value", exception.Message);
	}

	[Fact]
	public void Add_NullKey_ThrowsArgumentNullException()
	{
		// Arrange
		ByDirectionalDictionary<string, string> map = [];

		// Act & Assert
		Throws<ArgumentNullException>(() => map.Add(null!, "value"));
	}

	[Fact]
	public void Add_NullValue_ThrowsArgumentNullException()
	{
		// Arrange
		ByDirectionalDictionary<string, string> map = [];

		// Act & Assert
		Throws<ArgumentNullException>(() => map.Add("key", null!));
	}

	[Fact]
	public void TryAdd_ValidKeyValue_ReturnsTrue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		var result = map.TryAdd(1, "one");

		// Assert
		True(result);
		Single(map);
	}

	[Fact]
	public void TryAdd_DuplicateKey_ReturnsFalse()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var result = map.TryAdd(1, "two");

		// Assert
		False(result);
		Single(map);
	}

	[Fact]
	public void TryAdd_DuplicateValue_ReturnsFalse()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var result = map.TryAdd(2, "one");

		// Assert
		False(result);
		Single(map);
	}

	[Fact]
	public void Set_NewKeyValue_AddsSuccessfully()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		map.Set(1, "one");

		// Assert
		Single(map);
		Equal("one", map[1]);
	}

	[Fact]
	public void Set_ExistingKey_UpdatesValue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		map.Set(1, "uno");

		// Assert
		Single(map);
		Equal("uno", map[1]);
		False(map.ContainsValue("one"));
	}

	[Fact]
	public void Set_ExistingValue_UpdatesKey()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		map.Set(2, "one");

		// Assert
		Single(map);
		Equal(2, map.GetKey("one"));
		False(map.ContainsKey(1));
	}

	[Fact]
	public void Set_ConflictingKeyAndValue_RemovesBothAndAddsNew()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");

		// Act
		map.Set(1, "two");

		// Assert
		Single(map);
		Equal("two", map[1]);
		False(map.ContainsKey(2));
	}

	[Fact]
	public void GetValue_ExistingKey_ReturnsValue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var value = map.GetValue(1);

		// Assert
		Equal("one", value);
	}

	[Fact]
	public void GetValue_NonExistingKey_ThrowsKeyNotFoundException()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act & Assert
		Throws<KeyNotFoundException>(() => map.GetValue(1));
	}

	[Fact]
	public void GetKey_ExistingValue_ReturnsKey()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var key = map.GetKey("one");

		// Assert
		Equal(1, key);
	}

	[Fact]
	public void GetKey_NonExistingValue_ThrowsKeyNotFoundException()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act & Assert
		Throws<KeyNotFoundException>(() => map.GetKey("one"));
	}

	[Fact]
	public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var result = map.TryGetValue(1, out var value);

		// Assert
		True(result);
		Equal("one", value);
	}

	[Fact]
	public void TryGetValue_NonExistingKey_ReturnsFalse()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		var result = map.TryGetValue(1, out var value);

		// Assert
		False(result);
		Null(value);
	}

	[Fact]
	public void TryGetKey_ExistingValue_ReturnsTrueAndKey()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var result = map.TryGetKey("one", out var key);

		// Assert
		True(result);
		Equal(1, key);
	}

	[Fact]
	public void TryGetKey_NonExistingValue_ReturnsFalse()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		var result = map.TryGetKey("one", out var key);

		// Assert
		False(result);
		Equal(0, key);
	}

	[Fact]
	public void Indexer_Get_ExistingKey_ReturnsValue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var value = map[1];

		// Assert
		Equal("one", value);
	}

	[Fact]
	public void Indexer_Get_NonExistingKey_ThrowsKeyNotFoundException()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act & Assert
		Throws<KeyNotFoundException>(() => map[1]);
	}

	[Fact]
	public void Indexer_Set_UpdatesValue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		map[1] = "uno";

		// Assert
		Equal("uno", map[1]);
	}

	[Fact]
	public void RemoveByKey_ExistingKey_RemovesAndReturnsTrue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var result = map.RemoveByKey(1);

		// Assert
		True(result);
		Empty(map);
		False(map.ContainsKey(1));
		False(map.ContainsValue("one"));
	}

	[Fact]
	public void RemoveByKey_NonExistingKey_ReturnsFalse()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		var result = map.RemoveByKey(1);

		// Assert
		False(result);
	}

	[Fact]
	public void RemoveByValue_ExistingValue_RemovesAndReturnsTrue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var result = map.RemoveByValue("one");

		// Assert
		True(result);
		Empty(map);
		False(map.ContainsKey(1));
		False(map.ContainsValue("one"));
	}

	[Fact]
	public void RemoveByValue_NonExistingValue_ReturnsFalse()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		var result = map.RemoveByValue("one");

		// Assert
		False(result);
	}

	[Fact]
	public void Remove_ExistingKey_RemovesAndReturnsTrue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var result = map.Remove(1);

		// Assert
		True(result);
		Empty(map);
	}

	[Fact]
	public void ContainsKey_ExistingKey_ReturnsTrue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var result = map.ContainsKey(1);

		// Assert
		True(result);
	}

	[Fact]
	public void ContainsKey_NonExistingKey_ReturnsFalse()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		var result = map.ContainsKey(1);

		// Assert
		False(result);
	}

	[Fact]
	public void ContainsValue_ExistingValue_ReturnsTrue()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		var result = map.ContainsValue("one");

		// Assert
		True(result);
	}

	[Fact]
	public void ContainsValue_NonExistingValue_ReturnsFalse()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		var result = map.ContainsValue("one");

		// Assert
		False(result);
	}

	[Fact]
	public void Clear_RemovesAllMappings()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");
		map.Add(3, "three");

		// Act
		map.Clear();

		// Assert
		Empty(map);
		False(map.ContainsKey(1));
		False(map.ContainsValue("one"));
	}

	[Fact]
	public void Keys_ReturnsAllKeys()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");
		map.Add(3, "three");

		// Act
		var keys = map.Keys;

		// Assert
		Equal(3, keys.Count);
		Contains(1, keys);
		Contains(2, keys);
		Contains(3, keys);
	}

	[Fact]
	public void Values_ReturnsAllValues()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");
		map.Add(3, "three");

		// Act
		var values = map.Values;

		// Assert
		Equal(3, values.Count);
		Contains("one", values);
		Contains("two", values);
		Contains("three", values);
	}

	[Fact]
	public void GetEnumerator_IteratesAllPairs()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");
		map.Add(3, "three");

		// Act
		var pairs = new List<KeyValuePair<int, string>>();
		foreach(var pair in map) {
			pairs.Add(pair);
		}

		// Assert
		Equal(3, pairs.Count);
		Contains(new KeyValuePair<int, string>(1, "one"), pairs);
		Contains(new KeyValuePair<int, string>(2, "two"), pairs);
		Contains(new KeyValuePair<int, string>(3, "three"), pairs);
	}

	[Fact]
	public void Count_EmptyMap_ReturnsZero()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act & Assert
		Empty(map);
	}

	[Fact]
	public void Count_AfterAddingMultiple_ReturnsCorrectCount()
	{
		// Arrange
		ByDirectionalDictionary<int, string> map = [];

		// Act
		map.Add(1, "one");
		map.Add(2, "two");
		map.Add(3, "three");

		// Assert
		Equal(3, map.Count);
	}

	[Fact]
	public void ComplexScenario_AddSetRemove_MaintainsConsistency()
	{
		// Arrange
		ByDirectionalDictionary<string, int> map = [];

		// Act & Assert
		map.Add("A", 1);
		map.Add("B", 2);
		Equal(2, map.Count);

		map.Set("A", 3);  // Should keep B->2 and update A->3
		Equal(2, map.Count);
		Equal(3, map["A"]);
		True(map.ContainsValue(2));  // B->2 should still exist

		map.RemoveByKey("A");
		Single(map);
		False(map.ContainsKey("A"));
		False(map.ContainsValue(3));
		True(map.ContainsKey("B"));  // B->2 should still exist

		map.Add("C", 4);
		Equal(2, map.Count);
	}

	[Fact]
	public void BidirectionalLookup_WorksCorrectly()
	{
		// Arrange
		ByDirectionalDictionary<string, int> map = [];
		map.Add("Alice", 101);
		map.Add("Bob", 102);
		map.Add("Charlie", 103);

		// Act & Assert - Forward lookup
		Equal(101, map["Alice"]);
		Equal(102, map["Bob"]);
		Equal(103, map["Charlie"]);

		// Act & Assert - Reverse lookup
		Equal("Alice", map.GetKey(101));
		Equal("Bob", map.GetKey(102));
		Equal("Charlie", map.GetKey(103));
	}
}
