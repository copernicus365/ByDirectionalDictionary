using Xunit;

using static Xunit.Assert;

namespace DotNetXtensions.Tests;

public class CoreTests
{
	const int testVal = 5;

	[Fact]
	public void Add_ValidKeyValue_AddsSuccessfully()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act
		map.Add(1, "one");

		// Assert
		_single(map);
		Equal("one", map[1]);
		Equal(1, map.GetKey("one"));

		// indexer lookup - Reverse lookup
		Equal(1, map["one"]);
	}

	[Fact]
	public void Add_DuplicateKey_ThrowsArgumentException()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		var exception = Throws<ArgumentException>(() => map.Add(1, "two"));
		Contains("Key", exception.Message);
	}

	[Fact]
	public void Add_DuplicateValue_ThrowsArgumentException()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		var exception = Throws<ArgumentException>(() => map.Add(2, "one"));
		Contains("Value", exception.Message);
	}

	[Fact]
	public void Add_NullKey_ThrowsArgumentNullException()
	{
		// Arrange
		BidirectionalDictionary<string, string> map = [];

		// Act & Assert
		Throws<ArgumentNullException>(() => map.Add(null!, "value"));
	}

	[Fact]
	public void Add_NullValue_ThrowsArgumentNullException()
	{
		// Arrange
		BidirectionalDictionary<string, string> map = [];

		// Act & Assert
		Throws<ArgumentNullException>(() => map.Add("key", null!));
	}

	[Fact]
	public void TryAdd_ValidKeyValue_ReturnsTrue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act
		bool result = map.TryAdd(1, "one");

		// Assert
		True(result);
		_single(map);
	}

	[Fact]
	public void TryAdd_DuplicateKey_ReturnsFalse()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.TryAdd(1, "two");

		// Assert
		False(result);
		_single(map);
	}

	[Fact]
	public void TryAdd_DuplicateValue_ReturnsFalse()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.TryAdd(2, "one");

		// Assert
		False(result);
		_single(map);
	}

	[Fact]
	public void Set_NewKeyValue_AddsSuccessfully()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act
		map.Set(1, "one");

		// Assert
		_single(map);
		Equal("one", map[1]);
	}

	[Fact]
	public void Set_ExistingKey_UpdatesValue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		map.Set(1, "uno");

		// Assert
		_single(map);
		Equal("uno", map[1]);
		False(map.ContainsValue("one"));
	}

	[Fact]
	public void Set_ExistingValue_UpdatesKey()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		map.Set(2, "one");

		// Assert
		_single(map);
		Equal(2, map.GetKey("one"));
		False(map.ContainsKey(1));
	}

	[Fact]
	public void Set_ConflictingKeyAndValue_RemovesBothAndAddsNew()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");

		// Act
		map.Set(1, "two");

		// Assert
		_single(map);
		Equal("two", map[1]);
		False(map.ContainsKey(2));
	}

	[Fact]
	public void DefaultInstanceValuesCorrect_AllowDefaults_Force()
	{
		BidirectionalDictionary<string, int> map = [];
		True(map.AllowDefaults);
		True(map.Force);
	}

	// === Force ===

	[Fact]
	public void Set_Force_DuplicateValuesOverwrite()
	{
		var map = new BidirectionalDictionary<string, int> { Force = true };
		map["a1"] = testVal; // should not throw
		Equal(testVal, map["a1"]);
		_single(map);

		map["a2"] = testVal; // should not throw

		_single(map);
		Equal(testVal, map["a2"]);

		False(map.ContainsKey("a1"));
		True(map.ContainsValue(testVal));
	}


	[Fact]
	public void Set_Force_NOT_DuplicateValuesSet_Throw()
	{
		var map = new BidirectionalDictionary<string, int> { Force = false };
		map["a1"] = testVal; // should not throw
		Equal(testVal, map["a1"]);
		Throws<ArgumentException>(() => map["a2"] = testVal);
	}



	[Fact]
	public void GetValue_ExistingKey_ReturnsValue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		string value = map.GetValue(1);
		string valueFromIndexer = map[1]; // Indexer should also work for forward lookup

		// Assert
		Equal("one", value);
		Equal("one", valueFromIndexer);
	}

	[Fact]
	public void GetValue_NonExistingKey_ThrowsKeyNotFoundException()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act & Assert
		Throws<KeyNotFoundException>(() => map.GetValue(1));
	}

	[Fact]
	public void GetKey_ExistingValue_ReturnsKey()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		int key = map.GetKey("one");

		// Assert
		Equal(1, key);
	}

	[Fact]
	public void GetKey_NonExistingValue_ThrowsKeyNotFoundException()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act & Assert
		Throws<KeyNotFoundException>(() => map.GetKey("one"));
	}

	[Fact]
	public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.TryGetValue(1, out string? value);

		// Assert
		True(result);
		Equal("one", value);
	}

	[Fact]
	public void TryGetValue_NonExistingKey_ReturnsFalse()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act
		bool result = map.TryGetValue(1, out string? value);

		// Assert
		False(result);
		Null(value);
	}

	[Fact]
	public void TryGetKey_ExistingValue_ReturnsTrueAndKey()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.TryGetKey("one", out int key);

		// Assert
		True(result);
		Equal(1, key);
	}

	[Fact]
	public void TryGetKey_NonExistingValue_ReturnsFalse()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act
		bool result = map.TryGetKey("one", out int key);

		// Assert
		False(result);
		Equal(0, key);
	}

	[Fact]
	public void Indexer_Get_ExistingKey_ReturnsValue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		string value = map[1];

		// Assert
		Equal("one", value);
	}

	[Fact]
	public void Indexer_Get_NonExistingKey_ThrowsKeyNotFoundException()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act & Assert
		Throws<KeyNotFoundException>(() => map[1]);
	}

	[Fact]
	public void Indexer_Set_UpdatesValue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
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
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.RemoveByKey(1);

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
		BidirectionalDictionary<int, string> map = [];

		// Act
		bool result = map.RemoveByKey(1);

		// Assert
		False(result);
	}

	[Fact]
	public void RemoveByValue_ExistingValue_RemovesAndReturnsTrue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.RemoveByValue("one");

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
		BidirectionalDictionary<int, string> map = [];

		// Act
		bool result = map.RemoveByValue("one");

		// Assert
		False(result);
	}

	[Fact]
	public void Remove_ExistingKey_RemovesAndReturnsTrue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.Remove(1);

		// Assert
		True(result);
		Empty(map);
	}

	[Fact]
	public void ContainsKey_ExistingKey_ReturnsTrue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.ContainsKey(1);

		// Assert
		True(result);
	}

	[Fact]
	public void ContainsKey_NonExistingKey_ReturnsFalse()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act
		bool result = map.ContainsKey(1);

		// Assert
		False(result);
	}

	[Fact]
	public void ContainsValue_ExistingValue_ReturnsTrue()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.ContainsValue("one");

		// Assert
		True(result);
	}

	[Fact]
	public void ContainsValue_NonExistingValue_ReturnsFalse()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act
		bool result = map.ContainsValue("one");

		// Assert
		False(result);
	}

	[Fact]
	public void Clear_RemovesAllMappings()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];
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
		BidirectionalDictionary<int, string> map = [];
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
		BidirectionalDictionary<int, string> map = [];
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
		BidirectionalDictionary<int, string> map = [];
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
		BidirectionalDictionary<int, string> map = [];

		// Act & Assert
		Empty(map);
	}

	[Fact]
	public void Count_AfterAddingMultiple_ReturnsCorrectCount()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

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
		BidirectionalDictionary<string, int> map = [];

		// Act & Assert
		map.Add("A", 1);
		map.Add("B", 2);
		Equal(2, map.Count);

		map.Set("A", 3);  // Should keep B->2 and update A->3
		Equal(2, map.Count);
		Equal(3, map["A"]);
		True(map.ContainsValue(2));  // B->2 should still exist

		map.RemoveByKey("A");
		_single(map);
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
		BidirectionalDictionary<string, int> map = [];
		map.Add("Alice", 101);
		map.Add("Bob", 102);
		map.Add("Charlie", 103);

		// Act & Assert - Forward lookup
		Equal(101, map["Alice"]);
		Equal(102, map["Bob"]);
		Equal(103, map["Charlie"]);

		// Act & Assert - Reverse lookup
		Equal("Alice", map.GetKey(101));
		Equal("Alice", map[101]);
		Equal("Bob", map.GetKey(102));
		Equal("Charlie", map.GetKey(103));
	}

	// --- AllowDefaults ---

	[Fact]
	public void AllowDefaults_False_Throws()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = false };
		Throws<ArgumentException>(() => map["pending"] = 0);
	}

	[Fact]
	public void AllowDefaults_True_AllowsDefaultValue_Add()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = true };
		map.Add("pending", 0); // should not throw
		Equal(0, map["pending"]);
	}


	[Fact]
	public void AllowDefaults_True_AllowsDefaultValue_Set()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = true };
		map["pending"] = 0; // should not throw
		Equal(0, map["pending"]);
	}

	[Fact]
	public void AllowDefaults_False_Add_ThrowsOnDefaultValue()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = false };
		var ex = Throws<ArgumentException>(() => map.Add("pending", 0));
		Contains("value", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void AllowDefaults_False_Add_ThrowsOnDefaultKey()
	{
		var map = new BidirectionalDictionary<int, string> { AllowDefaults = false };
		var ex = Throws<ArgumentException>(() => map.Add(0, "pending"));
		Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void AllowDefaults_False_TryAdd_ThrowsOnDefaultValue()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.TryAdd("pending", 0));
	}

	[Fact]
	public void AllowDefaults_False_Set_ThrowsOnDefaultValue()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.Set("pending", 0));
	}

	[Fact]
	public void AllowDefaults_False_Set_ThrowsOnDefaultKey()
	{
		var map = new BidirectionalDictionary<int, string> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.Set(0, "hello"));
	}

	[Fact]
	public void AllowDefaults_False_GuidEmpty_ThrowsOnDefaultValue()
	{
		var map = new BidirectionalDictionary<string, Guid> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.Add("x", Guid.Empty));
	}

	[Fact]
	public void AllowDefaults_False_GuidEmpty_NonDefaultSucceeds()
	{
		var map = new BidirectionalDictionary<string, Guid> { AllowDefaults = false };
		var id = Guid.NewGuid();
		map.Add("x", id);
		Equal(id, map["x"]);
	}

	[Fact]
	public void AllowDefaults_Force_Overwrites()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = true, Force = true };
		map["pending"] = 0; // should not throw
		Equal(0, map["pending"]);
		_single(map);

		map["a2"] = 0; // should not throw

		_single(map);
		Equal(0, map["a2"]);

		False(map.ContainsKey("pending"));
		True(map.ContainsValue(0));
	}

	static void _single<T>(ICollection<T> coll)
		=> Equal(1, coll.Count); //`Single(coll)` is incorrect, uses enumerator, not Count, dumb
}
