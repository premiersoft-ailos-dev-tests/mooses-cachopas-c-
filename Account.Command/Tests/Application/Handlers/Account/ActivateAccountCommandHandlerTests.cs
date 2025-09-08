using System.Threading;
using System.Threading.Tasks;
using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Application.Commands.Accounts.ActivateAccount;
using Bankmore.Accounts.Command.Application.Commands.Accounts.DeactivateAccount;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using MediatR;
using Moq;
using NUnit.Framework;

namespace Bankmore.Accounts.Command.Tests.Application.Handlers.ActivateAccount
{
    [TestFixture]
    public class ActivateAccountHandlerTests
    {
        private Mock<IAccountsService> _store = null!;
        private Mock<IPasswordHasher> _hasher = null!;

        [SetUp]
        public void SetUp()
        {
            _store  = new Mock<IAccountsService>(MockBehavior.Strict);
            _hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        }

        private ActivateAccountHandler CreateSut()
            => new ActivateAccountHandler(_store.Object, _hasher.Object);

        [Test]
        public void Handle_WhenAccountNotFound_Throws_BusinessRuleException_With_INVALID_ACCOUNT()
        {
            // arrange
            var cmd = new ActivateAccountCommand(404, "pwd", "tok");
            _store.Setup(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(((int Id, bool Ativo, string Senha, string Salt)?)null);

            var sut = CreateSut();

            // act + assert
            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Account not found."));
            Assert.That(ex.Source,  Is.EqualTo("INVALID_ACCOUNT"));
            Assert.That(ex.HResult, Is.EqualTo(400));

            _store.Verify(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _store.VerifyNoOtherCalls();
            _hasher.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_WhenAccountAlreadyActive_Throws_BusinessRuleException_With_ALREADY_ACTIVE()
        {
            // arrange
            var cmd  = new ActivateAccountCommand(111, "pwd", "tok");
            var cred = (Id: cmd.NumeroConta, Ativo: true, Senha: "hash", Salt: "salt");

            _store.Setup(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(cred);

            var sut = CreateSut();

            // act + assert
            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Account is already active."));
            Assert.That(ex.Source,  Is.EqualTo("ALREADY_ACTIVE"));
            Assert.That(ex.HResult, Is.EqualTo(400));

            _store.Verify(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _store.VerifyNoOtherCalls();
            _hasher.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_WhenPasswordInvalid_Throws_UnauthorizedAccessException()
        {
            // arrange
            var cmd  = new ActivateAccountCommand(222, "wrong", "tok");
            var cred = (Id: cmd.NumeroConta, Ativo: false, Senha: "hash", Salt: "salt");

            _store.Setup(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(cred);

            _hasher.Setup(h => h.Verify(cmd.Password, cred.Senha, cred.Salt))
                   .Returns(false);

            var sut = CreateSut();

            // act + assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.Handle(cmd, CancellationToken.None));

            _store.Verify(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _hasher.Verify(h => h.Verify(cmd.Password, cred.Senha, cred.Salt), Times.Once);
            _store.VerifyNoOtherCalls();
            _hasher.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_WhenSuccess_Activates_And_Saves()
        {
            // arrange
            var cmd  = new ActivateAccountCommand(333, "right", "tok");
            var cred = (Id: cmd.NumeroConta, Ativo: false, Senha: "hash", Salt: "salt");

            _store.Setup(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(cred);

            _hasher.Setup(h => h.Verify(cmd.Password, cred.Senha, cred.Salt))
                   .Returns(true);

            _store.Setup(s => s.SetAccountActiveAsync(cmd.NumeroConta, true, It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            _store.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // act
            var result = await sut.Handle(cmd, CancellationToken.None);

            // assert
            Assert.That(result, Is.EqualTo(Unit.Value));
            _store.Verify(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _hasher.Verify(h => h.Verify(cmd.Password, cred.Senha, cred.Salt), Times.Once);
            _store.Verify(s => s.SetAccountActiveAsync(cmd.NumeroConta, true, It.IsAny<CancellationToken>()), Times.Once);
            _store.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _store.VerifyNoOtherCalls();
            _hasher.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_Propagates_CancellationToken_To_All_Calls()
        {
            // arrange
            var cmd  = new ActivateAccountCommand(444, "pwd", "tok");
            var cred = (Id: cmd.NumeroConta, Ativo: false, Senha: "hash", Salt: "salt");
            using var cts = new CancellationTokenSource();

            _store.Setup(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, cts.Token))
                  .ReturnsAsync(cred).Verifiable();

            _hasher.Setup(h => h.Verify(cmd.Password, cred.Senha, cred.Salt))
                   .Returns(true).Verifiable();

            _store.Setup(s => s.SetAccountActiveAsync(cmd.NumeroConta, true, cts.Token))
                  .Returns(Task.CompletedTask).Verifiable();

            _store.Setup(s => s.SaveChangesAsync(cts.Token))
                  .Returns(Task.CompletedTask).Verifiable();

            var sut = CreateSut();

            // act
            _ = await sut.Handle(cmd, cts.Token);

            // assert
            _store.Verify();
            _hasher.Verify();
        }
    }
}