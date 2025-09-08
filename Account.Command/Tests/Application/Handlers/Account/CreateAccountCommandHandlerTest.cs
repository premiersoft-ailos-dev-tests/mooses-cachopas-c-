using System.Threading;
using System.Threading.Tasks;
using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Application.Commands.Accounts.CreateAccount;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using Bankmore.Accounts.Command.Domain.Accounts;
using Moq;
using NUnit.Framework;

namespace Bankmore.Accounts.Command.Tests.Application.Handlers.CreateAccount
{
    [TestFixture]
    public class CreateAccountCommandHandlerTests
    {
        private Mock<IAccountsService> _service = null!;
        private Mock<IPasswordHasher> _hasher = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IAccountsService>(MockBehavior.Strict);
            _hasher  = new Mock<IPasswordHasher>(MockBehavior.Strict);
        }

        private CreateAccountCommandHandler CreateSut() => new(_service.Object, _hasher.Object);

        [Test]
        public void Handle_WhenCpfIsInvalid_Throws_BusinessRuleException()
        {
            var cmd = new CreateAccountCommand("Ana", "Secreta@123", "123.456.789-00");
            var sut = CreateSut();

            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("CPF inválido."));

            _service.VerifyNoOtherCalls();
            _hasher.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_WhenCpfAlreadyExists_Throws_BusinessRuleException()
        {
            const string cpfValido = "52998224725";
            var cmd = new CreateAccountCommand("Ana", "Secreta@123", cpfValido);

            _service.Setup(s => s.CpfExistsAsync(cpfValido, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var sut = CreateSut();

            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("CPF já cadastrado."));

            _service.Verify(s => s.CpfExistsAsync(cpfValido, It.IsAny<CancellationToken>()), Times.Once);
            _service.VerifyNoOtherCalls();
            _hasher.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_WhenSuccess_HashesPassword_Creates_And_Returns_Result()
        {
            const string cpfValido = "52998224725";
            var cmd = new CreateAccountCommand("Ana", "Secreta@123", cpfValido);

            const string expectedHash = "HASH_X";
            const string expectedSalt = "SALT_Y";

            _service.Setup(s => s.CpfExistsAsync(cpfValido, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

            _hasher.Setup(h => h.HashPassword(cmd.Senha))
                   .Returns((expectedHash, expectedSalt));
            
            CreateAccountModel? captured = null;

            _service.Setup(s => s.CreateAccountAsync(
                                It.IsAny<CreateAccountModel>(),
                                It.IsAny<CancellationToken>()))
                    .Callback<CreateAccountModel, CancellationToken>((m, _) => captured = m)
                    .ReturnsAsync(() =>
                    {
                        return new CreateAccountModel
                        {
                            Numero = 4321,
                            Nome = captured.Nome,
                            Cpf = captured.Cpf,
                            Ativa = captured.Ativa,
                            Senha = captured.Senha,
                            Salt = captured.Salt
                        };
                    });

            var sut = CreateSut();

            // act
            var res = await sut.Handle(cmd, CancellationToken.None);

            // assert 
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Numero, Is.EqualTo(4321));

            // assert 
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Nome, Is.EqualTo(cmd.Nome));
            Assert.That(captured.Cpf, Is.EqualTo(cmd.Cpf));
            Assert.That(captured.Ativa, Is.True);
            Assert.That(captured.Senha, Is.EqualTo(expectedHash));
            Assert.That(captured.Salt,  Is.EqualTo(expectedSalt));

            _service.Verify(s => s.CpfExistsAsync(cpfValido, It.IsAny<CancellationToken>()), Times.Once);
            _hasher.Verify(h => h.HashPassword(cmd.Senha), Times.Once);
            _service.Verify(s => s.CreateAccountAsync(
                                It.IsAny<CreateAccountModel>(),
                                It.IsAny<CancellationToken>()), Times.Once);
            _service.VerifyNoOtherCalls();
            _hasher.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_Propagates_CancellationToken_To_Service()
        {
            const string cpfValido = "52998224725";
            var cmd = new CreateAccountCommand("Ana", "Secreta@123", cpfValido);
            using var cts = new CancellationTokenSource();

            _service.Setup(s => s.CpfExistsAsync(cpfValido, cts.Token))
                    .ReturnsAsync(false).Verifiable();

            _hasher.Setup(h => h.HashPassword(cmd.Senha))
                   .Returns(("H","S")).Verifiable();

            _service.Setup(s => s.CreateAccountAsync(
                                It.IsAny<CreateAccountModel>(),
                                cts.Token))
                    .ReturnsAsync(new CreateAccountModel
                    {
                        Numero = 1,
                        Nome = "Ana",
                        Cpf = cpfValido,
                        Ativa = true,
                        Senha = "H",
                        Salt  = "S"
                    }).Verifiable();

            var sut = CreateSut();

            _ = await sut.Handle(cmd, cts.Token);

            _service.Verify();
            _hasher.Verify();
        }
    }
}