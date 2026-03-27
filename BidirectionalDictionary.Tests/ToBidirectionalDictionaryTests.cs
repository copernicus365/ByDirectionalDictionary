namespace Tests;

public class ToBidirectionalDictionaryTests
{
	// --- ToBidirectionalDictionary(source, force = false) ---

	[Fact]
	public void Default_CleanSource_ReturnsPopulatedMap()
	{
		Dictionary<int, string> source = new() { [1] = "one", [2] = "two", [3] = "three" };

		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary();

		HasExactly(map, (1, "one"), (2, "two"), (3, "three"));
	}

	[Fact]
	public void Default_Force_False_DuplicateValue_Throws()
	{
		// Simulated via a list of KVPs since Dictionary itself prevents duplicate keys
		List<KeyValuePair<int, string>> source = [
			new(1, "one"),
			new(2, "one"), // duplicate value
		];

		Throws<ArgumentException>(() => source.ToBidirectionalDictionary());
	}

	[Fact]
	public void Default_ReturnedMap_ForceProperty_IsFalse()
	{
		Dictionary<int, string> source = new() { [1] = "one" };

		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary();

		False(map.Force);
	}

	[Fact]
	public void ForceTrue_ValueConflict_Evicts_LastInWins()
	{
		List<KeyValuePair<int, string>> source = [
			new(1, "one"),
			new(2, "one"), // claims "one" from key 1
		];

		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(force: true);

		HasExactly(map, (2, "one"));
		False(map.ContainsKey(1));
	}

	[Fact]
	public void ForceTrue_ReturnedMap_ForceProperty_IsTrue()
	{
		Dictionary<int, string> source = new() { [1] = "one" };

		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(force: true);

		True(map.Force);
	}

	[Fact]
	public void KeyComparer_CaseInsensitive_Works()
	{
		Dictionary<string, int> source = new() { ["One"] = 1, ["Two"] = 2 };

		BidirectionalDictionary<string, int> map = source.ToBidirectionalDictionary(
			keyComparer: StringComparer.OrdinalIgnoreCase);

		Equal(1, map["one"]);   // case-insensitive lookup
		Equal(1, map["ONE"]);
	}

	[Fact]
	public void AllowDefaults_False_DefaultValue_Throws()
	{
		List<KeyValuePair<string, int>> source = [new("key", 0)]; // 0 is default for int

		Throws<ArgumentException>(() =>
			source.ToBidirectionalDictionary(allowDefaults: false));
	}

	[Fact]
	public void AllowDefaults_Null_UsesTypeDefault_True()
	{
		List<KeyValuePair<string, int>> source = [new("key", 0)];

		// allowDefaults not passed — defaults to true, so 0 is allowed
		BidirectionalDictionary<string, int> map = source.ToBidirectionalDictionary();

		HasExactly(map, ("key", 0));
	}

	// --- ToBidirectionalDictionary(source, out conflicts) ---

	[Fact]
	public void Conflicts_CleanSource_AddsAll_ConflictsNull()
	{
		Dictionary<int, string> source = new() { [1] = "one", [2] = "two" };

		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(out List<KVConflict<int, string>>? conflicts);

		HasExactly(map, (1, "one"), (2, "two"));
		Null(conflicts);
	}

	[Fact]
	public void Conflicts_ValueConflict_Skipped_Collected_FirstInWins()
	{
		List<KeyValuePair<int, string>> source = [
			new(1, "one"),
			new(2, "one"), // conflict — "one" already owned by 1
			new(3, "three"),
		];

		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(out List<KVConflict<int, string>>? conflicts);

		HasExactly(map, (1, "one"), (3, "three"));
		False(map.ContainsKey(2));

		NotNull(conflicts);
		Single(conflicts);
		Equal(2, conflicts[0].Key);
		Equal("one", conflicts[0].Value);
		Equal(1, conflicts[0].ConflictKey);
	}

	[Fact]
	public void Conflicts_ReturnedMap_ForceProperty_IsTrue()
	{
		Dictionary<int, string> source = new() { [1] = "one" };

		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(out _);

		True(map.Force);
	}

	[Fact]
	public void Conflicts_KeyComparer_CaseInsensitive_Works()
	{
		Dictionary<string, int> source = new() { ["One"] = 1 };

		BidirectionalDictionary<string, int> map = source.ToBidirectionalDictionary(
			out _,
			keyComparer: StringComparer.OrdinalIgnoreCase);

		Equal(1, map["one"]);
		Equal(1, map["ONE"]);
	}
}
