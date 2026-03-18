namespace Tests;

public class CollectionTests
{
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
		IsCount(3, map);
	}
}
