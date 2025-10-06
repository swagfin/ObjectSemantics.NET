using ObjectSemantics.NET.Tests.MoqModels;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class PropertyBooleanMapTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Map_From_Bool(bool isElectric)
        {
            Car car = new Car()
            {
                IsElectric = isElectric
            };
            string result = car.Map("Electric: {{ IsElectric }}");
            Assert.Equal($"Electric: {isElectric}", result, false, true, true);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Map_From_Nullable_Bool(bool? isLiked)
        {
            Car car = new Car()
            {
                IsLiked = isLiked
            };
            string result = car.Map("Liked: {{ IsLiked }}");
            Assert.Equal($"Liked: {isLiked}", result, false, true, true);
        }
    }
}
