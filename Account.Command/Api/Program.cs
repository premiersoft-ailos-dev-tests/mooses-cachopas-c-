using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Bankmore.Accounts.Command.Api.Security;
using Bankmore.Accounts.Command.Application.Commands.Accounts.CreateAccount;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using Bankmore.Accounts.Command.Infrastructure;
using System.Reflection;
using Bankmore.Command.Infra.Profiles;
using Bankmore.Accounts.Command.Application.Profiles.Aplication.Movments;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddFluentValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAccountCommandValidator>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly));
builder.Services.AddAutoMapper(
    typeof(MovmentsProfile).Assembly,  
    typeof(AccountDBProfile).Assembly,  
    Assembly.GetExecutingAssembly());

var cs = builder.Configuration.GetConnectionString("CommandDb")!;
builder.Services.AddCommandInfrastructure(cs);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"];
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BankMore - Accounts Command API",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "BankMore - Accounts Command API v1");
    o.RoutePrefix = "swagger";
});

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var (status, title, detail, type) = ex switch
        {
            BusinessRuleException bre => (400, "Regra de negócio violada", bre.Message, bre.ErrorType),
            ValidationException ve => (400, "Validação falhou", string.Join("; ", ve.Errors.Select(e => e.ErrorMessage)), "VALIDATION_ERROR"),
            UnauthorizedAccessException uae => (401, "Não autorizado", string.IsNullOrWhiteSpace(uae.Message) ? "Credenciais inválidas." : uae.Message, "USER_UNAUTHORIZED"),
            _ => (500, "Erro interno do servidor", "Ocorreu um erro inesperado.", "INTERNAL_ERROR")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();