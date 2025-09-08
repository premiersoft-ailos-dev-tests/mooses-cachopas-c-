using System.Net;
using System.Text.Json;
using Application.Abstractions;
using Application.Commands.Transfers;
using Infra.Contracts;
using Infra.Models; 
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Refit;

namespace Application.Tests.Commands.Transfers
{
    [TestFixture]
    public class TransferBetweenAccountsCommandHandlerTests
    {
        private Mock<IClock> _clock = null!;
        private Mock<IIdempotencyStore> _idemp = null!;
        private Mock<ILogger<TransferBetweenAccountsCommandHandler>> _logger = null!;
        private Mock<IMovmentsApi> _movApi = null!;
        private Mock<ITransfersStore> _txStore = null!;         
        private Mock<ITransfersStore> _transfersStore = null!;

        [SetUp]
        public void SetUp()
        {
            _clock = new Mock<IClock>(MockBehavior.Strict);
            _idemp = new Mock<IIdempotencyStore>(MockBehavior.Strict);
            _logger = new Mock<ILogger<TransferBetweenAccountsCommandHandler>>(MockBehavior.Loose);
            _movApi = new Mock<IMovmentsApi>(MockBehavior.Strict);
            _txStore = new Mock<ITransfersStore>(MockBehavior.Strict);
            _transfersStore = new Mock<ITransfersStore>(MockBehavior.Strict);
        }

        private TransferBetweenAccountsCommandHandler CreateSut()
            => new(
                _movApi.Object,
                _txStore.Object,
                _transfersStore.Object,
                _clock.Object,
                _idemp.Object,
                _logger.Object);
        

        private static ApiResponse<string> ApiOk(string content = "ok")
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            return new ApiResponse<string>(resp, content, new RefitSettings());
        }

        private static ApiResponse<string> ApiFail(HttpStatusCode status = HttpStatusCode.BadRequest, string content = "err")
        {
            var resp = new HttpResponseMessage(status);
            return new ApiResponse<string>(resp, content, new RefitSettings());
        }

        private static AccountValid Active()   => new AccountValid { Exists = true, Ativa = true };
        private static AccountValid Inactive() => new AccountValid { Exists = true, Ativa = false };
        private static AccountValid NotFound() => new AccountValid { Exists = false, Ativa = false };

        private static TransferBetweenAccountsCommand Cmd(
            int origem = 1, int destino = 2, decimal valor = 10m, string idem = "idem", string tok = "token")
            => new(origem, destino, valor, idem, tok);

        [Test]
        public async Task Handle_IdempotencyHit_ReturnsCachedResult_And_ShortCircuits()
        {
            var cached = new TransferBetweenAccountsResult(false, "REMOTE_ERROR", "cached");
            var json = JsonSerializer.Serialize(cached);

            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(json);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res, Is.EqualTo(cached));

            _idemp.Verify(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()), Times.Once);
            _idemp.VerifyNoOtherCalls();
            _transfersStore.VerifyNoOtherCalls();
            _movApi.VerifyNoOtherCalls();
            _clock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Handle_OrigemNaoExiste_Retorna_INVALID_ACCOUNT_E_SalvaNoIdemp()
        {
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(NotFound());

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"ErrorType\":\"INVALID_ACCOUNT\"") &&
                                   s.Contains("origem")),
                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.False);
            Assert.That(res.ErrorType, Is.EqualTo("INVALID_ACCOUNT"));
            Assert.That(res.ErrorMessage, Does.Contain("origem"));

            _transfersStore.Verify(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _idemp.VerifyAll();
        }

        [Test]
        public async Task Handle_OrigemInativa_Retorna_INACTIVE_ACCOUNT_E_SalvaNoIdemp()
        {
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Inactive());

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"ErrorType\":\"INACTIVE_ACCOUNT\"") &&
                                   s.Contains("origem inativa")),
                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.False);
            Assert.That(res.ErrorType, Is.EqualTo("INACTIVE_ACCOUNT"));

            _transfersStore.Verify(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _idemp.VerifyAll();
        }

        [Test]
        public async Task Handle_DestinoNaoExiste_Retorna_INVALID_ACCOUNT_E_SalvaNoIdemp()
        {
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Active());

            _transfersStore.Setup(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(NotFound());

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"ErrorType\":\"INVALID_ACCOUNT\"") &&
                                   s.Contains("destino")),
                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.False);
            Assert.That(res.ErrorType, Is.EqualTo("INVALID_ACCOUNT"));

            _transfersStore.Verify(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _transfersStore.Verify(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>()), Times.Once);
            _idemp.VerifyAll();
        }

        [Test]
        public async Task Handle_DestinoInativo_Retorna_INACTIVE_ACCOUNT_E_SalvaNoIdemp()
        {
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Active());

            _transfersStore.Setup(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Inactive());

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"ErrorType\":\"INACTIVE_ACCOUNT\"") &&
                                   s.Contains("destino inativa")),
                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.False);
            Assert.That(res.ErrorType, Is.EqualTo("INACTIVE_ACCOUNT"));

            _transfersStore.Verify(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _transfersStore.Verify(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>()), Times.Once);
            _idemp.VerifyAll();
        }

        [Test]
        public async Task Handle_FalhaNoDebito_Retorna_REMOTE_ERROR_SemTentarCredito()
        {
            var now = DateTime.UtcNow;
            _clock.SetupGet(c => c.UtcNow).Returns(now);
            
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Active());
            _transfersStore.Setup(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Active());

            _movApi.Setup(a => a.MovmentAsync(It.IsAny<MovmentRequest>(), "token", It.IsAny<string>(),It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ApiFail()); 

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"ErrorType\":\"REMOTE_ERROR\"") &&
                                   s.Contains("Falha ao debitar conta de origem")),
                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.False);
            Assert.That(res.ErrorType, Is.EqualTo("REMOTE_ERROR"));

            _movApi.Verify(a => a.MovmentAsync(It.IsAny<MovmentRequest>() ,"token", It.IsAny<string>(),It.IsAny<CancellationToken>()), Times.Once);
            _transfersStore.Verify(s => s.AddTransferAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
            _transfersStore.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _idemp.VerifyAll();
        }

       [Test]
public async Task Handle_FalhaNoCredito_FazRollbackDoDebito_E_Retorna_REMOTE_ERROR()
{
    var now = DateTime.UtcNow;
    _clock.SetupGet(c => c.UtcNow).Returns(now);

    _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()))
          .ReturnsAsync(string.Empty);

    _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Active());
    _transfersStore.Setup(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Active());

    string? firstOp = null;
    string? secondOp = null;
    string? thirdOp = null;
    var call = 0;

    _movApi
        .Setup(a => a.MovmentAsync(
            It.IsAny<MovmentRequest>(),
            "token",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
        .Callback<MovmentRequest, string, string, CancellationToken>((m, _, __, ___) =>
        {
            call++;
            var op = m.tipoOperacao; 
            if (call == 1) firstOp  = op; // débito
            else if (call == 2) secondOp = op; // crédito
            else if (call == 3) thirdOp  = op; // rollback débito
        })
        .ReturnsAsync(() =>
        {
            return call switch
            {
                1 => ApiOk(),   // débito ok
                2 => ApiFail(), // crédito falha
                _ => ApiOk()    // rollback débito ok
            };
        });

    _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
        It.Is<string>(s => s.Contains("\"ErrorType\":\"REMOTE_ERROR\"") &&
                           s.Contains("Falha ao creditar conta de destino")),
        It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);

    var sut = CreateSut();
    var res = await sut.Handle(Cmd(), CancellationToken.None);

    Assert.That(res.Success, Is.False);
    Assert.That(res.ErrorType, Is.EqualTo("REMOTE_ERROR"));

    Assert.That(firstOp,  Is.EqualTo("D")); 
    Assert.That(secondOp, Is.EqualTo("C")); 
    Assert.That(thirdOp,  Is.EqualTo("C")); 

    _movApi.Verify(a => a.MovmentAsync(
            It.IsAny<MovmentRequest>(),
            "token",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
        Times.Exactly(3));
    _idemp.VerifyAll();
}

        [Test]
        public async Task Handle_ExcecaoNoCredito_RefitApiException_FazRollback_E_Retorna_REMOTE_ERROR()
        {
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);
            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Active());
            _transfersStore.Setup(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(Active());
            
            var now = DateTime.UtcNow;
            _clock.SetupGet(c => c.UtcNow).Returns(now);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://local.test/v1/contas/movimento");
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Bad Request" };
            var apiEx = await ApiException.Create(request, HttpMethod.Post, response, new RefitSettings());

            var call = 0;
            _movApi
                .Setup(a => a.MovmentAsync(It.IsAny<MovmentRequest>(), "token", It.IsAny<string>(),It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    call++;
                    return call switch
                    {
                        1 => ApiOk(),      
                        2 => throw apiEx,  
                        _ => ApiOk()       
                    };
                });

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                    It.Is<string>(s => s.Contains("\"ErrorType\":\"REMOTE_ERROR\"")),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.False);
            Assert.That(res.ErrorType, Is.EqualTo("REMOTE_ERROR"));

            _movApi.Verify(a => a.MovmentAsync(It.IsAny<MovmentRequest>(), "token",It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            _idemp.VerifyAll();
        }

        [Test]
        public async Task Handle_Sucesso_Total_Persiste_E_SalvaIdemp()
        {
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);

            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Active());
            _transfersStore.Setup(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(Active());

            var now = new DateTime(2025, 01, 02, 03, 04, 05, DateTimeKind.Utc);
            _clock.SetupGet(c => c.UtcNow).Returns(now);

            _movApi.SetupSequence(a => a.MovmentAsync(It.IsAny<MovmentRequest>(),"token", It.IsAny<string>(),It.IsAny<CancellationToken>()))
                  .ReturnsAsync(ApiOk()) 
                  .ReturnsAsync(ApiOk()); 

            _transfersStore.Setup(s => s.AddTransferAsync(1, 2, now, 10m, It.IsAny<CancellationToken>()))
                           .Returns(Task.FromResult(1));
            _transfersStore.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"Success\":true")),
                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.True);
            Assert.That(res.ErrorType, Is.Null);
            Assert.That(res.ErrorMessage, Is.Null);

            _transfersStore.Verify(s => s.AddTransferAsync(1, 2, now, 10m, It.IsAny<CancellationToken>()), Times.Once);
            _transfersStore.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _movApi.Verify(a => a.MovmentAsync(It.IsAny<MovmentRequest>(), "token", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _idemp.VerifyAll();
        }

        [Test]
        public async Task Handle_FalhaAoPersistir_FazRollbackTotal_E_Retorna_PERSISTENCE_ERROR()
        {
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);

            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Active());
            _transfersStore.Setup(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(Active());

            var now = new DateTime(2025, 01, 02, 03, 04, 05, DateTimeKind.Utc);
            _clock.SetupGet(c => c.UtcNow).Returns(now);

            _movApi.SetupSequence(a => a.MovmentAsync(It.IsAny<MovmentRequest>(), "token",It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(ApiOk())  
                  .ReturnsAsync(ApiOk())  
                  .ReturnsAsync(ApiOk())  
                  .ReturnsAsync(ApiOk()); 

            _transfersStore.Setup(s => s.AddTransferAsync(1, 2, now, 10m, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new InvalidOperationException("db-down"));

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"ErrorType\":\"PERSISTENCE_ERROR\"")),
                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.False);
            Assert.That(res.ErrorType, Is.EqualTo("PERSISTENCE_ERROR"));

            _movApi.Verify(a => a.MovmentAsync(It.IsAny<MovmentRequest>(), "token", It.IsAny<string>(),It.IsAny<CancellationToken>()), Times.Exactly(4));
            _transfersStore.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _idemp.VerifyAll();
        }

        [Test]
        public async Task Handle_ExcecaoTopo_RefitApiException_Retorna_REMOTE_ERROR_E_SalvaNoIdemp()
        {
            var now = DateTime.UtcNow;
            _clock.SetupGet(c => c.UtcNow).Returns(now);
            
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Active());
            _transfersStore.Setup(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Active());

            var request = new HttpRequestMessage(HttpMethod.Post, "http://local.test/v1/contas/movimento");
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Bad Request" };
            var apiEx = await ApiException.Create(request, HttpMethod.Post, response, new RefitSettings());

            _movApi.Setup(a => a.MovmentAsync(It.IsAny<MovmentRequest>(), "token", It.IsAny<string>(),It.IsAny<CancellationToken>()))
                   .ThrowsAsync(apiEx);

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"ErrorType\":\"REMOTE_ERROR\"")),
                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.False);
            Assert.That(res.ErrorType, Is.EqualTo("REMOTE_ERROR"));

            _idemp.VerifyAll();
        }

        [Test]
        public async Task Handle_ExcecaoTopo_Geral_Retorna_UNEXPECTED_ERROR_E_SalvaNoIdemp()
        {
            var now = DateTime.UtcNow;
            _clock.SetupGet(c => c.UtcNow).Returns(now);
            
            _idemp.Setup(i => i.GetResultAsync("idem", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(string.Empty);

            _transfersStore.Setup(s => s.GetAccountAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Active());
            _transfersStore.Setup(s => s.GetAccountAsync(2, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(Active());

            _movApi.Setup(a => a.MovmentAsync(It.IsAny<MovmentRequest>(), "token",It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new Exception("boom"));

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"ErrorType\":\"UNEXPECTED_ERROR\"") &&
                                   s.Contains("boom")),
                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var res = await sut.Handle(Cmd(), CancellationToken.None);

            Assert.That(res.Success, Is.False);
            Assert.That(res.ErrorType, Is.EqualTo("UNEXPECTED_ERROR"));

            _idemp.VerifyAll();
        }

        [Test]
        public async Task Handle_PropagaCancellationToken_ParaTodasAsChamadas()
        {
            using var cts = new CancellationTokenSource();

            var cmd = new TransferBetweenAccountsCommand(10, 20, 50m, "idem", "token");

            _idemp.Setup(i => i.GetResultAsync("idem", cts.Token))
                  .ReturnsAsync(string.Empty)
                  .Verifiable();

            _transfersStore.Setup(s => s.GetAccountAsync(10, cts.Token))
                           .ReturnsAsync(Active())
                           .Verifiable();

            _transfersStore.Setup(s => s.GetAccountAsync(20, cts.Token))
                           .ReturnsAsync(Active())
                           .Verifiable();

            _movApi.SetupSequence(a => a.MovmentAsync(It.IsAny<MovmentRequest>(), "token", It.IsAny<string>(),cts.Token))
                  .ReturnsAsync(ApiOk())
                  .ReturnsAsync(ApiOk());

            var now = DateTime.UtcNow;
            _clock.SetupGet(c => c.UtcNow).Returns(now);

            _transfersStore.Setup(s => s.AddTransferAsync(10, 20, now, 50m, cts.Token))
                           .Returns(Task.FromResult(1))
                           .Verifiable();

            _transfersStore.Setup(s => s.SaveChangesAsync(cts.Token))
                           .Returns(Task.CompletedTask)
                           .Verifiable();

            _idemp.Setup(i => i.SaveAsync("idem", string.Empty,
                It.Is<string>(s => s.Contains("\"Success\":true")),
                cts.Token)).Returns(Task.CompletedTask).Verifiable();

            var sut = CreateSut();
            _ = await sut.Handle(cmd, cts.Token);

            _idemp.Verify();
            _transfersStore.Verify();
        }
    }
}