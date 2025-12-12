using OnlineSurvey.Domain.Shared;

namespace OnlineSurvey.Domain.Tests.Shared;

public class ValueObjectTests
{
    private class TestValueObject : ValueObject
    {
        public string Value1 { get; }
        public int Value2 { get; }

        public TestValueObject(string value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value1;
            yield return Value2;
        }
    }

    private class AnotherValueObject : ValueObject
    {
        public string Value { get; }

        public AnotherValueObject(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var vo1 = new TestValueObject("test", 123);
        var vo2 = new TestValueObject("test", 123);

        Assert.True(vo1.Equals(vo2));
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var vo1 = new TestValueObject("test", 123);
        var vo2 = new TestValueObject("test", 456);

        Assert.False(vo1.Equals(vo2));
    }

    [Fact]
    public void Equals_DifferentStringValues_ReturnsFalse()
    {
        var vo1 = new TestValueObject("test1", 123);
        var vo2 = new TestValueObject("test2", 123);

        Assert.False(vo1.Equals(vo2));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var vo = new TestValueObject("test", 123);

        Assert.False(vo.Equals(null));
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var vo1 = new TestValueObject("test", 123);
        var vo2 = new AnotherValueObject("test");

        Assert.False(vo1.Equals(vo2));
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var vo = new TestValueObject("test", 123);

        Assert.True(vo.Equals(vo));
    }

    [Fact]
    public void IEquatable_Equals_SameValues_ReturnsTrue()
    {
        var vo1 = new TestValueObject("test", 123);
        var vo2 = new TestValueObject("test", 123);

        Assert.True(((IEquatable<ValueObject>)vo1).Equals(vo2));
    }

    [Fact]
    public void IEquatable_Equals_Null_ReturnsFalse()
    {
        var vo = new TestValueObject("test", 123);

        Assert.False(((IEquatable<ValueObject>)vo).Equals(null));
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var vo1 = new TestValueObject("test", 123);
        var vo2 = new TestValueObject("test", 123);

        Assert.Equal(vo1.GetHashCode(), vo2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ReturnsDifferentHash()
    {
        var vo1 = new TestValueObject("test", 123);
        var vo2 = new TestValueObject("test", 456);

        Assert.NotEqual(vo1.GetHashCode(), vo2.GetHashCode());
    }

    [Fact]
    public void CanBeUsedInHashSet()
    {
        var vo1 = new TestValueObject("test", 123);
        var vo2 = new TestValueObject("test", 123);
        var vo3 = new TestValueObject("other", 456);

        var set = new HashSet<TestValueObject> { vo1, vo2, vo3 };

        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void CanBeUsedAsDictionaryKey()
    {
        var vo1 = new TestValueObject("test", 123);
        var vo2 = new TestValueObject("test", 123);

        var dict = new Dictionary<TestValueObject, string>
        {
            { vo1, "value1" }
        };

        Assert.True(dict.ContainsKey(vo2));
        Assert.Equal("value1", dict[vo2]);
    }
}