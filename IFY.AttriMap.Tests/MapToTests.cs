namespace IFY.AttriMap.Tests;

[TestClass]
public sealed class MapToTests
{
    public class TestSource
    {
        [MapTo<TestTarget>(nameof(TestTarget.TName))]
        public string SName { get; init; } = string.Empty;
        public string SValue { get; init; } = string.Empty;
        [MapTo(typeof(TestTarget), nameof(TestTarget.TDateOfBirth), nameof(StringToDateOnly))]
        public string SDateOfBirth { get; init; } = string.Empty;
        public static DateOnly StringToDateOnly(string input) => DateOnly.Parse(input); // Example transformer method
        [MapTo<TestTarget>]
        public int Count { get; init; }
    }

    public interface ITestSource
    {
        [MapTo<TestTarget>(nameof(TestTarget.TName))]
        string SName { get; }
        string SValue { get; }
        [MapTo(typeof(TestTarget), nameof(TestTarget.TDateOfBirth), nameof(StringToDateOnly))]
        string SDateOfBirth { get; }
        public DateOnly StringToDateOnly(string input) => DateOnly.Parse(input); // Example transformer method
        [MapTo<TestTarget>]
        int Count { get; }
    }
    public class TestSourceImpl : ITestSource
    {
        public string SName { get; init; } = string.Empty;
        public string SValue { get; init; } = string.Empty;
        public string SDateOfBirth { get; init; } = string.Empty;
        public int Count { get; init; }
    }

    public class TestTarget
    {
        public string TName { get; init; } = string.Empty;
        public string TValue { get; init; } = string.Empty;
        public DateOnly TDateOfBirth { get; init; }
        public int Count { get; init; }
    }

    // TODO: Map to interface

    [TestMethod]
    public void Source_maps_to_target()
    {
        // Arrange
        var source = new TestSource
        {
            SName = "Test Name",
            SValue = "Test Value",
            SDateOfBirth = "2001-02-03",
            Count = 10
        };

        // Act
        var target = source.ToTestTarget();

        // Assert
        Assert.IsNotNull(target);
        Assert.AreEqual("Test Name", target.TName);
        Assert.AreEqual("", target.TValue);
        Assert.AreEqual("2001-02-03", target.TDateOfBirth.ToString("yyyy-MM-dd"));
        Assert.AreEqual(10, target.Count);
    }

    [TestMethod]
    public void Source_interface_maps_to_target()
    {
        // Arrange
        var source = new TestSourceImpl
        {
            SName = "Test Name",
            SValue = "Test Value",
            SDateOfBirth = "2001-02-03",
            Count = 10
        };

        // Act
        var target = source.ToTestTarget();

        // Assert
        Assert.IsNotNull(target);
        Assert.AreEqual("Test Name", target.TName);
        Assert.AreEqual("", target.TValue);
        Assert.AreEqual("2001-02-03", target.TDateOfBirth.ToString("yyyy-MM-dd"));
        Assert.AreEqual(10, target.Count);
    }
}