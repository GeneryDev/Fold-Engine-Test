using System;
using System.Collections.Generic;
using System.Reflection;

namespace FoldEngine.Events {
    public interface IEventQueue {
        void Flush();
        void Unsubscribe(object listener);
    }

    public class EventQueue<T> : IEventQueue where T : struct {
        private const int StartingSize = 4;
        
        private T[] _events; //resized immediately, holds all queued events
        
        private List<Event.EventListener<T>> _listeners = new List<Event.EventListener<T>>();

        private int _insertionIndex = 0;
        private int _flushIndex = 0;

        internal readonly EventAttribute EventAttribute;

        public EventQueue() {
            EventAttribute = typeof(T).GetCustomAttribute<EventAttribute>();
            _events = new T[StartingSize];
        }

        public void Flush() {
            _flushIndex = 0;
            while(_flushIndex < _insertionIndex) {

                foreach(Event.EventListener<T> listener in _listeners) {
                    listener(ref _events[_flushIndex]);
                }
                
                _flushIndex++;
            }
            _insertionIndex = 0;
        }

        public void Unsubscribe(object listener) {
            _listeners.Remove(listener as Event.EventListener<T>);
        }

        public ref T Enqueue(T evt) {
            // Add the event to the array (resize if needed)
            // even if there are no listeners - this is so we can return a reference to it.
            if(_insertionIndex >= _events.Length) {
                Console.WriteLine("Resizing event array");
                Array.Resize(ref _events, _events.Length * 2);
            }
            _events[_insertionIndex] = evt;
            
            // If there are no listeners, do nothing else.
            if(_listeners.Count == 0) return ref _events[_insertionIndex];
            
            // If flush mode is immediate, invoke all listeners.
            if(EventAttribute.FlushMode == EventFlushMode.Immediate) {
                foreach(Event.EventListener<T> listener in _listeners) {
                    listener(ref _events[_flushIndex]);
                }
                return ref _events[_insertionIndex];
            }
            
            // The event was added, with the intention of adding more later.
            // Increment the insertion index.
            _insertionIndex++;

            return ref _events[_insertionIndex-1];
        }

        public static EventQueue<T> operator +(EventQueue<T> queue, Event.EventListener<T> listener) {
            queue._listeners.Add(listener);
            return queue;
        }

        public EventUnsubscriber Subscribe(Event.EventListener<T> listener) {
            _listeners.Add(listener);
            return new EventUnsubscriber() {
                EventQueue = this,
                Listener = listener
            };
        }
    }
    
    public struct EventUnsubscriber {
        internal IEventQueue EventQueue;
        internal object Listener;

        public void Unsubscribe() {
            EventQueue.Unsubscribe(Listener);
        }
    }
}