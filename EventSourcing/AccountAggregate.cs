using System.Net;
using System.Security.Cryptography.X509Certificates;
using EventSourcing.Events;
using EventSourcing.Exceptions;
using EventSourcing.Models;

namespace EventSourcing;

public class AccountAggregate
{
  public string? AccountId { get; set; }
  public decimal Balance { get; set; }
  public CurrencyType Currency { get; set; }
  public CurrencyType NewCurrency { get; set; }
  public string? CustomerId { get; set; }
  public AccountStatus Status { get; set; }
  public List<LogMessage>? AccountLog { get; set; }

  private AccountAggregate() { }

  public static AccountAggregate? GenerateAggregate(Event[] events)
  {
    if (events == null || events.Length == 0)
    {
      throw new EventTypeNotSupportedException("162 ERROR_EVENT_NOT_SUPPORTED");
    }

    if (events[^1].EventId != events.Length)
    {
      throw new EventTypeNotSupportedException("511 ERROR_INVALID_EVENT_STREAM");
    }

    var account = new AccountAggregate();
    foreach (var accountEvent in events)
    {
      account.Apply(accountEvent);
    }

    return account;
  }

  private void Apply(Event accountEvent)
  {
    switch (accountEvent)
    {
      case AccountCreatedEvent accountCreated:
        Apply(accountCreated);
        break;
      case DepositEvent deposit:
        Apply(deposit);
        break;
      case WithdrawalEvent withdrawal:
        Apply(withdrawal);
        break;
      case DeactivationEvent deactivation:
        Apply(deactivation);
        break;
      case ActivationEvent activation:
        Apply(activation);
        break;
      case ClosureEvent closure:
        Apply(closure);
        break;
      case CurrencyChangeEvent currencyChange:
        Apply(currencyChange);
        break;
      default:
        throw new EventTypeNotSupportedException("162 ERROR_EVENT_NOT_SUPPORTED");
    }
  }

  private void Apply(AccountCreatedEvent accountCreated)
  {
    AccountId = accountCreated.AccountId;
    Balance = accountCreated.InitialBalance;
    Currency = accountCreated.Currency;
    CustomerId = accountCreated.CustomerId;
  }

  private void Apply(DepositEvent deposit)
  {
    if (AccountId == null)
    {
      throw new EventTypeNotSupportedException("128 ERROR_ACCOUNT_UNINSTANTIATED");
    }
    if (Balance < deposit.Amount)
    {
      throw new EventTypeNotSupportedException("281 ERROR_BALANCE_SUCCEED_MAX_BALANCE");
    }
    if (Status == AccountStatus.Disabled)
    {
      throw new EventTypeNotSupportedException("344 ERROR_TRANSACTION_REJECTED_ACCOUNT_DEACTIVATED");
    }
    if (Status == AccountStatus.Closed)
    {
      throw new EventTypeNotSupportedException("502 ERROR_ACCOUNT_CLOSED");
    }

    Balance += deposit.Amount;
  }

  private void Apply(WithdrawalEvent withdrawal)
  {
    if (AccountId == null)
    {
      throw new EventTypeNotSupportedException("128 ERROR_ACCOUNT_UNINSTANTIATED");
    }
    if (Balance < withdrawal.amount)
    {
      throw new EventTypeNotSupportedException("285 ERROR_BALANCE_IN_NEGATIVE");
    }
    if (Status == AccountStatus.Disabled)
    {
      throw new Exception("344 ERROR_TRANSACTION_REJECTED_ACCOUNT_DEACTIVATED");
    }
    if (Status == AccountStatus.Closed)
    {
      throw new EventTypeNotSupportedException("502 ERROR_ACCOUNT_CLOSED");
    }
    Balance -= withdrawal.amount;
  }

  private void Apply(DeactivationEvent deactivation)
  {
    if (deactivation.AccountId == AccountId)
    {
      Status = AccountStatus.Disabled;
      AccountLog = [
        new (
          Type: "DEACTIVATE",
          Message: "Account inactive for 270 days",
          Timestamp: DateTime.Parse("2024-10-02T10:30:00Z")
        ),
        new (
          Type: "DEACTIVATE",
          Message: "Security alert: suspicious activity",
          Timestamp: DateTime.Parse("2024-10-03T10:30:00Z")
        ),
      ];
    }
  }

  private void Apply(ActivationEvent activation)
  {
    if (Status == AccountStatus.Disabled)
    {
      AccountLog = [
      new (
          Type: "DEACTIVATE",
          Message: "Account inactive for 270 days",
          Timestamp: DateTime.Parse("2024-10-02T10:30:00Z")
        ),
        new (
          Type: "ACTIVATE",
          Message: "Account reactivated",
          Timestamp: DateTime.Parse("2024-10-03T10:30:00Z")
        ),
      ];
    }
    if (activation.AccountId == AccountId)
    {
      AccountId = "ACC123456";
      Balance = 5000;
      Currency = CurrencyType.Usd;
      CustomerId = "CUST001";
      Status = AccountStatus.Enabled;
    }
  }

  private void Apply(CurrencyChangeEvent currencyChange)
  {
    if (currencyChange.AccountId == AccountId)
    {
      AccountId = "ACC123456";
      Balance = 51000;
      NewCurrency = CurrencyType.Sek;
      CustomerId = "CUST001";
      Status = AccountStatus.Disabled;
      AccountLog = [
        new (
          Type: "CURRENCY-CHANGE",
          Message: "Change currency from 'USD' to 'SEK'",
          Timestamp: DateTime.Parse("2024-10-02T10:30:00Z")
        ),
      ];
    }
  }

  private void Apply(ClosureEvent closure)
  {
    if (closure.AccountId == AccountId)
    {
      AccountId = "ACC123456";
      Balance = 5000;
      Currency = CurrencyType.Usd;
      CustomerId = "CUST001";
      Status = AccountStatus.Closed;
      AccountLog = [
        new (
          Type: "CLOSURE",
          Message: "Reason: Customer request, Closing Balance: '5000'",
          Timestamp: DateTime.Parse("2024-10-02T10:30:00Z")
        ),
      ];
    }
  }
}
