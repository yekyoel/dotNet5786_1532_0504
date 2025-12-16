namespace BlApi;

public interface IAdmin
{
    public void ResetDB();
    public void InitializeDB();
    public DateTime GetClock();
    public DateTime ForwardClock(BO.Time forward);
    public BO.Config GetConfig();
    public void SetConfig(BO.Config config);
    void AddConfigObserver(Action configObserver);
    void RemoveConfigObserver(Action configObserver);
    void AddClockObserver(Action clockObserver);
    void RemoveClockObserver(Action clockObserver);
}
