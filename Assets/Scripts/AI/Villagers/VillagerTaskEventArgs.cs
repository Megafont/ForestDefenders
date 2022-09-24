using System;


public class VillagerTaskEventArgs : EventArgs
{



    public VillagerTaskEventArgs(VillagerTasks task, object taskTarget)
    {
        Task = task;
        TaskTarget = taskTarget;
    }



    public VillagerTasks Task { get; private set; }
    public object TaskTarget { get; private set; } 

}
