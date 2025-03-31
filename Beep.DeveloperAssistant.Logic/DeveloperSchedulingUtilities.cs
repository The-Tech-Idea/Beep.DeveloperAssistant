using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;

namespace Beep.DeveloperAssistant.Logic
{
    public class DeveloperSchedulingUtilities
    {
        private readonly IDMEEditor _dmeEditor;
        private readonly SortedSet<ScheduledTask> _taskQueue;
        private readonly ConcurrentDictionary<Guid, TaskStatus> _taskStatus;
        private readonly ConcurrentDictionary<string, List<Guid>> _taskGroups;
        private readonly object _queueLock = new object();
        private CancellationTokenSource _cts;
        private Task _schedulerTask;
        private readonly int _maxConcurrentTasks;
        private readonly SemaphoreSlim _concurrencySemaphore;

        public DeveloperSchedulingUtilities(IDMEEditor dmeEditor = null, int maxConcurrentTasks = 5)
        {
            _dmeEditor = dmeEditor;
            _taskQueue = new SortedSet<ScheduledTask>(new ScheduledTaskComparer());
            _taskStatus = new ConcurrentDictionary<Guid, TaskStatus>();
            _taskGroups = new ConcurrentDictionary<string, List<Guid>>();
            _maxConcurrentTasks = maxConcurrentTasks;
            _concurrencySemaphore = new SemaphoreSlim(maxConcurrentTasks, maxConcurrentTasks);
        }

        #region Advanced Scheduler Methods

        public void EnqueueTask(ScheduledTask task)
        {
            if (task == null)
            {
                _dmeEditor?.AddLogMessage("DeveloperSchedulingUtilities", "EnqueueTask: task is null", DateTime.Now, 0, null, Errors.Failed);
                return;
            }
            lock (_queueLock)
            {
                _taskQueue.Add(task);
                _taskStatus[task.TaskId] = new TaskStatus { State = TaskState.Pending, ScheduledTime = task.ScheduledTime };
                if (!string.IsNullOrEmpty(task.GroupName))
                {
                    _taskGroups.GetOrAdd(task.GroupName, _ => new List<Guid>()).Add(task.TaskId);
                }
            }
            _dmeEditor?.AddLogMessage("DeveloperSchedulingUtilities", $"Task enqueued: {task.Description} (ID: {task.TaskId})", DateTime.Now, 0, null, Errors.Ok);
        }

        public void StartScheduler(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _schedulerTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    ScheduledTask taskToRun = null;
                    lock (_queueLock)
                    {
                        if (_taskQueue.Count > 0)
                        {
                            var firstTask = _taskQueue.Min;
                            if (firstTask.ScheduledTime <= DateTime.Now && CheckDependencies(firstTask))
                            {
                                taskToRun = firstTask;
                                _taskQueue.Remove(firstTask);
                                _taskStatus[taskToRun.TaskId].State = TaskState.Running;
                                _taskStatus[taskToRun.TaskId].LastRunTime = DateTime.Now;
                                taskToRun.OnStart?.Invoke(taskToRun);
                            }
                        }
                    }

                    if (taskToRun != null)
                    {
                        await _concurrencySemaphore.WaitAsync(_cts.Token);
                        try
                        {
                            await ExecuteTaskWithTimeoutAsync(taskToRun);
                        }
                        finally
                        {
                            _concurrencySemaphore.Release();
                        }
                    }
                    else
                    {
                        await Task.Delay(100, _cts.Token); // Reduced delay for responsiveness
                    }
                }
            }, _cts.Token);
        }

        public void StopScheduler()
        {
            _cts?.Cancel();
            try
            {
                _schedulerTask?.Wait();
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    _dmeEditor?.AddLogMessage("Scheduler", $"Stop error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                }
            }
        }

        #endregion

        #region Enhanced Scheduler Methods

        public List<ScheduledTask> ListScheduledTasks(string groupName = null)
        {
            lock (_queueLock)
            {
                if (string.IsNullOrEmpty(groupName))
                    return _taskQueue.ToList();
                if (_taskGroups.TryGetValue(groupName, out var groupIds))
                    return _taskQueue.Where(t => groupIds.Contains(t.TaskId)).ToList();
                return new List<ScheduledTask>();
            }
        }

        public bool RemoveTask(Guid taskId)
        {
            lock (_queueLock)
            {
                var task = _taskQueue.FirstOrDefault(t => t.TaskId == taskId);
                if (task != null)
                {
                    _taskQueue.Remove(task);
                    _taskStatus.TryRemove(taskId, out _);
                    if (!string.IsNullOrEmpty(task.GroupName) && _taskGroups.TryGetValue(task.GroupName, out var groupIds))
                        groupIds.Remove(taskId);
                    _dmeEditor?.AddLogMessage("DeveloperSchedulingUtilities", $"Task removed: {task.Description} (ID: {taskId})", DateTime.Now, 0, null, Errors.Ok);
                    return true;
                }
                return false;
            }
        }

        public bool UpdateTask(Guid taskId, DateTime? newScheduledTime = null, int? newPriority = null, TimeSpan? newRecurringInterval = null, string newCronExpression = null)
        {
            lock (_queueLock)
            {
                var task = _taskQueue.FirstOrDefault(t => t.TaskId == taskId);
                if (task != null)
                {
                    _taskQueue.Remove(task);
                    if (newScheduledTime.HasValue) task.ScheduledTime = newScheduledTime.Value;
                    if (newPriority.HasValue) task.Priority = newPriority.Value;
                    if (newRecurringInterval.HasValue) task.RecurringInterval = newRecurringInterval;
                    if (!string.IsNullOrEmpty(newCronExpression)) task.CronExpression = newCronExpression;
                    _taskQueue.Add(task);
                    _taskStatus[task.TaskId].ScheduledTime = task.ScheduledTime;
                    _dmeEditor?.AddLogMessage("DeveloperSchedulingUtilities", $"Task updated: {task.Description} (ID: {taskId})", DateTime.Now, 0, null, Errors.Ok);
                    return true;
                }
                return false;
            }
        }

        public TaskStatus GetTaskStatus(Guid taskId)
        {
            return _taskStatus.TryGetValue(taskId, out var status) ? status : null;
        }

        public async Task SaveTasksToFileAsync(string filePath)
        {
            try
            {
                var tasks = new List<TaskSnapshot>();
                lock (_queueLock)
                {
                    tasks = _taskQueue.Select(t => new TaskSnapshot
                    {
                        TaskId = t.TaskId,
                        ScheduledTime = t.ScheduledTime,
                        Priority = t.Priority,
                        Description = t.Description,
                        RecurringInterval = t.RecurringInterval,
                        CronExpression = t.CronExpression,
                        Dependencies = t.Dependencies,
                        GroupName = t.GroupName,
                        MaxRetries = t.MaxRetries,
                        Timeout = t.Timeout,
                        Status = _taskStatus.TryGetValue(t.TaskId, out var status) ? status : null
                    }).ToList();
                }
                var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve });
                await File.WriteAllTextAsync(filePath, json);
                _dmeEditor?.AddLogMessage("DeveloperSchedulingUtilities", $"Tasks saved to {filePath}", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("DeveloperSchedulingUtilities", $"Error saving tasks: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
        }


        public async Task LoadTasksFromFileAsync(string filePath, Func<string, Action> actionResolver)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _dmeEditor?.AddLogMessage("DeveloperSchedulingUtilities", $"File not found: {filePath}", DateTime.Now, 0, null, Errors.Failed);
                    return;
                }
                var json = await File.ReadAllTextAsync(filePath);
                var snapshots = JsonSerializer.Deserialize<List<TaskSnapshot>>(json, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve });
                lock (_queueLock)
                {
                    foreach (var snapshot in snapshots)
                    {
                        var task = new ScheduledTask
                        {
                            ScheduledTime = snapshot.ScheduledTime,
                            Priority = snapshot.Priority,
                            Action = actionResolver(snapshot.Description),
                            Description = snapshot.Description,
                            RecurringInterval = snapshot.RecurringInterval,
                            CronExpression = snapshot.CronExpression,
                            Dependencies = snapshot.Dependencies ?? new List<Guid>(),
                            GroupName = snapshot.GroupName,
                            MaxRetries = snapshot.MaxRetries,
                            Timeout = snapshot.Timeout
                        };
                        _taskQueue.Add(task);
                        _taskStatus[task.TaskId] = snapshot.Status ?? new TaskStatus { State = TaskState.Pending, ScheduledTime = task.ScheduledTime };
                        if (!string.IsNullOrEmpty(task.GroupName))
                            _taskGroups.GetOrAdd(task.GroupName, _ => new List<Guid>()).Add(task.TaskId);
                    }
                }
                _dmeEditor?.AddLogMessage("DeveloperSchedulingUtilities", $"Loaded {snapshots.Count} tasks from {filePath}", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("DeveloperSchedulingUtilities", $"Error loading tasks: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
        }

        public (int QueueSize, int ActiveTasks) GetSchedulerStats()
        {
            lock (_queueLock)
            {
                return (_taskQueue.Count, _taskStatus.Count(t => t.Value.State == TaskState.Running));
            }
        }

        #endregion

        #region Helper Scheduling Methods

        public Timer ScheduleTask(Action action, TimeSpan delay)
        {
            Timer timer = new Timer(_ => action(), null, delay, Timeout.InfiniteTimeSpan);
            return timer;
        }

        public Timer ScheduleRecurringTask(Action action, TimeSpan dueTime, TimeSpan period, CancellationToken token)
        {
            Timer timer = null;
            timer = new Timer(state =>
            {
                if (token.IsCancellationRequested)
                {
                    timer.Dispose();
                }
                else
                {
                    action();
                }
            }, null, dueTime, period);
            return timer;
        }

        public async Task ScheduleTaskAsync(Action action, TimeSpan delay, CancellationToken token, Action<long> progressCallback = null)
        {
            long elapsed = 0;
            while (elapsed < delay.TotalMilliseconds && !token.IsCancellationRequested)
            {
                await Task.Delay(100, token);
                elapsed += 100;
                progressCallback?.Invoke(elapsed);
            }
            if (!token.IsCancellationRequested)
            {
                action();
            }
        }

        #endregion

        #region Enhanced Execution Logic

        private async Task ExecuteTaskWithTimeoutAsync(ScheduledTask task)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
            if (task.Timeout.HasValue)
                cts.CancelAfter(task.Timeout.Value);

            try
            {
                await Task.Run(async () =>
                {
                    int retries = 0;
                    while (retries <= task.MaxRetries && !cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await Task.Run(task.Action, cts.Token);
                            _taskStatus[task.TaskId].State = TaskState.Completed;
                            _taskStatus[task.TaskId].ExecutionCount++;
                            task.OnComplete?.Invoke(task);
                            break;
                        }
                        catch (Exception ex)
                        {
                            retries++;
                            _taskStatus[task.TaskId].LastError = ex.Message;
                            if (retries > task.MaxRetries)
                            {
                                _taskStatus[task.TaskId].State = TaskState.Failed;
                                task.OnFailure?.Invoke(task, ex);
                                _dmeEditor?.AddLogMessage("Scheduler", $"Task {task.Description} (ID: {task.TaskId}) failed after {retries} retries: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                                break;
                            }
                            await Task.Delay(1000 * retries, cts.Token); // Exponential backoff
                        }
                    }
                }, cts.Token);

                if (task.CronExpression != null)
                {
                    var cron = CronExpression.Parse(task.CronExpression);
                    var next = cron.GetNextOccurrence(DateTime.Now, TimeZoneInfo.Local);
                    if (next.HasValue)
                    {
                        task.ScheduledTime = next.Value;
                        EnqueueTask(task);
                    }
                }
                else if (task.RecurringInterval.HasValue)
                {
                    task.ScheduledTime = DateTime.Now + task.RecurringInterval.Value;
                    EnqueueTask(task);
                }
            }
            catch (TaskCanceledException)
            {
                _taskStatus[task.TaskId].State = TaskState.Failed;
                _taskStatus[task.TaskId].LastError = "Task timed out";
                task.OnFailure?.Invoke(task, new TimeoutException("Task execution exceeded timeout"));
                _dmeEditor?.AddLogMessage("Scheduler", $"Task {task.Description} (ID: {task.TaskId}) timed out", DateTime.Now, 0, null, Errors.Failed);
            }
        }

        private bool CheckDependencies(ScheduledTask task)
        {
            if (task.Dependencies == null || !task.Dependencies.Any()) return true;
            return task.Dependencies.All(depId => _taskStatus.TryGetValue(depId, out var status) && status.State == TaskState.Completed);
        }

        #endregion

        #region Nested Classes

        public class ScheduledTask
        {
            public DateTime ScheduledTime { get; set; }
            public int Priority { get; set; }
            public Action Action { get; set; }
            public TimeSpan? RecurringInterval { get; set; }
            public string CronExpression { get; set; }
            public string Description { get; set; }
            public Guid TaskId { get; } = Guid.NewGuid();
            public List<Guid> Dependencies { get; set; } = new List<Guid>();
            public string GroupName { get; set; }
            public int MaxRetries { get; set; } = 0;
            public TimeSpan? Timeout { get; set; }
            public Action<ScheduledTask> OnStart { get; set; }
            public Action<ScheduledTask> OnComplete { get; set; }
            public Action<ScheduledTask, Exception> OnFailure { get; set; }
        }

        public class ScheduledTaskComparer : IComparer<ScheduledTask>
        {
            public int Compare(ScheduledTask x, ScheduledTask y)
            {
                if (x == null || y == null)
                    throw new ArgumentException("Cannot compare null ScheduledTask");
                int timeComparison = x.ScheduledTime.CompareTo(y.ScheduledTime);
                if (timeComparison != 0) return timeComparison;
                int priorityComparison = x.Priority.CompareTo(y.Priority);
                if (priorityComparison != 0) return priorityComparison;
                return x.TaskId.CompareTo(y.TaskId);
            }
        }

        public class TaskStatus
        {
            public TaskState State { get; set; }
            public DateTime ScheduledTime { get; set; }
            public DateTime? LastRunTime { get; set; }
            public int ExecutionCount { get; set; }
            public string LastError { get; set; }
        }

        public enum TaskState
        {
            Pending,
            Running,
            Completed,
            Failed
        }

        private class TaskSnapshot
        {
            public Guid TaskId { get; set; }
            public DateTime ScheduledTime { get; set; }
            public int Priority { get; set; }
            public string Description { get; set; }
            public TimeSpan? RecurringInterval { get; set; }
            public string CronExpression { get; set; }
            public List<Guid> Dependencies { get; set; }
            public string GroupName { get; set; }
            public int MaxRetries { get; set; }
            public TimeSpan? Timeout { get; set; }
            public TaskStatus Status { get; set; }
        }

        #endregion

        #region Windows Service Integration Example
        /*
         * In a Windows Service:
         * OnStart: var scheduler = new DeveloperSchedulingUtilities(DMEEditor); scheduler.StartScheduler(CancellationToken.None);
         * OnStop: scheduler.StopScheduler();
         */
        #endregion
    }
}