# BidirectionalDictionary setter analysis

Hey! So, the key part that makes a bidirectional dictionary difficult, is when SETTING a new value. Writing, in other words. Surprisingly, **Add** / adding, is simple! Just like a normal Dictionary, you throw if the key already exists, with the addition, now you also throw if the **value** already exists (because it will be the key in the reverse dictionary). So 'add' is easy, but this leaves the setter as the difficult part. To be clear, the index setter is what I'm referring to, as well as, in the original dictionary, the 'Set' method which it called.

So on the Setter, this ends up being the most important and the most difficult matter to get 100% correct, for a general purpose, bi-directional map / dictionary type. There are ambiguties involved, and there is the critical need to keep the data having full integrity, and the need to handle all this correctly.

This BidirectionalDictionary was a forked version of ashishkarn's TwoWayDictionary, and, originally, I had concerns that his setter was, while not incorrect, possibly doing overly much for such a general method (I thought that there perhaps could be a one or more setters, with that one being one that does extra work). This is the main thing that lead me to feeling the need to make my own fork. But gladly, I found in the end, it wasn't bad, and much of that was critical when you think it through, as you'll see with a lot of help from Claude (4.6). Just as important, there's greater clarity now, when comparing this type to other standard bi-directional mappers. It turns out you could call the original setter a "ForcePut" type. `force` meaning, it IGNORES if a value previously was assigned to a different key, that just gets OVERWRITTEN without warning or exception or notice. That to me seems like the best, most reasonable path to take as well, but I think now we'll allowing a property flag to choose which behavior is best desired.

Below is the AI discussion I had, and I think it's worth saving this, particularly the final question and answer / analysis. Not that performance was the key thing here, more figuring out what the best method of operating for this kind of type, but, perf wise, nicely, this even shaves off a few extra calls (surprisingly, because this seems to do more at the end of the day).

## Q (last question moved to TOP, final analysis, TLDR)

**User:** Ok, I want to do an overall comparison of what is now FOUR 'Set' methods. I just added a fourth back in, named Set_Original, which with minor changes (to match current variable name changes), matches the original code we started with.

So we just compared `ForcePut1` (which matches Guava's putForce I think it was called), to `Set_Final` (not the final name, we'll rename later)

Then your first `put` method or close to it (I think?), was `Set_ThrowIfConflict` (let's call it).

My questions:

* most importantly, compare `Set_Final` with Set_Original. How are they functioning similar, but also differently, and also, what if any optimizations or performance gains is there now in `Set_Final`. Am I correct that `Set_Final` may have TWO optimizations now (dropped calls to remove?)

Note: The four setter methods (note that now, `Set_Final` is now named just `Set` for both overloads):

```csharp
public void Set_Final(TKey key, TValue value) => Set_Final(key, value, Force);

public void Set_Final(TKey key, TValue value, bool force)
{
    ArgumentNullException.ThrowIfNull(key);
    ArgumentNullException.ThrowIfNull(value);

    if(_fmap.TryGetValue(key, out TValue? existingValue)) {
        if(_tvalsEqual(value, existingValue))
            return;
        // the value has changed, remove *existing* value from _rmap
        _rmap.Remove(existingValue);
    }

    // does the *new* value already exist? AND if so is it's key different??
    bool valueExistsWithDiffKey = _rmap.TryGetValue(value, out TKey? owningKey) && !_tkeysEqual(owningKey, key);
    if(valueExistsWithDiffKey) {
        if(!force)
            throw new ArgumentException($"Value '{value}' is already mapped to key '{owningKey}'.", nameof(value));
        _fmap.Remove(owningKey);
    }

    _fmap[key] = value;
    _rmap[value] = key;
}

public void Set_Original(TKey key, TValue value)
{
    ArgumentNullException.ThrowIfNull(key);
    ArgumentNullException.ThrowIfNull(value);

    // Check if this exact mapping already exists
    if(_fmap.TryGetValue(key, out var existingValue) && _tvalsEqual(existingValue, value))
        return; // No change needed

    // Remove existing mappings
    RemoveByKey(key);
    RemoveByValue(value);

    // Add the new mapping
    _fmap[key] = value;
    _rmap[value] = key;
}


public void ForcePut1(TKey key, TValue value)
{
    ArgumentNullException.ThrowIfNull(key);
    ArgumentNullException.ThrowIfNull(value);

    // Early-return if exact key→value map already exists (but only checking _forwardMap!)
    if(_fmap.TryGetValue(key, out TValue? existingValue)) {
        if(_tvalsEqual(value, existingValue))
            return;

        // KEY existed, VALUE is CHANGED. REMOVE existing value from _reverseMap. _forwardMap cleared in setter below
        _rmap.Remove(existingValue);
    }

    // NEW value MIGHT already be mapped to a different key, so REMOVE it from both IF found
    RemoveByValue(value);

    // Add the new mapping
    _fmap[key] = value;
    _rmap[value] = key;
}

public void Set_ThrowIfConflict(TKey key, TValue value)
{
    ArgumentNullException.ThrowIfNull(key);
    ArgumentNullException.ThrowIfNull(value);

    if(_fmap.TryGetValue(key, out TValue? existingValue)) {
        if(_tvalsEqual(value, existingValue))
            return; // exact mapping already exists, no-op
    }

    if(_rmap.TryGetValue(value, out TKey? owningKey) && !_tkeysEqual(owningKey, key))
        throw new ArgumentException($"Value '{value}' is already mapped to key '{owningKey}'.", nameof(value));

    if(existingValue != null)
        _rmap.Remove(existingValue);

    _fmap[key] = value;
    _rmap[value] = key;
}
```

+++

**GitHub Copilot:** Let me do a precise op-count trace for the worst case on all four methods first, then summarize.

---

### Full conflict path: `A→1` exists, calling `Set(A, 2)` where `B→2` also exists

**`Set_Original`:**
```
_fmap.TryGetValue(A)      → true, vals differ         op 1
RemoveByKey(A):
  _fmap.TryGetValue(A)    ← REDUNDANT (just did this) op 2 ✗
  _fmap.Remove(A)         ← WASTED (_fmap[A]=2 below) op 3 ✗
  _rmap.Remove(1)                                      op 4
RemoveByValue(2):
  _rmap.TryGetValue(2)                                 op 5
  _rmap.Remove(2)         ← WASTED (_rmap[2]=A below) op 6 ✗
  _fmap.Remove(B)                                      op 7
_fmap[A] = 2                                           op 8
_rmap[2] = A                                           op 9
```
**9 ops, 3 of which are wasteful**

**`Set_Final(force:true)`:**
```
_fmap.TryGetValue(A)      → true, vals differ         op 1
_rmap.Remove(1)                                        op 2
_rmap.TryGetValue(2)      → true                      op 3
_fmap.Remove(B)                                        op 4
_fmap[A] = 2                                           op 5
_rmap[2] = A                                           op 6
```
**6 ops, zero wasted**

---
Below is the entire discussion
---

### Your question: are there TWO optimizations? There are actually THREE

| Dropped operation | Why it's safe to drop |
|---|---|
| `_fmap.TryGetValue(key)` inside `RemoveByKey` | Already done on line 1 — result is in `existingValue` |
| `_fmap.Remove(key)` inside `RemoveByKey` | `_fmap[key] = value` at the end overwrites it, Remove + re-Add is strictly worse than a single indexer overwrite |
| `_rmap.Remove(value)` inside `RemoveByValue` | `_rmap[value] = key` at the end overwrites it — same reasoning |

You were right about two dropped Removes. The bonus third is the redundant `TryGetValue` that `RemoveByKey` was doing internally.

---

### Behavioral difference between `Set_Original` and `Set_Final`

They are **functionally identical when `force=true`** — both are forcePut semantics. The only behavioral addition is the `force=false` path in `Set_Final` which throws on value conflict, giving the Guava `put` / `Set_ThrowIfConflict` behavior in the same method body.

| | `Set_Original` | `Set_Final(force:true)` | `Set_Final(force:false)` |
|---|---|---|---|
| Semantics | forcePut | forcePut | put (throws on value conflict) |
| Full conflict ops | 9 | 6 | 6 (throws before writes) |
| Redundant TryGetValue | ✗ yes | ✓ no | ✓ no |
| Wasted Remove×2 | ✗ yes | ✓ no | ✓ no |

---

## Q

**User:** I need some very deep thinking on the following question. This is a bi-directional map / dictionary. Looking at the Set function, I'm concerned that the way this was written ... probably isn't standard. But for a bidirectional map / dictionary, I need the deepest of analysis, including online searching for comparisons, what the most "kosher" way of doing this is.

> Note line 143, how they wrote this current implementation (which maybe isn't the best), it DOES make sense however for *data-integrity* purposes, IF the key/value pair DOES already exist, the RISK is if the value being "set" has CHANGED, we do not want to ORPHAN data, particularly, we do not want the ORIGINAL value, which is the key in _reverseMap, to remain orphaned (set).

So deeply analyze what this code is doing within this Set, and then deeply look into what the most kosher way of doing this is. EVEN if we were to keep this same mode of operation, I am concerned that:

`_forwardMap.TryGetValue` may return TRUE, while the next check (is current value same as new input value) returns FALSE

Shouldn't then RemoveByValue on line 150 actually do:

`RemoveByValue(existingValue)`??


+++

**GitHub Copilot:** Let me read the relevant section of the `Set` method carefully before giving a full analysis.

---

## Deep Analysis of `Set`

### 1. What the method does, step by step

```csharp
public void Set(TKey key, TValue value)
{
    // (A) Guard: null-check both sides
    ArgumentNullException.ThrowIfNull(key);
    ArgumentNullException.ThrowIfNull(value);

    // (B) Early-return: if this exact key→value mapping already exists, done
    if(_forwardMap.TryGetValue(key, out TValue? existingValue) &&
         EqualityComparer<TValue>.Default.Equals(existingValue, value)) {
        return;
    }

    // (C) Remove stale mappings
    RemoveByKey(key);    // line 149
    RemoveByValue(value); // line 150

    // (D) Write the new mapping
    _forwardMap[key] = value;
    _reverseMap[value] = key;
}
```

The method tries to handle **all possible collisions** before writing:
- The key might already exist pointing to a *different* value (`oldKey → oldValue`)
- The new value might already be claimed by a *different* key (`otherKey → newValue`)

Both conflicts must be cleared to preserve bijectivity.

---

### 2. Direct answer to your question: `RemoveByValue(value)` vs `RemoveByValue(existingValue)`

**The current `RemoveByValue(value)` is correct.** Here is why, proven by tracing what each `Remove` call actually does:

`RemoveByKey(key)` (from the code you have):
```csharp
if(_forwardMap.TryGetValue(key, out var value)) {  // finds existingValue
    _forwardMap.Remove(key);          // removes key → existingValue
    _reverseMap.Remove(value);        // removes existingValue → key  ← the orphan you feared
    return true;
}
```

**`RemoveByKey(key)` already removes `existingValue` from `_reverseMap`.** The feared orphan is killed here, on line 149, before line 150 is ever reached.

Then `RemoveByValue(value)` (line 150) is solving an entirely *different* problem — cleaning up the case where the **new `value` is already owned by some other key**:

```csharp
if(_reverseMap.TryGetValue(value, out var key)) {  // finds otherKey
    _reverseMap.Remove(value);     // removes value → otherKey
    _forwardMap.Remove(key);       // removes otherKey → value  ← this is the orphan this call handles
    return true;
}
```

If you changed line 150 to `RemoveByValue(existingValue)`:
- It would be a **no-op** — `RemoveByKey(key)` already deleted `existingValue` from `_reverseMap`
- You'd **miss** clearing the `otherKey → value` conflict, leaving `_forwardMap` with a dangling entry → **data corruption**

Concrete proof with a worked example:

| State before `Set(A, 2)` | `_forwardMap` | `_reverseMap` |
|---|---|---|
| Initial: A→1, B→2 | `{A:1, B:2}` | `{1:A, 2:B}` |
| After `RemoveByKey(A)` | `{B:2}` | `{2:B}` — `1:A` gone ✓ |
| After `RemoveByValue(2)` | `{}` | `{}` — `B:2` gone ✓ |
| After write | `{A:2}` | `{2:A}` ✓ |

If instead `RemoveByValue(1)` (existingValue) were called after `RemoveByKey(A)`, the `B→2`/`2→B` pair would survive, and then the write `_forwardMap[A]=2; _reverseMap[2]=A` would **silently overwrite** `_reverseMap[2]` to `A` while leaving the **orphaned** `B→2` in `_forwardMap`.

---

### 3. Real bug: wrong equality comparer in the early-return check

This is the most tangible actual bug in the method. The early-return check uses:

```csharp
EqualityComparer<TValue>.Default.Equals(existingValue, value)
```

But the constructor accepts a custom comparer:

```csharp
_reverseMap = new Dictionary<TValue, TKey>(valueComparer);
```

The `_reverseMap` uses `valueComparer`. The early-return uses `Default`. **These can disagree.**

Example:
```csharp
var d = new BidirectionalDictionary<string, string>(
    StringComparer.OrdinalIgnoreCase,
    StringComparer.OrdinalIgnoreCase);

d.Add("hello", "WORLD");
d.Set("hello", "world");  // Should be a no-op — same key, same value by the chosen comparer
```

The early-return check does `Default.Equals("WORLD", "world")` → `false` (ordinal). So it doesn't return early. It then does `RemoveByKey("hello")` and `RemoveByValue("world")` — the second one hits `_reverseMap["world"]` (which doesn't exist because it was stored under `"WORLD"`), finds nothing, and the write proceeds. The result is still functionally correct, but:
- It does unnecessary work
- More critically, it is **semantically inconsistent**: the map thinks `"WORLD"` and `"world"` are different values even though its own comparer says they're the same

The fix is to use the dictionary's own comparer:
```csharp
if(_forwardMap.TryGetValue(key, out TValue? existingValue) &&
     _reverseMap.Comparer.Equals(existingValue, value)) {  // ← use the map's actual comparer
    return;
}
```

`Dictionary<TValue, TKey>.Comparer` is a public property that returns the `IEqualityComparer<TValue>` the dictionary was constructed with.

---

### 4. How this compares to the "kosher" standard (Guava BiMap)

The most authoritative reference for bidirectional maps is **Guava's `BiMap<K,V>`** (Java). Its design deliberately separates two different semantics:

| Method | Behavior on key conflict | Behavior on value conflict |
|---|---|---|
| `put(key, value)` | Replaces old mapping | **Throws** `IllegalArgumentException` |
| `forcePut(key, value)` | Replaces old mapping | Silently removes conflicting mapping |

Your `Set` method implements **`forcePut` semantics** — it silently wins all conflicts. This is a valid design choice, but Guava's rationale for making `put` stricter is: if you're about to blow away an existing `otherKey → newValue` mapping that another part of your code depends on, you probably want a loud failure, not a silent remove.

A more idiomatic C# API might look like:
```csharp
// Throws ArgumentException if value is already mapped to a different key
public void Add(TKey key, TValue value) { ... }  // already exists, good

// Replace key's mapping; throws if new value is already taken
public void Set(TKey key, TValue value) { ... }  // like Guava put

// Forcefully replace, removing any conflicting mapping
public void ForcePut1(TKey key, TValue value) { ... }  // like Guava forcePut
```

---

### 5. Minor structural observation: the early-return is asymmetric

The early-return only checks the forward direction:
```csharp
_forwardMap.TryGetValue(key, ...) && valuesEqual
```

In a correctly maintained map, this is sufficient — if `key → value` exists in forward, `value → key` must exist in reverse. But if defensiveness is desired (e.g., to detect internal corruption), you might also assert the reverse:
```csharp
Debug.Assert(!existingValue_equals_value || _reverseMap.ContainsKey(value),
    "Map is internally inconsistent");
```

---

### Summary

| Concern | Verdict |
|---|---|
| `RemoveByValue(value)` on line 150 is wrong | **No — it's correct.** It removes `otherKey → newValue` conflicts. |
| `RemoveByValue(existingValue)` would be better | **No — it would be a no-op + miss a real conflict.** `RemoveByKey` already handles `existingValue`. |
| Early-return comparer bug | **Real bug.** Should use `_reverseMap.Comparer` not `EqualityComparer<TValue>.Default`. |
| Overall Set semantics | Valid "forcePut" design. Consider separating "strict put" from "force put" per Guava's model. |

## Q

> **User:** Great analysis. I have a lot to respond to, it's very helpful to see this as a ForcePut (compared to Guava), among other points. But as a first step, the changes I just made may not be worth the minor performance gain, but look at my changes, I'll let you figure out what this is doing, and if it's correct, and if it's worth it


+++

**GitHub Copilot:** The logic is correct and the optimization is real. Let me trace all three execution paths precisely.

### Path A — Key exists, same value (early return)
```
TryGetValue(key) → true, existingValue = oldValue
Comparer.Equals(newValue, oldValue) → true
→ return  ✓
```
No change, no work.

### Path B — Key exists, *different* value (the optimized path)
```
TryGetValue(key) → true, existingValue = oldValue
Comparer.Equals(newValue, oldValue) → false
→ _reverseMap.Remove(existingValue)   ← directly removes oldValue→key, no lookup needed
→ RemoveByValue(newValue)             ← evicts any otherKey→newValue conflict
→ _forwardMap[key] = newValue         ← overwrites in place (no Remove + Add)
→ _reverseMap[newValue] = key         ✓
```
This is sound and is the real performance win. The original code called `RemoveByKey(key)` here, which would do a redundant `TryGetValue(key)` we already did, then a `Remove(key)` + `Remove(existingValue)`. The new code replaces all that with a single direct `_reverseMap.Remove(existingValue)` and an indexer overwrite.

### Path C — Key does NOT exist
```
TryGetValue(key) → false
→ else { RemoveByKey(key); }  ← **dead code**
→ RemoveByValue(newValue)     ← evicts any otherKey→newValue conflict
→ _forwardMap[key] = newValue
→ _reverseMap[newValue] = key  ✓
```

---

### The one problem: the `else` branch is dead code

When `TryGetValue(key)` returns `false`, `key` is definitionally not in `_forwardMap`. `RemoveByKey(key)` immediately does its own `TryGetValue(key)` → `false` and returns. It can **never** do anything. The branch should simply be deleted:

```csharp
if (_forwardMap.TryGetValue(key, out TValue? existingValue))
{
    if (_reverseMap.Comparer.Equals(value, existingValue))
        return;

    _reverseMap.Remove(existingValue);
}
// else: key not in map, nothing to clean up on the key side

RemoveByValue(value);
_forwardMap[key] = value;
_reverseMap[value] = key;
```

---

### Summary

| | Verdict |
|---|---|
| Core logic correctness | ✅ Correct across all paths |
| `_reverseMap.Comparer.Equals` fix | ✅ Correctly addresses the comparer bug from before |
| Performance win (Path B) | ✅ Real — saves one `TryGetValue`, converts `Remove`+`Add` to an overwrite |
| `else { RemoveByKey(key); }` | ❌ Unconditionally dead code — remove it |
| Worth the complexity? | Marginal. The trade-off is a slightly less linear flow for a dictionary-operation saved on one path. |

## Q

> **User:** Is there something that RemoveByValue is doing line 155 here, that could be shortened in some scenarios? I don't think so, but just looking


+++

**GitHub Copilot:** No, and your intuition is right. Here's why no shortcut is available:

`RemoveByValue(value)` at line 155 does two things:

```csharp
_reverseMap.Remove(value);      // 1. remove value → otherKey from reverse
_forwardMap.Remove(otherKey);   // 2. remove otherKey → value from forward
```

The question would be: in **Path B** (key existed, value changed), could either of those ops be skipped since we already did `_reverseMap.Remove(existingValue)` above?

- **Step 1** (`_reverseMap.Remove(value)`) — operates on `value`, not `existingValue`. These are guaranteed different (the comparer check above confirmed that), so no overlap.
- **Step 2** (`_forwardMap.Remove(otherKey)`) — `otherKey` is whatever key was previously mapped to `value`. Could it be our same `key`? No — that would require `_forwardMap[key] == value`, which directly contradicts the `existingValue != value` check we just passed. So `otherKey` is always a third, unrelated key and its forward entry still needs to be cleaned up.

In **Path C** (key didn't exist), there's nothing pre-done at all, so obviously no shortcut.

The two removals in `RemoveByValue` target entirely independent entries from what was already cleaned up. No deduplication is possible.

## Q

> **User:** Confirm that the current `ForcePut1` is like Guava's forcePut, and then, please make a Set, like Guava had


+++

**GitHub Copilot:** **Yes, confirmed.** `ForcePut1` is exactly `forcePut`: it silently evicts both the old `key→existingValue` mapping *and* any existing `otherKey→newValue` mapping. No complaints, no throws.

---

Guava's `put` semantics:
- Key already exists with a **different** value → **allowed**, replaces it (like forcePut)
- New value already belongs to a **different** key → **throws** `IllegalArgumentException`
- Exact mapping already exists → no-op

Here is the `Set` method to add alongside `ForcePut1`:

```csharp
/// <summary>
/// Sets the value for the specified key.
/// If the key already exists, its old mapping is removed and replaced.
/// Throws if the new value is already mapped to a different key.
/// Use <see cref="ForcePut1"/> to silently replace conflicting value mappings instead.
/// </summary>
/// <exception cref="ArgumentException">The value is already mapped to a different key.</exception>
/// <exception cref="ArgumentNullException">The key or value is null.</exception>
public void Set(TKey key, TValue value)
{
    ArgumentNullException.ThrowIfNull(key);
    ArgumentNullException.ThrowIfNull(value);

    if(_fmap.TryGetValue(key, out TValue? existingValue)) {
        if(_revComparer.Equals(value, existingValue))
            return; // exact mapping already exists, no-op
    }

    // Throw if new value is already claimed by a DIFFERENT key
    if(_rmap.TryGetValue(value, out TKey? owningKey) &&
        !_fmap.Comparer.Equals(owningKey, key))
        throw new ArgumentException(
            $"Value '{value}' is already mapped to key '{owningKey}'.", nameof(value));

    // Safe to replace: remove old value entry for this key (if any), then write
    if(existingValue is not null)
        _rmap.Remove(existingValue);

    _fmap[key] = value;
    _rmap[value] = key;
}
```

Key points:

- The `owningKey` check uses `_fmap.Comparer` (the key comparer) — consistent with how the forward map identifies keys, same reasoning as your `_revComparer` field for values.
- `existingValue is not null` is the way to test whether the `TryGetValue` branch was taken — `existingValue` is only populated if `TryGetValue` returned true, and `TValue : notnull` ensures a real populated value is never null, so this is a clean idiom here.
- No `RemoveByValue(value)` needed — we just threw if there was a conflict on that side.

## Q

> **User:** To be clear, this Set implementation you offered (like Guava's put), is essentially for scenarios where key/values should *always* be unique? I.e. where the given data being worked with, expects them always to be unique pairs, 1 to 1, as if two ids, that should never mix with another?


+++

**GitHub Copilot:** Yes, essentially — but the precision worth having is: **both methods always enforce uniqueness**, that's what a bidirectional map *is*. The difference is what happens when a conflict is detected on the *value* side:

- **`ForcePut1`** (forcePut): "I don't care who owned that value before — evict them, it's mine now." Useful when you're rebuilding state, syncing, or doing last-write-wins updates.

- **`Set`** (put): "I'm updating this key's mapping, but I *expect* the new value to be unowned. If it isn't, that's a bug in my calling code — tell me loudly."

The `Set` throw is essentially a **domain assertion**. You'd use it when your data model says two distinct keys should *never legitimately compete for the same value* — e.g.:
- Reassigning a username to a new user ID: if that user ID is already taken, that's a logic error upstream, not something to silently resolve
- Mapping country codes to currencies in a region where each currency is exclusive

`ForcePut1` is the right choice when the *caller* has authority to reassign — e.g. an admin tool overriding assignments, or a sync process where the incoming data is the source of truth.

So to directly answer: `Set` is for when a value conflict means *"something went wrong in the business logic"*, while `ForcePut1` is for when it means *"I'm intentionally taking over"*.

## Q

> **User:**  Looking at your suggested `Set` method, I entered that in code with function name `Set_ThrowIfConflict`. I have some questions about it. Particularly on the if(_rmap.TryGetValue... part, line 183

I want to understand the basics of what is being allowed or denied here (code aside). It seems this code ALLOWS a single key's value to change.... but does NOT allow this if that value had already been associated with an existing key?


+++

**GitHub Copilot:** Exactly right. The four scenarios, plain English:

| Situation | Result |
|---|---|
| `key → value` already exists exactly | No-op (early return) |
| `key` exists but mapped to a *different* value, and new `value` is unclaimed | **Allowed** — key's mapping is updated |
| `key` is new, `value` is unclaimed | **Allowed** — fresh insert |
| `value` is already owned by a **different** key | **Throws** |

The `!_tkeysEqual(owningKey, key)` part in the throw condition is worth noting specifically. It's asking: *"is the key that currently owns this value the same key I'm setting?"* In a healthy, consistent map, that can **never** be true at that point — if `_rmap[value] == key`, then `_fmap[key]` would have to equal `value`, which the early-return check already caught and returned from. So `owningKey == key` at that line would only occur if the map were internally corrupted. The `!_tkeysEqual` is a purely defensive guard you could technically drop, but it's harmless to keep.

The short version of the intent: **"You may reassign a key to a new value, provided no one else already has that value."**

## Q

> **User:** It seems like this checks for scenarios besides just interal data integrity, is that correct? Internal errors should dictate the two way map is NEVER allowed to be incorrect (worse of which, if the two dictionaries literally have a different count, but also each key/value pair should be the same in a full analysis).

It seems like this check, goes a bit beyond simply validating internal integrity, but is that correct or not?


+++

**GitHub Copilot:** Correct — it goes beyond internal integrity. They're two separate concerns:

**Internal integrity checks** ask: *"Is the map itself in a valid state?"*
- `_fmap.Count == _rmap.Count`
- For every `_fmap[k] == v`, does `_rmap[v] == k`?

These would be `Debug.Assert`s or a dedicated `Validate()` method. If they ever fire, it's a bug *inside* the class.

**What `Set_ThrowIfConflict` is doing** is different — it asks: *"Is the operation the caller is requesting valid under the rules of this method?"*

The map itself is healthy and internally consistent. There's no corruption. It's purely an **input precondition**: you're calling `Set` with a value that is already legitimately owned by another key. That's not a map integrity failure — it's the caller doing something `Set` semantics don't permit (but `ForcePut1` would allow).

The analogy: a database table with a unique constraint on a column isn't *broken* when an `INSERT` violates it. The table is fine. The *request* is the problem.

So to be precise:

| Check type | Responsibility | What it catches |
|---|---|---|
| Internal integrity | Class invariant, always true | Bugs inside BidirectionalDictionary itself |
| `Set_ThrowIfConflict`'s `TryGetValue` check | API contract / precondition | Caller attempting a semantically invalid operation on valid data |

The `!_tkeysEqual(owningKey, key)` part is the only sliver that *could* relate to integrity — as noted last time, if that negation ever turns out to be false at that point, it would imply internal corruption. But its primary purpose is still the API contract guard.

## Q

> **User:** I'm wondering if the two could be combined, but with either an argument or a property boolean setting, (DontAllowValueKeyToChange ... horrible name). Maybe not best idea, but it helps to understand, maybe make a merged third method, for comparison


+++

**GitHub Copilot:** Here's the merged version — the only behavioral fork is the single conflict point:

```csharp
public void Set(TKey key, TValue value, bool force = false)
{
    ArgumentNullException.ThrowIfNull(key);
    ArgumentNullException.ThrowIfNull(value);

    if(_fmap.TryGetValue(key, out TValue? existingValue)) {
        if(_tvalsEqual(value, existingValue))
            return;
        _rmap.Remove(existingValue); // key's old value → evicted from reverse
    }

    if(_rmap.TryGetValue(value, out TKey? owningKey) && !_tkeysEqual(owningKey, key)) {
        if(!force)
            throw new ArgumentException(
                $"Value '{value}' is already mapped to key '{owningKey}'.", nameof(value));
        _fmap.Remove(owningKey); // force: evict the conflicting forward entry
        // _rmap[value] gets overwritten below — no explicit remove needed
    }

    _fmap[key] = value;
    _rmap[value] = key;
}
```

Then `ForcePut1` and `Set_ThrowIfConflict` collapse to one-liners:
```csharp
public void ForcePut1(TKey key, TValue value) => Set(key, value, force: true);
```

---

**Why the force path only removes `_fmap[owningKey]` and not `_rmap[value]`:**

`_rmap[value] = key` at the bottom is an overwrite — it replaces whatever was there. So the reverse entry is handled implicitly. The only orphan that needs explicit cleanup is `owningKey`'s forward entry, since nothing will overwrite `_fmap[owningKey]`.

---

**On whether this merged design is worth it:**

The single parameter does express the relationship cleanly — it makes obvious they're the *same operation with one policy difference*. The downside is that `force` is a stringly-typed boolean trap: `Set(key, value, true)` at a call site is opaque. `ForcePut1(key, value)` is unambiguous. A reasonable middle ground is to keep both named methods but have them delegate to the merged internal implementation (marked `private` or kept as the single `Set` with a named enum instead of bool):

```csharp
public enum SetConflictPolicy { Throw, Force }
public void Set(TKey key, TValue value, SetConflictPolicy onConflict = SetConflictPolicy.Throw)
```

That makes call sites self-documenting without requiring two separate method bodies.

## Q

> **User:** What are the performance implications of this merged route? (I renamed this, for the moment, `Set_Final`)

Assuming that, usually (or whether or not, we want to optimize to this being rare), there won't be a conflict like this often (a value that had a different key).

In the current way (with my minor modifications of your code, please check first), is it correct that `RemoveByValue(value);` is being compared to lines 184-189. It *seems* to me that, with my stated optimized route (no conflict), that BOTH routes start with a call to:

_rmap.TryGetValue(...)

and if we usually don't expect this to be true, the performance in those cases will be identical, except that the old `ForcePut1` way had the overhead of an extra method call


+++

**GitHub Copilot:** Your analysis is correct. Let me trace them side by side for the no-conflict path only (the common case you're optimizing for).

**`ForcePut1` no-conflict path:**
```csharp
RemoveByValue(value);           // method call overhead
  → _rmap.TryGetValue(value)    // → false, returns
_fmap[key] = value;
_rmap[value] = key;
```

**`Set_Final` no-conflict path:**
```csharp
_rmap.TryGetValue(value, ...)   // → false, valueHasDiffKey = false
// if block skipped
_fmap[key] = value;
_rmap[value] = key;
```

One `_rmap.TryGetValue` call each — identical work at the dictionary level. The only difference is `ForcePut1` has the method call frame for `RemoveByValue`. Whether that matters in practice depends on whether the JIT inlines it (it likely will — `RemoveByValue` is small and called frequently), so at runtime the difference is probably zero.

**The conflict path is where `Set_Final` actually wins more concretely:**

| | `ForcePut1` (conflict) | `Set_Final` force=true (conflict) |
|---|---|---|
| `_rmap.TryGetValue(value)` | ✓ (inside RemoveByValue) | ✓ |
| `_rmap.Remove(value)` | ✓ (inside RemoveByValue) | ✗ — overwritten by `_rmap[value]=key` below |
| `_fmap.Remove(owningKey)` | ✓ (inside RemoveByValue) | ✓ |

`Set_Final` saves one explicit `_rmap.Remove` on the conflict path by relying on the indexer overwrite — that's a real saved dictionary operation, not just method call overhead.

So your instinct is right: the no-conflict win is marginal (method call that's likely inlined anyway), but the structural clarity and the conflict-path savings are the stronger arguments for the merged form.

## Q

> **User:** But if anything, the merged way, surprisingly, has promise to if anything be faster even (if including when there is a conflict)


+++

**GitHub Copilot:** Yes, and it's not just marginal — it's a concrete saved dictionary operation. Here's the precise count:

**`ForcePut1` — conflict path** (key exists with different value + new value owned by another key):
```
_rmap.Remove(existingValue)        ← 1
RemoveByValue(value):
  _rmap.TryGetValue(value)         ← 2
  _rmap.Remove(value)              ← 3   ← THIS is the one that's wasted
  _fmap.Remove(owningKey)          ← 4
_fmap[key] = value                 ← 5
_rmap[value] = key                 ← 6
```
**6 dictionary operations**

**`Set_Final(force:true)` — same conflict path:**
```
_rmap.Remove(existingValue)        ← 1
_rmap.TryGetValue(value)           ← 2
_fmap.Remove(owningKey)            ← 3
_fmap[key] = value                 ← 4
_rmap[value] = key                 ← 5  ← overwrites implicitly, no Remove needed
```
**5 dictionary operations**

The saved op is `_rmap.Remove(value)` — `ForcePut1` explicitly removes it via `RemoveByValue`, then immediately re-adds it via `_rmap[value] = key`. Those two operations on the same key cancel out. `Set_Final` never does the remove at all, letting the final indexer assignment handle it in one shot.

So the summary across all paths:

| Path | `ForcePut1` ops | `Set_Final` ops | Savings |
|---|---|---|---|
| Exact match (no-op) | 1 (`TryGetValue`) | 1 (`TryGetValue`) | 0 |
| Key exists, no value conflict | 4 + method call | 4 | method call frame |
| Key new, no value conflict | 3 + method call | 3 | method call frame |
| Full conflict (key + value both collide) | 6 + method call | 5 | 1 dict op + method call |

`Set_Final` is never slower and is genuinely faster on the conflict path — the surprise being that the merged form isn't just cleaner, it eliminates a structurally redundant remove.
