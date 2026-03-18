namespace Tests;

public class AllowDefaultsTests
{
	[Fact]
	public void False_Throws()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = false };
		Throws<ArgumentException>(() => map["pending"] = 0);
	}

	[Fact]
	public void True_AllowsDefaultValue_Add()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = true };
		map.Add("pending", 0); // should not throw
		Equal(0, map["pending"]);
	}

	[Fact]
	public void True_AllowsDefaultValue_Set()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = true };
		map["pending"] = 0; // should not throw
		Equal(0, map["pending"]);
	}

	[Fact]
	public void False_Add_ThrowsOnDefaultValue()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = false };
		var ex = Throws<ArgumentException>(() => map.Add("pending", 0));
		Contains("value", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void False_Add_ThrowsOnDefaultKey()
	{
		var map = new BidirectionalDictionary<int, string> { AllowDefaults = false };
		var ex = Throws<ArgumentException>(() => map.Add(0, "pending"));
		Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void False_TryAdd_ThrowsOnDefaultValue()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.TryAdd("pending", 0));
	}

	[Fact]
	public void False_Set_ThrowsOnDefaultValue()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.Set("pending", 0));
	}

	[Fact]
	public void False_Set_ThrowsOnDefaultKey()
	{
		var map = new BidirectionalDictionary<int, string> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.Set(0, "hello"));
	}

	[Fact]
	public void False_GuidEmpty_ThrowsOnDefaultValue()
	{
		var map = new BidirectionalDictionary<string, Guid> { AllowDefaults = false };
		Throws<ArgumentException>(() => map.Add("x", Guid.Empty));
	}

	[Fact]
	public void False_GuidEmpty_NonDefaultSucceeds()
	{
		var map = new BidirectionalDictionary<string, Guid> { AllowDefaults = false };
		var id = Guid.NewGuid();
		map.Add("x", id);
		Equal(id, map["x"]);
	}

	[Fact]
	public void Force_Overwrites()
	{
		var map = new BidirectionalDictionary<string, int> { AllowDefaults = true, Force = true };
		map["pending"] = 0; // should not throw
		Equal(0, map["pending"]);
		IsSingle(map);

		map["a2"] = 0; // should not throw

		IsSingle(map);
		Equal(0, map["a2"]);

		False(map.ContainsKey("pending"));
		True(map.ContainsValue(0));
	}
}
