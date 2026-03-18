namespace Tests;

public class SetTests
{
	const int testVal = 5;

	[Fact]
	public void Set_NewKeyValue_AddsSuccessfully()
	{
		// Arrange
		BidirectionalDictionary<int, string> map = [];

		// Act
		map.Set(1, "one");

		// Assert
		IsSingle(map);
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
		IsSingle(map);
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
		IsSingle(map);
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
		IsSingle(map);
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

	[Fact]
	public void Set_Force_DuplicateValuesOverwrite()
	{
		var map = new BidirectionalDictionary<string, int> { Force = true };
		map["a1"] = testVal; // should not throw
		Equal(testVal, map["a1"]);
		IsSingle(map);

		map["a2"] = testVal; // should not throw

		IsSingle(map);
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
}
