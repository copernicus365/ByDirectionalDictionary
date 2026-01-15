using System;
using System.Collections.Generic;
using Xunit;

namespace TwoWayDictionary.Tests
{
    public class TwoWayDictionaryTests
    {
        [Fact]
        public void Add_ValidKeyValue_AddsSuccessfully()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            map.Add(1, "one");

            // Assert
            Assert.Equal(1, map.Count);
            Assert.Equal("one", map[1]);
            Assert.Equal(1, map.GetKey("one"));
        }

        [Fact]
        public void Add_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => map.Add(1, "two"));
            Assert.Contains("Key", exception.Message);
        }

        [Fact]
        public void Add_DuplicateValue_ThrowsArgumentException()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => map.Add(2, "one"));
            Assert.Contains("Value", exception.Message);
        }

        [Fact]
        public void Add_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var map = new TwoWayDictionary<string, string>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => map.Add(null!, "value"));
        }

        [Fact]
        public void Add_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            var map = new TwoWayDictionary<string, string>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => map.Add("key", null!));
        }

        [Fact]
        public void TryAdd_ValidKeyValue_ReturnsTrue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            var result = map.TryAdd(1, "one");

            // Assert
            Assert.True(result);
            Assert.Equal(1, map.Count);
        }

        [Fact]
        public void TryAdd_DuplicateKey_ReturnsFalse()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var result = map.TryAdd(1, "two");

            // Assert
            Assert.False(result);
            Assert.Equal(1, map.Count);
        }

        [Fact]
        public void TryAdd_DuplicateValue_ReturnsFalse()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var result = map.TryAdd(2, "one");

            // Assert
            Assert.False(result);
            Assert.Equal(1, map.Count);
        }

        [Fact]
        public void Set_NewKeyValue_AddsSuccessfully()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            map.Set(1, "one");

            // Assert
            Assert.Equal(1, map.Count);
            Assert.Equal("one", map[1]);
        }

        [Fact]
        public void Set_ExistingKey_UpdatesValue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            map.Set(1, "uno");

            // Assert
            Assert.Equal(1, map.Count);
            Assert.Equal("uno", map[1]);
            Assert.False(map.ContainsValue("one"));
        }

        [Fact]
        public void Set_ExistingValue_UpdatesKey()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            map.Set(2, "one");

            // Assert
            Assert.Equal(1, map.Count);
            Assert.Equal(2, map.GetKey("one"));
            Assert.False(map.ContainsKey(1));
        }

        [Fact]
        public void Set_ConflictingKeyAndValue_RemovesBothAndAddsNew()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");
            map.Add(2, "two");

            // Act
            map.Set(1, "two");

            // Assert
            Assert.Equal(1, map.Count);
            Assert.Equal("two", map[1]);
            Assert.False(map.ContainsKey(2));
        }

        [Fact]
        public void GetValue_ExistingKey_ReturnsValue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var value = map.GetValue(1);

            // Assert
            Assert.Equal("one", value);
        }

        [Fact]
        public void GetValue_NonExistingKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => map.GetValue(1));
        }

        [Fact]
        public void GetKey_ExistingValue_ReturnsKey()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var key = map.GetKey("one");

            // Assert
            Assert.Equal(1, key);
        }

        [Fact]
        public void GetKey_NonExistingValue_ThrowsKeyNotFoundException()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => map.GetKey("one"));
        }

        [Fact]
        public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var result = map.TryGetValue(1, out var value);

            // Assert
            Assert.True(result);
            Assert.Equal("one", value);
        }

        [Fact]
        public void TryGetValue_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            var result = map.TryGetValue(1, out var value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetKey_ExistingValue_ReturnsTrueAndKey()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var result = map.TryGetKey("one", out var key);

            // Assert
            Assert.True(result);
            Assert.Equal(1, key);
        }

        [Fact]
        public void TryGetKey_NonExistingValue_ReturnsFalse()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            var result = map.TryGetKey("one", out var key);

            // Assert
            Assert.False(result);
            Assert.Equal(0, key);
        }

        [Fact]
        public void Indexer_Get_ExistingKey_ReturnsValue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var value = map[1];

            // Assert
            Assert.Equal("one", value);
        }

        [Fact]
        public void Indexer_Get_NonExistingKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => map[1]);
        }

        [Fact]
        public void Indexer_Set_UpdatesValue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            map[1] = "uno";

            // Assert
            Assert.Equal("uno", map[1]);
        }

        [Fact]
        public void RemoveByKey_ExistingKey_RemovesAndReturnsTrue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var result = map.RemoveByKey(1);

            // Assert
            Assert.True(result);
            Assert.Equal(0, map.Count);
            Assert.False(map.ContainsKey(1));
            Assert.False(map.ContainsValue("one"));
        }

        [Fact]
        public void RemoveByKey_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            var result = map.RemoveByKey(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RemoveByValue_ExistingValue_RemovesAndReturnsTrue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var result = map.RemoveByValue("one");

            // Assert
            Assert.True(result);
            Assert.Equal(0, map.Count);
            Assert.False(map.ContainsKey(1));
            Assert.False(map.ContainsValue("one"));
        }

        [Fact]
        public void RemoveByValue_NonExistingValue_ReturnsFalse()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            var result = map.RemoveByValue("one");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Remove_ExistingKey_RemovesAndReturnsTrue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var result = map.Remove(1);

            // Assert
            Assert.True(result);
            Assert.Equal(0, map.Count);
        }

        [Fact]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var result = map.ContainsKey(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsKey_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            var result = map.ContainsKey(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsValue_ExistingValue_ReturnsTrue()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");

            // Act
            var result = map.ContainsValue("one");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsValue_NonExistingValue_ReturnsFalse()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            var result = map.ContainsValue("one");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Clear_RemovesAllMappings()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");
            map.Add(2, "two");
            map.Add(3, "three");

            // Act
            map.Clear();

            // Assert
            Assert.Equal(0, map.Count);
            Assert.False(map.ContainsKey(1));
            Assert.False(map.ContainsValue("one"));
        }

        [Fact]
        public void Keys_ReturnsAllKeys()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");
            map.Add(2, "two");
            map.Add(3, "three");

            // Act
            var keys = map.Keys;

            // Assert
            Assert.Equal(3, keys.Count);
            Assert.Contains(1, keys);
            Assert.Contains(2, keys);
            Assert.Contains(3, keys);
        }

        [Fact]
        public void Values_ReturnsAllValues()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");
            map.Add(2, "two");
            map.Add(3, "three");

            // Act
            var values = map.Values;

            // Assert
            Assert.Equal(3, values.Count);
            Assert.Contains("one", values);
            Assert.Contains("two", values);
            Assert.Contains("three", values);
        }

        [Fact]
        public void GetEnumerator_IteratesAllPairs()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();
            map.Add(1, "one");
            map.Add(2, "two");
            map.Add(3, "three");

            // Act
            var pairs = new List<KeyValuePair<int, string>>();
            foreach (var pair in map)
            {
                pairs.Add(pair);
            }

            // Assert
            Assert.Equal(3, pairs.Count);
            Assert.Contains(new KeyValuePair<int, string>(1, "one"), pairs);
            Assert.Contains(new KeyValuePair<int, string>(2, "two"), pairs);
            Assert.Contains(new KeyValuePair<int, string>(3, "three"), pairs);
        }

        [Fact]
        public void Count_EmptyMap_ReturnsZero()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act & Assert
            Assert.Equal(0, map.Count);
        }

        [Fact]
        public void Count_AfterAddingMultiple_ReturnsCorrectCount()
        {
            // Arrange
            var map = new TwoWayDictionary<int, string>();

            // Act
            map.Add(1, "one");
            map.Add(2, "two");
            map.Add(3, "three");

            // Assert
            Assert.Equal(3, map.Count);
        }

        [Fact]
        public void ComplexScenario_AddSetRemove_MaintainsConsistency()
        {
            // Arrange
            var map = new TwoWayDictionary<string, int>();

            // Act & Assert
            map.Add("A", 1);
            map.Add("B", 2);
            Assert.Equal(2, map.Count);

            map.Set("A", 3);  // Should keep B->2 and update A->3
            Assert.Equal(2, map.Count);
            Assert.Equal(3, map["A"]);
            Assert.True(map.ContainsValue(2));  // B->2 should still exist

            map.RemoveByKey("A");
            Assert.Equal(1, map.Count);
            Assert.False(map.ContainsKey("A"));
            Assert.False(map.ContainsValue(3));
            Assert.True(map.ContainsKey("B"));  // B->2 should still exist

            map.Add("C", 4);
            Assert.Equal(2, map.Count);
        }

        [Fact]
        public void BidirectionalLookup_WorksCorrectly()
        {
            // Arrange
            var map = new TwoWayDictionary<string, int>();
            map.Add("Alice", 101);
            map.Add("Bob", 102);
            map.Add("Charlie", 103);

            // Act & Assert - Forward lookup
            Assert.Equal(101, map["Alice"]);
            Assert.Equal(102, map["Bob"]);
            Assert.Equal(103, map["Charlie"]);

            // Act & Assert - Reverse lookup
            Assert.Equal("Alice", map.GetKey(101));
            Assert.Equal("Bob", map.GetKey(102));
            Assert.Equal("Charlie", map.GetKey(103));
        }
    }
}
