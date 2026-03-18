namespace Tests;

public static class TestsX
{
	public static void Single<T>(ICollection<T> coll)
		=> Equal(1, coll.Count); // `Single(coll)` is incorrect, uses enumerator not Count

	public static void IsSingle<TKey, TValue>(BidirectionalDictionary<TKey, TValue> map)
	{
		IsCount(1, map);
		True(map.CountsAreEqual);
	}

	public static void IsCount<TKey, TValue>(int expectedCount, BidirectionalDictionary<TKey, TValue> map)
	{
		Equal(expectedCount, map.Count);
		True(map.CountsAreEqual);
	}
}
