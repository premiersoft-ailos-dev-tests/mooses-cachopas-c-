using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Application.Commands.Transactions.Movments;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using Bankmore.Accounts.Command.Domain.Enums.Movments;
using Bankmore.Accounts.Command.Domain.Models.Movments;
using Bankmore.Accounts.Query.Application.Abstraction; 
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Bankmore.Accounts.Command.Tests.Application.Handlers.Movments
{
    [TestFixture]
    public class MovmentHandlerTests
    {
        private Mock<ITransactionsService> _tx = null!;
        private Mock<IAccountsService> _acc = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<IIdempotencyStore> _idemp = null!;
        private Mock<IClock> _clock = null!;
        private Mock<ILogger<MovmentHandler>> _logger = null!;

        [SetUp]
        public void SetUp()
        {
            _tx    = new Mock<ITransactionsService>(MockBehavior.Strict);
            _acc   = new Mock<IAccountsService>(MockBehavior.Strict);
            _mapper= new Mock<IMapper>(MockBehavior.Strict);
            _idemp = new Mock<IIdempotencyStore>(MockBehavior.Strict);
            _clock = new Mock<IClock>(MockBehavior.Loose);
            _logger= new Mock<ILogger<MovmentHandler>>(MockBehavior.Loose);
        }

        private MovmentHandler CreateSut()
            => new(_tx.Object, _acc.Object, _mapper.Object, _clock.Object, _idemp.Object, _logger.Object);
        
        private static MovmentModel Model(int idConta, decimal valor, MovmentType tipo)
            => new MovmentModel { IdConta = idConta, Valor = valor, MovmentType = tipo };

        private static AccountValid Active(int numero = 1, string nome = "Ana")
            => new(true, numero, nome, true);

        private static AccountValid Inactive(int numero = 1, string nome = "Ana")
            => new(true, numero, nome, false);

        private static AccountValid NotFound()
            => new(false, 0, string.Empty, false);

        private static MovmentCommand Cmd(int? idConta, decimal valor, string token, MovmentType tipo, string idem = "idem-1")
            => new(idConta, valor, token, tipo, idem);
        
        private static string Base64Url(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string JwtFor(string accountId)
        {
            var header = Base64Url("{\"alg\":\"none\",\"typ\":\"JWT\"}");
            var payload = Base64Url($"{{\"sub\":\"{accountId}\",\"accountId\":\"{accountId}\"}}");
            const string signature = "sig";
            return $"{header}.{payload}.{signature}";
        }
        

        [Test]
        public async Task Handle_IdempotencyHit_ReturnsCached_And_ShortCircuits()
        {
            var cmd = Cmd(1, 10m, JwtFor("1"), MovmentType.Credit, "idem-1");
            var cached = new MovmentResult(true);
            var json = JsonSerializer.Serialize(cached);

            _idemp.Setup(i => i.GetResultAsync("idem-1", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(json);

            var sut = CreateSut();
            var res = await sut.Handle(cmd, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(res.Success, Is.True);
                _idemp.Verify(i => i.GetResultAsync("idem-1", It.IsAny<CancellationToken>()), Times.Once);
                _mapper.VerifyNoOtherCalls();
                _acc.VerifyNoOtherCalls();
                _tx.VerifyNoOtherCalls();
                _idemp.VerifyNoOtherCalls();
            });
        }

        [Test]
        public void Handle_InvalidAccountNumber_WhenIdContaNull_AndTokenResolvesToZero_Throws()
        {
            var cmd = Cmd(null, 10m, JwtFor("0"), MovmentType.Credit, "idem-1");

            _idemp.Setup(i => i.GetResultAsync("idem-1", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _mapper.Setup(m => m.Map<MovmentModel>(cmd))
                   .Returns(Model(0, 10m, MovmentType.Credit));

            var sut = CreateSut();

            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));

            Assert.Multiple(() =>
            {
                Assert.That(ex!.Message, Is.EqualTo("Numero de Conta Inválido"));
                _mapper.Verify(m => m.Map<MovmentModel>(cmd), Times.Once);
                _acc.VerifyNoOtherCalls();
                _tx.VerifyNoOtherCalls();
                _idemp.Verify(i => i.GetResultAsync("idem-1", It.IsAny<CancellationToken>()), Times.Once);
                _idemp.VerifyNoOtherCalls();
            });
        }

        [Test]
        public void Handle_AccountNotFound_ExistsFalse_Throws()
        {
            var cmd = Cmd(999, 10m, JwtFor("999"), MovmentType.Credit);

            _idemp.Setup(i => i.GetResultAsync(cmd.IdempotencyKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _mapper.Setup(m => m.Map<MovmentModel>(cmd))
                   .Returns(Model(999, 10m, MovmentType.Credit));

            _acc.Setup(a => a.GetAccountAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(NotFound());

            var sut = CreateSut();

            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));

            Assert.Multiple(() =>
            {
                Assert.That(ex!.Message, Is.EqualTo("Conta não encontrada."));
                _acc.Verify(a => a.GetAccountAsync(999, It.IsAny<CancellationToken>()), Times.Once);
            });
        }

        [Test]
        public void Handle_AccountInactive_Throws()
        {
            var cmd = Cmd(10, 10m, JwtFor("10"), MovmentType.Credit);

            _idemp.Setup(i => i.GetResultAsync(cmd.IdempotencyKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _mapper.Setup(m => m.Map<MovmentModel>(cmd))
                   .Returns(Model(10, 10m, MovmentType.Credit));

            _acc.Setup(a => a.GetAccountAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Inactive(10, "Ana"));

            var sut = CreateSut();

            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));

            Assert.That(ex!.Message, Is.EqualTo("Conta não esta ativa."));
        }

        [Test]
        public void Handle_DefaultMovmentType_Throws()
        {
            var cmd = Cmd(10, 10m, JwtFor("10"), MovmentType.Default);

            _idemp.Setup(i => i.GetResultAsync(cmd.IdempotencyKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _mapper.Setup(m => m.Map<MovmentModel>(cmd))
                   .Returns(Model(10, 10m, MovmentType.Default));

            _acc.Setup(a => a.GetAccountAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Active(10, "Ana"));

            var sut = CreateSut();

            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));

            Assert.That(ex!.Message, Is.EqualTo("Tipo de movimentação inválido."));
        }

        [Test]
        public void Handle_Debit_WithDifferentTokenAccount_Throws_Permission()
        {
            var cmd = Cmd(10, 50m, JwtFor("123"), MovmentType.Debit);

            _idemp.Setup(i => i.GetResultAsync(cmd.IdempotencyKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _mapper.Setup(m => m.Map<MovmentModel>(cmd))
                   .Returns(Model(10, 50m, MovmentType.Debit));

            _acc.Setup(a => a.GetAccountAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Active(10, "Ana"));

            var sut = CreateSut();

            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));

            Assert.That(ex!.Message, Is.EqualTo("Conta não possui permissão para realizar essa operação."));
        }

        [Test]
        public void Handle_Debit_InsufficientBalance_Throws()
        {
            var cmd = Cmd(42, 100m, JwtFor("42"), MovmentType.Debit);

            _idemp.Setup(i => i.GetResultAsync(cmd.IdempotencyKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _mapper.Setup(m => m.Map<MovmentModel>(cmd))
                   .Returns(Model(42, 100m, MovmentType.Debit));

            _acc.Setup(a => a.GetAccountAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Active(42, "Bob"));

            _tx.Setup(t => t.GetSaldoAtualAsync(42, It.IsAny<CancellationToken>()))
               .ReturnsAsync(99m); 

            var sut = CreateSut();

            var ex = Assert.ThrowsAsync<BusinessRuleException>(() => sut.Handle(cmd, CancellationToken.None));

            Assert.That(ex!.Message, Is.EqualTo("Conta não possui saldo suficiente."));
        }

        [Test]
        public async Task Handle_Success_Registers_Persists_And_Caches()
        {
            var cmd = Cmd(77, 10m, JwtFor("999"), MovmentType.Credit, "idem-OK");

            _idemp.Setup(i => i.GetResultAsync("idem-OK", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _mapper.Setup(m => m.Map<MovmentModel>(cmd))
                   .Returns(Model(77, 10m, MovmentType.Credit));

            _acc.Setup(a => a.GetAccountAsync(77, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Active(77, "Ana"));

            _tx.Setup(t => t.RegisterMovment(
                        It.Is<MovmentModel>(m => m.IdConta == 77 && m.Valor == 10m && m.MovmentType == MovmentType.Credit),
                        It.IsAny<CancellationToken>()))
               .Returns(Task.FromResult(true));

            _tx.Setup(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            _idemp.Setup(i => i.SaveAsync(
                        "idem-OK",
                        string.Empty,
                        It.Is<string>(s => s.Contains("\"Success\":true")),
                        It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(cmd, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(res.Success, Is.True);
                Assert.That(res.ErrorType, Is.Null);
                Assert.That(res.ErrorMessage, Is.Null);

                _tx.Verify(t => t.RegisterMovment(It.IsAny<MovmentModel>(), It.IsAny<CancellationToken>()), Times.Once);
                _tx.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
                _idemp.Verify(i => i.SaveAsync("idem-OK", string.Empty, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            });
        }

        [Test]
        public async Task Handle_Propagates_CancellationToken()
        {
            using var cts = new CancellationTokenSource();

            var cmd = Cmd(88, 10m, JwtFor("88"), MovmentType.Debit, "idem-token");

            _idemp.Setup(i => i.GetResultAsync("idem-token", cts.Token))
                  .ReturnsAsync(string.Empty).Verifiable();

            _mapper.Setup(m => m.Map<MovmentModel>(cmd))
                   .Returns(Model(88, 10m, MovmentType.Debit)).Verifiable();

            _acc.Setup(a => a.GetAccountAsync(88, cts.Token))
                .ReturnsAsync(Active(88, "Zoe")).Verifiable();

            _tx.Setup(t => t.GetSaldoAtualAsync(88, cts.Token))
               .ReturnsAsync(100m).Verifiable();

            _tx.Setup(t => t.RegisterMovment(It.IsAny<MovmentModel>(), cts.Token))
               .Returns(Task.FromResult(true)).Verifiable();

            _tx.Setup(t => t.SaveChangesAsync(cts.Token))
               .Returns(Task.CompletedTask).Verifiable();

            _idemp.Setup(i => i.SaveAsync("idem-token", string.Empty, It.IsAny<string>(), cts.Token))
                  .Returns(Task.CompletedTask).Verifiable();

            var sut = CreateSut();
            _ = await sut.Handle(cmd, cts.Token);

            Assert.Multiple(() =>
            {
                _idemp.Verify();
                _mapper.Verify();
                _acc.Verify();
                _tx.Verify();
            });
        }
    }
}