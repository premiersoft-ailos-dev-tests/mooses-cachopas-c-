using System.Threading;
using System.Threading.Tasks;
using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Application.Commands.Accounts.ActivateAccount;
using Bankmore.Accounts.Command.Application.Commands.Accounts.DeactivateAccount;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using MediatR;
using Moq;
using NUnit.Framework;

namespace Bankmore.Accounts.Command.Tests.Application.Handlers.DeactivateAccount
{
    [TestFixture]
    public class DeactivateAccountCommandHandlerTests
    {
        private Mock<IAccountsStore> _store = null!;
        private Mock<IPasswordHasher> _hasher = null!;

        [SetUp]
        public void SetUp()
        {
            _store  = new Mock<IAccountsStore>(MockBehavior.Strict);
            _hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        }

        private DeactivateAccountCommandHandler CreateSut()
            => new DeactivateAccountCommandHandler(_store.Object, _hasher.Object);

        [Test]
        public void Handle_WhenAccountNotFoundOrAlreadyInactive_Throws_BusinessRuleException_INVALID_ACCOUNT()
        {
            // arrange
            var cmd1 = new DeactivateAccountCommand(404, "pwd", "tok");
            _store.Setup(s => s.GetAccountWithCredentialsAsync(cmd1.NumeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(((int Id, bool Ativo, string Senha, string Salt)?)null);

            var sut = CreateSut();

            // act + assert
            var ex1 = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd1, CancellationToken.None));
            Assert.That(ex1!.Message, Is.EqualTo("Account not found or already inactive."));
            Assert.That(ex1.Source,  Is.EqualTo("INVALID_ACCOUNT"));
            Assert.That(ex1.HResult, Is.EqualTo(400));

            _store.Verify(s => s.GetAccountWithCredentialsAsync(cmd1.NumeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _store.Reset(); 

            // arrange 
            var cmd2 = new DeactivateAccountCommand(1001, "pwd", "tok");
            var credInactive = (Id: cmd2.NumeroConta, Ativo: false, Senha: "hash", Salt: "salt");

            _store.Setup(s => s.GetAccountWithCredentialsAsync(cmd2.NumeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(credInactive);

            // act + assert
            var ex2 = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd2, CancellationToken.None));
            Assert.That(ex2!.Message, Is.EqualTo("Account not found or already inactive."));
            Assert.That(ex2.Source,  Is.EqualTo("INVALID_ACCOUNT"));
            Assert.That(ex2.HResult, Is.EqualTo(400));

            _store.Verify(s => s.GetAccountWithCredentialsAsync(cmd2.NumeroConta, It.IsAny<CancellationToken>()), Times.Once);
            _store.VerifyNoOtherCalls();
            _hasher.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_WhenPasswordInvalid_Throws_UnauthorizedAccessException()
        {
            // arrange
            var cmd  = new DeactivateAccountCommand(2002, "wrong", "tok");
            var cred = (Id: cmd.NumeroConta, Ativo: true, Senha: "hash", Salt: "salt");

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
        public async Task Handle_WhenSuccess_Deactivates_And_Saves()
        {
            // arrange
            var cmd  = new DeactivateAccountCommand(3003, "right", "tok");
            var cred = (Id: cmd.NumeroConta, Ativo: true, Senha: "hash", Salt: "salt");

            _store.Setup(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(cred);

            _hasher.Setup(h => h.Verify(cmd.Password, cred.Senha, cred.Salt))
                   .Returns(true);

            _store.Setup(s => s.SetAccountActiveAsync(cmd.NumeroConta, false, It.IsAny<CancellationToken>()))
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
            _store.Verify(s => s.SetAccountActiveAsync(cmd.NumeroConta, false, It.IsAny<CancellationToken>()), Times.Once);
            _store.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _store.VerifyNoOtherCalls();
            _hasher.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_Propagates_CancellationToken_ToStoreCalls()
        {
            // arrange
            var cmd  = new DeactivateAccountCommand(4004, "pwd", "tok");
            var cred = (Id: cmd.NumeroConta, Ativo: true, Senha: "hash", Salt: "salt");
            using var cts = new CancellationTokenSource();

            _store.Setup(s => s.GetAccountWithCredentialsAsync(cmd.NumeroConta, cts.Token))
                  .ReturnsAsync(cred).Verifiable();

            _hasher.Setup(h => h.Verify(cmd.Password, cred.Senha, cred.Salt))
                   .Returns(true).Verifiable();

            _store.Setup(s => s.SetAccountActiveAsync(cmd.NumeroConta, false, cts.Token))
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