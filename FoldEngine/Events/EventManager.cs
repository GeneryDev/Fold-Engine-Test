using System;
using System.Collections.Generic;
using FoldEngine.Scenes;

namespace FoldEngine.Events {
    public class EventManager {
        public Scene Owner;
        private event EventHandler<Event> Dummy = DummyEvent;

        public readonly Dictionary<Type, EventHandler<Event>> EventDictionary =
            new Dictionary<Type, EventHandler<Event>>();

        public EventHandler<Event> this[Type type] => EventDictionary[type];

        public EventManager(Scene owner) {
            Owner = owner;
        }

        public void RegisterEventType(Type type) {
            if(!EventDictionary.ContainsKey(type)) EventDictionary.Add(type, Dummy);
        }

        public void RegisterEventType<T>() where T : Event {
            RegisterEventType(typeof(T));
        }

        public void InvokeEvent(Event evt) {
            RegisterEventType(evt.GetType());
            EventDictionary[evt.GetType()].Invoke(this, evt);
        }

        private static void DummyEvent(object sender, Event evt) { }
    }
}