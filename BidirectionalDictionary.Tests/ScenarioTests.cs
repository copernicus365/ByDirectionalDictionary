namespace Tests;

public class ScenarioTests
{
	[Fact]
	public void ComplexScenario_AddSetRemove_MaintainsConsistency()
	{
		// Arrange
		BidirectionalDictionary<string, int> map = [];

		// Act & Assert
		map.Add("A", 1);
		map.Add("B", 2);
		IsCount(2, map);

		map.Set("A", 3);  // Should keep B->2 and update A->3
		IsCount(2, map);
		Equal(3, map["A"]);
		True(map.ContainsValue(2));  // B->2 should still exist

		map.RemoveByKey("A");
		IsSingle(map);
		False(map.ContainsKey("A"));
		False(map.ContainsValue(3));
		True(map.ContainsKey("B"));  // B->2 should still exist

		map.Add("C", 4);
		IsCount(2, map);
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
}
