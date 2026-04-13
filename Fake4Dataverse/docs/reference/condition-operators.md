# Condition Operators Reference

Fake4Dataverse supports 40+ `ConditionOperator` values for `QueryExpression`
filtering. The same operator families are also implemented for direct FetchXml
filtering and `FetchXmlToQueryExpressionRequest` where an equivalent FetchXml
operator exists.

## Usage

```csharp
var query = new QueryExpression("contact");
query.ColumnSet = new ColumnSet("fullname", "emailaddress1");

// Single-value operator
query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

// No-value operator
query.Criteria.AddCondition("emailaddress1", ConditionOperator.NotNull);

// Multi-value operator
query.Criteria.AddCondition("statuscode", ConditionOperator.In, 1, 2, 3);

var results = service.RetrieveMultiple(query);
```

## Operator Reference

### Comparison Operators

| Operator | Values | Description |
|---|---|---|
| `Equal` | 1 value | Field equals the specified value |
| `NotEqual` | 1 value | Field does not equal the specified value |
| `Null` | none | Field is null |
| `NotNull` | none | Field is not null |
| `GreaterThan` | 1 value | Field is greater than the value |
| `GreaterEqual` | 1 value | Field is greater than or equal to the value |
| `LessThan` | 1 value | Field is less than the value |
| `LessEqual` | 1 value | Field is less than or equal to the value |
| `Between` | 2 values | Field is between value1 and value2 (inclusive) |
| `NotBetween` | 2 values | Field is not between value1 and value2 |

### String Operators

| Operator | Values | Description |
|---|---|---|
| `Like` | 1 value | Pattern match using `%` as wildcard |
| `NotLike` | 1 value | Does not match the pattern |
| `BeginsWith` | 1 value | Field starts with the specified string |
| `DoesNotBeginWith` | 1 value | Field does not start with the specified string |
| `EndsWith` | 1 value | Field ends with the specified string |
| `DoesNotEndWith` | 1 value | Field does not end with the specified string |
| `Contains` | 1 value | Field contains the substring |
| `DoesNotContain` | 1 value | Field does not contain the substring |

```csharp
// Find contacts whose name starts with "A"
query.Criteria.AddCondition("lastname", ConditionOperator.BeginsWith, "A");

// Equivalent using Like with wildcard
query.Criteria.AddCondition("lastname", ConditionOperator.Like, "A%");
```

### Collection Operators

| Operator | Values | Description |
|---|---|---|
| `In` | N values | Field is one of the specified values |
| `NotIn` | N values | Field is not one of the specified values |
| `ContainValues` | N values | Multi-select option set contains all specified values |
| `DoesNotContainValues` | N values | Multi-select option set does not contain the specified values |

```csharp
// Filter to specific status codes
query.Criteria.AddCondition("statuscode", ConditionOperator.In, 1, 2, 3);
```

### Date/Time — Relative Day

| Operator | Values | Description |
|---|---|---|
| `Yesterday` | none | Date is yesterday |
| `Today` | none | Date is today |
| `Tomorrow` | none | Date is tomorrow |

### Date/Time — Relative Week / Month / Year

| Operator | Values | Description |
|---|---|---|
| `Last7Days` | none | Within the last 7 days |
| `Next7Days` | none | Within the next 7 days |
| `LastWeek` | none | Date falls in the previous calendar week |
| `ThisWeek` | none | Date falls in the current calendar week |
| `NextWeek` | none | Date falls in the next calendar week |
| `LastMonth` | none | Date falls in the previous calendar month |
| `ThisMonth` | none | Date falls in the current calendar month |
| `NextMonth` | none | Date falls in the next calendar month |
| `LastYear` | none | Date falls in the previous calendar year |
| `ThisYear` | none | Date falls in the current calendar year |
| `NextYear` | none | Date falls in the next calendar year |

### Date/Time — LastX / NextX

| Operator | Values | Description |
|---|---|---|
| `LastXHours` | 1 (int) | Within the last X hours |
| `NextXHours` | 1 (int) | Within the next X hours |
| `LastXDays` | 1 (int) | Within the last X days |
| `NextXDays` | 1 (int) | Within the next X days |
| `LastXWeeks` | 1 (int) | Within the last X weeks |
| `NextXWeeks` | 1 (int) | Within the next X weeks |
| `LastXMonths` | 1 (int) | Within the last X months |
| `NextXMonths` | 1 (int) | Within the next X months |
| `LastXYears` | 1 (int) | Within the last X years |
| `NextXYears` | 1 (int) | Within the next X years |

```csharp
// Contacts modified in the last 30 days
query.Criteria.AddCondition("modifiedon", ConditionOperator.LastXDays, 30);
```

### Date/Time — Absolute

| Operator | Values | Description |
|---|---|---|
| `On` | 1 (date) | Date is on the specific date |
| `OnOrBefore` | 1 (date) | Date is on or before the specified date |
| `OnOrAfter` | 1 (date) | Date is on or after the specified date |

```csharp
// Records created on or after January 1, 2025
query.Criteria.AddCondition("createdon", ConditionOperator.OnOrAfter, new DateTime(2025, 1, 1));
```

### Date/Time — OlderThanX (Age-Based)

| Operator | Values | Description |
|---|---|---|
| `OlderThanXMinutes` | 1 (int) | Older than X minutes ago |
| `OlderThanXHours` | 1 (int) | Older than X hours ago |
| `OlderThanXDays` | 1 (int) | Older than X days ago |
| `OlderThanXWeeks` | 1 (int) | Older than X weeks ago |
| `OlderThanXMonths` | 1 (int) | Older than X months ago |
| `OlderThanXYears` | 1 (int) | Older than X years ago |

### User & Business Unit Context

| Operator | Values | Description |
|---|---|---|
| `EqualUserId` | none | Field equals the current user (`CallerId`) |
| `NotEqualUserId` | none | Field does not equal the current user |
| `EqualBusinessId` | none | Field equals the current user's business unit |
| `NotEqualBusinessId` | none | Field does not equal the current user's business unit |

```csharp
// Records owned by the current user
query.Criteria.AddCondition("ownerid", ConditionOperator.EqualUserId);
```

## Notes

- **Date operators use the environment's Clock for "now"** — inject a `FakeClock` via `env.Clock` for deterministic testing of relative date filters.
- **String operators are case-insensitive**, matching Dataverse behavior.
- **`In` / `NotIn`** accept arrays of any comparable type (int, string, Guid, etc.).
- **`Between` / `NotBetween`** require exactly two values and the comparison is inclusive.
- **`Like` wildcards** — use `%` to match any sequence of characters (e.g., `%corp%` matches "Contoso Corp Ltd").
- Equivalent FetchXml operators are supported for direct FetchXml execution and
	`FetchXmlToQueryExpressionRequest` conversion.
- `QueryExpressionToFetchXmlRequest` serializes the operators implemented by its
	handler and throws `NotSupportedException` for unsupported operators instead
	of silently downgrading them.
