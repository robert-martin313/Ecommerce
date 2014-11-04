﻿namespace MrCMS.Tasks
{
    public interface ITaskRunner
    {
        BatchExecutionResult ExecutePendingTasks();
        BatchExecutionResult ExecuteLuceneTasks();
    }
}