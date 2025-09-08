using System;
using System.Threading;
using System.Threading.Tasks;
using Bankmore.Accounts.Query.Application.Abstraction;
using Bankmore.Accounts.Query.Application.Comon.Exceptions;
using Bankmore.Accounts.Query.Application.Comon.Models;
using Bankmore.Accounts.Query.Application.Queries.Accounts.GetBalance;
using Moq;
using NUnit.Framework;

namespace Bankmore.Accounts.Query.Tests.Application.Handlers
{
    [TestFixture]
    public class GetBalanceQueryHandlerTests
    {
        private Mock<IAccountsReadStore> _store = null!;
        private GetBalanceQueryHandler CreateSut() => new(_store.Object);

        [SetUp]
        public void SetUp()
        {
            _store = new Mock<IAccountsReadStore>(MockBehavior.Strict);
        }

        [Test]
        public void Handle_AccountNotFound_Throws_NotFoundAppException()
        {
            // arrange
            var query = new GetBalanceQuery(404, "tok", true);

            _store.Setup(s => s.GetAccountAsync(query.numeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((AccountInfo?)null);

            var sut = CreateSut();

            // act + assert
            var ex = Assert.ThrowsAsync<NotFoundAppException>(() => sut.Handle(query, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Conta não encontrada."));

            _store.Verify(s => s.GetAccountAsync(query.numeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _store.Verify(s => s.GetBalanceAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
            _store.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_AccountInactive_Throws_ForbiddenAppException()
        {
            // arrange
            var query = new GetBalanceQuery(1001, "tok", false);
            var inactive = new AccountInfo
            {
                NumeroConta = query.numeroConta,
                Nome = "Ana",
                Activa = false
            };

            _store.Setup(s => s.GetAccountAsync(query.numeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(inactive);

            var sut = CreateSut();

            // act + assert
            var ex = Assert.ThrowsAsync<ForbiddenAppException>(() => sut.Handle(query, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Conta inativa."));

            _store.Verify(s => s.GetAccountAsync(query.numeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _store.Verify(s => s.GetBalanceAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
            _store.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_BalanceNotFound_Throws_NotFoundAppException()
        {
            // arrange
            var query = new GetBalanceQuery(111, "tok", true);
            var active = new AccountInfo
            {
                NumeroConta = query.numeroConta,
                Nome = "Ana",
                Activa = true
            };

            _store.Setup(s => s.GetAccountAsync(query.numeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(active);

            _store.Setup(s => s.GetBalanceAsync(query.numeroConta, query.IncludeTariffs, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((BalanceProjection?)null);

            var sut = CreateSut();

            // act + assert
            var ex = Assert.ThrowsAsync<NotFoundAppException>(() => sut.Handle(query, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Saldo não encontrado."));

            _store.Verify(s => s.GetAccountAsync(query.numeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _store.Verify(s => s.GetBalanceAsync(query.numeroConta, query.IncludeTariffs, It.IsAny<CancellationToken>()), Times.Once);
            _store.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_Success_MapsResultCorrectly_And_CallsStoreWithCorrectArgs()
        {
            // arrange
            var query = new GetBalanceQuery(222, "tok", false);
            var active = new AccountInfo
            {
                NumeroConta = query.numeroConta,
                Nome = "Ana",
                Activa = true
            };
            var asOf = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            var balance = new BalanceProjection
            {
                AvailableBalance = 123.45m,
                LedgerBalance = 200.00m,
                Currency = "BRL",
                AsOfUtc = asOf,
                Version = 42
            };

            _store.Setup(s => s.GetAccountAsync(query.numeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(active);

            _store.Setup(s => s.GetBalanceAsync(query.numeroConta, query.IncludeTariffs, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(balance);

            var sut = CreateSut();

            // act
            var res = await sut.Handle(query, CancellationToken.None);
            
            Assert.That(res.SaldoDisponivel, Is.EqualTo(balance.AvailableBalance));
            Assert.That(res.Nome,           Is.EqualTo(active.Nome));
            Assert.That(res.NumerConta,     Is.EqualTo(active.NumeroConta));
            Assert.That(res.AsOfUtc,        Is.EqualTo(balance.AsOfUtc));

            _store.Verify(s => s.GetAccountAsync(query.numeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _store.Verify(s => s.GetBalanceAsync(query.numeroConta, query.IncludeTariffs, It.IsAny<CancellationToken>()), Times.Once);
            _store.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_PropagatesCancellationToken_ToStore()
        {
            // arrange
            var query = new GetBalanceQuery(333, "tok", true);
            var active = new AccountInfo
            {
                NumeroConta = query.numeroConta,
                Nome = "Ana",
                Activa = true
            };
            var balance = new BalanceProjection
            {
                AvailableBalance = 0m,
                LedgerBalance = 0m,
                Currency = "BRL",
                AsOfUtc = DateTime.UtcNow,
                Version = 0
            };

            using var cts = new CancellationTokenSource();

            _store.Setup(s => s.GetAccountAsync(query.numeroConta, cts.Token))
                  .ReturnsAsync(active)
                  .Verifiable();

            _store.Setup(s => s.GetBalanceAsync(query.numeroConta, query.IncludeTariffs, cts.Token))
                  .ReturnsAsync(balance)
                  .Verifiable();

            var sut = CreateSut();

            // act
            _ = await sut.Handle(query, cts.Token);

            // assert
            _store.Verify();
        }
    }
}