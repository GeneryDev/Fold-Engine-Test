using System;
using System.Collections.Generic;
using FoldEngine.Scenes;

namespace FoldEngine.Events {
    public class EventMap {
        /// <summary>
        /// The scene this event map belongs to
        /// </summary>
        private readonly Scene _scene;

        /// <summary>
        /// The map containing the event queues.<br></br>
        /// </summary>
        internal readonly Dictionary<Type, IEventQueue> Map = new Dictionary<Type, IEventQueue>();

        private List<IEventQueue> _afterSystemQueues = new List<IEventQueue>();
        private List<IEventQueue> _endQueues = new List<IEventQueue>();

        /// <summary>
        /// Creates an EventMap attached to the given scene.
        /// </summary>
        /// <param name="scene"></param>
        internal EventMap(Scene scene) => _scene = scene;
        
        private EventQueue<T> Get<T>() where T : struct {
            if(!Map.ContainsKey(typeof(T))) {
                var queue = new EventQueue<T>();
                Map[typeof(T)] = queue;
                switch(queue.EventAttribute.FlushMode) {
                    case EventFlushMode.AfterSystem: {
                        _afterSystemQueues.Add(queue);
                        break;
                    }
                    case EventFlushMode.End: {
                        _endQueues.Add(queue);
                        break;
                    }
                    case EventFlushMode.Immediate:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return (EventQueue<T>) Map[typeof(T)];
        }

        public void Invoke<T>(T evt) where T : struct {
            Get<T>().Enqueue(evt);
        }

        public EventUnsubscriber Subscribe<T>(Event.EventListener<T> listener) where T : struct {
            return Get<T>().Subscribe(listener);
        }

        public void FlushAfterSystem() {
            foreach(IEventQueue queue in _afterSystemQueues) {
                queue.Flush();
            }
        }

        public void FlushEnd() {
            foreach(IEventQueue queue in _endQueues) {
                queue.Flush();
            }
        }
    }
}