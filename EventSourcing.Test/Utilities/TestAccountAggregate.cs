using EventSourcing.Models;
using static EventSourcing.Events.Event;

namespace EventSourcing.Test.Utilities;

public class TestAccountAggregate
{
  public string? AccountId { get; set; }
  public decimal Balance { get; set; }
  public CurrencyType Currency { get; set; }
  public string? CustomerId { get; set; }
  public AccountStatus Status { get; set; }
  public List<LogMessage>? AccountLog { get; set; }
  public CurrencyType NewCurrency { get; set; }
}
