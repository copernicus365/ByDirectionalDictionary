namespace Tests;

public class AddTests
{
	[Fact]
	public void Add_ValidKeyValue_AddsSuccessfully()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act
		map.Add(1, "one");

		// Assert
		IsSingle(map);
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
		IsSingle(map);
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
		IsSingle(map);
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
		IsSingle(map);
	}
}
