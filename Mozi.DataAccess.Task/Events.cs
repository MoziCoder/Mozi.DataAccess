namespace Mozi.DataAccess.TaskQuence
{
    public delegate void TaskAdded(object sender, SqlTask st);
    public delegate void TaskStateChange(object sender, SqlTask ts,SqlTaskState oldState,SqlTaskState newState);
}
