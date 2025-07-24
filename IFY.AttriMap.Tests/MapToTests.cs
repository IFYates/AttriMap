namespace IFY.AttriMap.Tests;

[TestClass]
public sealed class MapToTests
{
    public class TestSource
    {
        [MapTo<TestTarget>(nameof(TestTarget.Name))]
        public string Name { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        [MapTo(typeof(TestTarget), nameof(TestTarget.BirthDate), nameof(StringToDateOnly))]
        public string DateOfBirth { get; init; } = string.Empty;
        public static DateOnly StringToDateOnly(string input) => DateOnly.Parse(input); // Example transformer method
    }

    public interface ITestSource
    {
        [MapTo<TestTarget>(nameof(TestTarget.Name))]
        string Name { get; }
        string Value { get; }
        [MapTo(typeof(TestTarget), nameof(TestTarget.BirthDate), nameof(StringToDateOnly))]
        string DateOfBirth { get; }
        public static DateOnly StringToDateOnly(string input) => DateOnly.Parse(input); // Example transformer method
    }
    public class TestSourceImpl : ITestSource
    {
        public string Name { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string DateOfBirth { get; init; } = string.Empty;
    }

    public class TestTarget
    {
        public string Name { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public DateOnly BirthDate { get; init; }
    }

    [TestMethod]
    public void Source_maps_to_target()
    {
        // Arrange
        var source = new TestSource
        {
            Name = "Test Name",
            Value = "Test Value",
            DateOfBirth = "2001-02-03",
        };

        // Act
        var target = source.ToTestTarget();

        // Assert
        Assert.IsNotNull(target);
        Assert.AreEqual("Test Name", target.Name);
        Assert.AreEqual("", target.Value);
        Assert.AreEqual("2001-02-03", target.BirthDate.ToString("yyyy-MM-dd"));
    }

    [TestMethod]
    public void Source_interface_maps_to_target()
    {
        // Arrange
        var source = new TestSourceImpl
        {
            Name = "Test Name",
            Value = "Test Value",
            DateOfBirth = "2001-02-03",
        };

        // Act
        var target = source.ToTestTarget();

        // Assert
        Assert.IsNotNull(target);
        Assert.AreEqual("Test Name", target.Name);
        Assert.AreEqual("", target.Value);
        Assert.AreEqual("2001-02-03", target.BirthDate.ToString("yyyy-MM-dd"));
    }
}