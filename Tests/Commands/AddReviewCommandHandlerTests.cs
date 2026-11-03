using ApplicationServices.Commands;
using ApplicationServices.Commands.Handlers;
using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Products;
using DomainLayer.Models.Reviews;
using Moq;

namespace Tests.Commands
{
    public class AddReviewCommandHandlerTests
    {
        private Mock<IReadProductRepository> _products = null!;
        private Mock<ITableRepository<Review>> _reviews = null!;
        private AddReviewCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _products = new Mock<IReadProductRepository>();
            _reviews = new Mock<ITableRepository<Review>>();
            _handler = new AddReviewCommandHandler(_products.Object, _reviews.Object);

            _products.Setup(p => p.GetByIdAsync("prod-1")).ReturnsAsync(new Product { Id = "prod-1" });
        }

        [Test]
        public async Task ValidReview_IsStored_InProductPartition()
        {
            await _handler.Handle(new AddReviewCommand("prod-1", 5, "Great"), CancellationToken.None);

            _reviews.Verify(r => r.InsertOrUpdateAsync(
                It.Is<Review>(x => x.PartitionKey == "prod-1" && x.Rating == 5)), Times.Once);
        }

        [TestCase(0)]
        [TestCase(6)]
        public void RatingOutOfRange_ThrowsValidation(int rating)
        {
            Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(new AddReviewCommand("prod-1", rating, "Text"), CancellationToken.None));

            _reviews.Verify(r => r.InsertOrUpdateAsync(It.IsAny<Review>()), Times.Never);
        }
    }
}