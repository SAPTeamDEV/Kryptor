using SAPTeam.Kryptor;

namespace Kryptor.Tests
{
    public class TransformersTests
    {
        [Fact]
        public void Rotate_EmptyCollection_ReturnsEmpty()
        {
            // Arrange
            var emptyCollection = Enumerable.Empty<int>();

            // Act
            int[] result = emptyCollection.ToArray();
            Transformers.Rotate(result, 3);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Rotate_PositivePositions_ReturnsRotatedCollection()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            var result = collection.ToArray();
            Transformers.Rotate(result, 2);

            // Assert
            Assert.Equal(new List<int> { 4, 5, 1, 2, 3 }, result);
        }

        [Fact]
        public void Pick_ReturnsShuffledItems()
        {
            // Arrange
            var collection = new List<int> { 10, 20, 30, 40, 50 };

            // Act
            var result = Transformers.Pick(collection, 3, seed: 123).ToList();

            // Assert
            Assert.NotEqual(collection, result); // Shuffled
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void Mix_CombinesAndShufflesCollections()
        {
            // Arrange
            var collection1 = new List<string> { "A", "B", "C" };
            var collection2 = new List<string> { "X", "Y", "Z" };

            // Act
            var result = Transformers.Mix(seed: 456, collection1, collection2).ToList();

            // Assert
            Assert.NotEqual(collection1.Concat(collection2), result); // Shuffled
            Assert.Equal(6, result.Count);
        }

        // Add more test cases for other methods...

        // Note: You can also write tests for ToInt32 and ToInt64 methods.
    }
}
