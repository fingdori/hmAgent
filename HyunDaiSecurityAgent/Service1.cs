using System;
using System.ServiceProcess;
using System.Threading;

namespace HyunDaiSecurityAgent
{
    public partial class Service1 : ServiceBase
    {
        private EventBinding _eventBinding = new EventBinding();
        private Thread _thread;

        public Service1()
        {
            InitializeComponent();
        }

        public void OnDebug() {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            ThreadStart threadStart = new ThreadStart(_eventBinding.Run);
            _thread = new Thread(threadStart);
            _thread.Start();
        }

        protected override void OnStop()
        {
            _thread.Abort();
        }
    }
}
