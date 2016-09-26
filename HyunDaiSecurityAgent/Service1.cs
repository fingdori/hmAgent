using System;
using System.ServiceProcess;
using System.Threading;

namespace HyunDaiSecurityAgent
{
    public partial class Service1 : ServiceBase
    {
        private EventBinding eventBinding = new EventBinding();
        private Thread thread;

        public Service1()
        {
            InitializeComponent();
        }

        public void OnDebug() {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            ThreadStart threadStart = new ThreadStart(eventBinding.Run);
            thread = new Thread(threadStart);
            thread.Start();
            System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory +  "startHyunDai.txt");
        }

        protected override void OnStop()
        {
        }
    }
}
