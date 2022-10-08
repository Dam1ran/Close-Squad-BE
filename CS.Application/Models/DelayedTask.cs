namespace CS.Application.Models;
public class DelayedTask {
  public DateTimeOffset ExecuteAt { get; private set;}
  public Task Task { get; private set;}
  public bool Executing { get; set; }
  public DelayedTask(Task task, DateTimeOffset executeAt) {
    Task = task;
    ExecuteAt = executeAt;
  }

}
