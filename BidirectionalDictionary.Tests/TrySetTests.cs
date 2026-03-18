namespace Tests;

public class TrySetTests
{
	// --- success (true) cases ---

	[Fact]
	public void TrySet_NewKeyValue_OnEmpty_ReturnsTrue_Adds()
	{
		BidirectionalDictionary<string, int> map = [];

		True(map.TrySet("one", 1, out string? diffKey));

		Null(diffKey);
		IsSingle(map);
		Equal(1, map["one"]);
		Equal("one", map[1]);
	}

	[Fact]
	public void SameValueAlreadySet_ReturnsTrue_NoChange()
	{
		BidirectionalDictionary<string, int> map = [];
		map["one"] = 1;

		True(map.TrySet("one", 1, out string? diffKey));

		IsSingle(map); // important! didn't add!

		Null(diffKey);
		Equal(1, map["one"]);
	}

	[Fact]
	public void ExistingKey_NewValueNotClaimed_ReturnsTrue_Updates()
	{
		BidirectionalDictionary<string, int> map = [];
		map.Add("one", 1);

		True(map.TrySet("one", 2, out string? diffKey));

		Null(diffKey);
		IsSingle(map);
		Equal(2, map["one"]);
		False(map.ContainsValue(1));
	}

	[Fact]
	public void NewKeyExistingValueSameKey_NotApplicable_NewKey()
	{
		// value not yet in map at all — fresh add via TrySet
		BidirectionalDictionary<string, int> map = [];
		map.Add("one", 1);

		True(map.TrySet("two", 2, out string? diffKey));

		IsCount(2, map);
		Null(diffKey);
		Equal(2, map["two"]);
	}

	[Fact]
	public void ValueBelongsToSameKey_ReturnsTrueNoop()
	{
		// key already owns this value — the "same key" branch inside the conflict check
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");

		// key=2 already owns "two" — no conflict
		True(map.TrySet(2, "two", out _));


		IsCount(2, map);
	}

	// --- failure (false) cases ---

	[Fact]
	public void ValueOwnedByDifferentKey_ReturnsFalseWithConflictKey()
	{
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");

		// "two" is owned by key 2; trying to assign it to key 1 is a conflict
		False(map.TrySet(1, "two", out int diffKey));

		Equal(2, diffKey);
	}

	[Fact]
	public void Conflict_DictionaryIsUntouched()
	{
		// Verifies the critical "no state corruption on false" guarantee
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");

		void validateState()
		{
			// Both pairs must be intact — forward AND reverse
			IsCount(2, map);
			Equal("one", map[1]);
			Equal("two", map[2]);
			Equal(1, map.GetKey("one"));
			Equal(2, map.GetKey("two"));
		}

		validateState();

		False(map.TrySet(1, "two", out int diffKey));

		validateState(); // UNCHANGED after TrySet

		Equal(2, diffKey);
	}

	[Fact]
	public void Conflict_NewKeyExistingValue_DictionaryIsUntouched()
	{
		// New key (not yet in map), but value belongs to another key
		BidirectionalDictionary<int, string> map = [];
		map.Add(1, "one");

		void validateState()
		{
			IsSingle(map);
			Equal("one", map[1]);
			Equal(1, map.GetKey("one"));
		}

		validateState();

		False(map.TrySet(99, "one", out int diffKey));

		validateState(); // UNCHANGED after TrySet

		Equal(1, diffKey);
	}

	// --- differentKeyOwns output guarantees ---

	[Fact]
	public void OnTrue_DifferentKeyOwnsIsAlwaysDefault()
	{
		// Use string keys so that TKey? is a nullable reference type, letting us verify null
		BidirectionalDictionary<string, string> map = [];
		map.Add("one", "1");

		// Success via early-return (already same)
		map.TrySet("one", "1", out string? diffKey1);
		Null(diffKey1);

		// Success via normal add
		map.TrySet("two", "2", out string? diffKey2);
		Null(diffKey2);

		// Success via key value-update
		map.TrySet("one", "uno", out string? diffKey3);
		Null(diffKey3);
	}

	[Fact]
	public void OnFalse_DifferentKeyOwnsIsTheConflictingKey()
	{
		BidirectionalDictionary<int, string> map = [];
		map.Add(10, "ten");
		map.Add(20, "twenty");

		void validateState()
		{
			IsCount(2, map);
			Equal("ten", map[10]);
			Equal("twenty", map[20]);
		}

		validateState();

		False(map.TrySet(10, "twenty", out int diffKey));

		validateState(); // UNCHANGED after TrySet

		Equal(20, diffKey);
	}

	// --- null / default guard ---

	[Fact]
	public void NullKey_Throws()
	{
		BidirectionalDictionary<string, string> map = [];
		Throws<ArgumentNullException>(() => map.TrySet(null!, "value", out _));
	}

	[Fact]
	public void NullValue_Throws()
	{
		BidirectionalDictionary<string, string> map = [];
		Throws<ArgumentNullException>(() => map.TrySet("key", null!, out _));
	}

	[Fact]
	public void AllowDefaultsFalse_DefaultKey_Throws()
	{
		var map = new BidirectionalDictionary<int, string> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.TrySet(0, "value", out _));
	}

	[Fact]
	public void AllowDefaultsFalse_DefaultValue_Throws()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.TrySet("key", 0, out _));
	}
}
