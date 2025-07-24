namespace IFY.AttriMap.Tests;

[TestClass]
public sealed class MapFromTests
{
    public class TestSource
    {
        public string SName { get; init; } = string.Empty;
        public string SValue { get; init; } = string.Empty;
        public string SDateOfBirth { get; init; } = string.Empty;
    }

    public interface ITestSource
    {
        string SName { get; }
        string SValue { get; }
        string SDateOfBirth { get; }
    }
    public class TestSourceImpl : ITestSource
    {
        public string SName { get; init; } = string.Empty;
        public string SValue { get; init; } = string.Empty;
        public string SDateOfBirth { get; init; } = string.Empty;
    }

    public class TestTarget
    {
        [MapFrom<TestSource>(nameof(TestSource.SName))]
        [MapFrom<ITestSource>(nameof(ITestSource.SName))]
        public string TName { get; init; } = string.Empty;
        public string TValue { get; init; } = string.Empty;
        [MapFrom(typeof(TestSource), nameof(TestSource.SDateOfBirth), nameof(StringToDateOnly))]
        [MapFrom(typeof(ITestSource), nameof(ITestSource.SDateOfBirth), nameof(StringToDateOnly))]
        public DateOnly TDateOfBirth { get; init; }
        public static DateOnly StringToDateOnly(string input) => DateOnly.Parse(input); // Example transformer method
    }

    [TestMethod]
    public void Target_maps_from_source()
    {
        // Arrange
        var source = new TestSource
        {
            SName = "Test Name",
            SValue = "Test Value",
            SDateOfBirth = "2001-02-03",
        };

        // Act
        var target = source.ToTestTarget();

        // Assert
        Assert.IsNotNull(target);
        Assert.AreEqual("Test Name", target.TName);
        Assert.AreEqual("", target.TValue);
        Assert.AreEqual("2001-02-03", target.TDateOfBirth.ToString("yyyy-MM-dd"));
    }

    [TestMethod]
    public void Target_maps_from_source_interface()
    {
        // Arrange
        var source = new TestSourceImpl
        {
            SName = "Test Name",
            SValue = "Test Value",
            SDateOfBirth = "2001-02-03",
        };

        // Act
        var target = source.ToTestTarget();

        // Assert
        Assert.IsNotNull(target);
        Assert.AreEqual("Test Name", target.TName);
        Assert.AreEqual("", target.TValue);
        Assert.AreEqual("2001-02-03", target.TDateOfBirth.ToString("yyyy-MM-dd"));
    }
}