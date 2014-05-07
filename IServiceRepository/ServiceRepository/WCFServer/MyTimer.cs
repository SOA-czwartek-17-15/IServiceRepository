using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using WCFServer.Models;

namespace WCFServer
{
    class MyTimer
    {
        private int m_nStart = 0;
        Repository repos;

        public MyTimer(Repository rep){
            repos = rep;
        }

        public void StartTimer()
        {
            m_nStart = Environment.TickCount;
            Timer oTimer = new Timer();
            oTimer.Elapsed += new ElapsedEventHandler(OnTimeEvent);
            oTimer.Interval = 1000;
            oTimer.Enabled = true;
            Console.Read();
            oTimer.Stop();
        }

        private void OnTimeEvent(object oSource,
            ElapsedEventArgs oElapsedEventArgs)
        {
            //Console.WriteLine("Upłyneło {0} milisekud",Environment.TickCount - m_nStart);
            repos.KillZombieServices();
        }
    }
}
