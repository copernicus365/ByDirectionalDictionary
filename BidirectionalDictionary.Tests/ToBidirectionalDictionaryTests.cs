namespace Tests;

public class ToBidirectionalDictionaryTests
{
	[Fact]
	public void CleanSource_ReturnsPopulatedMap()
	{
		Dictionary<int, string> source = new() { [1] = "one", [2] = "two", [3] = "three" };
		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary();

		List<Dude> source2 = [new(1, "one"), new(2, "two"), new(3, "three")];
		BidirectionalDictionary<int, string> map2 = source2.ToBidirectionalDictionary(x => x.Id, x => x.Name);

		HasExactly(map, (1, "one"), (2, "two"), (3, "three"));
		IsEqual(map, map2);
	}

	[Fact]
	public void Force_False_DuplicateValue_Throws()
	{
		List<KeyValuePair<int, string>> source = [new(1, "one"), new(2, "one")];
		Throws<ArgumentException>(() => source.ToBidirectionalDictionary());

		List<Dude> source2 = [new(1, "one"), new(2, "one")];
		Throws<ArgumentException>(() => source2.ToBidirectionalDictionary(x => x.Id, x => x.Name));
	}

	[Fact]
	public void ReturnedMap_ForceProperty_IsFalse()
	{
		Dictionary<int, string> source = new() { [1] = "one" };
		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary();

		List<Dude> source2 = [new(1, "one")];
		BidirectionalDictionary<int, string> map2 = source2.ToBidirectionalDictionary(x => x.Id, x => x.Name);

		False(map.Force);
		False(map2.Force);
	}

	[Fact]
	public void ForceTrue_ValueConflict_Evicts_LastInWins()
	{
		List<KeyValuePair<int, string>> source = [new(1, "one"), new(2, "one")]; // key 2 claims "one" from key 1
		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(force: true);

		List<Dude> source2 = [new(1, "one"), new(2, "one")];
		BidirectionalDictionary<int, string> map2 = source2.ToBidirectionalDictionary(x => x.Id, x => x.Name, force: true);

		HasExactly(map, (2, "one"));
		False(map.ContainsKey(1));
		IsEqual(map, map2);
	}

	[Fact]
	public void ForceTrue_ReturnedMap_ForceProperty_IsTrue()
	{
		Dictionary<int, string> source = new() { [1] = "one" };
		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(force: true);

		List<Dude> source2 = [new(1, "one")];
		BidirectionalDictionary<int, string> map2 = source2.ToBidirectionalDictionary(x => x.Id, x => x.Name, force: true);

		True(map.Force);
		True(map2.Force);
	}

	[Fact]
	public void KeyComparer_CaseInsensitive_Works()
	{
		Dictionary<string, int> source = new() { ["One"] = 1, ["Two"] = 2 };
		BidirectionalDictionary<string, int> map = source.ToBidirectionalDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);

		List<Dudet> source2 = [new("One", 1), new("Two", 2)];
		BidirectionalDictionary<string, int> map2 = source2.ToBidirectionalDictionary(x => x.Key, x => x.Val, keyComparer: StringComparer.OrdinalIgnoreCase);

		Equal(1, map["one"]);
		Equal(1, map["ONE"]);
		IsEqual(map, map2);
	}

	[Fact]
	public void AllowDefaults_False_DefaultValue_Throws()
	{
		List<KeyValuePair<string, int>> source = [new("key", 0)]; // 0 is default for int
		Throws<ArgumentException>(() => source.ToBidirectionalDictionary(allowDefaults: false));

		List<Dudet> source2 = [new("key", 0)];
		Throws<ArgumentException>(() => source2.ToBidirectionalDictionary(x => x.Key, x => x.Val, allowDefaults: false));
	}

	[Fact]
	public void AllowDefaults_Null_UsesTypeDefault_True()
	{
		List<KeyValuePair<string, int>> source = [new("key", 0)];

		// allowDefaults not passed — defaults to true, so 0 is allowed
		BidirectionalDictionary<string, int> map = source.ToBidirectionalDictionary();

		HasExactly(map, ("key", 0));
	}

	[Fact]
	public void Conflicts_CleanSource_AddsAll_ConflictsNull()
	{
		Dictionary<int, string> source = new() { [1] = "one", [2] = "two" };
		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(out List<KVConflict<int, string>>? conflicts);

		List<Dude> source2 = [new(1, "one"), new(2, "two")];
		BidirectionalDictionary<int, string> map2 = source2.ToBidirectionalDictionary(x => x.Id, x => x.Name, out List<KVConflict<int, string>>? conflicts2);

		HasExactly(map, (1, "one"), (2, "two"));
		Null(conflicts);
		IsEqual(map, map2);
		Null(conflicts2);
	}

	[Fact]
	public void Conflicts_ValueConflict_Skipped_Collected_FirstInWins()
	{
		List<KeyValuePair<int, string>> source = [new(1, "one"), new(2, "one"), new(3, "three")]; // "one" already owned by 1
		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(out List<KVConflict<int, string>>? conflicts);

		List<Dude> source2 = [new(1, "one"), new(2, "one"), new(3, "three")];
		BidirectionalDictionary<int, string> map2 = source2.ToBidirectionalDictionary(x => x.Id, x => x.Name, out List<KVConflict<int, string>>? conflicts2);

		HasExactly(map, (1, "one"), (3, "three"));
		False(map.ContainsKey(2));
		NotNull(conflicts);
		Single(conflicts);
		Equal(2, conflicts[0].Key);
		Equal("one", conflicts[0].Value);
		Equal(1, conflicts[0].ConflictKey);
		IsEqual(map, map2);
		NotNull(conflicts2);
		Single(conflicts2);
	}

	[Fact]
	public void Conflicts_ReturnedMap_ForceProperty_IsTrue()
	{
		Dictionary<int, string> source = new() { [1] = "one" };
		BidirectionalDictionary<int, string> map = source.ToBidirectionalDictionary(out _);

		List<Dude> source2 = [new(1, "one")];
		BidirectionalDictionary<int, string> map2 = source2.ToBidirectionalDictionary(x => x.Id, x => x.Name, out _);

		True(map.Force);
		True(map2.Force);
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

	record Dude(int Id, string Name);

	record Dudet(string Key, int Val);
}
