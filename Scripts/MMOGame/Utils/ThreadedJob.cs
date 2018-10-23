using System.Collections;
using System.Threading;

public class ThreadedJob
{
    private bool m_IsDone = false;
    private object m_Handle = new object();
#if !DISABLE_THREAD
    private Thread m_Thread = null;
#endif

    public bool IsDone
    {
        get
        {
            bool tmp;
            lock (m_Handle)
            {
                tmp = m_IsDone;
            }
            return tmp;
        }
        set
        {
            lock (m_Handle)
            {
                m_IsDone = value;
            }
        }
    }

    public virtual void Start()
    {
#if !DISABLE_THREAD
        m_Thread = new Thread(Run);
        m_Thread.Start();
#else
        UnityEngine.Debug.Log("Running Thread Function [" + GetType().Name + "]");
        Run();
#endif
    }

    public virtual void Abort()
    {
#if !DISABLE_THREAD
        m_Thread.Abort();
#endif
    }

    protected virtual void ThreadFunction() { }

    protected virtual void OnFinished() { }

    public virtual bool Update()
    {
        if (IsDone)
        {
            OnFinished();
            return true;
        }
        return false;
    }

    public IEnumerator WaitFor()
    {
        while (!Update())
        {
            yield return 0;
        }
    }

    private void Run()
    {
        ThreadFunction();
        IsDone = true;
    }
}
