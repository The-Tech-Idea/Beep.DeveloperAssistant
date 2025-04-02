using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Extensions;
using Beep.DeveloperAssistant.Logic;

namespace Beep.DeveloperAssistant.MenuCommands
{
    [AddinAttribute(
        Caption = "Scheduling",
        Name = "DeveloperSchedulingMenuCommands",
        menu = "Developer",
        misc = "DeveloperSchedulingMenuCommands",
        ObjectType = "Beep",
        addinType = AddinType.Class,
        iconimage = "Schedulerutilities.svg",
        order = 8,
        Showin = ShowinType.Menu
    )]
    public class DeveloperSchedulingMenuCommands : IFunctionExtension
    {
        public IPassedArgs Passedargs { get; set; }
        public IDMEEditor DMEEditor { get; set; }

       
        private DeveloperSchedulingUtilities _scheduler;
        private CancellationTokenSource _cts;

        public DeveloperSchedulingMenuCommands(IAppManager pvisManager)
        {
     
            DMEEditor = pvisManager.DMEEditor;
            _scheduler = new DeveloperSchedulingUtilities(DMEEditor);
            _cts = new CancellationTokenSource();
            if (pvisManager.Tree != null)
            {
                tree = (ITree)pvisManager.Tree;
                ExtensionsHelpers = tree.ExtensionsHelpers;
            }
        }
        private ITree tree;
        public IFunctionandExtensionsHelpers ExtensionsHelpers { get; set; }

        #region Commands for DeveloperSchedulingUtilities

        [CommandAttribute(
            Caption = "Enqueue Task",
            Name = "EnqueueTaskCmd",
            Click = true,
            iconimage = "enqueue.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo EnqueueTaskCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string description = Microsoft.VisualBasic.Interaction.InputBox("Enter task description:", "Enqueue Task", "Sample Task");
                string delayStr = Microsoft.VisualBasic.Interaction.InputBox("Enter delay in seconds (or leave blank for immediate):", "Delay", "5");
                string priorityStr = Microsoft.VisualBasic.Interaction.InputBox("Enter priority (lower = higher priority):", "Priority", "1");
                string cronExpr = Microsoft.VisualBasic.Interaction.InputBox("Enter cron expression (e.g., '*/5 * * * * *' for every 5s, leave blank for one-time):", "Cron Expression", "");
                string groupName = Microsoft.VisualBasic.Interaction.InputBox("Enter group name (optional):", "Group Name", "");
                string maxRetriesStr = Microsoft.VisualBasic.Interaction.InputBox("Enter max retries (0 for none):", "Max Retries", "0");
                string timeoutStr = Microsoft.VisualBasic.Interaction.InputBox("Enter timeout in seconds (optional):", "Timeout", "");

                var task = new DeveloperSchedulingUtilities.ScheduledTask
                {
                    ScheduledTime = string.IsNullOrEmpty(delayStr) ? DateTime.Now : DateTime.Now.AddSeconds(int.Parse(delayStr)),
                    Priority = int.Parse(priorityStr),
                    Action = () => MessageBox.Show($"{description} executed!", "Task Executed", MessageBoxButtons.OK, MessageBoxIcon.Information),
                    Description = description,
                    CronExpression = string.IsNullOrEmpty(cronExpr) ? null : cronExpr,
                    GroupName = string.IsNullOrEmpty(groupName) ? null : groupName,
                    MaxRetries = int.Parse(maxRetriesStr),
                    Timeout = string.IsNullOrEmpty(timeoutStr) ? null : (TimeSpan?)TimeSpan.FromSeconds(int.Parse(timeoutStr)),
                    OnStart = t => DMEEditor.AddLogMessage("Task", $"Started: {t.Description}", DateTime.Now, 0, null, Errors.Ok),
                    OnComplete = t => DMEEditor.AddLogMessage("Task", $"Completed: {t.Description}", DateTime.Now, 0, null, Errors.Ok),
                    OnFailure = (t, ex) => DMEEditor.AddLogMessage("Task", $"Failed: {t.Description} - {ex.Message}", DateTime.Now, 0, null, Errors.Failed)
                };

                _scheduler.EnqueueTask(task);
                DMEEditor.AddLogMessage("Success", $"Task '{description}' enqueued (ID: {task.TaskId})", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error enqueuing task: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Start Scheduler",
            Name = "StartSchedulerCmd",
            Click = true,
            iconimage = "start.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo StartSchedulerCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                _scheduler.StartScheduler(_cts.Token);
                DMEEditor.AddLogMessage("Success", "Scheduler started", DateTime.Now, 0, null, Errors.Ok);
                MessageBox.Show("Scheduler has been started. Tasks will run as scheduled.", "Scheduler Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error starting scheduler: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Stop Scheduler",
            Name = "StopSchedulerCmd",
            Click = true,
            iconimage = "stop.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo StopSchedulerCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                _scheduler.StopScheduler();
                _cts = new CancellationTokenSource(); // Reset CTS for next start
                DMEEditor.AddLogMessage("Success", "Scheduler stopped", DateTime.Now, 0, null, Errors.Ok);
                MessageBox.Show("Scheduler has been stopped.", "Scheduler Stopped", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error stopping scheduler: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "List Scheduled Tasks",
            Name = "ListScheduledTasksCmd",
            Click = true,
            iconimage = "list.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ListScheduledTasksCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string groupName = Microsoft.VisualBasic.Interaction.InputBox("Enter group name (leave blank for all tasks):", "List Tasks", "");
                var tasks = _scheduler.ListScheduledTasks(groupName);
                if (tasks.Any())
                {
                    var taskList = string.Join("\n", tasks.Select(t => $"ID: {t.TaskId}, Desc: {t.Description}, Time: {t.ScheduledTime}, Priority: {t.Priority}, Cron: {t.CronExpression ?? "N/A"}"));
                    MessageBox.Show($"Scheduled Tasks:\n{taskList}", "Task List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DMEEditor.AddLogMessage("Success", $"Listed {tasks.Count} scheduled tasks", DateTime.Now, 0, null, Errors.Ok);
                }
                else
                {
                    MessageBox.Show("No tasks scheduled.", "Task List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DMEEditor.AddLogMessage("Success", "No tasks found", DateTime.Now, 0, null, Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error listing tasks: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Remove Task",
            Name = "RemoveTaskCmd",
            Click = true,
            iconimage = "remove.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo RemoveTaskCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string taskIdStr = Microsoft.VisualBasic.Interaction.InputBox("Enter task ID to remove:", "Remove Task", "");
                if (Guid.TryParse(taskIdStr, out Guid taskId))
                {
                    bool success = _scheduler.RemoveTask(taskId);
                    if (success)
                    {
                        DMEEditor.AddLogMessage("Success", $"Task ID {taskId} removed", DateTime.Now, 0, null, Errors.Ok);
                        MessageBox.Show($"Task {taskId} removed.", "Task Removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Task ID {taskId} not found", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
                else
                {
                    DMEEditor.AddLogMessage("Fail", "Invalid task ID format", DateTime.Now, 0, null, Errors.Failed);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error removing task: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Update Task",
            Name = "UpdateTaskCmd",
            Click = true,
            iconimage = "update.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo UpdateTaskCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string taskIdStr = Microsoft.VisualBasic.Interaction.InputBox("Enter task ID to update:", "Update Task", "");
                if (Guid.TryParse(taskIdStr, out Guid taskId))
                {
                    string delayStr = Microsoft.VisualBasic.Interaction.InputBox("Enter new delay in seconds (optional):", "Delay", "");
                    string priorityStr = Microsoft.VisualBasic.Interaction.InputBox("Enter new priority (optional):", "Priority", "");
                    string cronExpr = Microsoft.VisualBasic.Interaction.InputBox("Enter new cron expression (optional, e.g., '*/5 * * * * *'):", "Cron Expression", "");

                    bool success = _scheduler.UpdateTask(
                        taskId,
                        newScheduledTime: string.IsNullOrEmpty(delayStr) ? null : (DateTime?)DateTime.Now.AddSeconds(int.Parse(delayStr)),
                        newPriority: string.IsNullOrEmpty(priorityStr) ? null : (int?)int.Parse(priorityStr),
                        newCronExpression: string.IsNullOrEmpty(cronExpr) ? null : cronExpr
                    );

                    if (success)
                    {
                        DMEEditor.AddLogMessage("Success", $"Task ID {taskId} updated", DateTime.Now, 0, null, Errors.Ok);
                        MessageBox.Show($"Task {taskId} updated.", "Task Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Task ID {taskId} not found", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
                else
                {
                    DMEEditor.AddLogMessage("Fail", "Invalid task ID format", DateTime.Now, 0, null, Errors.Failed);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error updating task: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "View Task Status",
            Name = "ViewTaskStatusCmd",
            Click = true,
            iconimage = "status.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ViewTaskStatusCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                string taskIdStr = Microsoft.VisualBasic.Interaction.InputBox("Enter task ID to view status:", "Task Status", "");
                if (Guid.TryParse(taskIdStr, out Guid taskId))
                {
                    var status = _scheduler.GetTaskStatus(taskId);
                    if (status != null)
                    {
                        var statusMsg = $"Task ID: {taskId}\nState: {status.State}\nScheduled: {status.ScheduledTime}\nLast Run: {status.LastRunTime?.ToString() ?? "N/A"}\nExecutions: {status.ExecutionCount}\nLast Error: {status.LastError ?? "None"}";
                        MessageBox.Show(statusMsg, "Task Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DMEEditor.AddLogMessage("Success", $"Displayed status for task ID {taskId}", DateTime.Now, 0, null, Errors.Ok);
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Fail", $"Task ID {taskId} not found", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
                else
                {
                    DMEEditor.AddLogMessage("Fail", "Invalid task ID format", DateTime.Now, 0, null, Errors.Failed);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error viewing task status: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Save Tasks to File",
            Name = "SaveTasksToFileCmd",
            Click = true,
            iconimage = "save.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> SaveTasksToFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*", Title = "Save tasks to file" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        await _scheduler.SaveTasksToFileAsync(sfd.FileName);
                        DMEEditor.AddLogMessage("Success", $"Tasks saved to {sfd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        MessageBox.Show($"Tasks saved to {sfd.FileName}", "Save Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error saving tasks: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "Load Tasks from File",
            Name = "LoadTasksFromFileCmd",
            Click = true,
            iconimage = "load.png",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public async Task<IErrorsInfo> LoadTasksFromFileCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*", Title = "Load tasks from file" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        // Simple resolver: maps description to a MessageBox action
                        Func<string, Action> actionResolver = desc => () => MessageBox.Show($"{desc} executed!", "Task Executed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await _scheduler.LoadTasksFromFileAsync(ofd.FileName, actionResolver);
                        DMEEditor.AddLogMessage("Success", $"Tasks loaded from {ofd.FileName}", DateTime.Now, 0, null, Errors.Ok);
                        MessageBox.Show($"Tasks loaded from {ofd.FileName}", "Load Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error loading tasks: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        [CommandAttribute(
            Caption = "View Scheduler Stats",
            Name = "ViewSchedulerStatsCmd",
            Click = true,
            iconimage = "Schedulerutilities.svg",
            ObjectType = "Beep",
            PointType = EnumPointType.Function,
            Showin = ShowinType.Menu)]
        public IErrorsInfo ViewSchedulerStatsCmd(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                var (queueSize, activeTasks) = _scheduler.GetSchedulerStats();
                var statsMsg = $"Queue Size: {queueSize}\nActive Tasks: {activeTasks}";
                MessageBox.Show(statsMsg, "Scheduler Stats", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DMEEditor.AddLogMessage("Success", $"Scheduler stats: {statsMsg}", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Error viewing scheduler stats: {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        #endregion
    }
}