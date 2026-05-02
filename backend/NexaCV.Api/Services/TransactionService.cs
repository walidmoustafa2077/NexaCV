using NexaCV.Api.DTOs.Transactions;
using NexaCV.Api.Enums;
using NexaCV.Api.Extensions;
using NexaCV.Api.Models;
using NexaCV.Api.Repositories;
using NexaCV.Api.Services.Payment;

namespace NexaCV.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly IResumeRepository _resumes;
    private readonly ITransactionRepository _transactions;
    private readonly IRegenerationRepository _regenerations;
    private readonly ICurrencyService _currency;
    private readonly PaymentGatewayFactory _gatewayFactory;

    public TransactionService(
        IResumeRepository resumes,
        ITransactionRepository transactions,
        IRegenerationRepository regenerations,
        ICurrencyService currency,
        PaymentGatewayFactory gatewayFactory)
    {
        _resumes = resumes;
        _transactions = transactions;
        _regenerations = regenerations;
        _currency = currency;
        _gatewayFactory = gatewayFactory;
    }

    public async Task<CheckoutResponse> CheckoutAsync(Guid resumeId, Guid userId, string currency)
    {
        var resume = await _resumes.GetWithTemplateAsync(resumeId)
            ?? throw new KeyNotFoundException("Resume not found.");

        if (resume.UserId != userId)
            throw new ForbiddenException("Access denied.");

        if (resume.Status != ResumeStatus.Completed)
            throw new InvalidOperationException("Resume must be COMPLETED before checkout.");

        // Idempotency: return 409 if a PENDING transaction already exists for this resume
        var existingTx = await _transactions.GetByResumeIdAsync(resumeId);
        if (existingTx is { PaymentStatus: PaymentStatus.Pending })
            throw new ConflictException("A pending transaction already exists for this resume.");

        // --- Price Engine: all prices are stored in USD; convert on the fly ---
        var exchangeRate = await _currency.GetExchangeRateAsync(currency);

        var baseAmount = Math.Round(resume.Template.BasePriceUsd * exchangeRate, 2);
        var regenUsd = await _regenerations.GetUsdCostSumAsync(resumeId);
        var regenAmount = Math.Round(regenUsd * exchangeRate, 2);
        var totalAmount = baseAmount + regenAmount;

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ResumeId = resumeId,
            BaseAmount = baseAmount,
            RegenAmount = regenAmount,
            TotalAmount = totalAmount,
            Currency = currency.ToUpperInvariant(),
            ExchangeRateUsed = exchangeRate,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _transactions.AddAsync(tx);

        var gateway = _gatewayFactory.Resolve(currency);
        var result = await gateway.CreateSessionAsync(
            new PaymentRequest(tx.Id, tx.TotalAmount, currency, resumeId));

        tx.GatewayRefId = result.GatewayRefId;
        await _transactions.UpdateAsync(tx);

        return tx.ToCheckoutResponse(result.PaymentUrl);
    }

    public async Task<TransactionDto> GetByIdAsync(Guid txId, Guid userId)
    {
        var tx = await _transactions.GetByIdAsync(txId)
            ?? throw new KeyNotFoundException("Transaction not found.");

        if (tx.UserId != userId)
            throw new ForbiddenException("Access denied.");

        return tx.ToDto();
    }

    public async Task FulfillAsync(string gatewayRefId)
    {
        var tx = await _transactions.GetByGatewayRefIdAsync(gatewayRefId)
            ?? throw new KeyNotFoundException("Transaction not found for the given gateway reference.");

        // Idempotency: already fulfilled — return without re-processing
        if (tx.PaymentStatus == PaymentStatus.Success)
            return;

        tx.PaymentStatus = PaymentStatus.Success;
        tx.CompletedAt = DateTime.UtcNow;
        await _transactions.UpdateAsync(tx);

        var resume = await _resumes.GetByIdAsync(tx.ResumeId)
            ?? throw new KeyNotFoundException("Associated resume not found.");

        resume.Status = ResumeStatus.Paid;
        resume.UpdatedAt = DateTime.UtcNow;
        await _resumes.UpdateAsync(resume);
    }
}
