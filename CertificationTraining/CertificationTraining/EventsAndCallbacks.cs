using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificationTraining
{
    public static class EventsAndCallbacks
    {
        // Before async and await, events were the only way to implement asynchrony
        // Now events are used primarily for inter-process communication
        // Think of a delegate as a piece of data that contains a reference
        //  to a particular method on a class
        // Like when you give your phone number to the mechanic to call me when I can get the car back

        // You can use an Action delegate to create a binding point for subscribers
        public static void AlarmListener1()
        {
            Console.WriteLine("Alarm listener 1 called, I do something cool for another process");
        }

        public static void AlarmListener2()
        {
            Console.WriteLine("Alarm listener 2 called, I do cool stuff too but somewhere else");
        }

        // Delegates added to a published event are called on the same thread as the thread publishing the event.
        // If a delegate blocks this thread, the entire publication mechanism is blocked.
        // This means a malicious or badly written subscriber has the ability to block the publication of events.
        // This can be addressed by the publisher starting an individual task to run each of the event subscribers
        // The delegate object in a publisher exposes a method called GetInvokationList, which can be used to get a list of all subscribers
        public static void DemoActionDelegate()
        {
            Alarm alarm = new Alarm();

            // connect two listener methods, subscribe to the event with two behaviors
            // the += operator is overloaded
            // it adds a behavior to the ones for this delegate
            // not guaranteed to call in order of adding
            alarm.OnAlarmRaised += AlarmListener1;
            alarm.OnAlarmRaised += AlarmListener2;

            // raise the alarm
            alarm.RaiseAlarm();
            Console.WriteLine("Alarm Raised!");
        }
    }

    public class Alarm
    {
        // Alarms
        // A process interested in alarms can bind subscribers to this event
        public Action OnAlarmRaised { get; set; }

        public void RaiseAlarm()
        {
            // must first check if there are any subscribers            
            if (OnAlarmRaised != null)
            {
                OnAlarmRaised();
            }

            // OnAlarmRaised?.Invoke();
            // A delegate exposes an Invoke method to invoke the methods bound to the delegate
        }
    }
}
