using System;
using System.Collections.Generic;

namespace FoldEngine.Input {
    public class ControlScheme {
        public string Name;
        private List<IInputDevice> _devices = new List<IInputDevice>();
        private Dictionary<string, IAction> _actions = new Dictionary<string, IAction>();

        public ControlScheme(string name) {
            Name = name;
        }

        public bool IsBeingUsed {
            get {
                foreach(IInputDevice device in _devices) {
                    if(device.IsBeingUsed) return true;
                }

                return false;
            }
        }

        public T Get<T>(string identifier) where T : class, IAction {
            if(_actions.ContainsKey(identifier)) {
                IAction action = _actions[identifier];
                if(action is T actionT) return actionT;
            }

            return GetDefaultAction<T>();
        }

        private T GetDefaultAction<T>() where T : class, IAction {
            IAction defaultAction = null;
            if(typeof(T) == typeof(ButtonAction)) defaultAction = ButtonAction.Default as T;
            if(typeof(T) == typeof(AnalogAction)) defaultAction = AnalogAction.Default as T;
            return (T) defaultAction;
        }

        public void AddDevice(IInputDevice device) {
            _devices.Add(device);
        }

        public void PutAction(string identifier, IAction action) {
            _actions[identifier] = action;
        }
    }
}