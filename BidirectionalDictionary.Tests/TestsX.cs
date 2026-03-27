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

	public static void HasExactly<TKey, TValue>(BidirectionalDictionary<TKey, TValue> map, params (TKey Key, TValue Value)[] expected)
		where TKey : notnull
		where TValue : notnull
	{
		IsCount(expected.Length, map);
		foreach((TKey key, TValue value) in expected) {
			Equal(value, map[key]);
			Equal(key, map[value]);
		}
	}

	public static void IsEqual<TKey, TValue>(IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2)
	{
		bool equal = dict1.DictionariesAreEqual(dict2);
		True(equal);
	}
}
